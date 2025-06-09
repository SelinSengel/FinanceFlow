using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FinanceFlow.BlazorServerApp.Services
{
    public class BlazorAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedLocalStorage _storage;
        private readonly HttpClient _http;
        private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
        private const string TokenKey = "authToken";

        public BlazorAuthStateProvider(
            ProtectedLocalStorage storage,
            HttpClient http)
        {
            _storage = storage;
            _http = http;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var result = await _storage.GetAsync<string>(TokenKey);
            var token = result.Success ? result.Value : null;

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(_anonymous);

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var claims = ParseClaimsFromJwt(token);
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwtAuthType"));
            return new AuthenticationState(user);
        }

        public async Task NotifyUserAuthentication(string token)
        {
            // Token'ı sakla
            await _storage.SetAsync(TokenKey, token);

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwtAuthType");
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(user)));
        }

        public Task NotifyUserLogout()
        {
            _http.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(_anonymous)));
            return Task.CompletedTask;
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            if (string.IsNullOrWhiteSpace(jwt)) return claims;

            try
            {
                var parts = jwt.Split('.');
                var payload = parts.Length > 1 ? parts[1] : throw new FormatException();
                // Base64 padding ekle
                payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                var bytes = Convert.FromBase64String(payload);
                var keyVals = JsonSerializer.Deserialize<Dictionary<string, object>>(bytes);

                if (keyVals == null) return claims;

                // 1) NameIdentifier
                if (keyVals.TryGetValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", out var id))
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, id.ToString()));

                // 2) Name
                if (keyVals.TryGetValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", out var nm))
                    claims.Add(new Claim(ClaimTypes.Name, nm.ToString()));

                // 3) Email
                if (keyVals.TryGetValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", out var em))
                    claims.Add(new Claim(ClaimTypes.Email, em.ToString()));

                // 4) Role
                if (keyVals.TryGetValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out var rl))
                    claims.Add(new Claim(ClaimTypes.Role, rl.ToString()));

                // Diğer standart JWT claim’lerini de isterseniz ekleyebilirsiniz:
                if (keyVals.TryGetValue("exp", out var exp)) claims.Add(new Claim("exp", exp.ToString()!));
                if (keyVals.TryGetValue("iss", out var iss)) claims.Add(new Claim("iss", iss.ToString()!));
                if (keyVals.TryGetValue("aud", out var aud)) claims.Add(new Claim("aud", aud.ToString()!));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ParseClaimsFromJwt error: {ex.Message}");
            }

            return claims;
        }


        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}

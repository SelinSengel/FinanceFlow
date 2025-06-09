using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;
    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
    private const string TokenKey = "authToken";

    public CustomAuthStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
        Console.WriteLine("CustomAuthStateProvider: Constructor called.");
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        Console.WriteLine("CustomAuthStateProvider: GetAuthenticationStateAsync called.");
        string? token = null;
        try
        {
            token = await GetTokenAsync();
            Console.WriteLine($"CustomAuthStateProvider: Token from localStorage: {(string.IsNullOrWhiteSpace(token) ? "NULL or EMPTY" : "Retrieved")}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CustomAuthStateProvider: ERROR getting token from localStorage: {ex.Message}");
            Console.WriteLine($"CustomAuthStateProvider: StackTrace: {ex.StackTrace}");
            // JavaScript hatası durumunda, kullanıcıyı anonim olarak işaretle ve devam etmeyi dene
            // Bu, uygulamanın tamamen çökmesini engelleyebilir.
            return new AuthenticationState(_anonymous);
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            Console.WriteLine("CustomAuthStateProvider: Token is null or whitespace, returning anonymous.");
            return new AuthenticationState(_anonymous);
        }

        ClaimsPrincipal? claimsPrincipal = null;
        try
        {
            claimsPrincipal = GetClaimsPrincipalFromToken(token);
            Console.WriteLine($"CustomAuthStateProvider: ClaimsPrincipal from token: {(claimsPrincipal == null ? "NULL" : "Parsed")}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CustomAuthStateProvider: ERROR parsing token or creating ClaimsPrincipal: {ex.Message}");
            Console.WriteLine($"CustomAuthStateProvider: StackTrace: {ex.StackTrace}");
            // Token parse edilemiyorsa, geçersizdir. Temizleyip anonim dönelim.
            await RemoveTokenAsync(); // Sorunlu token'ı temizle
            return new AuthenticationState(_anonymous);
        }


        if (claimsPrincipal == null)
        {
            Console.WriteLine("CustomAuthStateProvider: ClaimsPrincipal is null (token invalid or expired), removing token and returning anonymous.");
            await RemoveTokenAsync();
            return new AuthenticationState(_anonymous);
        }

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            Console.WriteLine("CustomAuthStateProvider: Authorization header set on HttpClient.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CustomAuthStateProvider: ERROR setting Authorization header: {ex.Message}");
            // Bu kritik bir hata olmayabilir, loglayıp devam edelim.
        }

        Console.WriteLine("CustomAuthStateProvider: Returning authenticated state.");
        return new AuthenticationState(claimsPrincipal);
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        Console.WriteLine("CustomAuthStateProvider: MarkUserAsAuthenticated called.");
        await SetTokenAsync(token);
        var claimsPrincipal = GetClaimsPrincipalFromToken(token); // Bu metodun null dönebileceğini unutma
        Console.WriteLine($"CustomAuthStateProvider: Notifying auth state changed. ClaimsPrincipal is {(claimsPrincipal == null ? "NULL" : "VALID")}");
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal ?? _anonymous)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        Console.WriteLine("CustomAuthStateProvider: MarkUserAsLoggedOut called.");
        await RemoveTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = null;
        Console.WriteLine("CustomAuthStateProvider: Notifying auth state changed for logout.");
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    private ClaimsPrincipal? GetClaimsPrincipalFromToken(string token)
    {
        Console.WriteLine("CustomAuthStateProvider: GetClaimsPrincipalFromToken called.");
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            if (!tokenHandler.CanReadToken(token))
            {
                Console.WriteLine("CustomAuthStateProvider: TokenHandler cannot read token.");
                return null;
            }

            var jwtToken = tokenHandler.ReadJwtToken(token);

            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                Console.WriteLine("CustomAuthStateProvider: Token is expired.");
                return null;
            }

            Console.WriteLine("CustomAuthStateProvider: Token successfully parsed and validated (not expired).");
            var identity = new ClaimsIdentity(jwtToken.Claims, "jwtAuthType"); // "jwtAuthType" önemli olabilir, bazı sistemler bunu bekler.
            return new ClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CustomAuthStateProvider: ERROR in GetClaimsPrincipalFromToken: {ex.Message}");
            Console.WriteLine($"CustomAuthStateProvider: StackTrace: {ex.StackTrace}");
            return null;
        }
    }

    private async Task SetTokenAsync(string token)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
            Console.WriteLine("CustomAuthStateProvider: Token set in localStorage.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CustomAuthStateProvider: ERROR setting token in localStorage: {ex.Message}");
        }
    }

    private async Task<string?> GetTokenAsync()
    {
        // Bu metodun kendisi de exception fırlatabilir, üst katmanda yakalanıyor.
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
    }

    private async Task RemoveTokenAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
            Console.WriteLine("CustomAuthStateProvider: Token removed from localStorage.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CustomAuthStateProvider: ERROR removing token from localStorage: {ex.Message}");
        }
    }
}
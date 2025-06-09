using System;
using FinanceFlow.Shared.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;


namespace FinanceFlow.BlazorServerApp.Services
{
    public class AuthApiService
    {
        private readonly HttpClient _http;
        private readonly ProtectedLocalStorage _storage;
        private readonly BlazorAuthStateProvider _authProvider;
        private const string TokenKey = "authToken";

        public AuthApiService(
            HttpClient http,
            ProtectedLocalStorage storage,
            AuthenticationStateProvider authProvider)
        {
            _http = http;
            _storage = storage;
            _authProvider = (BlazorAuthStateProvider)authProvider;
        }

        private async Task StoreTokenAsync(string token)
        {
            await _storage.SetAsync(TokenKey, token);
            await _authProvider.NotifyUserAuthentication(token);
        }

        public async Task<AuthOperationResponse> LoginIndividualAsync(LoginModel model)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/login-individual", model);
                if (!response.IsSuccessStatusCode)
                    return new AuthOperationResponse
                    {
                        Succeeded = false,
                        Message = await response.Content.ReadAsStringAsync()
                    };

                var dto = await response.Content.ReadFromJsonAsync<TokenResponseModel>();
                if (dto == null || string.IsNullOrWhiteSpace(dto.Token))
                    return new AuthOperationResponse
                    {
                        Succeeded = false,
                        Message = "API'den geçersiz yanıt alındı."
                    };

                await StoreTokenAsync(dto.Token);
                return new AuthOperationResponse
                {
                    Succeeded = true,
                    Token = dto.Token,
                    Message = "Giriş başarılı."
                };
            }
            catch (Exception ex)
            {
                return new AuthOperationResponse
                {
                    Succeeded = false,
                    Message = $"Giriş sırasında hata: {ex.Message}"
                };
            }
        }

        public async Task<AuthOperationResponse> LoginCorporateAsync(LoginModel model)
            => await LoginInternalAsync("api/auth/login-corporate", model);

        private async Task<AuthOperationResponse> LoginInternalAsync(string endpoint, object model)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(endpoint, model);
                if (!response.IsSuccessStatusCode)
                    return new AuthOperationResponse
                    {
                        Succeeded = false,
                        Message = await response.Content.ReadAsStringAsync()
                    };

                var dto = await response.Content.ReadFromJsonAsync<TokenResponseModel>();
                if (dto == null || string.IsNullOrWhiteSpace(dto.Token))
                    return new AuthOperationResponse
                    {
                        Succeeded = false,
                        Message = "API'den geçersiz yanıt alındı."
                    };

                await StoreTokenAsync(dto.Token);
                return new AuthOperationResponse
                {
                    Succeeded = true,
                    Token = dto.Token,
                    Message = "Giriş başarılı."
                };
            }
            catch (Exception ex)
            {
                return new AuthOperationResponse
                {
                    Succeeded = false,
                    Message = $"Giriş sırasında hata: {ex.Message}"
                };
            }
        }

        public async Task<AuthOperationResponse> RegisterIndividualAsync(RegisterModel model)
            => await RegisterInternalAsync("api/auth/register-individual", model);

        public async Task<AuthOperationResponse> RegisterCorporateAsync(CorporateRegisterModel model)
            => await RegisterInternalAsync("api/auth/register-corporate", model);

        private async Task<AuthOperationResponse> RegisterInternalAsync(string endpoint, object model)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(endpoint, model);
                if (!response.IsSuccessStatusCode)
                    return new AuthOperationResponse
                    {
                        Succeeded = false,
                        Message = await response.Content.ReadAsStringAsync()
                    };

                var dto = await response.Content.ReadFromJsonAsync<TokenResponseModel>();
                if (dto == null || string.IsNullOrWhiteSpace(dto.Token))
                    return new AuthOperationResponse
                    {
                        Succeeded = false,
                        Message = "API'den geçersiz yanıt alındı."
                    };

                await StoreTokenAsync(dto.Token);
                return new AuthOperationResponse
                {
                    Succeeded = true,
                    Token = dto.Token,
                    Message = "Kayıt başarılı."
                };
            }
            catch (Exception ex)
            {
                return new AuthOperationResponse
                {
                    Succeeded = false,
                    Message = $"Kayıt sırasında hata: {ex.Message}"
                };
            }
        }

        public async Task LogoutAsync()
        {
            await _storage.DeleteAsync(TokenKey);
            await _authProvider.NotifyUserLogout();
        }
    }
}

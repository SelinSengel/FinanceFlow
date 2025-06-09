using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinanceFlow.Shared.Models;

namespace FinanceFlow.BlazorServerApp.Services
{
    public class ExpenseApiClient : IExpenseService
    {
        private readonly HttpClient _http;
        public ExpenseApiClient(HttpClient http) => _http = http;

        public Task<decimal> GetTodayTotalAsync()
            => _http.GetFromJsonAsync<decimal>("api/expenses/today-total");

        public async Task<IEnumerable<ExpenseModel>> GetAllAsync(int userId)
            => await _http.GetFromJsonAsync<IEnumerable<ExpenseModel>>($"api/expenses?userId={userId}");

        public async Task<IEnumerable<ExpenseModel>> GetAllSharedAsync()
            => await _http.GetFromJsonAsync<IEnumerable<ExpenseModel>>("api/expenses/shared");

        public async Task<ExpenseModel> CreateAsync(int userId, ExpenseModel m)
            => await _http.PostAsJsonAsync($"api/expenses/{userId}/create", m)
                          .Result.Content.ReadFromJsonAsync<ExpenseModel>();

        public async Task<bool> UpdateAsync(int userId, ExpenseModel m)
            => (await _http.PutAsJsonAsync($"api/expenses/{userId}/{m.Id}", m)).IsSuccessStatusCode;

        public async Task<bool> DeleteAsync(int userId, int expenseId)
            => (await _http.DeleteAsync($"api/expenses/{userId}/{expenseId}")).IsSuccessStatusCode;

        public async Task<IEnumerable<HighlightModel>> GetHighlightsAsync()
            => await _http.GetFromJsonAsync<IEnumerable<HighlightModel>>("api/expenses/highlights");

    }
}

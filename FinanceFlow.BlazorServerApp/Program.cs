using FinanceFlow.BlazorServerApp.Services;
using FinanceFlow.Shared.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using static System.Net.WebRequestMethods;

var builder = WebApplication.CreateBuilder(args);

// 1) Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// 2) AuthState + Protected Storage
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<BlazorAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<BlazorAuthStateProvider>());


// 3) Authentication & Authorization
builder.Services.AddAuthentication("Bearer") 
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "http://localhost:7141";
        options.RequireHttpsMetadata = false;
        options.Audience = "financeflow_api";
    });
builder.Services.AddAuthorization();

// 4) API & ExpenseClient
builder.Services.AddScoped<ExpenseApiClient>();
// A) Auth API servisiniz (login/register)
builder.Services.AddScoped<AuthApiService>();

// B) HttpClient for UI’dan doğrudan API çağırmak için
builder.Services.AddScoped(sp =>
{
    // Burada NavigationManager yerine doğrudan sabit base URL kullanıyoruz
    return new HttpClient { BaseAddress = new Uri("http://localhost:7141/") };
});

// C) Expense API client (typed client)
builder.Services.AddHttpClient<IExpenseService, ExpenseApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:7141/");
});

// 5) CORS (isteğe bağlı)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorDev", policy =>
        policy.WithOrigins("http://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}


//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// CORS
app.UseCors("AllowBlazorDev");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

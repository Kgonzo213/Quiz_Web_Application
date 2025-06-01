using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Programowanie_Projekt_Web.Repo;

var builder = WebApplication.CreateBuilder(args);

// Dodanie Razor Pages
builder.Services.AddRazorPages();
builder.Services.AddSession(); // Dodaj obsługę sesji

// Rejestracja repozytorium
builder.Services.AddScoped<IAskRepos, AskRepos>();

var app = builder.Build();

app.UseSession(); // Włącz middleware sesji
// Konfiguracja potoku HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();

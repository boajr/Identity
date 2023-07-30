using Boa.Identity.Telegram;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using TestTelegram.Data;
using TestTelegram.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var botToken = builder.Configuration.GetValue<string>("Telegram:BotToken")
               ?? throw new Exception("'Telegram:BotToken' string not found.");
builder.Services.AddTelegramBot(options =>
    options.BotToken = botToken);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddBoaIdentity<IdentityTelegramUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddBoaEntityFrameworkTelegramStores<ApplicationDbContext>()
    .AddBoaResetPasswordServiceDefaultUIEmail()
    .AddBoaResetPasswordServiceTelegram();
builder.Services.AddRazorPages();

builder.Services.AddTransient<IEmailSender, EmailSender>();  // forse va bene anche scoped o singleton???

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

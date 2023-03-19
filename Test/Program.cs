using Boa.Identity.Telegram;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Test.Data;
using Test.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

builder.Services.AddTransient<IEmailSender, EmailSender>();

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




app.UsePathBase("/ambrogio");




app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

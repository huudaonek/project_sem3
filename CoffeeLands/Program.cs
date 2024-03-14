using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using CoffeeLands.Data;
using CoffeeLands.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;
using CoffeeLands.Services;
using System.Configuration;
using CoffeeLands.ViewModels.Mail;
using CoffeeLands.ViewModels.Momo;
using System.Text.Json.Serialization;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CoffeeLandsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CoffeeLandsContext") ?? throw new InvalidOperationException("Connection string 'CoffeeLandsContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


builder.Services.AddMvc().AddSessionStateTempDataProvider();
builder.Services.AddMvc().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});
builder.Services.AddSession();

var provider = builder.Services.BuildServiceProvider();
var config = provider.GetRequiredService<IConfiguration>();
builder.Services.AddDbContext<CoffeeLandsContext>(item => item.UseSqlServer(config.GetConnectionString("CoffeeLandsContext")));

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(
    options =>
{
    options.LoginPath = "/Users/Login";
    options.AccessDeniedPath = "/Users/AccessDenied";
});
//builder.Services.AddScoped<IPayPalService, PayPalService>();
builder.Services.AddSingleton<IVNPayService, VNPayService>();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<IMailService, MailService>();


builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();


app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

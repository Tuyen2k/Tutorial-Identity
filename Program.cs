using AppDb.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using System.Security.Claims;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

// nạp value cho configuration
builder.Configuration["Authentication:Google:ClientId"] = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
builder.Configuration["Authentication:Google:ClientSecret"] = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
builder.Configuration["Authentication:Microsoft:ClientId"] = Environment.GetEnvironmentVariable("MICROSOFT_CLIENT_ID");
builder.Configuration["Authentication:Microsoft:ClientSecret"] = Environment.GetEnvironmentVariable("MICROSOFT_CLIENT_SECRET");
builder.Configuration["Authentication:Github:ClientId"] = Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID");
builder.Configuration["Authentication:Github:ClientSecret"] = Environment.GetEnvironmentVariable("GITHUB_CLIENT_SECRET");
builder.Configuration["MailSettings:Password"] = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

builder.Services.Configure<MailSetting>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddSingleton<MailSetting>(sp => sp.GetRequiredService<IOptions<MailSetting>>().Value);


builder.Services.AddIdentity<AppUser, IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();




builder.Services.AddSingleton<IEmailSender, SendMessage>();


// Truy cập IdentityOptions
builder.Services.Configure<IdentityOptions> (options => {
    // Thiết lập về Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
    options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

    // Cấu hình Lockout - khóa user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes (5); // Khóa 5 phút
    options.Lockout.MaxFailedAccessAttempts = 5; // Thất bại 5 lần thì khóa
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình về User.
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+" + " ";
    options.User.RequireUniqueEmail = true;  // Email là duy nhất

    // Cấu hình đăng nhập.
    options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
    options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
    options.SignIn.RequireConfirmedAccount = true;
});

builder.Services.ConfigureApplicationCookie (options => {
    options.LoginPath = "/dangnhap/";
    options.LogoutPath = "/dangxuat/";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});


// configure external authentication
builder.Services.AddAuthentication()
    .AddGoogle(options => {
        var googleConfigue = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = googleConfigue.GetValue<string>("ClientId");
        options.ClientSecret = googleConfigue.GetValue<string>("ClientSecret");
        options.CallbackPath = "/login-google";
    })
    .AddMicrosoftAccount(options => {
        var microsoftConfigue = builder.Configuration.GetSection("Authentication:Microsoft");
        options.ClientId = microsoftConfigue.GetValue<string>("ClientId");
        options.ClientSecret = microsoftConfigue.GetValue<string>("ClientSecret");
        options.CallbackPath = "/login-microsoft";

        // tạo scope
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("User.Read");

        // mapping dữ liệu user trên microsoft account
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email", "string");
    })
    
    ;

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

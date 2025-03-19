using AppDb.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using System.Security.Claims;
using DotNetEnv;
using TutorialIdentity.Services;
using TutorialIdentity.Models;
using Serilog;
using Microsoft.CodeAnalysis.Options;


var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    string connectionString = Environment.GetEnvironmentVariable("DATABASE_URL"); // iis vmware

    if (string.IsNullOrEmpty(connectionString)) {
        connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); // dev
        if (string.IsNullOrEmpty(connectionString)) {
            throw new Exception("Không tìm thấy chuỗi kết nối trong appsettings.json hoặc biến môi trường DATABASE_URL");
        }
    }
    options.UseSqlServer(connectionString);    
});

// Cấu hình Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Đọc từ appsettings.json
    .WriteTo.Console() // Ghi log ra console
    .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day) // Ghi log ra file
    .CreateLogger();

// Thay thế Logger mặc định của .NET bằng Serilog
builder.Host.UseSerilog();

// cấu hình mail appsettings.json
//builder.Services.Configure<MailSetting>(builder.Configuration.GetSection("MailSettings"));


// cấu hình mail env
builder.Services.Configure<MailSetting>(option => {
    string password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? "";
    option.Mail = "tuyen4835@gmail.com";
    option.DisplayName = "Admin app tutorial identity";
    option.Host = "smtp.gmail.com";
    option.Port = 587;
    option.Password = password;

    Log.Information("MailSetting.Password: {0}", option.Password.ToString());
});


// congigue service
builder.Services.AddSingleton<MailSetting>(sp => sp.GetRequiredService<IOptions<MailSetting>>().Value);
builder.Services.AddScoped<SignInManager<AppUser>, CustomSignInManager>();


// cấu hình Identity
builder.Services.AddIdentity<AppUser, AppRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();


// cấu hình DI
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

// Cấu hình Cookie Authentication
builder.Services.ConfigureApplicationCookie (options => {
    options.LoginPath = "/login/";
    options.LogoutPath = "/logout/";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});


// configure external authentication
builder.Services.AddAuthentication()
    .AddGoogle(option => {
        // lấy thông tin từ appsettings.json
        //var googleConfigue = builder.Configuration.GetSection("Authentication:Google");
        //options.ClientId = googleConfigue.GetValue<string>("ClientId");
        //options.ClientSecret = googleConfigue.GetValue<string>("ClientSecret");

        // lấy thông tin từ env
        string clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? "";
        string clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? "";

        // cấu hình
        option.ClientId = clientId;
        option.ClientSecret = clientSecret;
        option.CallbackPath = "/login-google";

        Log.Information($"AddGoogle: ID: {option.ClientId} - {option.ClientSecret}");
    })
    .AddMicrosoftAccount(option => {
        // lấy thông tin từ appsettings.json
        //var microsoftConfigue = builder.Configuration.GetSection("Authentication:Microsoft");
        //options.ClientId = microsoftConfigue.GetValue<string>("ClientId");
        //options.ClientSecret = microsoftConfigue.GetValue<string>("ClientSecret");


        // lấy thông tin từ env
        string clientId = Environment.GetEnvironmentVariable("MICROSOFT_CLIENT_ID") ?? "";
        string clientSecret = Environment.GetEnvironmentVariable("MICROSOFT_CLIENT_SECRET") ?? "";

        // cấu hình
        option.ClientId = clientId;
        option.ClientSecret = clientSecret;
        option.CallbackPath = "/login-microsoft";

        // tạo scope
        option.Scope.Clear();
        option.Scope.Add("openid");
        option.Scope.Add("profile");
        option.Scope.Add("email");
        option.Scope.Add("User.Read");

        // mapping dữ liệu user trên microsoft account
        option.ClaimActions.MapJsonKey(ClaimTypes.Email, "email", "string");

        Log.Information($"AddMicrosoftAccount: ID: {option.ClientId} - {option.ClientSecret}");
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

//app.UseSerilogRequestLogging(); // Ghi log request HTTP

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();


/* note 11/03/2025
 * tutorial identity đã hosting được
 * lỗi redirect_url khi login bằng google, microsoft
 * Lỗi connectionString với SQL server
 */
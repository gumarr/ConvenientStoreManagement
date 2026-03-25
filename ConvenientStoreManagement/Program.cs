using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Services.Implementations;
using ConvenientStoreManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddAuthentication(options =>
{
    // Cookie là scheme chính — mọi request authenticated đều dùng cookie
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // KHÔNG set DefaultChallengeScheme — để GoogleLogin page tự gọi Challenge(Google)
})
.AddCookie(options =>
{
    options.LoginPath = "/Authentication/Login";
    options.LogoutPath = "/Authentication/Logout";
    options.AccessDeniedPath = "/Authentication/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;

    // Middleware tự xử lý /signin-google, sau đó redirect về RedirectUri
    // mà ta truyền vào lúc Challenge() — là /Authentication/GoogleCallback
    options.CallbackPath = "/signin-google";

    options.Scope.Add("email");
    options.Scope.Add("profile");
    options.SaveTokens = true;

    // Bắt lỗi cancel/deny từ Google trước khi middleware throw exception.
    // Khi user bấm Cancel, Google redirect về /signin-google?error=access_denied
    // — OnRemoteFailure intercept và redirect về Login thay vì crash.
    options.Events.OnRemoteFailure = context =>
    {
        context.Response.Redirect("/Authentication/Login?cancelled=true");
        context.HandleResponse();
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();

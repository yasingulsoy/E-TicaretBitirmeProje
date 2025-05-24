 using System.Security.Claims;
using ETicaret.Data.Context;
using ETicaret.Service.Abstract;
using ETicaret.Service.Concrete;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".ETicaret.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout=TimeSpan.FromDays(1);
    options.IOTimeout=TimeSpan.FromMinutes(10);
});//Session yap�s� favorileri listelemek i�in
builder.Services.AddDbContext<DatabaseContext>();//DatabaseContext hatas� varsa bunu ekle
builder.Services.AddScoped(typeof(IService<>),typeof(Service<>));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(x =>
{
    x.LoginPath = "/Account/SignIn";//Kullan�c� giri� i�in
    x.AccessDeniedPath = "/AccessDenied";//Kullan�c�n�n Eri�imi d���nda ki yerleri engellemek i�in
    x.Cookie.Name = "Account";
    x.Cookie.MaxAge=TimeSpan.FromDays(1);//Hesab�n ne kadar a��k kalabilece�ini g�sterir.
    x.Cookie.IsEssential = true;//Kal�c� cookie olu�turma
});
builder.Services.AddAuthorization(x =>
{
    x.AddPolicy("AdminPolicy", policy => policy.RequireClaim(ClaimTypes.Role,"Admin"));//AdminPolicynin kullan�ld��� yerlere sadece adminler girebilir
    x.AddPolicy("UserPolicy", policy => policy.RequireClaim(ClaimTypes.Role, "Admin","User","Customer"));//UserPolicynin kullan�ld��� yerlere rolleri tan�ml� olanlar girebilir.
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession(); //Session kullanaca��m�z� belirttik

app.UseAuthentication();//�nce oturum a�ma
app.UseAuthorization();//Sonra yetkilendirme
app.MapControllerRoute(
            name: "admin",
            pattern: "{area:exists}/{controller=Main}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "Home",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

using NewERP.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddHttpClient<LoginService>();
builder.Services.AddHttpClient<EmployeService>();
builder.Services.AddHttpClient<DepartmentService>();
builder.Services.AddHttpClient<GenderService>();
builder.Services.AddHttpClient<SalaireService>();
builder.Services.AddHttpClient<DataService>();

// 2. Activer la session
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. Authentification par cookie
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Login/Login"; // URL pour redirection login
        options.LogoutPath = "/Login/Logout";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// 4. Ajouter les contrôleurs MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 5. Gérer les erreurs
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 6. Middleware requis dans l'ordre correct
app.UseHttpsRedirection();
app.UseStaticFiles(); // ← nécessaire si tu as des fichiers CSS/JS

app.UseRouting();

app.UseAuthentication(); // ← Ajoute ça AVANT Authorization
app.UseAuthorization();

app.UseSession(); // ← Active la session ici AVANT les routes

// 7. Routing MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();

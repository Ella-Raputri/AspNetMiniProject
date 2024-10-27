using RentalCarFront.Handler;
using RentalCarFront.Service;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set the session timeout
    options.Cookie.HttpOnly = true; // Make the cookie HTTP only
    options.Cookie.IsEssential = true; // Make the session cookie essential
});

// Add authentication with cookies
builder.Services.AddAuthentication("Cookies") // Replace with your scheme name
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/User/Login"; // Path for redirecting when unauthorized
});

// Register your services
builder.Services.AddScoped<IUser, UserHandler>();

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

// Use CORS
app.UseCors("AllowAllOrigins");

// Enable authentication
app.UseAuthentication(); // This should come before UseAuthorization
app.UseAuthorization();

// Enable session
app.UseSession(); // Ensure this is added before your route mapping

// Map routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

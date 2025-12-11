using Microsoft.EntityFrameworkCore;
using SensoreApp.Data; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // REQUIRED for MVC

// --- DATABASE CONTEXT AND CONNECTION STRING ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                       "Data Source=SensoreTrace.db"; 

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString)); 
// --------------------------------------------------

var app = builder.Build();

// --- INITIALIZE DATABASE AND SEED USER DATA ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating or seeding the database.");
    }
}
// ---------------------------------------------

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization(); 

// --- MVC ROUTING: Sets the default route ---
// When the app starts, it will look for Controller=Home, Action=Index.
// We override this below to point to Login first.
app.MapControllerRoute(
    name: "default",
    // CRITICAL FIX: Directs root URL to the Account Controller's Login action
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
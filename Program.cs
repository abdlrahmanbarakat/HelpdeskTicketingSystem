var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add in-memory cache required for session storage.
builder.Services.AddDistributedMemoryCache();

// Enable session support. Session will be used to store simple login info (UserId, FullName).
builder.Services.AddSession(options =>
{
    // Keep session idle timeout short for simplicity (adjust as needed).
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddSingleton<HelpdeskSystem.Data.Db>();

// Register UserDb so it can be injected into controllers/pages.
// This allows constructors to receive UserDb; it depends on Db which is already registered.
builder.Services.AddScoped<HelpdeskSystem.Data.UserDb>();
builder.Services.AddScoped<HelpdeskSystem.Data.CategoryDb>();
builder.Services.AddScoped<HelpdeskSystem.Data.TicketDb>();

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

// Enable session middleware so HttpContext.Session is available in controllers.
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
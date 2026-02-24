var builder = WebApplication.CreateBuilder(args);

// add controllers with views
builder.Services.AddControllersWithViews();

// in-memory cache required for session
builder.Services.AddDistributedMemoryCache();

// enable session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddSingleton<HelpdeskSystem.Data.Db>();

// register data services
builder.Services.AddScoped<HelpdeskSystem.Data.UserDb>();
builder.Services.AddScoped<HelpdeskSystem.Data.CategoryDb>();
builder.Services.AddScoped<HelpdeskSystem.Data.TicketDb>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// enable session middleware
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
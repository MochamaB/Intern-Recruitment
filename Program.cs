using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Workflows.Data;
using Workflows.Models;
using Workflows.Services;


var builder = WebApplication.CreateBuilder(args);
// Add all the DB Contexts Here. The connection strings in GetConnectionString() is in appsettings.json.
builder.Services.AddDbContext<WorkflowsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WorkflowsContext") ?? throw new InvalidOperationException("Connection string 'WorkflowsContext' not found.")));
builder.Services.AddDbContext<KtdaleaveContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("KTDALeaveContext") ?? throw new InvalidOperationException("Connection string 'KTDALeaveContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IApprovalService,ApprovalService>();


// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout as needed
    options.Cookie.HttpOnly = true; // Make the session cookie accessible only to the server
    options.Cookie.IsEssential = true; // Make the session cookie essential
});
builder.Services.AddHttpContextAccessor();

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

app.UseAuthorization();

// Add session middleware
app.UseSession();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();

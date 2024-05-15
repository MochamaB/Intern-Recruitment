using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Workflows.Data;
using Workflows.Models;
var builder = WebApplication.CreateBuilder(args);
// Add all the DB Contexts Here. The connection strings in GetConnectionString() is in appsettings.json.
builder.Services.AddDbContext<WorkflowsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WorkflowsContext") ?? throw new InvalidOperationException("Connection string 'WorkflowsContext' not found.")));
builder.Services.AddDbContext<KtdaleaveContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("KTDALeaveContext") ?? throw new InvalidOperationException("Connection string 'KTDALeaveContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();

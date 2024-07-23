using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Workflows.Authorization;
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
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthorizationHandler, ApprovalViewHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CustomRoleHandler>();
builder.Services.AddSingleton<CustomAuthorizationService>();
builder.Services.AddScoped<IAuthorizationService, CustomAuthorizationService>();
builder.Services.AddScoped<IRelationshipService, RelationshipService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewApproval", policy =>
        policy.Requirements.Add(new ApprovalViewRequirement()));
});


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
// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var ktdaContext = services.GetRequiredService<KtdaleaveContext>();
    var workflowsContext = services.GetRequiredService<WorkflowsContext>();

    ktdaContext.Database.Migrate();
    workflowsContext.Database.Migrate();

    SettingsSeeder.SeedSettings(ktdaContext, workflowsContext);
}

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

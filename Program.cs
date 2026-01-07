using LitackaApi.Data;
using LitackaApi.Services.Cards;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.SlidingExpiration = true;
    opt.ExpireTimeSpan = TimeSpan.FromDays(30);
});


builder.Services.AddOpenApi();

builder.Services.AddScoped<ICardsService, CardsService>();

var app = builder.Build();

await LitackaApi.Auth.IdentitySeed.SeedAsync(app.Services, app.Configuration);


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();


app.MapOpenApi();      // /openapi/v1.json
app.MapScalarApiReference(); // /scalar


app.Run();

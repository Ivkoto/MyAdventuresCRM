using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using SuiteCase.Core.Security;
using SuiteCase.Server.Data;
using SuiteCase.Server.Features.Customers;
using SuiteCase.Server.Security;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddValidation();

builder.Services.AddDbContext<SuiteCaseDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DatabaseConnection"));
});

builder.Services.AddDataProtection().SetApplicationName("SuiteCase");
//.PersistKeysToFileSystem(new DirectoryInfo(builder.Configuration["DataProtection:KeyRingPath"]!));

builder.Services.AddScoped<ISensitiveDataProtector, SensitiveDataProtector>();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "SuiteCase API v1");
    });

    await app.Services.InitializeDatabaseAsync();
}

app.UseHttpsRedirection();
app.MapCustomerEndpoints();
app.MapFallbackToFile("/index.html");

app.Run();

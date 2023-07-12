using Microsoft.AspNetCore.Authentication;



using MLFZUssdApplication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddTransient<AllCredentialHolder.QuickRefHolder>();

builder.Services.AddSingleton<SecuritySystemCheck.SecurityCheck>();

//builder.Services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();

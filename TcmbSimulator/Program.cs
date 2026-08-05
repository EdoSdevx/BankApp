using TcmbSimulator.Configuration;
using TcmbSimulator.Data;
using TcmbSimulator.Middleware;
using TcmbSimulator.Services;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("BankCode", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-Bank-Code",
        Description = "The five-digit sender bank code, for example 00001."
    });

    options.AddSecurityDefinition("Timestamp", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-Timestamp",
        Description = "The current Unix timestamp in seconds."
    });

    options.AddSecurityDefinition("Signature", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-Signature",
        Description = "Uppercase hexadecimal HMAC-SHA256 of bankCode, timestamp, and the exact JSON body."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("BankCode", document, externalResource: null)] = [],
        [new OpenApiSecuritySchemeReference("Timestamp", document, externalResource: null)] = [],
        [new OpenApiSecuritySchemeReference("Signature", document, externalResource: null)] = []
    });
});
builder.Services.Configure<BankAuthenticationOptions>(
    builder.Configuration.GetSection(BankAuthenticationOptions.SectionName));
builder.Services.AddScoped<TcmbDatabaseContext>();
builder.Services.AddScoped<IPaymentOrderDataAccess, PaymentOrderDataAccess>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.UseMiddleware<BankHmacAuthenticationMiddleware>();

app.MapControllers();

app.Run();

using BankApp2.Configuration;
using BankApp2.Data;
using BankApp2.Middleware;
using BankApp2.Services;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("SwitchCode", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-Switch-Code",
        Description = "Payment switch identifier, currently TCMB."
    });

    options.AddSecurityDefinition("Timestamp", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-Timestamp",
        Description = "Current Unix timestamp in seconds."
    });

    options.AddSecurityDefinition("Signature", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-Signature",
        Description = "Hexadecimal HMAC-SHA256 of switchCode, timestamp, and the exact JSON body."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("SwitchCode", document, externalResource: null)] = [],
        [new OpenApiSecuritySchemeReference("Timestamp", document, externalResource: null)] = [],
        [new OpenApiSecuritySchemeReference("Signature", document, externalResource: null)] = []
    });
});
builder.Services.Configure<SwitchAuthenticationOptions>(
    builder.Configuration.GetSection(SwitchAuthenticationOptions.SectionName));
builder.Services.AddScoped<RecipientDatabaseContext>();
builder.Services.AddScoped<IIncomingPaymentDataAccess, IncomingPaymentDataAccess>();
builder.Services.AddScoped<IIncomingPaymentService, IncomingPaymentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.UseMiddleware<SwitchHmacAuthenticationMiddleware>();

app.MapControllers();

app.Run();

using RateLimitApi.Config;
using RateLimitApi.Services;
using RateLimitApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddHttpClient();
builder.Services.AddSingleton<AlertConfig>();
builder.Services.AddSingleton<AbuseConfig>();
builder.Services.AddScoped<SqlRepository>();
builder.Services.AddScoped<RateLimitService>();
builder.Services.AddScoped<AbuseDetectionService>();
builder.Services.AddScoped<AlertService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

using SkyRoute.API.Data;
using SkyRoute.API.Pricing;
using SkyRoute.API.Providers;
using SkyRoute.API.Services;

var builder = WebApplication.CreateBuilder(args);

const string AngularDevCorsPolicy = "AllowAngularDev";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<InMemoryFlightStore>();
builder.Services.AddSingleton<InMemoryBookingStore>();

builder.Services.AddSingleton<GlobalAirPricingStrategy>();
builder.Services.AddSingleton<BudgetWingsPricingStrategy>();

builder.Services.AddScoped<IFlightProvider, GlobalAirProvider>();
builder.Services.AddScoped<IFlightProvider, BudgetWingsProvider>();

builder.Services.AddScoped<FlightAggregatorService>();
builder.Services.AddScoped<BookingService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(AngularDevCorsPolicy);
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

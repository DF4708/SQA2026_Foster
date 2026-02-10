using AdamTibi.OpenWeather;
using Uqs.Weather;
using Uqs.Weather.Wrappers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// IClient is the OpenWeather API dependency. Registering as Singleton keeps a single instance,
// which is both efficient and consistent with the fact that this
// client is configured once and then reused across requests.
builder.Services.AddSingleton<IClient>(sp =>
{
    // Optional load-test switch: when enabled, the controller uses a local stub instead of
    // making real network calls.
    bool isLoad = bool.TryParse(builder.Configuration["LoadTest:IsActive"], out var active) && active;
    if (isLoad)
    {
        return new ClientStub();
    }

    string apiKey = builder.Configuration["OpenWeather:Key"];

    // Creating the HttpClient here keeps construction out of the controller.
    return new Client(apiKey, new HttpClient());
});

// INowWrapper is stateless, so Singleton is appropriate.
// Tests can inject a fake implementation to control the current time.
builder.Services.AddSingleton<INowWrapper, NowWrapper>();

// IRandomWrapper is Transient to avoid shared mutable Random state across the app.
// If tests need deterministic randomness, they can inject a predictable implementation.
builder.Services.AddTransient<IRandomWrapper, RandomWrapper>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

using Microsoft.AspNetCore.Mvc;
using AdamTibi.OpenWeather;
using Uqs.Weather.Wrappers;

namespace Uqs.Weather.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private const int FORECAST_DAYS = 5;
    // IClient is injected so the controller isn't responsible for creating network clients.
    // That separation keeps unit tests fast and local (tests can supply a stub instead of
    // hitting the real OpenWeather API).
    private readonly IClient _client;

    // INowWrapper abstracts DateTime.Now so time-dependent behavior can be tested deterministically.
    private readonly INowWrapper _nowWrapper;

    // IRandomWrapper abstracts Random so random output can be controlled in tests.
    private readonly IRandomWrapper _randomWrapper;

    // ILogger is injected so tests can avoid writing real logs (NullLogger) while still
    // allowing the controller to log in production.
    private readonly ILogger<WeatherForecastController> _logger;

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public WeatherForecastController(ILogger<WeatherForecastController> logger,
        IClient client, INowWrapper nowWrapper, IRandomWrapper randomWrapper)
    {
        // Instead of "new Client(...)" (or other direct instantiation) inside the controller,
        // dependencies are supplied by the DI container. That keeps this focused on
        // behavior and makes it easy to swap in fakes/stubs during unit testing.
        _logger = logger;
        _client = client;
        _nowWrapper = nowWrapper;
        _randomWrapper = randomWrapper;
    }

    [HttpGet("ConvertCToF")]
    public double ConvertCToF(double c)
    {
        double f = c * (9d / 5d) + 32;
        _logger.LogInformation("conversion requested");
        return f;
    }

    [HttpGet("GetRealWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> GetReal()
    {
        const decimal GREENWICH_LAT = 51.4810m;
        const decimal GREENWICH_LON = 0.0052m;
        OneCallResponse res = await _client.OneCallAsync
            (GREENWICH_LAT, GREENWICH_LON, new[] {
                Excludes.Current, Excludes.Minutely,
                Excludes.Hourly, Excludes.Alerts }, Units.Metric);

        WeatherForecast[] wfs = new WeatherForecast[FORECAST_DAYS];
        for (int i = 0; i < wfs.Length; i++)
        {
            var wf = wfs[i] = new WeatherForecast();
            wf.Date = res.Daily[i + 1].Dt;
            double forecastedTemp = res.Daily[i + 1].Temp.Day;
            wf.TemperatureC = (int)Math.Round(forecastedTemp);
            wf.Summary = MapFeelToTemp(wf.TemperatureC);
        }
        return wfs;
    }

    [HttpGet("GetRandomWeatherForecast")]
    public IEnumerable<WeatherForecast> GetRandom()
    {
        WeatherForecast[] wfs = new WeatherForecast[FORECAST_DAYS];
        for (int i = 0; i < wfs.Length; i++)
        {
            var wf = wfs[i] = new WeatherForecast();
            // Using INowWrapper keeps "now" controllable in unit tests (no dependency on real time).
            wf.Date = _nowWrapper.Now.AddDays(i + 1);

            // Using IRandomWrapper lets tests provide deterministic random values when needed.
            wf.TemperatureC = _randomWrapper.Next(-20, 55);
            wf.Summary = MapFeelToTemp(wf.TemperatureC);
        }
        return wfs;
    }

    private string MapFeelToTemp(int temperatureC)
    {
        if (temperatureC <= 0)
        {
            return Summaries.First();
        }
        int summariesIndex = (temperatureC / 5) + 1;
        if (summariesIndex >= Summaries.Length)
        {
            return Summaries.Last();
        }
        return Summaries[summariesIndex];
    }
}
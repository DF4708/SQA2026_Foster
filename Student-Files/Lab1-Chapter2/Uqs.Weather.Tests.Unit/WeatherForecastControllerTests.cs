using Microsoft.Extensions.Logging.Abstractions;
using Uqs.Weather;
using Uqs.Weather.Controllers;
using Uqs.Weather.Wrappers;
using AdamTibi.OpenWeather;
using Xunit;

namespace Uqs.Weather.Tests.Unit;

/*
SUT: WeatherForecastController
Behavior under test: ConvertCToF temperature conversion.
*/
public class WeatherForecastControllerTests
{
    private static WeatherForecastController CreateController()
    {
        // Using NullLogger so the test does not write actual log output.
        var logger = NullLogger<WeatherForecastController>.Instance;

        // ConvertCToF doesn't use these collaborators, but the controller constructor requires them.
        // Keeping them as simple fakes avoids pulling in mocking frameworks for the first lab.
        // Even though my unit tests focus on ConvertCToF (which doesn’t use time/random/network),
        // the controller still depends on these components for other behaviors, so I identified
        // them as relevant to overall controller testability. See written summary.
        IClient client = new ClientStub();
        INowWrapper nowWrapper = new FixedNowWrapper(new DateTime(2026, 2, 6, 12, 0, 0));
        IRandomWrapper randomWrapper = new FixedRandomWrapper(0);

        return new WeatherForecastController(logger, client, nowWrapper, randomWrapper);
    }

    [Fact]
    public void ConvertCToF_WhenCelsiusIsZero_Returns32F()
    {
        // Arrange
        var controller = CreateController();

        // Act
        double result = controller.ConvertCToF(0);

        // Assert
        Assert.Equal(32d, result, precision: 10);
    }

    [Fact]
    public void ConvertCToF_WhenCelsiusIsNegativeOne_Returns30Point2F()
    {
        // Arrange
        var controller = CreateController();

        // Act
        double result = controller.ConvertCToF(-1);

        // Assert
        Assert.Equal(30.2d, result, precision: 10);
    }

    [Theory]
    [InlineData(0d, 32d)]        // Celsius input, expected Fahrenheit output
    [InlineData(100d, 212d)]     // Celsius input, expected Fahrenheit output
    [InlineData(-40d, -40d)]     // Celsius input, expected Fahrenheit output
    [InlineData(37d, 98.6d)]     // Celsius input, expected Fahrenheit output
    [InlineData(10d, 50d)]       // Celsius input, expected Fahrenheit output
    public void ConvertCToF_MultipleInputs_ReturnExpectedFahrenheit(double celsius, double expectedFahrenheit)
    {
        // Arrange
        var controller = CreateController();

        // Act
        double result = controller.ConvertCToF(celsius);

        // Assert
        Assert.Equal(expectedFahrenheit, result, precision: 10);
    }

    private sealed class FixedNowWrapper : INowWrapper
    {
        private readonly DateTime _now;

        public FixedNowWrapper(DateTime now)
        {
            _now = now;
        }

        public DateTime Now => _now;
    }

    private sealed class FixedRandomWrapper : IRandomWrapper
    {
        private readonly int _value;

        public FixedRandomWrapper(int value)
        {
            _value = value;
        }

        public int Next(int minValue, int maxValue)
        {
            return _value;
        }
    }
}

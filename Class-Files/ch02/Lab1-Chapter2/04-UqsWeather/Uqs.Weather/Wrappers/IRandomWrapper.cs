namespace Uqs.Weather.Wrappers;

public interface IRandomWrapper
{
    // Wrapper around Random so callers can avoid non-deterministic behavior in unit tests.
    // Tests can inject a fake/random sequence instead of relying on true randomness.
    int Next(int minValue, int maxValue);
}
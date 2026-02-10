namespace Uqs.Weather.Wrappers;

public class RandomWrapper : IRandomWrapper
{
    // Random is intentionally wrapped so controllers/services don't need to touch Random.Shared
    // (or "new Random()") directly. That keeps randomness injectable and therefore testable.
    // This instance is created per wrapper instance; combined with a Transient lifetime, it avoids
    // shared mutable statez across requests.
    private readonly Random _random = new Random();

    public int Next(int minValue, int maxValue)
    {
        return _random.Next(minValue, maxValue);
    }
}

namespace Uqs.Weather.Wrappers;

public interface INowWrapper
{
    // Wrapper around DateTime.Now so callers don't have to depend on the real system clock.
    // This makes time-based behavior deterministic in unit tests.
    DateTime Now { get; }
}


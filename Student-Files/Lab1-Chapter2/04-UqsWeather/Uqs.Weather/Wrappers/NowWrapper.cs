namespace Uqs.Weather.Wrappers;

public class NowWrapper : INowWrapper
{
    // Centralizing DateTime.Now behind an interface keeps the controller easy to test.
    // In production this returns the real time; in tests a fake can return a fixed value.
         public DateTime Now => DateTime.Now;
}
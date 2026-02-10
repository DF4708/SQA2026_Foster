using AdamTibi.OpenWeather;

namespace Uqs.Weather;

/// An IClient implementation used for local/demo scenarios where you don't want
/// real network calls. This is helpful for manual testing without needing
/// an API key or internet access.
///
/// Note: This stub uses DateTime.Now directly because it is not part of the unit-test 
/// for this lab (the controller gets time via INowWrapper). Keeping this stub simple makes
/// it easier to reason about - when you're just trying to run the app locally.

public class ClientStub : IClient
{
    public Task<OneCallResponse> OneCallAsync(
        decimal latitude, decimal longitude, IEnumerable<Excludes> excludes, Units unit)
    {
        const int DAYS = 7;
        OneCallResponse res = new OneCallResponse();
        res.Daily = new Daily[DAYS];
        DateTime now = DateTime.Now;
        for (int i = 0; i < DAYS; i++)
        {
            res.Daily[i] = new Daily();
            res.Daily[i].Dt = now.AddDays(i);
            res.Daily[i].Temp = new Temp();
            res.Daily[i].Temp.Day = Random.Shared.Next(-20, 55);
        }
        return Task.FromResult(res);
    }
}
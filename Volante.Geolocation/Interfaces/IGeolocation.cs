using Volante.Geolocation.Entities;

namespace Volante.Geolocation.Interfaces
{
    public interface IGeolocation
    {
        GeolocationInfo LocateByIp(string ip);
    }
}

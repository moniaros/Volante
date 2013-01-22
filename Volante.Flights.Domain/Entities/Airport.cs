using Volante.Domain.Entities;

namespace Volante.Flights.Domain.Entities
{
    public class Airport : PointOfInterest
    {
        public int GmtZone { get; set; }
        public string IataCode { get; set; }
        public AirportRating Rating { get; set; }

    }
}

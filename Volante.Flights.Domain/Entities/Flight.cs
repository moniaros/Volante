using System.Collections.Generic;
using System.Linq;

namespace Volante.Flights.Domain.Entities
{
    public class Flight
    {
        public List<FlightLeg> Legs { get; set; }
    }

    public static class FlightExtensions
    {
        public static Airport From(this Flight flight)
        {
            return flight.Legs.First().From();
        }

        public static Airport To(this Flight flight)
        {
            return flight.Legs.Last().To();
        }
    }
}

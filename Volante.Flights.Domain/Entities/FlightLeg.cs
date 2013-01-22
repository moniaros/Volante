using System.Collections.Generic;
using System.Linq;
using Volante.Domain.Entities;

namespace Volante.Flights.Domain.Entities
{
    public class FlightLeg
    {
        public int Index { get; set; }
        public List<FlightSegment> Segments { get; set; }
        public Provider Provider { get; set; }
        public Airline Airline { get; set; }
    }

    public static class FlightLegExtensions
    {
        public static Airport From(this FlightLeg flightLeg)
        {
            return flightLeg.Segments.First().From;
        }

        public static Airport To(this FlightLeg flightLeg)
        {
            return flightLeg.Segments.Last().To;
        }
    }
}
using System;

namespace Volante.Flights.Domain.Entities
{
    public class FlightSegment
    {
        public Airport From { get; set; }
        public Airport To { get; set; }
        public int FligthTime { get; set; }
        public Aircraft Aircraft { get; set; }
        public Airline Airline { get; set; }
        public DateTime Departure { get; set; }
        public DateTime Arrival { get; set; }
        public ClassType Class { get; set; }
    }
}
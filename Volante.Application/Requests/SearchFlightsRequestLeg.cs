using System;
using System.Collections.Generic;
using Volante.Flights.Domain.Entities;

namespace Volante.Application.Requests
{
    public class SearchFlightsRequestLeg
    {
        public List<Airport> Departure { get; set; }
        public List<Airport> Arrival { get; set; }
        public TimeSpan? TimeRangeFrom { get; set; }
        public DateTime DateRangeFrom { get; set; }
        public TimeSpan? TimeRangeTo { get; set; }
        public DateTime DateRangeTo { get; set; }
    }
}
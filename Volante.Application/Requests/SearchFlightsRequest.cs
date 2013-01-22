using System.Collections.Generic;
using Volante.Flights.Domain.Entities;

namespace Volante.Application.Requests
{
    public class SearchFlightsRequest
    {
        public List<SearchFlightsRequestLeg> Legs { get; set; }
        public List<Airline> Airlines { get; set; }
        public List<ClassType> Classes { get; set; }
        public bool DirectOnly { get; set; }
        public bool IncludeNearbyAirports { get; set; }
    }
}

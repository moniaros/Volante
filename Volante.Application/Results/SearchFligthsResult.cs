using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Volante.Flights.Domain.Entities;

namespace Volante.Application.Results
{
    public class SearchFligthsResult
    {
        public FlightsStats Statistics { get; set; }
    }


    public class FlightsStats
    {
        public List<ChangesStats> Changes { get; set; }
        public Dictionary<FlightLeg, LegStats> FligthLegs { get; set; }
        public Dictionary<ClassType, decimal> Classes { get; set; }
        public Dictionary<Aircraft, decimal> Aircrafts { get; set; }
        public Dictionary<int, decimal> SystemRanks { get; set; }
        public decimal PriceFrom { get; set; }
        public decimal PriceTo { get; set; }
        public TimeSpan FlightTime { get; set; }
    }

    public class ChangesStats
    {
        public int NumberOfChanges { get; set; }
        public decimal PriceFrom { get; set; }
    }

    public class TimeStats
    {
        public DateTime TimeFrom { get; set; }
        public DateTime TimeTo { get; set; }
    }

    public class LegStats
    {
        public TimeStats Departure { get; set; }
        public TimeStats Arrival { get; set; }
    }
}

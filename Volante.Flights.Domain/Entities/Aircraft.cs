using System.Collections.Generic;

namespace Volante.Flights.Domain.Entities
{
    public class Aircraft
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string Producer { get; set; }

        public string Description { get; set; }

        public int NumberOfEngines { get; set; }

        public AircraftRating Rating { get; set; }

        public List<string> Pictures { get; set; }
    }
}

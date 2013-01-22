using System.Collections.Generic;
using Volante.Domain.Entities;

namespace Volante.Flights.Domain.Entities
{
    public class Airline
    {
        public string IataCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        public Country Origin { get; set; }
        public int NumberOfAircrafts { get; set; }
        public List<Aircraft> AircraftTypes { get; set; }
        public AirlineRating Rating { get; set; }
        public Dictionary<Airport,List<Airport>> Routes { get; set; }
        public ContactInfo ContactInfo { get; set; }
    }

    public class ContactInfo
    {
        public string Address { get; set; }
        public string Notes { get; set; }
        public string Phone { get; set; }
        public string WebSite { get; set; }
    }
}

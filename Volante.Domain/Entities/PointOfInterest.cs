using System.Collections.Generic;

namespace Volante.Domain.Entities
{
    public class PointOfInterest
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ComuteInfo> Comutation { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public List<string> Pictures { get; set; } 
    }
}

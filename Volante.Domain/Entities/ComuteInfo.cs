using System.Collections.Generic;

namespace Volante.Domain.Entities
{
    public class ComuteInfo
    {
        /// <summary>
        /// Miles
        /// </summary>
        public double Distance { get; set; }

        public PointOfInterest Target { get; set; }
        
        /// <summary>
        /// Minutes
        /// </summary>
        public int AverageTravelTime { get; set; }

        public List<ComuteLeg> Routes { get; set; } 

    }
}
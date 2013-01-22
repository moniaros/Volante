using System.Collections.Generic;

namespace Volante.Domain.Entities
{
    public class ComuteLeg
    {
        public List<ComuteType> Segments { get; set; }
        public int AverageTravelTime { get; set; }
        public string Description { get; set; }
    }
}

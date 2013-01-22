using System.Collections.Generic;

namespace Volante.Domain.Entities
{
    public abstract class Rating
    {
        public List<string> Advantages { get; set; }
        public List<string> Disadvantages { get; set; }
        public abstract int OverallScore { get; }
    }
}

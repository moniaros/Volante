using Volante.Domain.Entities;

namespace Volante.Flights.Domain.Entities
{
    public class AircraftRating : Rating
    {
        public override int OverallScore
        {
            get { return (Comfort + Modernity + Security + Speed) / 4; }
        }

        public int Comfort { get; set; }

        public int Modernity { get; set; }

        public int Security { get; set; }

        public int Speed { get; set; }
    }
}

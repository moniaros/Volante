using Volante.Domain.Entities;

namespace Volante.Flights.Domain.Entities
{
    public class AirportRating : Rating
    {
        public override int OverallScore
        {
            get { return (Communication + CustomerService + Modernity)/3; }
        }

        public int Communication { get; set; }

        public int CustomerService { get; set; }

        public int Modernity { get; set; }
    }
}

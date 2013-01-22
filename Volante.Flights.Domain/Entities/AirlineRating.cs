using Volante.Domain.Entities;

namespace Volante.Flights.Domain.Entities
{
    public class AirlineRating : Rating
    {
        public override int  OverallScore
        {
            get { return (CustomerSupport + OnBoardService + Punctuality + TermsAndConditions + AverageAircraftRating.OverallScore) / 5; }
        }

        public int CustomerSupport { get; set; }

        public int OnBoardService { get; set; }

        public int Punctuality { get; set; }

        public int TermsAndConditions { get; set; }

        public AircraftRating AverageAircraftRating { get; set; }
    }
}

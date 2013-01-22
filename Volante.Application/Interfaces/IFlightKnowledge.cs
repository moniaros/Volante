using System.Collections.Generic;
using Volante.Flights.Domain.Entities;

namespace Volante.Application.Interfaces
{
    public interface IFlightKnowledge
    {
        List<Airport> MatchAirports(string searchString, int range, bool includeNearbyAirports);
    }
}

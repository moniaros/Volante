using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Volante.Application.Interfaces;
using Volante.Application.Requests;
using Volante.Flights.Domain.Entities;

namespace Volante.Application.Test
{
    [TestFixture]
    public class IFlightsSearchTest
    {
        private MockRepository _mockRepository;
        private IFlightsSearch _flightsSearch;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository();
            _flightsSearch = _mockRepository.StrictMock<IFlightsSearch>();
        }

        [Test]
        public void SearchFlightsTest()
        {
            //Arrange
            var departure = new Airport {Code = "KTW"};
            var arrival = new Airport {Code = "LON"};
            var dateFrom = new DateTime(2013, 4, 10);
            var dateTo = dateFrom.AddDays(7.0);
            var legs = new[] 
                {   new SearchFlightsRequestLeg
                        {
                            Departure = new List<Airport> {departure},
                            Arrival = new List<Airport> {arrival},
                            DateRangeFrom = dateFrom,
                            DateRangeTo = dateFrom
                        },
                    new SearchFlightsRequestLeg
                        {
                            Departure = new List<Airport> {arrival},
                            Arrival = new List<Airport> {departure},
                            DateRangeFrom = dateTo,
                            DateRangeTo = dateTo
                        }
                }.ToList();

            //Act
            var searchToken = _flightsSearch.SearchFlightsAsync(legs);
            Thread.Sleep(2000);
            _flightsSearch.SearchFlightsResult(searchToken);

            //Assert
            Assert.Greater(searchToken,0);
        }

        public void SearchFlightsTestCompleted()
        {
               
        }
    }
}

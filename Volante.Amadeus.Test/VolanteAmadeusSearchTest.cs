using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Volante.Amadeus.Interfaces;


namespace Volante.Amadeus.Test
{
    [TestFixture]
    public class VolanteAmadeusSearchTest
    {
        [SetUp]
        public void SetUp()
        {
            
        }

        [Test]
        public void SearchFlights_IntegrationTest()
        {
            //Arrange
            IAmadeus amadeus = new AmadeusAdapter();
            
            //Act
            amadeus.SearchFlights();

            //Assert
        }
    }
}

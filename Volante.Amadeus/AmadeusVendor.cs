using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Volante.Amadeus.Interfaces;
using Volante.Flights.Domain.Entities;

namespace Volante.Amadeus
{
    public class AmadeusAdapter : IAmadeus
    {
        AmadeusWebServicesPTClient client = null;
        Session session = null;
        //agency data
        private string _username = "WSESKESK";
        private string _agencyCode = "WAWPQ28SW";
        private string _organization = "NMC-POLAND";
        private string _pwd = "NEcyWmV5cjM=";
        private int _dataLength = 8;

        public void SearchFlights()
        {
            try
            {

            

                var maxResultsNumber = 30;
                var travelBoardSearch = new Fare_MasterPricerTravelBoardSearch();
                var classType = ClassType.Economic;
                //ilość paxów
                var adultCount = 1;
                var childCount = 0;
                var infantCount = 0;
                var youthCount = 0;

                session = CreateSession(_username, _agencyCode, _organization, _pwd, _dataLength, false);

                travelBoardSearch.numberOfUnit = new NumberOfUnitDetailsType[2];
                travelBoardSearch.numberOfUnit[0] = new NumberOfUnitDetailsType
                {
                    numberOfUnits = (adultCount + childCount + youthCount).ToString(CultureInfo.InvariantCulture),
                    typeOfUnit = "PX"
                };

                //liczba zwracanych itemów
                travelBoardSearch.numberOfUnit[1] = new NumberOfUnitDetailsType()
                {
                    numberOfUnits = maxResultsNumber.ToString(CultureInfo.InvariantCulture),
                    typeOfUnit = "RC"
                };

                //definicja typów pasażerów
                var adultDefinition = AddPaxReference(null, adultCount, "ADT", 0);
                var youthDefinition = AddPaxReference(null, youthCount, "YTH", adultCount);
                var childDefinition = AddPaxReference(null, childCount, "CH", adultCount + youthCount);
                var infantDefinition = AddPaxReference(null, infantCount, "INF", 0);


                var paxReferenceCount = (adultDefinition != null ? 1 : 0)
                                        + (childDefinition != null ? 1 : 0)
                                        + (infantDefinition != null ? 1 : 0)
                                        + (youthDefinition != null ? 1 : 0);

                travelBoardSearch.paxReference = new TravellerReferenceInformationType[paxReferenceCount];

                paxReferenceCount = 0;
                if (adultDefinition != null) travelBoardSearch.paxReference[paxReferenceCount++] = adultDefinition;
                if (youthDefinition != null) travelBoardSearch.paxReference[paxReferenceCount++] = youthDefinition;
                if (childDefinition != null) travelBoardSearch.paxReference[paxReferenceCount++] = childDefinition;
                if (infantDefinition != null) travelBoardSearch.paxReference[paxReferenceCount] = infantDefinition;

                //definicja ServiceClass
                travelBoardSearch.travelFlightInfo = new TravelFlightInformationType();
                travelBoardSearch.travelFlightInfo.cabinId = new CabinIdentificationType { cabin = GetAmadeusServiceClass(classType) };

                //definicja segmentów lotu
                var departureAirportCode = "KTW";
                var arrivalAirportCode = "LON";
                var departureDate = new DateTime(2013, 3, 10);
                travelBoardSearch.itinerary = new[]
                {
                    BuildItinerary(1, departureAirportCode, arrivalAirportCode,departureDate)
                };

                //fare types
                travelBoardSearch.fareOptions = new Fare_MasterPricerTravelBoardSearchFareOptions
                {
                    pricingTickInfo = new PricingTicketingDetailsType{pricingTicketing = new string[4]}
                };

                travelBoardSearch.fareOptions.pricingTickInfo.pricingTicketing[0] = "TAC"; //ticketability - powoduje zwrócenie tylko lotów, dla których można utworzyć bilet (zalecane)
                travelBoardSearch.fareOptions.pricingTickInfo.pricingTicketing[1] = "RU"; // unifares
                travelBoardSearch.fareOptions.pricingTickInfo.pricingTicketing[2] = "RP"; // published
                travelBoardSearch.fareOptions.pricingTickInfo.pricingTicketing[3] = "ET"; // indicate to request Electronic Tickets recommendations only. Not the paper ones.

                //credit card fee
                travelBoardSearch.feeOption = new Fare_MasterPricerTravelBoardSearchFeeOption[1];
                travelBoardSearch.feeOption[0] = new Fare_MasterPricerTravelBoardSearchFeeOption();
                travelBoardSearch.feeOption[0].feeTypeInfo = new SelectionDetailsType();
                travelBoardSearch.feeOption[0].feeTypeInfo.carrierFeeDetails = new SelectionDetailsInformationType { type = "OB" };
                travelBoardSearch.feeOption[0].feeDetails = new Fare_MasterPricerTravelBoardSearchFeeOptionFeeDetails[1];
                travelBoardSearch.feeOption[0].feeDetails[0] = new Fare_MasterPricerTravelBoardSearchFeeOptionFeeDetails();
                travelBoardSearch.feeOption[0].feeDetails[0].feeInfo = new SpecificDataInformationType();
                travelBoardSearch.feeOption[0].feeDetails[0].feeInfo.dataTypeInformation = new DataTypeInformationType();
                travelBoardSearch.feeOption[0].feeDetails[0].feeInfo.dataTypeInformation.subType = "FCA";
                travelBoardSearch.feeOption[0].feeDetails[0].feeInfo.dataTypeInformation.option = "IN";
        
                IncrementSessionSequenceNumber();

                Fare_MasterPricerTravelBoardSearchReply response = null;

                try
                {
                    response = client.Fare_MasterPricerTravelBoardSearch(ref session, travelBoardSearch);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("12|Presentation|soap message header incorrect"))
                    {
                        CloseSession();
                        CreateSession(_username, _agencyCode, _organization, _pwd, _dataLength, false);
                        response = client.Fare_MasterPricerTravelBoardSearch(ref session, travelBoardSearch);
                    }
                    throw;
                }

            }
            finally
            {
                CloseSession();
            }
        }

        public Session CreateSession(string username, string agency, string organization, string password, int dataLength, bool forceNewSession)
        {
            //AuthenticateRequest
            client = new AmadeusWebServicesPTClient();
            var sec_auth = new Security_Authenticate();

            sec_auth.dutyCode = new ReferenceInformationTypeI();
            sec_auth.dutyCode.dutyCodeDetails = new ReferencingDetailsTypeI2();
            sec_auth.dutyCode.dutyCodeDetails.referenceIdentifier = "SU";
            sec_auth.dutyCode.dutyCodeDetails.referenceQualifier = "DUT";

            sec_auth.passwordInfo = new BinaryDataType[1];
            sec_auth.passwordInfo[0] = new BinaryDataType();
            sec_auth.passwordInfo[0].dataLength = dataLength.ToString();
            sec_auth.passwordInfo[0].dataType = "E";
            sec_auth.passwordInfo[0].binaryData = password;

            sec_auth.systemDetails = new SystemDetailsInfoType();
            sec_auth.systemDetails.organizationDetails = new SystemDetailsTypeI1();
            sec_auth.systemDetails.organizationDetails.organizationId = organization;

            sec_auth.userIdentifier = new UserIdentificationType2[1];
            sec_auth.userIdentifier[0] = new UserIdentificationType2();
            sec_auth.userIdentifier[0].originator = username;
            sec_auth.userIdentifier[0].originatorTypeCode = "U";
            sec_auth.userIdentifier[0].originIdentification = new OriginatorIdentificationDetailsTypeI3();
            sec_auth.userIdentifier[0].originIdentification.sourceOffice = agency;

            session = new Session { SessionId = string.Empty, SecurityToken = string.Empty, SequenceNumber = "1" };
            var sec_auth_reply = client.Security_Authenticate(ref session, sec_auth);

            if (sec_auth_reply.errorSection != null)
            {
                var sb = new StringBuilder();
                foreach (var s in sec_auth_reply.errorSection.interactiveFreeText.freeText)
                    sb.Append(s);
                throw new Exception(sb.ToString());
            }

            return session;
        }

        public void CloseSession()
        {
            IncrementSessionSequenceNumber();
            try
            {
                var sec_signout = new Security_SignOut();
                client.Security_SignOut(ref session, sec_signout);
            }
            catch (Exception exception)
            {
                
            }
            //AmadeusSessionPool.DeleteSession(session);
        }

        private void IncrementSessionSequenceNumber(Session s)
        {
            int sequenceNumber = int.Parse(s.SequenceNumber);
            sequenceNumber++;
            s.SequenceNumber = sequenceNumber.ToString();
        }

        private void IncrementSessionSequenceNumber()
        {
            IncrementSessionSequenceNumber(this.session);
        }

        private Fare_MasterPricerTravelBoardSearchItinerary BuildItinerary(int segmentIdx, string departureCode, string arrivalCode, DateTime departureDate)
        {
            return new Fare_MasterPricerTravelBoardSearchItinerary
            {
                requestedSegmentRef = new OriginAndDestinationRequestType{segRef = "1"},
                departureLocalization = new DepartureLocationType
                {
                    departurePoint = new ArrivalLocationDetailsType_120834C{locationId = departureCode, airportCityQualifier = "C"}
                },
                arrivalLocalization = new ArrivalLocalizationType
                {
                    arrivalPointDetails = new ArrivalLocationDetailsType{locationId = arrivalCode,airportCityQualifier = "C"}
                },
                timeDetails = new DateAndTimeInformationType
                {
                    firstDateTimeDetail = new DateAndTimeDetailsTypeI{ date = departureDate.ToString("ddMMyy")}
                }
            };
        }

        private TravellerReferenceInformationType AddPaxReference(TravellerReferenceInformationType paxDefinition, int paxCount, string personType, int referenceStartNumber)
        {
            if (paxCount < 1) return paxDefinition;

            if (paxDefinition == null)
            {
                paxDefinition = new TravellerReferenceInformationType
                {
                    ptc = new string[] {personType}, 
                    traveller = new TravellerDetailsType[paxCount]
                };
            }

            for (var i = 0; i < paxCount; i++)
            {
                var traveller = new TravellerDetailsType();

                paxDefinition.traveller[i] = traveller;

                if (personType == "INF")
                {
                    traveller.infantIndicator = "1";
                    traveller.@ref = (i + 1).ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    traveller.@ref = ((i + 1) + referenceStartNumber).ToString(CultureInfo.InvariantCulture); //globalny numer referencyjny paxa
                }
            }
            return paxDefinition;
        }

        private string[] GetAmadeusServiceClass(ClassType? serviceClass)
        {
            switch (serviceClass)
            {
                case ClassType.Business:    return new[] { "C" };
                case ClassType.First:       return new[] { "F" };
                case ClassType.EconomicPremium:
                case ClassType.Economic:    
                default:                    return new[] { "M", "W", "Y" };
            }
        }

        //private string GetPaxType(PersonTypeEnum person, bool useShortChildCode)
        //{
        //    switch (person)
        //    {
        //        case PersonTypeEnum.Senior:
        //        case PersonTypeEnum.Adult: return "ADT";
        //        case PersonTypeEnum.Youth: return "YTH";
        //        case PersonTypeEnum.Child: return useShortChildCode ? "CH" : "CHD";
        //        case PersonTypeEnum.Infant: return "INF";
        //        default: return "ADT";
        //    }
        //}
    }
}

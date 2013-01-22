using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace Volante.Worldspan
{
    public class WorldSpanVendor
    {
        private string url;
        private string session;
        protected string pcc;
        private string uid;
        private string pwd;
        protected string _5ca = "";
        protected string iataNumber;
        protected string companyPhone;
        protected string destionationCid;
        protected string destinationQueue;
        protected string destinationQueueCategory;
        protected string destinationQueueOrder;
        protected string destinationQueueResign;
        protected string destinationQueueCancel;
        protected string destinationQueueCancelTicket;
        protected bool isDebugMode = false;


        public WorldSpanVendor()
        {
            const string connectionString = "http://xmlpropp.worldspan.com:8800;WXMLDEV;8Y1;esky;esky0000;6320427;0 801 420 303 ESKY;09O;201;1";
            string[] cs = connectionString.Split(';');
            if ((cs.Length != 10) && (cs.Length != 11)) return;
            this.url = cs[0];
            this.session = cs[1];
            this.pcc = cs[2];
            this.uid = cs[3];
            this.pwd = cs[4];
            this.iataNumber = cs[5];
            this.companyPhone = cs[6];
            this.destionationCid = cs[7];
            this.destinationQueue = cs[8];
            this.destinationQueueCategory = cs[9];
        }


        protected string ParseToWorldSpanDate(DateTime input)
        {

            string month = "";

            switch (input.Month.ToString().PadLeft(2, '0'))
            {
                case "01": month = "JAN"; break;
                case "02": month = "FEB"; break;
                case "03": month = "MAR"; break;
                case "04": month = "APR"; break;
                case "05": month = "MAY"; break;
                case "06": month = "JUN"; break;
                case "07": month = "JUL"; break;
                case "08": month = "AUG"; break;
                case "09": month = "SEP"; break;
                case "10": month = "OCT"; break;
                case "11": month = "NOV"; break;
                case "12": month = "DEC"; break;
            }

            return input.Day.ToString().PadLeft(2, '0') + month;
        }
        sealed public class AcceptAllCertificatePolicy : ICertificatePolicy
        {
            public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
            {
                return true;
            }
        }
        public string SendHttpRequest(string url, string requestXml, string contentType, string method, bool acceptGzip, string userName, string userPassword, int timeout)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            byte[] content;
            Stream stream = null;
            string responseXml = string.Empty;
            StreamReader reader = null;

            int attemptToWrite = 0;
            int attemptToRead = 0;
            const int MAXIMUM_ALLOWED_ATTEMPT_TO_HTTP_ACTION = 2;
            bool errors = true;

            while (errors)
            {
                errors = false;

                #region Wysłanie żądania do serwera

                try
                {
                    ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();
                    content = Encoding.UTF8.GetBytes(requestXml);
                    request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = method;
                    request.Accept = "*/*";
                    //request.CookieContainer = new CookieContainer();
                    request.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; EmbeddedWB 14.52 from: http://www.bsalsa.com/ EmbeddedWB 14,52; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0)";
                    if (timeout == 0) request.Timeout = 1000000;
                    else request.Timeout = timeout;

                    if (acceptGzip)
                    {
                        request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                    }

                    if (userName != null && userPassword != null)
                    {
                        string authInfo = userName + ":" + userPassword;
                        authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                        request.Headers["Authorization"] = "Basic " + authInfo;
                        //request.Credentials = new NetworkCredential(userName, userPassword ); 
                    }

                    if (method != "GET")
                    {
                        request.ContentType = contentType;
                        request.ContentLength = content.Length;
                        stream = request.GetRequestStream();
                        stream.Write(content, 0, content.Length);
                        stream.Flush();
                    }
                }
                catch (System.Exception)
                {
                    errors = true;
                    attemptToWrite++;
                    if (attemptToWrite > MAXIMUM_ALLOWED_ATTEMPT_TO_HTTP_ACTION)
                    {
                        throw;
                    }
                    continue;
                }
                finally
                {
                    if (stream != null) stream.Close();
                    stream = null;
                }

                #endregion

                #region Odebranie odpowiedzi z serwera

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    stream = response.GetResponseStream();
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                        stream = new GZipStream(stream, CompressionMode.Decompress);
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                        stream = new DeflateStream(stream, CompressionMode.Decompress);
                    reader = new StreamReader(stream, Encoding.UTF8);
                    responseXml = reader.ReadToEnd();
                }
                catch (System.Exception ex)
                {
                    errors = true;
                    attemptToRead++;
                    if (attemptToRead > MAXIMUM_ALLOWED_ATTEMPT_TO_HTTP_ACTION)
                    {
                        throw;
                    }
                }
                finally
                {
                    if (reader != null) reader.Close();
                    reader = null;
                    if (stream != null) stream.Close();
                    stream = null;
                    if (response != null) response.Close();
                    response = null;
                }

                #endregion
            }

            return responseXml;
        }

        protected void SendRequest(string requestXml, XmlDocument xmlDoc)
        {

            string xml;
            XmlNamespaceManager nsm = null;

            #region Przygotowanie XML z treścią żądania

            xml = "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\">";
            xml += "<SOAP-ENV:Header>";
            xml += "<t:Transaction xmlns:t=\"xxs\">";
            xml += "<tc>";
            xml += "<iden u=\"" + this.uid + "\" p=\"" + this.pwd + "\" />";
            xml += "<provider session=\"" + this.session + "\" pcc=\"" + this.pcc + "\">Worldspan</provider>";
            xml += "<trace></trace>";
            xml += "</tc>";
            xml += "</t:Transaction>";
            xml += "</SOAP-ENV:Header>";
            xml += "<SOAP-ENV:Body>";
            xml += "<ns1:ProviderTransaction xmlns:ns1=\"xxs\">";
            xml += "<REQ>";
            xml += requestXml;
            xml += "</REQ>";
            xml += "</ns1:ProviderTransaction>";
            xml += "</SOAP-ENV:Body>";
            xml += "</SOAP-ENV:Envelope>";

            #endregion

            #region Wysłanie żądania do serwera XMLPro Server

            string rxml = SendHttpRequest(url, xml, "text/xml", "POST", false, null, null, 1000000);

            #endregion

            #region Przetworzenie odpowiedzi serwera XMLPro Server

            xmlDoc.LoadXml(rxml);
            nsm = new XmlNamespaceManager(xmlDoc.NameTable);
            nsm.AddNamespace("SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/");
            nsm.AddNamespace("ns1", "xxs");

            // Sprawdzenie czy serwer XMLPro Server nie zwrócił błędu 
            if (xmlDoc.DocumentElement.SelectSingleNode("SOAP-ENV:Body/ns1:ProviderTransactionResponse/RSP/XXW/ERROR", nsm) != null)
            {
                XmlNode errorNode = xmlDoc.DocumentElement.SelectSingleNode("SOAP-ENV:Body/ns1:ProviderTransactionResponse/RSP/XXW/ERROR", nsm);
                //WorldSpanException ex = new WorldSpanException(errorNode.SelectSingleNode("FACILITY", nsm).InnerText, errorNode.SelectSingleNode("CATEGORY", nsm).InnerText, errorNode.SelectSingleNode("CODE", nsm).InnerText, errorNode.SelectSingleNode("TYPE", nsm).InnerText, errorNode.SelectSingleNode("TEXT", nsm).InnerText);
                //throw ex;
            }

            #endregion

            xmlDoc.LoadXml(xmlDoc.DocumentElement.SelectSingleNode("SOAP-ENV:Body/ns1:ProviderTransactionResponse/RSP", nsm).InnerXml);

        }

        //public override SearchFlightsResult SearchFlights(SearchFlightsCommand command)
        //{
        //    var result = new SearchFlightsResult();
        //    var xmlDoc = new XmlDocument();

        //    try
        //    {

        //        #region Przygotowanie XML żądania dla komunikatu PSC5

        //        string requestXml = "<PSC5>";

        //        requestXml += "<PRICE_DISPLAY>B</PRICE_DISPLAY>";

        //        requestXml += "<OPT>O</OPT><OPT>16</OPT>";

        //        if ("PL" == "PL")
        //        {
        //            requestXml += "<OPT>H</OPT>";
        //        }

        //        //if (parameters.AdditionalCheckForCorporateFlights)
        //        //{
        //        //    // Account Code Only - only fares with this account code will be returned
        //        //    requestXml += "<OPT>2</OPT>";
        //        //}
        //        requestXml += "<OPT>U</OPT>"; // Taryfy publiczne i prywatne

        //        #region Informacja o wymaganiu lotów bezpośrednich

        //        //if (searchParameters.DirectFlights.Value || parameters.AdditionalCheckForDirectFlights) requestXml += "<OPT>I</OPT>";

        //        #endregion

        //        #region Informacja o miejscu wylotu

        //        requestXml += "<POI_ORI>";
        //        requestXml += "<CIT>" + command.Departure + "</CIT>";
        //        requestXml += "</POI_ORI>";

        //        #endregion

        //        #region Informacja o kodzie waluty w jakiej mają być podawane ceny

        //        requestXml += "<ISO_CUR_COD>" + "PLN" + "</ISO_CUR_COD>";
        //        requestXml += "<ISO_CTY_SAL>" + "PL" + "</ISO_CTY_SAL>";

        //        #endregion

        //        #region Informacje o pasażerach


        //        //bool adults = searchParameters.GetPersonCountByType(PersonTypeEnum.Adult) > 0
        //        //                || searchParameters.GetPersonCountByType(PersonTypeEnum.Youth) > 0
        //        //                || searchParameters.GetPersonCountByType(PersonTypeEnum.Senior) > 0;

        //        //foreach (int age in command.PassengerAges)
        //        //{
        //        //int personTypeCount = searchParameters.GetPersonCountByType(personType);
        //        //if (personTypeCount > 0)
        //        //{
        //        requestXml += "<PTC_INF>";
        //        requestXml += "<NUM_PAX>" + "2" + "</NUM_PAX>";
        //        requestXml += "<PTC>" + "ADT" + "</PTC>";
        //        requestXml += "</PTC_INF>";
        //        //}
        //        //}

        //        #endregion

        //        #region Informacje o odcinkach lotu

        //        //for (int x = 0; x < searchParameters.Legs.Count; x++)
        //        //{
        //        //    SearchFlightsParametersLeg leg = searchParameters.Legs[x];

        //        //if (x != 0)
        //        //{
        //        //    if (leg.DepartureAirportCode != searchParameters.Legs[x - 1].ArrivalAirportCode)
        //        //    {
        //        //requestXml += "<DES_INF>";
        //        //requestXml += "<POI_DES>";
        //        //requestXml += "<CIT>" + command.Departure + "</CIT>";
        //        //requestXml += "</POI_DES>";
        //        //requestXml += "<STO_TYP>2</STO_TYP>";
        //        //requestXml += "</DES_INF>";
        //        //    }
        //        //}

        //        //if (DateTime.Now.AddYears(1) < leg.DepartureDate) return;

        //        requestXml += "<DES_INF>";
        //        requestXml += "<DEP_DAT>" + ParseToWorldSpanDate(command.DepartureDate) + "</DEP_DAT>";
        //        //if (searchParameters.ServiceClass.Value != ServiceClassEnum.Any) requestXml += "<CAB_CLA>" + ParseToWorldSpanServiceClass(searchParameters.ServiceClass.Value) + "</CAB_CLA>";
        //        requestXml += "<POI_DES>";
        //        requestXml += "<CIT>" + command.Arrival + "</CIT>";
        //        requestXml += "</POI_DES>";
        //        requestXml += "</DES_INF>";
        //        //}

        //        #endregion

        //        #region Liczba zwracanych połączeń
        //        requestXml += "<NUM_ALT>100</NUM_ALT>";
        //        #endregion

        //        #region Informacje o kodach linii lotniczych wybranych do realizacji połaczeń

        //        //if (searchParameters.Airlines.Count > 0)
        //        //{
        //        //    requestXml += "<ARL_INF>";
        //        //    requestXml += "<ARL_OPT>B</ARL_OPT>";
        //        //    foreach (string airline in searchParameters.Airlines)
        //        //    {
        //        //        requestXml += "<ARL_COD>" + airline + "</ARL_COD>";
        //        //    }
        //        //    requestXml += "</ARL_INF>";

        //        //}
        //        //else if (context.PartnerCode == "EDESTINOS")
        //        //{
        //        //    requestXml += "<ARL_INF><EXC_ARL><ARL_COD>G3</ARL_COD></EXC_ARL></ARL_INF>";
        //        //}

        //        requestXml += "<SRC_INF>";

        //        foreach (string accCode in new List<string>())
        //        {
        //            requestXml += "<ACC_COD>";
        //            requestXml += accCode;
        //            requestXml += "</ACC_COD>";
        //        }

        //        requestXml += "</SRC_INF>";
        //        requestXml += "</PSC5>";

        //        #endregion

        //        #endregion

        //        #region  Wysłanie żądania do serwera XMLPro Server

        //        //long startTime = PerformanceCountersManager.GetDateTicks();

        //        SendRequest(requestXml, xmlDoc);

        //        #endregion

        //        //#region Przetworzenie wyników obsługi żądania

        //        foreach (XmlNode item in xmlDoc.DocumentElement.SelectNodes("ALT_INF"))
        //        {

        //            var flight = new Flight();

        //            #region Przetwarzanie listy segmentów

        //            int segmentNo = 0;
        //            int legNo = 0;
        //            //Leg leg = new Leg(flight);

        //            //if (item.SelectNodes("ACC_COD").Count > 0)
        //            //{
        //            //    flight.CorporateCode = item.SelectNodes("ACC_COD")[0].InnerText;
        //            //}

        //            //foreach (XmlNode info in item.SelectNodes("FLI_INF"))
        //            //{

        //            //    #region Wyszuknie odcinka lotu do którego należu segment

        //            //    if ((searchParameters.Legs.Count > 1) && (legNo < searchParameters.Legs.Count - 1))
        //            //    {
        //            //        bool isNextLeg = false;
        //            //        SearchFlightsParametersLeg nextLeg = searchParameters.Legs[legNo + 1];

        //            //        if (info.SelectSingleNode("DEP_ARP").InnerText.ToUpper() == nextLeg.DepartureAirportCode.ToUpper())
        //            //        {
        //            //            isNextLeg = true;
        //            //        }
        //            //        else
        //            //        {
        //            //            if (ConvertUtility.ParseNullToEmptyString(DictionaryMapper.Instance.GetCityCodeByAirportCode(info.SelectSingleNode("DEP_ARP").InnerText)).ToUpper() == nextLeg.DepartureAirportCode.ToUpper())
        //            //            {
        //            //                isNextLeg = true;
        //            //            }
        //            //        }

        //            //        if (isNextLeg)
        //            //        {
        //            //            leg = new Leg(flight);
        //            //            legNo++;
        //            //        }


        //            //    }

        //            //    #endregion

        //            //    #region Utworzenie segmentu



        //            //    #region DepartureDate

        //            //    DateTime departureDate = ParseFromWorldSpanDate(info.SelectSingleNode("FLI_DAT").InnerText);
        //            //    departureDate = departureDate.Add(TimeSpan.Parse(ParseFromWorldSpanTime(info.SelectSingleNode("DEP_TIM").InnerText)));

        //            //    #endregion

        //            //    #region ArrivalDate

        //            //    DateTime arrivalDate = ParseFromWorldSpanDate(info.SelectSingleNode("FLI_DAT").InnerText);
        //            //    arrivalDate = arrivalDate.Add(TimeSpan.Parse(ParseFromWorldSpanTime(info.SelectSingleNode("ARR_TIM").InnerText)));
        //            //    if (info.SelectSingleNode("DAY_CHG_IND") != null && info.SelectSingleNode("DEP_ARR_DAT_DIF") != null)
        //            //    {
        //            //        arrivalDate = arrivalDate.AddDays(Convert.ToInt32(info.SelectSingleNode("DEP_ARR_DAT_DIF").InnerText));
        //            //    }

        //            //    #endregion

        //            //    Segment segment = new Segment(
        //            //        leg,
        //            //        info.SelectSingleNode("ARL_COD").InnerText,
        //            //        info.SelectSingleNode("FLI_NUM").InnerText,
        //            //        departureDate,
        //            //        info.SelectSingleNode("DEP_ARP").InnerText,
        //            //        arrivalDate,
        //            //        info.SelectSingleNode("ARR_ARP").InnerText,
        //            //        info.SelectSingleNode("FAR_CLA").InnerText);


        //            //    segment.AircraftCode = info.SelectSingleNode("EQP_TYP").InnerText; ;
        //            //    segment.ServiceClass = (segment.BookingClass != "Z" && (searchParameters.ServiceClass == ServiceClassEnum.Any || segment.BookingClass != "D")) ? ParseFromBookingClassToServiceClass(segment.BookingClass) : searchParameters.ServiceClass.Value;

        //            //    #region FlightTime

        //            //    TimeSpan flightTime = TimeSpan.Zero;

        //            //    XmlNodeList addFliSvcs = info.SelectNodes("ADD_FLI_SVC");
        //            //    foreach (XmlNode addFliSvc in addFliSvcs)
        //            //    {
        //            //        if (addFliSvc.SelectSingleNode("ELA_FLI_TIM") != null)
        //            //        {
        //            //            flightTime = flightTime.Add(ParseFromWorldSpanFlightTime(addFliSvc.SelectSingleNode("ELA_FLI_TIM").InnerText));
        //            //        }

        //            //        if (addFliSvc.SelectSingleNode("ARR_ARP") != null)
        //            //        {
        //            //            Esky.IBE.Common.Models.Flights.StopoverInfo stopover = new Esky.IBE.Common.Models.Flights.StopoverInfo();

        //            //            stopover.AirportCode = addFliSvc.SelectSingleNode("ARR_ARP").InnerText;

        //            //            segment.Stopovers.Add(stopover);
        //            //        }
        //            //    }

        //            //    if (info.SelectSingleNode("NUM_STO") != null)
        //            //    {
        //            //        segment.NumberOfStops = int.Parse(info.SelectSingleNode("NUM_STO").InnerText);
        //            //    }
        //            //    segment.FlightTime = flightTime;

        //            //    #endregion


        //            //    #region Obsługa Rezerwacji Opcjonalnej
        //            //    //if (!flight.OptionalReservation)
        //            //    //    flight.OptionalReservation = itemPropertiesManager.ItemPropertyPresent(segment.AirlineCode, ItemPropertiesEnum.OptionalReservation);
        //            //    #endregion

        //            //    #endregion

        //            //    #region Dodanie informacji o taryfach

        //            //    segmentNo++;

        //            //    List<PersonTypeEnum> passengerTypesServed = new List<PersonTypeEnum>();
        //            //    foreach (Person passenger in searchParameters.Persons)
        //            //    {
        //            //        if (passengerTypesServed.Contains(passenger.Code)) continue;
        //            //        else passengerTypesServed.Add(passenger.Code);

        //            //        XmlNode tripInfo = null;
        //            //        XmlNodeList tripInfos = null;


        //            //        #region Informacje o podróży dla typu pasażera i linii lotniczej

        //            //        tripInfos = item.SelectNodes("TRI_INF[PTC_DTL_INF/PTC='" + ParseToWorldSpanPassengerType(adults, passenger.Code) + "' and PTC_DTL_INF/ARL_COD='" + segment.AirlineCode + "']");
        //            //        if (tripInfos.Count == 1)
        //            //        {
        //            //            tripInfo = tripInfos[0];
        //            //        }
        //            //        else
        //            //        {
        //            //            for (int x = 0; x < tripInfos.Count; x++)
        //            //            {
        //            //                XmlNodeList airports = tripInfos[x].SelectNodes("ARP_COD");
        //            //                for (int y = 0; y < airports.Count - 1; y++)
        //            //                {
        //            //                    if (airports[y].InnerText == segment.DepartureCityCode && airports[y + 1].InnerText == segment.ArrivalCityCode)
        //            //                    {
        //            //                        tripInfo = tripInfos[x];
        //            //                        break;
        //            //                    }
        //            //                }
        //            //                if (tripInfo != null) break;
        //            //            }
        //            //        }
        //            //        #endregion

        //            //        #region Informacje o podróży dla typu pasażera

        //            //        if (tripInfo == null)
        //            //        {

        //            //            tripInfos = item.SelectNodes("TRI_INF[PTC_DTL_INF/PTC='" + ParseToWorldSpanPassengerType(adults, passenger.Code) + "']");
        //            //            if (tripInfos.Count == 1)
        //            //            {
        //            //                tripInfo = tripInfos[0];
        //            //            }
        //            //            else
        //            //            {
        //            //                for (int x = 0; x < tripInfos.Count; x++)
        //            //                {
        //            //                    XmlNodeList airports = tripInfos[x].SelectNodes("ARP_COD");
        //            //                    for (int y = 0; y < airports.Count - 1; y++)
        //            //                    {
        //            //                        if (airports[y].InnerText == segment.DepartureCityCode && airports[y + 1].InnerText == segment.ArrivalCityCode)
        //            //                        {
        //            //                            tripInfo = tripInfos[x];
        //            //                            break;
        //            //                        }
        //            //                    }
        //            //                    if (tripInfo != null) break;
        //            //                }
        //            //            }
        //            //        }

        //            //        #endregion

        //            //        if (tripInfo != null)
        //            //        {
        //            //            string addData = string.Empty;
        //            //            if (tripInfo.SelectSingleNode("MKT_PUL") != null)
        //            //            {
        //            //                addData = tripInfo.SelectSingleNode("MKT_PUL/ORI").InnerText + ";" + tripInfo.SelectSingleNode("MKT_PUL/DES").InnerText;
        //            //            }

        //            //            Fare fare = new Fare(segment, passenger.Code, tripInfo.SelectSingleNode("PTC_DTL_INF/FAR_BAS_COD").InnerText, addData);
        //            //        }
        //            //        else
        //            //        {
        //            //            new Fare(segment, passenger.Code, string.Empty, string.Empty);
        //            //        }
        //            //    }

        //            //    #endregion
        //            //}


        //            #endregion


        //            //#region Wypełnianie cen lotu

        //            decimal providerBaseAmount = Convert.ToDecimal(item.SelectSingleNode("BAS_FAR").InnerText, CultureInfo.InvariantCulture);
        //            decimal providerTaxAmount = Convert.ToDecimal(item.SelectSingleNode("TAX").InnerText, CultureInfo.InvariantCulture);
        //            string providerCurrencyCode = item.SelectSingleNode("ISO_CUR_COD").InnerText;

        //            result.Flights.Add(new Flight { AirlineCode = "LF", Departure = command.Departure, Arrival = command.Arrival, DepartureDate = command.DepartureDate, ArrivalDate = command.ArrivalDate, Price = providerBaseAmount });

        //            //if (LOSpecialPromo.IsPromotionFlight(flight, context))
        //            //{
        //            //    providerBaseAmount = Math.Round(providerBaseAmount * 0.8m);
        //            //}

        //            //#region Znajdź ticketing fee

        //            //XmlNode ticketingFeeNode = item.SelectSingleNode("TIC_FEE_INF/FEE_TTL/TTL_FEE_AMT");

        //            //if (ticketingFeeNode != null)
        //            //{
        //            //    providerTaxAmount += ConvertUtility.ParseXmlToDecimal(ticketingFeeNode.InnerText);
        //            //}

        //            //#endregion

        //            //foreach (Leg flightLeg in flight.Legs)
        //            //{
        //            //    foreach (Person person in searchParameters.Persons)
        //            //    {
        //            //        Passenger pax = new Passenger(person, flightLeg);
        //            //        decimal paxBaseAmount = providerBaseAmount / (searchParameters.Persons.Count * flight.Legs.Count);
        //            //        decimal paxTaxAmount = providerTaxAmount / (searchParameters.Persons.Count * flight.Legs.Count);
        //            //        pax.Price.ProviderBasePrice = new PriceItem(new PriceItemValue(paxBaseAmount, providerCurrencyCode), parameters.Context.CurrencyCode, providerCurrencyCode, context.ServiceType);
        //            //        pax.Price.ProviderTaxPrice = new PriceItem(new PriceItemValue(paxTaxAmount, providerCurrencyCode), parameters.Context.CurrencyCode, providerCurrencyCode, context.ServiceType);
        //            //    }
        //            //}

        //            //#endregion

        //            //if (flight.Legs.Count == searchParameters.Legs.Count) AddFlightToSearchResult(flight);
        //        }

        //        //lock (this.SearchResultItems)
        //        //{
        //        //    foreach (WorldSpanSpecialFilter filter in WorldSpanFiltersFactory.GetActiveFilters())
        //        //    {
        //        //        filter.ApplyFilter(this.SearchResultItems);
        //        //    }

        //        //}
        //        //#endregion
        //    }
        //    catch (Exception e)
        //    {
        //        //this.ex = e;
        //    }

        //    //result.Flights.Add(new Flight {AirlineCode = "LF", Departure = command.Departure, Arrival = command.Arrival,DepartureDate = command.DepartureDate,ArrivalDate = command.ArrivalDate, Price = 700.0M});
        //    //result.Flights.Add(new Flight
        //    //                   {
        //    //                       AirlineCode = "FR",
        //    //                       Departure = command.Departure,
        //    //                       Arrival = command.Arrival,
        //    //                       DepartureDate = command.DepartureDate,
        //    //                       ArrivalDate = command.ArrivalDate,
        //    //                       Price = 160.0M
        //    //                   });
        //    //result.Flights.Add(new Flight
        //    //                   {
        //    //                       AirlineCode = "FR",
        //    //                       Departure = command.Departure,
        //    //                       Arrival = command.Arrival,
        //    //                       DepartureDate = command.DepartureDate,
        //    //                       ArrivalDate = command.ArrivalDate,
        //    //                       Price = 100.0M
        //    //                   });
        //    return result;
        //}


    }
}

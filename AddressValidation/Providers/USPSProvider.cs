using AddressValidation.Model;
using NLog;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Web;

namespace AddressValidation.Providers
{
    /// <summary>
    /// </summary>
    public class USPSProvider : AbstractProvider
    {
        private const string REPLACE_UNSAFE_CHARACTERS_REGEX = @"[\\;\/?:@=&<>#%{}|\^~\[\]`]";

        private readonly string mUserId;
        private readonly string mUspsVerifyUrl;
        private readonly ILogger mLogger;
       
        public USPSProvider()
        {
            mLogger = LogManager.GetCurrentClassLogger();
            Name = "USPS";
            mUserId = Properties.Settings.Default.USPS_USER_ID;
            mUspsVerifyUrl = Properties.Settings.Default.USPS_VERIFY_URL;
        }

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        public USPSProvider(string userId)
        {
            Name = "USPS";
            mUserId = userId;
            mLogger = LogManager.GetCurrentClassLogger();
        }

        public override ValidationResponse Validate(UserAddress address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            var sb = new StringBuilder();

            var settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.OmitXmlDeclaration = true;
            settings.NewLineHandling = NewLineHandling.None;

            using (var writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartElement("AddressValidateRequest");
                writer.WriteAttributeString("USERID", mUserId);
                writer.WriteStartElement("Address");
                writer.WriteElementString("Address1", String.IsNullOrWhiteSpace(address.Address1) ? "" : ReplaceUnsafeCharacters(address.Address1));
                writer.WriteElementString("Address2", String.IsNullOrWhiteSpace(address.Address2) ? "" : ReplaceUnsafeCharacters(address.Address2));
                writer.WriteElementString("City", String.IsNullOrWhiteSpace(address.City) ? "" : ReplaceUnsafeCharacters(address.City));
                writer.WriteElementString("State", String.IsNullOrWhiteSpace(address.StateProvince) ? "" : ReplaceUnsafeCharacters(address.StateProvince));
                writer.WriteElementString("Zip5", String.IsNullOrWhiteSpace(address.Zip5) ? "" : ReplaceUnsafeCharacters(address.Zip5));
                writer.WriteElementString("Zip4", String.IsNullOrWhiteSpace(address.Zip4) ? "" : ReplaceUnsafeCharacters(address.Zip4));
                writer.WriteEndElement(); //Address
                writer.WriteEndElement(); //AddressValidateRequest
                writer.Flush();
            }
            ValidationResponse response = null;
            try
            {
                var url = String.Format("{0}&XML={1}", mUspsVerifyUrl, HttpUtility.UrlEncode(sb.ToString()));
                mLogger.Debug("Request URL: {0}", url);
                var webClient = new WebClient();
                var rawResponse = webClient.DownloadString(url);
                mLogger.Debug("Raw response: {0}", rawResponse);
                response = ParseResult(rawResponse);
            }
            catch (Exception ex)
            {
                mLogger.Error(ex, "Caught exception validating address {0}", (sb != null) ? sb.ToString() : "UNAVAILABLE");
            }
            return response;
        }

        public string ReplaceUnsafeCharacters(string addressPart, string replace = " ")
        {
            if (String.IsNullOrWhiteSpace(addressPart))
            {
                return addressPart;
            }
            return Regex.Replace(addressPart, REPLACE_UNSAFE_CHARACTERS_REGEX, replace);
        }

        private ValidationResponse ParseResult(string rawResponse)
        {
            var response = new ValidationResponse();
            var document = XElement.Parse(rawResponse, LoadOptions.None);
            var address = document.Descendants("Address").First();
            if (address.Descendants("Error").Any())
            {
                var error = address.Descendants("Error").First();
                var uspsError = new USPSError();
                if (error.Element("Description") != null) {
                    uspsError.Description = error.Element("Description").Value;
                }
                if (error.Element("Source") != null)
                {
                    uspsError.Source = error.Element("Source").Value;
                }
                if (error.Element("HelpContext") != null)
                {
                    uspsError.HelpContext = error.Element("HelpContext").Value;
                }
                if (error.Element("HelpFile") != null)
                {
                    uspsError.HelpFile = error.Element("HelpFile").Value;
                }
                if (error.Element("Number") != null)
                {
                    uspsError.Number = error.Element("Number").Value;
                }
                response.Error = uspsError;
            } else
            {
                var cleansedAddress = new UserAddress();
                if (address.Element("Address1") != null)
                {
                    cleansedAddress.Address1 = address.Element("Address1").Value;
                }
                if (address.Element("Address2") != null)
                {
                    cleansedAddress.Address2 = address.Element("Address2").Value;
                }
                if (address.Element("City") != null)
                {
                    cleansedAddress.City = address.Element("City").Value;
                }
                if (address.Element("State") != null)
                {
                    cleansedAddress.StateProvince = address.Element("State").Value;
                }
                if (address.Element("Zip5") != null)
                {
                    cleansedAddress.PostalCode = address.Element("Zip5").Value;
                    if (address.Element("Zip4") != null)
                    {
                        var zip4 = address.Element("Zip4").Value;
                        if (!String.IsNullOrWhiteSpace(zip4))
                        {
                            cleansedAddress.PostalCode += String.Format("-{0}", zip4);
                        }
                    }
                }
                response.CleansedAddress = cleansedAddress;
            }
            return response;
        }
    }
}

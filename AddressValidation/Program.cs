using System;
using System.Linq;
using AddressValidation.Model;
using AddressValidation.Providers;
using NLog;

namespace AddressValidation
{
    class Program
    {
        static void Main(string[] args)
        {
            ILogger mLogger = LogManager.GetCurrentClassLogger();
            mLogger.Info("Starting address validation...");

            IProvider provider = new USPSProvider();
            using (UserAddressEntities addressContext = new UserAddressEntities())
            {
                using (AddressValidationEntities reportingContext = new AddressValidationEntities())
                {
                    var query = addressContext.UserAddresses.Where(x => x.EnterpriseID == 8);
                    foreach (var address in query)
                    {
                        try
                        {
                            var response = provider.Validate(address);
                            if (response == null)
                            {
                                mLogger.Error("An error occurred trying to validate address {0}. Response was null.", address.UserAddressID);
                            }
                            else
                            {
                                if (response.Error != null)
                                {
                                    mLogger.Info("Address {0} failed USPS validation. USPS Error Description: {1}", address.UserAddressID, response.Error.Description);
                                    mLogger.Info(address.ToString());
                                }
                                else
                                {
                                    mLogger.Debug("Address passed USPS validation, cleansed address below:");
                                    mLogger.Debug(response.CleansedAddress.ToString());
                                }
                                reportingContext.AddressValidations.Add(CreateReportRecord(address, response));
                                reportingContext.SaveChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            mLogger.Error(ex, "Caught exception validating address {0}.", address.UserAddressID);
                        }
                    }
                }
            }
            mLogger.Info("Ending address validation.");
            // Console.Read();
        }

        private static Model.AddressValidation CreateReportRecord(UserAddress address, ValidationResponse response)
        {
            var reportRecord = new Model.AddressValidation
            {
                EnterpriseID = address.EnterpriseID,
                UserID = address.UserID,
                LoginID = address.LoginID,
                UserName = address.UserName,
                LastName = address.LastName,
                MiddleName = address.MiddleName,
                FirstName = address.FirstName,
                UserAddressID = address.UserAddressID,
                Address1 = address.Address1,
                Address2 = address.Address2,
                City = address.City,
                StateProvince = address.StateProvince,
                PostalCode = address.PostalCode,
                ValidatedDate = DateTime.Now,
            };
            if (response.CleansedAddress != null)
            {
                reportRecord.USPSValidationResult = true;
                reportRecord.CleansedAddress1 = response.CleansedAddress.Address1;
                reportRecord.CleansedAddress2 = response.CleansedAddress.Address2;
                reportRecord.CleansedCity = response.CleansedAddress.City;
                reportRecord.CleansedStateProvince = response.CleansedAddress.StateProvince;
                reportRecord.CleansedPostalCode = response.CleansedAddress.PostalCode;
            }
            if (response.Error != null)
            {
                reportRecord.USPSValidationResult = false;
                reportRecord.ErrorNumber = response.Error.Number;
                reportRecord.ErrorSource = response.Error.Source;
                reportRecord.ErrorDescription = response.Error.Description;
                reportRecord.ErrorHelpContext = response.Error.HelpContext;
                reportRecord.ErrorHelpFile = response.Error.HelpFile;
            }
            return reportRecord;
        }
    }
}

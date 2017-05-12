using AddressValidation.Model;

namespace AddressValidation.Providers
{
    /// <summary>
    ///     Defines a standard interface for all shipping providers.
    /// </summary>
    public interface IProvider
    {
        /// <summary>
        ///     The name of the provider.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Retrieves rates from the provider.
        /// </summary>
        ValidationResponse Validate(UserAddress address);
    }
}

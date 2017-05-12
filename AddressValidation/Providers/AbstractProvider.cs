using AddressValidation.Model;

namespace AddressValidation.Providers
{
    /// <summary>
    ///     A base implementation of the <see cref="IShippingProvider" /> interface.
    ///     All provider-specific classes should inherit from this class.
    /// </summary>
    public abstract class AbstractProvider : IProvider
    {
        public string Name { get; set; }

        public virtual ValidationResponse Validate(UserAddress address)
        {
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddressValidation.Model
{
    public class ValidationResponse
    {
        public UserAddress CleansedAddress { get; set; }

        public USPSError Error { get; set; }
    }
}

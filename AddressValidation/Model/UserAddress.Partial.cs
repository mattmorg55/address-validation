using System;
using System.Text;
using System.Text.RegularExpressions;

namespace AddressValidation.Model
{
    public partial class UserAddress
    {
        public string Zip5 {
            get {
                if (String.IsNullOrWhiteSpace(this.PostalCode))
                {
                    return "";
                }
                return this.PostalCode.Substring(0, Math.Min(5, this.PostalCode.Length));
            }
        }

        public string Zip4 {
            get {
                if (String.IsNullOrWhiteSpace(this.PostalCode))
                {
                    return "";
                }
                var zipNumeric = Regex.Replace(this.PostalCode, "[^0-9]", "");
                if (zipNumeric.Length < 9)
                {
                    return "";
                }
                return zipNumeric.Substring(5, 4);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            AppendLine(sb, Address1);
            AppendLine(sb, Address2);
            AppendLine(sb, City);
            AppendLine(sb, StateProvince);
            Append(sb, PostalCode);
            return sb.ToString();
        }

        private void AppendLine(StringBuilder sb, string line)
        {
            if (!String.IsNullOrWhiteSpace(line))
            {
                sb.AppendLine(line);
            }
        }

        private void Append(StringBuilder sb, string line)
        {
            if (!String.IsNullOrWhiteSpace(line))
            {
                sb.Append(line);
            }
        }
    }
}

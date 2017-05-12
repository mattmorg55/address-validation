using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AddressValidation.Providers;

namespace AddressValidation.Tests
{
    [TestClass]
    public class USPSProviderTests
    {
        [TestMethod]
        public void TestReplaceUnsafeCharacters()
        {
            USPSProvider provider = new USPSProvider();
            string xml = @"a;a/a?a:a@a=a&a<a>a#a%a{a}a|a\a^a~a[a]a`";

            string clean = provider.ReplaceUnsafeCharacters(xml);

            Assert.AreEqual("a a a a a a a a a a a a a a a a a a a a ", clean);
        }
    }
}

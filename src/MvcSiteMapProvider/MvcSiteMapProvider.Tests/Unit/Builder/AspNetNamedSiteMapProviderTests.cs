using System;
using System.Web;
using MvcSiteMapProvider.Builder;
using NUnit.Framework;

namespace MvcSiteMapProvider.Tests.Unit.Builder
{
    [TestFixture]
    public class AspNetNamedSiteMapProviderTests
    {
        [Test]
        public void Ctor_NullProviderName_ThrowsArgumentNullException()
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() => new AspNetNamedSiteMapProvider(null));
        }

        [Test]
        public void Ctor_EmptyProviderName_ThrowsArgumentNullException()
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() => new AspNetNamedSiteMapProvider(string.Empty));
        }

        [Test]
        public void GetProvider_WithExistingProviderName_ReturnsProvider()
        {
            // arrange
            var existingName = System.Web.SiteMap.Provider.Name; // Use the default provider's name.
            var target = new AspNetNamedSiteMapProvider(existingName);

            // act
            var result = target.GetProvider();

            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo(existingName));
        }

        [Test]
        public void GetProvider_WithNonExistingProviderName_ThrowsInvalidOperationException()
        {
            // arrange
            var name = Guid.NewGuid().ToString("N");
            var target = new AspNetNamedSiteMapProvider(name);

            // act & assert
            Assert.Throws<InvalidOperationException>(() => target.GetProvider());
        }
    }
}

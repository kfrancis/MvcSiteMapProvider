using System.Web;
using MvcSiteMapProvider.Builder;
using NUnit.Framework;

namespace MvcSiteMapProvider.Tests.Unit.Builder
{
    [TestFixture]
    public class AspNetDefaultSiteMapProviderTests
    {
        [Test]
        public void GetProvider_ShouldReturnNonNullProvider()
        {
            // arrange
            var target = new AspNetDefaultSiteMapProvider();

            // act
            var result = target.GetProvider();

            // assert
            Assert.That(result, Is.Not.Null);
        }
    }
}

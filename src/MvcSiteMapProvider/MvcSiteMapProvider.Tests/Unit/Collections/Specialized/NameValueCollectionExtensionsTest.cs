using System.Collections.Specialized;
using MvcSiteMapProvider.Collections.Specialized;
using NUnit.Framework;

namespace MvcSiteMapProvider.Tests.Unit.Collections.Specialized
{
    [TestFixture]
    public class NameValueCollectionExtensionsTest
    {
        [Test]
        public void AddWithCaseCorrection_KeyMatchesDifferentCase_AddsWithCorrectCaseKey()
        {
            // arrange
            var collection = new NameValueCollection();
            var keyset = new[] { "MyConfiguredKey" }; // correct casing

            // act
            collection.AddWithCaseCorrection("myconfiguredkey", "123", keyset);

            // assert
            Assert.That(collection.Count, Is.EqualTo(1));
            Assert.That(collection["MyConfiguredKey"], Is.EqualTo("123"));
            // Ensure only the corrected case key exists (cannot rely on case-sensitive lookup)
            Assert.That(collection.AllKeys, Is.EquivalentTo(new[] { "MyConfiguredKey" }));
        }

        [Test]
        public void AddWithCaseCorrection_KeyDoesNotExistInKeyset_DoesNotAdd()
        {
            var collection = new NameValueCollection();
            var keyset = new[] { "SomeOtherKey" };

            collection.AddWithCaseCorrection("MissingKey", "abc", keyset);

            Assert.That(collection.Count, Is.EqualTo(0));
        }

        [Test]
        public void AddWithCaseCorrection_MultipleMatchingCaseVariants_UsesFirstMatchOnly()
        {
            var collection = new NameValueCollection();
            var keyset = new[] { "PrimaryKey", "primarykey" }; // both match ignoring case

            collection.AddWithCaseCorrection("PrImArYkEy", "value", keyset);

            // Only first variant should be used
            Assert.That(collection.Count, Is.EqualTo(1));
            Assert.That(collection.GetKey(0), Is.EqualTo("PrimaryKey"));
        }

        [Test]
        public void AddWithCaseCorrection_RepeatedAdds_AddsMultipleValuesUnderSameCorrectCaseKey()
        {
            var collection = new NameValueCollection();
            var keyset = new[] { "RouteId" };

            collection.AddWithCaseCorrection("routeid", "1", keyset);
            collection.AddWithCaseCorrection("ROUTEID", "2", keyset);

            var values = collection.GetValues("RouteId");
            Assert.That(values, Is.Not.Null);
            // values will not be null because key was added
            Assert.That(values.Length, Is.EqualTo(2));
            Assert.That(values, Is.EquivalentTo(new[] { "1", "2" }));
        }
    }
}

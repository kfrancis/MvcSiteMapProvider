using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Linq;
using Moq;
using MvcSiteMapProvider.Builder;
using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.Collections.Specialized;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Web.Script.Serialization;
using NUnit.Framework;

namespace MvcSiteMapProvider.Tests.Unit.Collections.Specialized
{
    [TestFixture]
    public class AttributeDictionaryTest
    {
        [SetUp]
        public void Init()
        {
            _mSiteMap = new Mock<ISiteMap>();
            _mSiteMap.SetupGet(s => s.IsReadOnly).Returns(false); // writable by default
            _mSiteMap.SetupGet(s => s.CacheKey).Returns("SiteMapCacheKey");

            _mCache = new Mock<IRequestCache>();
            _mLocalization = new Mock<ILocalizationService>();
            _mReservedProvider = new Mock<IReservedAttributeNameProvider>();
            _mJsonDeserializer = new Mock<IJsonToDictionaryDeserializer>();

            // By default, all keys are regular (allowed)
            _mReservedProvider.Setup(p => p.IsRegularAttribute(It.IsAny<string>())).Returns(true);
        }

        private Mock<ISiteMap> _mSiteMap;
        private Mock<IRequestCache> _mCache;
        private Mock<ILocalizationService> _mLocalization;
        private Mock<IReservedAttributeNameProvider> _mReservedProvider;
        private Mock<IJsonToDictionaryDeserializer> _mJsonDeserializer;

        private AttributeDictionary NewTarget()
        {
            return new AttributeDictionary("nodeKey", "Attributes", _mSiteMap.Object, _mLocalization.Object,
                _mReservedProvider.Object, _mJsonDeserializer.Object, _mCache.Object);
        }

        [Test]
        public void Add_StringValue_UsesLocalizationExtraction()
        {
            // arrange
            var target = NewTarget();
            _mLocalization.Setup(l => l.ExtractExplicitResourceKey("title", "My Title"))
                .Returns("RES:My Title");
            _mLocalization.Setup(l => l.GetResourceString("title", "RES:My Title", _mSiteMap.Object))
                .Returns("LOC:My Title");

            // act
            target.Add("title", "My Title");
            var value = target["title"]; // triggers localization GetResourceString

            // assert
            Assert.That(value, Is.EqualTo("LOC:My Title"));
            _mLocalization.Verify(l => l.ExtractExplicitResourceKey("title", "My Title"), Times.Once);
            _mLocalization.Verify(l => l.GetResourceString("title", "RES:My Title", _mSiteMap.Object), Times.Once);
        }

        [Test]
        public void Add_NonStringValue_DoesNotUseLocalizationExtraction()
        {
            // arrange
            var target = NewTarget();

            // act
            target.Add("count", 5);

            // assert
            Assert.That(target["count"], Is.EqualTo(5));
            _mLocalization.Verify(l => l.ExtractExplicitResourceKey(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public void Add_ReservedKeyWithThrow_ThrowsReservedKeyException()
        {
            // arrange
            _mReservedProvider.Setup(p => p.IsRegularAttribute("reserved"))
                .Returns(false); // not a regular attribute => reserved
            var target = NewTarget();

            // act / assert
            Assert.Throws<ReservedKeyException>(() => target.Add("reserved", "value", true));
        }

        [Test]
        public void Add_ReservedKeyWithoutThrow_DoesNotAdd()
        {
            // arrange
            _mReservedProvider.Setup(p => p.IsRegularAttribute("reserved"))
                .Returns(false);
            var target = NewTarget();

            // act
            target.Add("reserved", "value", false); // should silently ignore

            // assert
            Assert.That(target.ContainsKey("reserved"), Is.False);
        }

        [Test]
        public void IndexerSet_StringValue_LocalizesOnSetAndGet()
        {
            // arrange
            var target = NewTarget();
            _mLocalization.Setup(l => l.ExtractExplicitResourceKey("caption", "Hello"))
                .Returns("RES:Hello");
            _mLocalization.Setup(l => l.GetResourceString("caption", "RES:Hello", _mSiteMap.Object))
                .Returns("LOC:Hello");

            // act
            target["caption"] = "Hello"; // set
            var result = target["caption"]; // get localized

            // assert
            Assert.That(result, Is.EqualTo("LOC:Hello"));
            _mLocalization.Verify(l => l.ExtractExplicitResourceKey("caption", "Hello"), Times.Once);
            _mLocalization.Verify(l => l.GetResourceString("caption", "RES:Hello", _mSiteMap.Object), Times.Once);
        }

        [Test]
        public void AddRange_FromJson_UsesDeserializer()
        {
            // arrange
            var target = NewTarget();
            var dict = new Dictionary<string, object> { { "a", "1" }, { "b", 2 } };
            _mJsonDeserializer.Setup(j => j.Deserialize("{json}")).Returns(dict);
            _mLocalization.Setup(l => l.ExtractExplicitResourceKey(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((k, v) => v); // pass-through
            _mLocalization.Setup(l => l.GetResourceString(It.IsAny<string>(), It.IsAny<string>(), _mSiteMap.Object))
                .Returns<string, string, ISiteMap>((k, v, s) => v);

            // act
            target.AddRange("{json}");

            // assert
            Assert.That(target.Count, Is.EqualTo(2));
            _mJsonDeserializer.Verify(j => j.Deserialize("{json}"), Times.Once);
        }

        [Test]
        public void AddRange_FromNameValueCollection_AddsAll()
        {
            // arrange
            var nvc = new NameValueCollection
            {
                { "k1", "v1" }, { "k2", "v2" }
            };
            var target = NewTarget();
            _mLocalization.Setup(l => l.ExtractExplicitResourceKey(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((k, v) => v);
            _mLocalization.Setup(l => l.GetResourceString(It.IsAny<string>(), It.IsAny<string>(), _mSiteMap.Object))
                .Returns<string, string, ISiteMap>((k, v, s) => v);

            // act
            target.AddRange(nvc);

            // assert
            Assert.That(target.Count, Is.EqualTo(2));
            Assert.That(target.ContainsKey("k1"), Is.True);
        }

        [Test]
        public void AddRange_FromXElement_AddsAll()
        {
            // arrange
            var xml = new XElement("siteMapNode",
                new XAttribute("attr1", "value1"),
                new XAttribute("attr2", "value2"));
            var target = NewTarget();
            _mLocalization.Setup(l => l.ExtractExplicitResourceKey(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((k, v) => v);
            _mLocalization.Setup(l => l.GetResourceString(It.IsAny<string>(), It.IsAny<string>(), _mSiteMap.Object))
                .Returns<string, string, ISiteMap>((k, v, s) => v);

            // act
            target.AddRange(xml);

            // assert
            Assert.That(target.Count, Is.EqualTo(2));
            Assert.That(target.ContainsKey("attr1"), Is.True);
        }

        [Test]
        public void Remove_RemovesResourceKey()
        {
            // arrange
            var target = NewTarget();
            _mLocalization.Setup(l => l.ExtractExplicitResourceKey("key", "val")).Returns("val");
            _mLocalization.Setup(l => l.GetResourceString("key", "val", _mSiteMap.Object)).Returns("val");
            target.Add("key", "val");

            // act
            var removed = target.Remove("key");

            // assert
            Assert.That(removed, Is.True);
            _mLocalization.Verify(l => l.RemoveResourceKey("key"), Times.Once);
        }

        [Test(Description =
            "Current implementation clears before enumerating Keys so RemoveResourceKey is never called. Test documents existing behavior.")]
        public void Clear_DoesNotInvokeRemoveResourceKey_DueToImplementationOrder()
        {
            // arrange
            var target = NewTarget();
            _mLocalization.Setup(l => l.ExtractExplicitResourceKey(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((k, v) => v);
            _mLocalization.Setup(l => l.GetResourceString(It.IsAny<string>(), It.IsAny<string>(), _mSiteMap.Object))
                .Returns<string, string, ISiteMap>((k, v, s) => v);
            target.Add("one", "1");
            target.Add("two", "2");

            // act
            target.Clear();

            // assert
            Assert.That(target.Count, Is.EqualTo(0));
            _mLocalization.Verify(l => l.RemoveResourceKey(It.IsAny<string>()), Times.Never);
        }
    }
}

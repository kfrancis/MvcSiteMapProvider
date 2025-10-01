using System;
using Moq;
using MvcSiteMapProvider.Collections.Specialized;
using NUnit.Framework;

namespace MvcSiteMapProvider.Tests.Unit.Collections.Specialized
{
    [TestFixture]
    public class MetaRobotsValueCollectionTest
    {
        [SetUp]
        public void SetUp()
        {
            _mSiteMap = new Mock<ISiteMap>();
            _mSiteMap.SetupGet(s => s.IsReadOnly).Returns(false);
        }

        private Mock<ISiteMap> _mSiteMap;

        private MetaRobotsValueCollection NewTarget()
        {
            return new MetaRobotsValueCollection(_mSiteMap.Object);
        }

        [Test]
        public void Add_DefaultIndexFollowPair_ProducesEmptyContentString()
        {
            var target = NewTarget();
            target.Add("index");
            target.Add("follow");
            Assert.That(target.GetMetaRobotsContentString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void Add_OnlyIndex_ProducesEmptyContentString()
        {
            var target = NewTarget();
            target.Add("index");
            Assert.That(target.GetMetaRobotsContentString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void Add_OnlyFollow_ProducesEmptyContentString()
        {
            var target = NewTarget();
            target.Add("follow");
            Assert.That(target.GetMetaRobotsContentString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void Add_NonDefaultCombination_ReturnsCommaSeparatedValues()
        {
            var target = NewTarget();
            target.Add("noindex");
            target.Add("follow");
            Assert.That(target.GetMetaRobotsContentString(), Is.EqualTo("noindex,follow"));
        }

        [Test]
        public void Add_DuplicateValue_Ignored()
        {
            var target = NewTarget();
            target.Add("noindex");
            target.Add("noindex");
            Assert.That(target.Count, Is.EqualTo(1));
        }

        [Test]
        public void Add_CaseInsensitivity_NormalizesToLower()
        {
            var target = NewTarget();
            target.Add("NoIndex");
            Assert.That(target[0], Is.EqualTo("noindex"));
        }

        [Test]
        public void HasNoIndexAndNoFollow_WithBothExplicitValues_IsTrue()
        {
            var target = NewTarget();
            target.Add("noindex");
            target.Add("nofollow");
            Assert.That(target.HasNoIndexAndNoFollow, Is.True);
        }

        [Test]
        public void HasNoIndexAndNoFollow_WithNoneShortcut_IsTrue()
        {
            var target = NewTarget();
            target.Add("none");
            Assert.That(target.HasNoIndexAndNoFollow, Is.True);
        }

        [Test]
        public void AddRange_String_SplitsAndAddsUnique()
        {
            var target = NewTarget();
            target.AddRange("noindex,follow,noindex", new[] { ',' });
            Assert.That(target.Count, Is.EqualTo(2));
            Assert.That(target.GetMetaRobotsContentString(), Is.EqualTo("noindex,follow"));
        }

        [Test]
        public void AddRange_IEnumerable_AddsAll()
        {
            var target = NewTarget();
            target.AddRange(new[] { "noarchive", "nocache" });
            Assert.That(target.Count, Is.EqualTo(2));
        }

        [Test]
        public void Insert_Duplicate_Ignored()
        {
            var target = NewTarget();
            target.Add("noindex");
            target.Insert(0, "noindex");
            Assert.That(target.Count, Is.EqualTo(1));
        }

        [Test]
        public void InsertRange_SplitsInsertedItems()
        {
            var target = NewTarget();
            target.Add("nosnippet");
            target.InsertRange(0, new[] { "noarchive", "nocache" });
            // Because index increases by 2 between insertions, order will be: noarchive, nosnippet, nocache
            Assert.That(target[0], Is.EqualTo("noarchive"));
            Assert.That(target[1], Is.EqualTo("nosnippet"));
            Assert.That(target[2], Is.EqualTo("nocache"));
        }

        [Test]
        public void Add_IndexThenNoIndex_Throws()
        {
            var target = NewTarget();
            target.Add("index");
            Assert.Throws<ArgumentException>(() => target.Add("noindex"));
        }

        [Test]
        public void Add_NoIndexThenIndex_Throws()
        {
            var target = NewTarget();
            target.Add("noindex");
            Assert.Throws<ArgumentException>(() => target.Add("index"));
        }

        [Test]
        public void Add_FollowThenNoFollow_Throws()
        {
            var target = NewTarget();
            target.Add("follow");
            Assert.Throws<ArgumentException>(() => target.Add("nofollow"));
        }

        [Test]
        public void Add_NoFollowThenFollow_Throws()
        {
            var target = NewTarget();
            target.Add("nofollow");
            Assert.Throws<ArgumentException>(() => target.Add("follow"));
        }

        [Test]
        public void Add_NoneThenFollow_Throws()
        {
            var target = NewTarget();
            target.Add("none");
            Assert.Throws<ArgumentException>(() => target.Add("follow"));
        }

        [Test]
        public void Add_FollowThenNone_Throws()
        {
            var target = NewTarget();
            target.Add("follow");
            Assert.Throws<ArgumentException>(() => target.Add("none"));
        }

        [Test]
        public void Add_UnrecognizedValue_Throws()
        {
            var target = NewTarget();
            Assert.Throws<ArgumentException>(() => target.Add("notreal"));
        }

        [Test]
        public void GetMetaRobotsContentString_WithIndexAndNoArchive_NotDefault()
        {
            var target = NewTarget();
            target.Add("index");
            target.Add("noarchive");
            Assert.That(target.GetMetaRobotsContentString(), Is.EqualTo("index,noarchive"));
        }
    }
}

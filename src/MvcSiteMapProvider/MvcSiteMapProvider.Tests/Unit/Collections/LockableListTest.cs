using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using MvcSiteMapProvider.Collections;

namespace MvcSiteMapProvider.Tests.Unit.Collections
{
    [TestFixture]
    public class LockableListTest
    {
        private class TestLockableList<T> : LockableList<T>
        {
            public TestLockableList(ISiteMap siteMap) : base(siteMap) { }
        }

        private Mock<ISiteMap> CreateSiteMapMock(bool isReadOnly)
        {
            var m = new Mock<ISiteMap>();
            m.SetupGet(x => x.IsReadOnly).Returns(isReadOnly);
            return m;
        }

        private LockableList<string> CreateList(bool isReadOnly = false)
        {
            return new TestLockableList<string>(CreateSiteMapMock(isReadOnly).Object);
        }

        [Test]
        public void Add_WhenNotReadOnly_AddsItem()
        {
            var list = CreateList();
            list.Add("a");
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo("a"));
        }

        [Test]
        public void Add_WhenReadOnly_Throws()
        {
            var list = CreateList(true);
            Assert.Throws<InvalidOperationException>(() => list.Add("a"));
            Assert.That(list.Count, Is.EqualTo(0));
        }

        [Test]
        public void AddRange_WhenNotReadOnly_AddsAll()
        {
            var list = CreateList();
            list.AddRange(new[] {"a","b","c"});
            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(list[1], Is.EqualTo("b"));
        }

        [Test]
        public void Insert_WhenNotReadOnly_InsertsAtIndex()
        {
            var list = CreateList();
            list.AddRange(new[]{"a","c"});
            list.Insert(1, "b");
            Assert.That(list[1], Is.EqualTo("b"));
            Assert.That(list.Count, Is.EqualTo(3));
        }

        [Test]
        public void InsertRange_WhenNotReadOnly_InsertsItems()
        {
            var list = CreateList();
            list.Add("a");
            list.InsertRange(1, new[]{"b","c"});
            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(list[2], Is.EqualTo("c"));
        }

        [Test]
        public void Remove_WhenNotReadOnly_RemovesItem()
        {
            var list = CreateList();
            list.AddRange(new[]{"a","b","c"});
            var removed = list.Remove("b");
            Assert.That(removed, Is.True);
            Assert.That(list, Is.EquivalentTo(new[]{"a","c"}));
        }

        [Test]
        public void RemoveAll_WhenNotReadOnly_RemovesMatching()
        {
            var list = CreateList();
            list.AddRange(new[]{"a","bb","ccc","dd"});
            var removedCount = list.RemoveAll(s => s.Length == 2);
            Assert.That(removedCount, Is.EqualTo(2));
            Assert.That(list, Is.EquivalentTo(new[]{"a","ccc"}));
        }

        [Test]
        public void RemoveAt_WhenNotReadOnly_RemovesIndex()
        {
            var list = CreateList();
            list.AddRange(new[]{"a","b","c"});
            list.RemoveAt(1);
            Assert.That(list, Is.EquivalentTo(new[]{"a","c"}));
        }

        [Test]
        public void RemoveRange_WhenNotReadOnly_RemovesRange()
        {
            var list = CreateList();
            list.AddRange(new[]{"a","b","c","d","e"});
            list.RemoveRange(1, 3); // remove b,c,d
            Assert.That(list, Is.EquivalentTo(new[]{"a","e"}));
        }

        [Test]
        public void Reverse_WhenNotReadOnly_Reverses()
        {
            var list = CreateList();
            list.AddRange(new[]{"a","b","c"});
            list.Reverse();
            Assert.That(list, Is.EqualTo(new[]{"c","b","a"}));
        }

        [Test]
        public void ReverseRange_WhenNotReadOnly_ReversesSubset()
        {
            var list = CreateList();
            list.AddRange(new[]{"a","b","c","d","e"});
            list.Reverse(1,3); // b,c,d -> d,c,b
            Assert.That(list, Is.EqualTo(new[]{"a","d","c","b","e"}));
        }

        [Test]
        public void Sort_DefaultSort_Sorts()
        {
            var list = CreateList();
            list.AddRange(new[]{"b","c","a"});
            list.Sort();
            Assert.That(list, Is.EqualTo(new[]{"a","b","c"}));
        }

        [Test]
        public void Sort_WithComparison_Sorts()
        {
            var list = CreateList();
            list.AddRange(new[]{"bb","a","ccc"});
            list.Sort((x,y) => x.Length.CompareTo(y.Length));
            Assert.That(list, Is.EqualTo(new[]{"a","bb","ccc"}));
        }

        [Test]
        public void Sort_WithComparer_Sorts()
        {
            var list = CreateList();
            list.AddRange(new[]{"bb","a","ccc"});
            list.Sort(Comparer<string>.Create((x,y)=> y.Length.CompareTo(x.Length)));
            Assert.That(list, Is.EqualTo(new[]{"ccc","bb","a"}));
        }

        [Test]
        public void Sort_Range_SortsSubset()
        {
            var list = CreateList();
            list.AddRange(new[]{"z","b","c","a","y"});
            list.Sort(1,3,StringComparer.Ordinal); // sort b,c,a -> a,b,c
            Assert.That(list, Is.EqualTo(new[]{"z","a","b","c","y"}));
        }

        [Test]
        public void TrimExcess_WhenNotReadOnly_DoesNotThrow()
        {
            var list = CreateList();
            list.AddRange(new[]{"a","b"});
            Assert.DoesNotThrow(()=> list.TrimExcess());
        }

        [Test]
        public void CopyTo_CopiesItems()
        {
            var list = CreateList();
            list.AddRange(new[]{"a","b","c"});
            var dest = new List<string>();
            list.CopyTo(dest);
            Assert.That(dest, Is.EqualTo(new[]{"a","b","c"}));
        }

        [Test]
        public void CopyTo_NullItem_ThrowsNotSupportedException()
        {
            var list = CreateList();
            list.Add("a");
            list.Add(null); // null allowed in list
            list.Add("b");
            var dest = new List<string>();
            Assert.Throws<NotSupportedException>(() => list.CopyTo(dest));
            Assert.That(dest, Is.EqualTo(new[]{"a"}), "Only items before null should be copied");
        }

        [Test]
        public void IsReadOnly_ReflectsSiteMap()
        {
            var m = new Mock<ISiteMap>();
            bool readOnly = false;
            m.SetupGet(x => x.IsReadOnly).Returns(()=> readOnly);
            var list = new TestLockableList<string>(m.Object);
            Assert.That(list.IsReadOnly, Is.False);
            readOnly = true;
            Assert.That(list.IsReadOnly, Is.True);
        }

        [Test]
        public void MutatingMethods_WhenReadOnly_ThrowInvalidOperationException()
        {
            var list = CreateList(true);
            Assert.Multiple(() =>
            {
                Assert.Throws<InvalidOperationException>(() => list.Add("a"));
                Assert.Throws<InvalidOperationException>(() => list.AddRange(new[]{"a"}));
                Assert.Throws<InvalidOperationException>(() => list.Clear());
                Assert.Throws<InvalidOperationException>(() => list.Insert(0, "a"));
                Assert.Throws<InvalidOperationException>(() => list.InsertRange(0, new[]{"a"}));
                Assert.Throws<InvalidOperationException>(() => list.Remove("a"));
                Assert.Throws<InvalidOperationException>(() => list.RemoveAll(x => true));
                Assert.Throws<InvalidOperationException>(() => list.RemoveAt(0));
                Assert.Throws<InvalidOperationException>(() => list.RemoveRange(0,0));
                Assert.Throws<InvalidOperationException>(() => list.Reverse());
                Assert.Throws<InvalidOperationException>(() => list.Reverse(0,0));
                Assert.Throws<InvalidOperationException>(() => list.Sort());
                Assert.Throws<InvalidOperationException>(() => list.Sort((x,y)=>0));
                Assert.Throws<InvalidOperationException>(() => list.Sort(StringComparer.Ordinal));
                Assert.Throws<InvalidOperationException>(() => list.Sort(0,0,StringComparer.Ordinal));
                Assert.Throws<InvalidOperationException>(() => list.TrimExcess());
            });
        }
    }
}

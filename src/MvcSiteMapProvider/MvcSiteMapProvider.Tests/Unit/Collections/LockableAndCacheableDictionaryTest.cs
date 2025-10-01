using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using MvcSiteMapProvider.Collections;
using MvcSiteMapProvider.Caching;

namespace MvcSiteMapProvider.Tests.Unit.Collections
{
    [TestFixture]
    public class LockableAndCacheableDictionaryTest
    {
        private class TestLockableDictionary<TKey, TValue> : LockableDictionary<TKey, TValue>
        {
            public TestLockableDictionary(ISiteMap siteMap) : base(siteMap) { }
        }

        private class TestCacheableDictionary<TKey, TValue> : CacheableDictionary<TKey, TValue>
        {
            public TestCacheableDictionary(ISiteMap siteMap, ICache cache) : base(siteMap, cache) { }
            protected override bool CachingEnabled { get { return true; } }
        }

        private Mock<ISiteMap> CreateSiteMap(bool isReadOnly)
        {
            var m = new Mock<ISiteMap>();
            m.SetupGet(s => s.IsReadOnly).Returns(isReadOnly);
            return m;
        }

        private LockableDictionary<string,int> CreateLockable(bool readOnly=false)
        {
            return new TestLockableDictionary<string,int>(CreateSiteMap(readOnly).Object);
        }

        private TestCacheableDictionary<string,int> CreateCacheable(out Mock<ICache> cacheMock, bool readOnly=false)
        {
            var siteMap = CreateSiteMap(readOnly);
            cacheMock = new Mock<ICache>();
            return new TestCacheableDictionary<string,int>(siteMap.Object, cacheMock.Object);
        }

        #region LockableDictionary Tests

        [Test]
        public void Lockable_Add_WhenNotReadOnly_Succeeds()
        {
            var d = CreateLockable();
            d.Add("a",1);
            Assert.That(d["a"], Is.EqualTo(1));
        }

        [Test]
        public void Lockable_Add_WhenReadOnly_Throws()
        {
            var d = CreateLockable(true);
            Assert.Throws<InvalidOperationException>(()=> d.Add("a",1));
        }

        [Test]
        public void Lockable_IndexerSet_WhenReadOnly_Throws()
        {
            var d = CreateLockable(true);
            Assert.Throws<InvalidOperationException>(()=> d["a"]=1);
        }

        [Test]
        public void Lockable_Remove_WhenReadOnly_Throws()
        {
            var d = CreateLockable(true);
            Assert.Throws<InvalidOperationException>(()=> d.Remove("a"));
        }

        [Test]
        public void Lockable_CopyTo_CopiesAll()
        {
            var d = CreateLockable();
            d.Add("a",1); d.Add("b",2);
            var target = new Dictionary<string,int>();
            d.CopyTo(target);
            Assert.That(target.Count, Is.EqualTo(2));
            Assert.That(target["b"], Is.EqualTo(2));
        }

        #endregion

        #region CacheableDictionary Tests

        [Test]
        public void Cacheable_Add_WhenWritable_WritesToBase()
        {
            Mock<ICache> cache;
            var dict = CreateCacheable(out cache, false);
            dict.Add("a",1);
            Assert.That(dict["a"], Is.EqualTo(1));
            cache.Verify(c=>c.GetValue<IDictionary<string,int>>(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Cacheable_Add_WhenReadOnly_CreatesCacheCopy()
        {
            Mock<ICache> cache;
            var dict = CreateCacheable(out cache, true);
            IDictionary<string,int> cached = null; // will hold cached copy
            cache.Setup(c=>c.GetValue<IDictionary<string,int>>(It.IsAny<string>())).Returns(()=> cached);
            cache.Setup(c=>c.SetValue<IDictionary<string,int>>(It.IsAny<string>(), It.IsAny<IDictionary<string,int>>()))
                .Callback<string, IDictionary<string,int>>((k,v)=> cached=v);

            dict.Add("a",1);
            Assert.That(dict["a"], Is.EqualTo(1));
            Assert.That(cached, Is.Not.Null);
            Assert.That(cached.ContainsKey("a"), Is.True);
        }

        [Test]
        public void Cacheable_ReadAfterReadOnlyWrite_ReadsFromCachedSnapshot()
        {
            Mock<ICache> cache;
            var dict = CreateCacheable(out cache, true);
            IDictionary<string,int> cached = null;
            cache.Setup(c=>c.GetValue<IDictionary<string,int>>(It.IsAny<string>())).Returns(()=> cached);
            cache.Setup(c=>c.SetValue<IDictionary<string,int>>(It.IsAny<string>(), It.IsAny<IDictionary<string,int>>()))
                .Callback<string, IDictionary<string,int>>((k,v)=> cached=v);

            dict.Add("a",1); // creates cache
            cached["b"] = 2; // mutate cache directly
            Assert.That(dict.ContainsKey("b"), Is.True);
        }

        [Test]
        public void Cacheable_Indexer_UpdateExisting_InReadOnlyMode_UpdatesCacheNotThrow()
        {
            Mock<ICache> cache;
            var dict = CreateCacheable(out cache, true);
            IDictionary<string,int> cached = null;
            cache.Setup(c=>c.GetValue<IDictionary<string,int>>(It.IsAny<string>())).Returns(()=> cached);
            cache.Setup(c=>c.SetValue<IDictionary<string,int>>(It.IsAny<string>(), It.IsAny<IDictionary<string,int>>()))
                .Callback<string, IDictionary<string,int>>((k,v)=> cached=v);

            dict["a"] = 1; // add
            dict["a"] = 2; // update existing value
            Assert.That(dict["a"], Is.EqualTo(2));
        }

        [Test]
        public void Cacheable_Clear_InReadOnlyMode_ClearsCacheCopy()
        {
            Mock<ICache> cache;
            var dict = CreateCacheable(out cache, true);
            IDictionary<string,int> cached = null;
            cache.Setup(c=>c.GetValue<IDictionary<string,int>>(It.IsAny<string>())).Returns(()=> cached);
            cache.Setup(c=>c.SetValue<IDictionary<string,int>>(It.IsAny<string>(), It.IsAny<IDictionary<string,int>>()))
                .Callback<string, IDictionary<string,int>>((k,v)=> cached=v);

            dict.Add("a",1);
            dict.Add("b",2);
            dict.Clear();
            Assert.That(dict.Count, Is.EqualTo(0));
            Assert.That(cached.Count, Is.EqualTo(0));
        }

        [Test]
        public void Cacheable_Remove_InReadOnlyMode_RemovesFromCacheCopy()
        {
            Mock<ICache> cache;
            var dict = CreateCacheable(out cache, true);
            IDictionary<string,int> cached = null;
            cache.Setup(c=>c.GetValue<IDictionary<string,int>>(It.IsAny<string>())).Returns(()=> cached);
            cache.Setup(c=>c.SetValue<IDictionary<string,int>>(It.IsAny<string>(), It.IsAny<IDictionary<string,int>>()))
                .Callback<string, IDictionary<string,int>>((k,v)=> cached=v);

            dict.Add("a",1);
            dict.Add("b",2);
            var removed = dict.Remove("a");
            Assert.That(removed, Is.True);
            Assert.That(dict.ContainsKey("a"), Is.False);
            Assert.That(cached.ContainsKey("a"), Is.False);
        }

        [Test]
        public void Cacheable_AddRange_InReadOnlyMode_AddsAllToCacheCopy()
        {
            Mock<ICache> cache;
            var dict = CreateCacheable(out cache, true);
            IDictionary<string,int> cached = null;
            cache.Setup(c=>c.GetValue<IDictionary<string,int>>(It.IsAny<string>())).Returns(()=> cached);
            cache.Setup(c=>c.SetValue<IDictionary<string,int>>(It.IsAny<string>(), It.IsAny<IDictionary<string,int>>()))
                .Callback<string, IDictionary<string,int>>((k,v)=> cached=v);

            dict.AddRange(new Dictionary<string,int>{{"a",1},{"b",2}});
            Assert.That(dict.Count, Is.EqualTo(2));
            Assert.That(dict.ContainsKey("a"), Is.True);
            Assert.That(cached.Count, Is.EqualTo(2));
        }

        [Test]
        public void Cacheable_CopyTo_UsesReadOperationDictionary()
        {
            Mock<ICache> cache;
            var dict = CreateCacheable(out cache, true);
            IDictionary<string,int> cached = null;
            cache.Setup(c=>c.GetValue<IDictionary<string,int>>(It.IsAny<string>())).Returns(()=> cached);
            cache.Setup(c=>c.SetValue<IDictionary<string,int>>(It.IsAny<string>(), It.IsAny<IDictionary<string,int>>()))
                .Callback<string, IDictionary<string,int>>((k,v)=> cached=v);

            dict.Add("a",1);
            var array = new KeyValuePair<string,int>[1];
            dict.CopyTo(array,0);
            Assert.That(array[0].Key, Is.EqualTo("a"));
        }

        #endregion
    }
}

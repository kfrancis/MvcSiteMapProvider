using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using MvcSiteMapProvider.Collections;

namespace MvcSiteMapProvider.Tests.Unit.Collections
{
    [TestFixture]
    public class ThreadSafeDictionaryTest
    {
        private ThreadSafeDictionary<string, int> Create()
        {
            return new ThreadSafeDictionary<string, int>();
        }

        [Test]
        public void Add_NewKey_IncrementsCount()
        {
            var dict = Create();
            dict.Add("a", 1);
            Assert.That(dict.Count, Is.EqualTo(1));
            Assert.That(dict["a"], Is.EqualTo(1));
        }

        [Test]
        public void Add_DuplicateKey_IsIgnored()
        {
            var dict = Create();
            dict.Add("a", 1);
            dict.Add("a", 2); // ignored
            Assert.That(dict.Count, Is.EqualTo(1));
            Assert.That(dict["a"], Is.EqualTo(1));
        }

        [Test]
        public void Add_KeyValuePair_DuplicateIgnored()
        {
            var dict = Create();
            dict.Add(new KeyValuePair<string,int>("a", 1));
            dict.Add(new KeyValuePair<string,int>("a", 2));
            Assert.That(dict.Count, Is.EqualTo(1));
            Assert.That(dict["a"], Is.EqualTo(1));
        }

        [Test]
        public void Indexer_Set_UpdatesExisting()
        {
            var dict = Create();
            dict.Add("a", 1);
            dict["a"] = 5; // update
            Assert.That(dict["a"], Is.EqualTo(5));
            Assert.That(dict.Count, Is.EqualTo(1));
        }

        [Test]
        public void Indexer_Set_AddsNew()
        {
            var dict = Create();
            dict["a"] = 10; // add via indexer
            Assert.That(dict.Count, Is.EqualTo(1));
            Assert.That(dict["a"], Is.EqualTo(10));
        }

        [Test]
        public void RemoveSafe_RemovesIfExists_DoesNothingIfNot()
        {
            var dict = Create();
            dict.Add("a", 1);
            dict.RemoveSafe("a");
            Assert.That(dict.ContainsKey("a"), Is.False);
            dict.RemoveSafe("missing"); // should not throw
            Assert.That(dict.Count, Is.EqualTo(0));
        }

        [Test]
        public void MergeSafe_Upserts()
        {
            var dict = Create();
            dict.MergeSafe("a", 1); // add
            Assert.That(dict["a"], Is.EqualTo(1));
            dict.MergeSafe("a", 2); // replace
            Assert.That(dict["a"], Is.EqualTo(2));
            Assert.That(dict.Count, Is.EqualTo(1));
        }

        [Test]
        public void Remove_ByKey_RemovesAndReturnsTrue()
        {
            var dict = Create();
            dict.Add("a", 1);
            var removed = dict.Remove("a");
            Assert.That(removed, Is.True);
            Assert.That(dict.ContainsKey("a"), Is.False);
        }

        [Test]
        public void Remove_ByKey_ReturnsFalseIfMissing()
        {
            var dict = Create();
            var removed = dict.Remove("a");
            Assert.That(removed, Is.False);
        }

        [Test]
        public void Remove_KeyValuePair_RequiresMatchingValue()
        {
            var dict = Create();
            dict.Add("a", 1);
            var removedWrongValue = dict.Remove(new KeyValuePair<string,int>("a", 2));
            Assert.That(removedWrongValue, Is.False);
            var removed = dict.Remove(new KeyValuePair<string,int>("a", 1));
            Assert.That(removed, Is.True);
            Assert.That(dict.ContainsKey("a"), Is.False);
        }

        [Test]
        public void ContainsKey_And_TryGetValue_Work()
        {
            var dict = Create();
            dict.Add("a", 1);
            int value;
            Assert.That(dict.ContainsKey("a"), Is.True);
            Assert.That(dict.TryGetValue("a", out value), Is.True);
            Assert.That(value, Is.EqualTo(1));
            Assert.That(dict.TryGetValue("b", out value), Is.False);
        }

        [Test]
        public void Keys_ReturnsSnapshot()
        {
            var dict = Create();
            dict.Add("a", 1);
            dict.Add("b", 2);
            var keys = dict.Keys; // snapshot
            dict.Add("c", 3);
            Assert.That(keys, Is.Not.EqualTo(dict.Keys));
            Assert.That(keys.Count, Is.EqualTo(2));
            Assert.That(dict.Keys.Count, Is.EqualTo(3));
        }

        [Test]
        public void Values_ReturnsSnapshot()
        {
            var dict = Create();
            dict.Add("a", 1);
            dict.Add("b", 2);
            var values = dict.Values;
            dict.Add("c", 3);
            Assert.That(values.Count, Is.EqualTo(2));
            Assert.That(dict.Values.Count, Is.EqualTo(3));
        }

        [Test]
        public void Clear_EmptiesDictionary()
        {
            var dict = Create();
            dict.Add("a", 1);
            dict.Add("b", 2);
            dict.Clear();
            Assert.That(dict.Count, Is.EqualTo(0));
            Assert.That(dict.ContainsKey("a"), Is.False);
        }

        [Test]
        public void IsReadOnly_IsFalse()
        {
            var dict = Create();
            Assert.That(dict.IsReadOnly, Is.False);
        }

        [Test]
        public void GetEnumerator_ThrowsNotSupported()
        {
            var dict = Create();
            Assert.Throws<NotSupportedException>(() => { var _ = dict.GetEnumerator(); });
            Assert.Throws<NotSupportedException>(() => { foreach (var kv in (IEnumerable)dict) { } });
        }

        [Test]
        public void Concurrency_AddsConsistent()
        {
            var dict = Create();
            var tasks = new List<Task>();
            int taskCount = 8;
            int itemsPerTask = 250;
            for (int t = 0; t < taskCount; t++)
            {
                int taskId = t;
                tasks.Add(Task.Run(() =>
                {
                    for (int i = 0; i < itemsPerTask; i++)
                    {
                        // half via Add, half via indexer
                        string key = "k_" + (taskId * itemsPerTask + i).ToString();
                        if (i % 2 == 0)
                            dict.Add(key, i);
                        else
                            dict[key] = i;

                        // attempt duplicate adds
                        dict.Add(key, i + 1); // should be ignored
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());
            Assert.That(dict.Count, Is.EqualTo(taskCount * itemsPerTask));
        }

        [Test]
        public void MergeSafe_Concurrency_UpsertsWithoutDuplicateGrowth()
        {
            var dict = Create();
            dict.Add("shared", 0);
            var tasks = Enumerable.Range(0, 10).Select(i => Task.Run(() =>
            {
                for (int c = 0; c < 100; c++)
                {
                    dict.MergeSafe("shared", c);
                }
            })).ToArray();
            Task.WaitAll(tasks);
            Assert.That(dict.Count, Is.EqualTo(1));
            int final;
            dict.TryGetValue("shared", out final);
            Assert.That(final, Is.GreaterThanOrEqualTo(0));
        }
    }
}

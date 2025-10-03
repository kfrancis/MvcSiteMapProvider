using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using MvcSiteMapProvider.Collections;
using NUnit.Framework;

namespace MvcSiteMapProvider.Tests.Unit.Collections
{
    [TestFixture]
    public class ObservableDictionaryTest
    {
        private ObservableDictionary<string, int> Create()
        {
            // Use public ctor that accepts an existing dictionary to avoid protected default constructor
            return new ObservableDictionary<string, int>(new Dictionary<string, int>());
        }

        private sealed class EventCapture
        {
            public readonly List<NotifyCollectionChangedEventArgs> CollectionEvents =
                new List<NotifyCollectionChangedEventArgs>();

            public readonly List<string> PropertyEvents = new List<string>();

            public void Attach(ObservableDictionary<string, int> target)
            {
                target.PropertyChanged += OnPropertyChanged;
                target.CollectionChanged += OnCollectionChanged;
            }

            private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                PropertyEvents.Add(e.PropertyName);
            }

            private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                CollectionEvents.Add(e);
            }

            public void Clear()
            {
                PropertyEvents.Clear();
                CollectionEvents.Clear();
            }

            public void AssertSinglePropertyBatch()
            {
                var expected = new[] { "Count", "Item[]", "Keys", "Values" };
                Assert.That(PropertyEvents, Is.EqualTo(expected), "PropertyChanged sequence incorrect");
            }
        }

        [Test]
        public void Add_NewKey_RaisesAddEvents()
        {
            var dict = Create();
            var events = new EventCapture();
            events.Attach(dict);

            dict.Add("a", 1);

            Assert.That(dict.Count, Is.EqualTo(1));
            Assert.That(dict["a"], Is.EqualTo(1));

            events.AssertSinglePropertyBatch();
            Assert.That(events.CollectionEvents.Count, Is.EqualTo(1));
            var ev = events.CollectionEvents.Single();
            Assert.That(ev.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(ev.NewItems.Count, Is.EqualTo(1));
            var added = (KeyValuePair<string, int>)ev.NewItems[0];
            Assert.That(added.Key, Is.EqualTo("a"));
            Assert.That(added.Value, Is.EqualTo(1));
        }

        [Test]
        public void Add_NullKey_ThrowsArgumentNullException()
        {
            var dict = Create();
            Assert.Throws<ArgumentNullException>(() => dict.Add(null, 1));
        }

        [Test]
        public void Add_DuplicateKey_ThrowsArgumentException_NoEvents()
        {
            var dict = Create();
            var events = new EventCapture();
            events.Attach(dict);

            dict.Add("a", 1);
            events.Clear();

            Assert.Throws<ArgumentException>(() => dict.Add("a", 2));
            Assert.That(dict["a"], Is.EqualTo(1));
            Assert.That(events.PropertyEvents, Is.Empty);
            Assert.That(events.CollectionEvents, Is.Empty);
        }

        [Test]
        public void Indexer_AddNewKey_RaisesAddEvents()
        {
            var dict = Create();
            var events = new EventCapture();
            events.Attach(dict);

            dict["a"] = 1;

            events.AssertSinglePropertyBatch();
            var ev = events.CollectionEvents.Single();
            Assert.That(ev.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            var added = (KeyValuePair<string, int>)ev.NewItems[0];
            Assert.That(added.Key, Is.EqualTo("a"));
            Assert.That(added.Value, Is.EqualTo(1));
        }

        [Test]
        public void Indexer_UpdateExistingDifferentValue_RaisesReplaceEvents()
        {
            var dict = Create();
            var events = new EventCapture();
            events.Attach(dict);
            dict.Add("a", 1);
            events.Clear();

            dict["a"] = 2;

            events.AssertSinglePropertyBatch();
            var ev = events.CollectionEvents.Single();
            Assert.That(ev.Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));
            var newItem = (KeyValuePair<string, int>)ev.NewItems[0];
            var oldItem = (KeyValuePair<string, int>)ev.OldItems[0];
            Assert.That(newItem.Value, Is.EqualTo(2));
            Assert.That(oldItem.Value, Is.EqualTo(1));
        }

        [Test]
        public void Indexer_UpdateExistingSameValue_NoEvents()
        {
            var dict = Create();
            var events = new EventCapture();
            events.Attach(dict);
            dict.Add("a", 1);
            events.Clear();

            dict["a"] = 1; // same value

            Assert.That(events.PropertyEvents, Is.Empty);
            Assert.That(events.CollectionEvents, Is.Empty);
        }

        [Test]
        public void Indexer_NullKey_ThrowsArgumentNullException()
        {
            var dict = Create();
            Assert.Throws<ArgumentNullException>(() => dict[null] = 1);
        }

        [Test]
        public void Remove_ExistingKey_RaisesReset()
        {
            var dict = Create();
            var events = new EventCapture();
            events.Attach(dict);
            dict.Add("a", 1);
            events.Clear();

            var removed = dict.Remove("a");

            Assert.That(removed, Is.True);
            Assert.That(dict.Count, Is.EqualTo(0));
            events.AssertSinglePropertyBatch();
            var ev = events.CollectionEvents.Single();
            Assert.That(ev.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
        }

        [Test]
        public void Remove_MissingKey_NoEvents()
        {
            var dict = Create();
            var events = new EventCapture();
            events.Attach(dict);

            var removed = dict.Remove("missing");

            Assert.That(removed, Is.False);
            Assert.That(events.PropertyEvents, Is.Empty);
            Assert.That(events.CollectionEvents, Is.Empty);
        }

        [Test]
        public void Remove_NullKey_ThrowsArgumentNullException()
        {
            var dict = Create();
            Assert.Throws<ArgumentNullException>(() => dict.Remove(null));
        }

        [Test]
        public void Remove_KeyValuePair_DelegatesToKey()
        {
            var dict = Create();
            var events = new EventCapture();
            events.Attach(dict);
            dict.Add("a", 1);
            events.Clear();

            var removed = dict.Remove(new KeyValuePair<string, int>("a", 999));
            Assert.That(removed, Is.True);
            events.AssertSinglePropertyBatch();
            var ev = events.CollectionEvents.Single();
            Assert.That(ev.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
        }

        [Test]
        public void Clear_NonEmpty_RaisesReset()
        {
            var dict = Create();
            var events = new EventCapture();
            events.Attach(dict);
            dict.Add("a", 1);
            dict.Add("b", 2);
            events.Clear();

            dict.Clear();

            Assert.That(dict.Count, Is.EqualTo(0));
            events.AssertSinglePropertyBatch();
            var ev = events.CollectionEvents.Single();
            Assert.That(ev.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
        }

        [Test]
        public void Clear_Empty_NoEvents()
        {
            var dict = Create();
            var events = new EventCapture();
            events.Attach(dict);

            dict.Clear();

            Assert.That(events.PropertyEvents, Is.Empty);
            Assert.That(events.CollectionEvents, Is.Empty);
        }

        [Test]
        public void AddRange_Null_ThrowsArgumentNullException()
        {
            var dict = Create();
            Assert.Throws<ArgumentNullException>(() => dict.AddRange(null));
        }

        [Test]
        public void AddRange_Empty_DoesNothing_NoEvents()
        {
            var dict = Create();
            var events = new EventCapture();
            events.Attach(dict);
            dict.AddRange(new Dictionary<string, int>());
            Assert.That(events.PropertyEvents, Is.Empty);
            Assert.That(events.CollectionEvents, Is.Empty);
        }

        [Test]
        public void AddRange_IntoEmpty_AddsAll_RaisesAddOnce()
        {
            var dict = Create();
            var events = new EventCapture();
            events.Attach(dict);
            var items = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };

            dict.AddRange(items);

            Assert.That(dict.Count, Is.EqualTo(3));
            Assert.That(dict.Keys, Is.EquivalentTo(items.Keys));
            events.AssertSinglePropertyBatch();
            var ev = events.CollectionEvents.Single();
            Assert.That(ev.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(ev.NewItems.Count, Is.EqualTo(3));
        }

        [Test]
        public void AddRange_IntoNonEmpty_NoDuplicates_RaisesAddOnce()
        {
            var dict = Create();
            dict.Add("a", 1);
            var events = new EventCapture();
            events.Attach(dict);
            events.Clear();

            dict.AddRange(new Dictionary<string, int> { { "b", 2 }, { "c", 3 } });

            Assert.That(dict.Count, Is.EqualTo(3));
            events.AssertSinglePropertyBatch();
            var ev = events.CollectionEvents.Single();
            Assert.That(ev.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(ev.NewItems.Count, Is.EqualTo(2));
        }

        [Test]
        public void AddRange_IntoNonEmpty_WithDuplicate_Throws_NoChange_NoEvents()
        {
            var dict = Create();
            dict.Add("a", 1);
            var events = new EventCapture();
            events.Attach(dict);
            events.Clear();

            var exItems = new Dictionary<string, int> { { "a", 9 }, { "b", 2 } }; // duplicate key a
            Assert.Throws<ArgumentException>(() => dict.AddRange(exItems));
            Assert.That(dict.Count, Is.EqualTo(1));
            Assert.That(dict.ContainsKey("b"), Is.False);
            Assert.That(events.PropertyEvents, Is.Empty);
            Assert.That(events.CollectionEvents, Is.Empty);
        }

        [Test]
        public void TryGetValue_ReturnsExpected()
        {
            var dict = Create();
            dict.Add("a", 1);
            var found = dict.TryGetValue("a", out var value);
            Assert.That(found, Is.True);
            Assert.That(value, Is.EqualTo(1));
            var notFound = dict.TryGetValue("b", out value);
            Assert.That(notFound, Is.False);
        }

        [Test]
        public void IsReadOnly_IsFalse()
        {
            var dict = Create();
            Assert.That(dict.IsReadOnly, Is.False);
        }
    }
}

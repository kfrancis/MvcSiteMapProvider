using System;
using System.Collections.Generic;

namespace MvcSiteMapProvider.Collections
{
    /// <summary>
    /// Generic list that is aware of the ISiteMap interface and can be made read-only
    /// depending on the IsReadOnly property of ISiteMap.
    /// </summary>
    public class LockableList<T>
        : List<T>
    {
        public LockableList(ISiteMap siteMap)
        {
            this.siteMap = siteMap ?? throw new ArgumentNullException(nameof(siteMap));
        }

        protected readonly ISiteMap siteMap;

        /// <summary>
        /// Adds an object to the end of the <see cref="T:LockableList"/>
        /// </summary>
        /// <param name="item">The item to add to the list.</param>
        public virtual new void Add(T item)
        {
            ThrowIfReadOnly();
            base.Add(item);
        }

        public virtual new void AddRange(IEnumerable<T> collection)
        {
            ThrowIfReadOnly();
            base.AddRange(collection);
        }

        public virtual new void Clear()
        {
            ThrowIfReadOnly();
            base.Clear();
        }

        public virtual new void Insert(int index, T item)
        {
            ThrowIfReadOnly();
            base.Insert(index, item);
        }

        public virtual new void InsertRange(int index, IEnumerable<T> collection)
        {
            ThrowIfReadOnly();
            base.InsertRange(index, collection);
        }

        public virtual bool IsReadOnly
        {
            get { return siteMap.IsReadOnly; }
        }

        public virtual new bool Remove(T item)
        {
            ThrowIfReadOnly();
            return base.Remove(item);
        }

        public virtual new int RemoveAll(Predicate<T> match)
        {
            ThrowIfReadOnly();
            return base.RemoveAll(match);
        }

        public virtual new void RemoveAt(int index)
        {
            ThrowIfReadOnly();
            base.RemoveAt(index);
        }

        public virtual new void RemoveRange(int index, int count)
        {
            ThrowIfReadOnly();
            base.RemoveRange(index, count);
        }

        public virtual new void Reverse()
        {
            ThrowIfReadOnly();
            base.Reverse();
        }

        public virtual new void Reverse(int index, int count)
        {
            ThrowIfReadOnly();
            base.Reverse(index, count);
        }

        public virtual new void Sort()
        {
            ThrowIfReadOnly();
            base.Sort();
        }

        public virtual new void Sort(Comparison<T> comparison)
        {
            ThrowIfReadOnly();
            base.Sort(comparison);
        }

        public virtual new void Sort(IComparer<T> comparer)
        {
            ThrowIfReadOnly();
            base.Sort(comparer);
        }

        public virtual new void Sort(int index, int count, IComparer<T> comparer)
        {
            ThrowIfReadOnly();
            base.Sort(index, count, comparer);
        }

        public virtual new void TrimExcess()
        {
            ThrowIfReadOnly();
            base.TrimExcess();
        }

        // Property access to internal list
        protected LockableList<T> List
        {
            get { return this; }
        }

        public virtual void CopyTo(IList<T> destination)
        {
            foreach (var item in List)
            {
                if (!item.GetType().IsPointer)
                {
                    destination.Add(item);
                }
                else
                {
                    throw new NotSupportedException(Resources.Messages.CopyOperationDoesNotSupportReferenceTypes);
                }
            }
        }

        protected virtual void ThrowIfReadOnly()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException(string.Format(Resources.Messages.SiteMapReadOnly));
            }
        }
    }
}
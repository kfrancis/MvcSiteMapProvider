using System;
using System.Collections;
using System.Collections.Generic;

namespace MvcSiteMapProvider.Collections.Specialized
{
    /// <summary>
    /// A specialized collection that provides a read-only wrapper for a <see cref="T:MvcSiteMapProvider.ISiteMapNodeCollection"/>.
    /// </summary>
    public class ReadOnlySiteMapNodeCollection
            : ISiteMapNodeCollection
    {
        private readonly ISiteMapNodeCollection siteMapNodeCollection;

        public ReadOnlySiteMapNodeCollection(
                    ISiteMapNodeCollection siteMapNodeCollection
            )
        {
            this.siteMapNodeCollection = siteMapNodeCollection ?? throw new ArgumentNullException(nameof(siteMapNodeCollection));
        }

        public int Count
        {
            get { return siteMapNodeCollection.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public ISiteMapNode this[int index]
        {
            get
            {
                return siteMapNodeCollection[index];
            }
            set
            {
                throw new NotSupportedException(Resources.Messages.CollectionReadOnly);
            }
        }

        public void Add(ISiteMapNode item)
        {
            throw new NotSupportedException(Resources.Messages.CollectionReadOnly);
        }

        public void AddRange(IEnumerable<ISiteMapNode> collection)
        {
            throw new NotSupportedException(Resources.Messages.CollectionReadOnly);
        }

        public void Clear()
        {
            throw new NotSupportedException(Resources.Messages.CollectionReadOnly);
        }

        public bool Contains(ISiteMapNode item)
        {
            return siteMapNodeCollection.Contains(item);
        }

        public void CopyTo(ISiteMapNode[] array, int arrayIndex)
        {
            siteMapNodeCollection.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ISiteMapNode> GetEnumerator()
        {
            return siteMapNodeCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return siteMapNodeCollection.GetEnumerator();
        }

        public int IndexOf(ISiteMapNode item)
        {
            return siteMapNodeCollection.IndexOf(item);
        }

        public void Insert(int index, ISiteMapNode item)
        {
            throw new NotSupportedException(Resources.Messages.CollectionReadOnly);
        }

        public bool Remove(ISiteMapNode item)
        {
            throw new NotSupportedException(Resources.Messages.CollectionReadOnly);
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException(Resources.Messages.CollectionReadOnly);
        }

        public void RemoveRange(int index, int count)
        {
            throw new NotSupportedException(Resources.Messages.CollectionReadOnly);
        }
    }
}
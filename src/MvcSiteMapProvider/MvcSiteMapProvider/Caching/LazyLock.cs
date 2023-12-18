using System;

namespace MvcSiteMapProvider.Caching
{
    /// <summary>
    /// A lightweight lazy lock container for managing cache item storage and retrieval.
    /// </summary>
    /// <remarks>
    /// Caching strategy inspired by this post:
    /// http://www.superstarcoders.com/blogs/posts/micro-caching-in-asp-net.aspx
    /// </remarks>
    public sealed class LazyLock
    {
        private volatile bool got;
        private object value;
        private readonly object _lockObject = new object();

        public TValue Get<TValue>(Func<TValue> activator)
        {
            if (!got)
            {
                if (activator == null)
                {
                    return default;
                }

                lock (_lockObject)
                {
                    if (!got)
                    {
                        value = activator();

                        got = true;
                    }
                }
            }

            return (TValue)value;
        }
    }
}
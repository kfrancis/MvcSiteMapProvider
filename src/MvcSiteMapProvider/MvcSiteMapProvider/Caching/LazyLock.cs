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
        private volatile bool _got;
        private object _value;
        private readonly object _lockObject = new object();

        public TValue Get<TValue>(Func<TValue> activator)
        {
            if (!_got)
            {
                if (activator == null)
                {
                    return default;
                }

                lock (_lockObject)
                {
                    if (!_got)
                    {
                        _value = activator();

                        _got = true;
                    }
                }
            }

            return (TValue)_value;
        }
    }
}

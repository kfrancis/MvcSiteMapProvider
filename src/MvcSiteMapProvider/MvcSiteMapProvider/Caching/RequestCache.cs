using MvcSiteMapProvider.Web.Mvc;
using System;
using System.Web;

namespace MvcSiteMapProvider.Caching
{
    /// <summary>
    /// Provides type-safe access to <see cref="P:System.Web.HttpContext.Items"/>.
    /// </summary>
    public class RequestCache
        : IRequestCache
    {
        public RequestCache(
            IMvcContextFactory mvcContextFactory
            )
        {
            _mvcContextFactory = mvcContextFactory ?? throw new ArgumentNullException(nameof(mvcContextFactory));
        }

        private readonly IMvcContextFactory _mvcContextFactory;

        protected HttpContextBase Context
        {
            get
            {
                return _mvcContextFactory.CreateHttpContext();
            }
        }

        public virtual T GetValue<T>(string key)
        {
            return Context.Items.Contains(key) ? (T)Context.Items[key] : default;
        }

        public virtual void SetValue<T>(string key, T value)
        {
            Context.Items[key] = value;
        }
    }
}

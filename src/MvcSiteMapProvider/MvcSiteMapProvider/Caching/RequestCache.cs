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
            this.mvcContextFactory = mvcContextFactory ?? throw new ArgumentNullException(nameof(mvcContextFactory));
        }

        private readonly IMvcContextFactory mvcContextFactory;

        protected HttpContextBase Context
        {
            get
            {
                return mvcContextFactory.CreateHttpContext();
            }
        }

        public virtual T GetValue<T>(string key)
        {
            if (Context.Items.Contains(key))
            {
                return (T)Context.Items[key];
            }
            return default;
        }

        public virtual void SetValue<T>(string key, T value)
        {
            Context.Items[key] = value;
        }
    }
}
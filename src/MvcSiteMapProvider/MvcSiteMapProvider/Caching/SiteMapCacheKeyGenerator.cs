using MvcSiteMapProvider.Web.Mvc;
using System;
using System.Text;

namespace MvcSiteMapProvider.Caching
{
    /// <summary>
    /// The default cache key generator. This class generates a unique cache key for each
    /// DnsSafeHost.
    /// </summary>
    public class SiteMapCacheKeyGenerator
        : ISiteMapCacheKeyGenerator
    {
        protected readonly IMvcContextFactory MvcContextFactory;

        public SiteMapCacheKeyGenerator(
                    IMvcContextFactory mvcContextFactory
            )
        {
            MvcContextFactory = mvcContextFactory ?? throw new ArgumentNullException(nameof(mvcContextFactory));
        }

        public virtual string GenerateKey()
        {
            var builder = new StringBuilder();
            builder.Append("sitemap://");
            builder.Append(GetHostName());
            builder.Append("/");

            return builder.ToString();
        }

        protected virtual string GetHostName()
        {
            var context = MvcContextFactory.CreateHttpContext();
            var request = context.Request;

            // In a cloud or web farm environment, use the HTTP_HOST
            // header to derive the host name.
            return request.ServerVariables["HTTP_HOST"] ?? request.Url.DnsSafeHost;
        }
    }
}

using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.Web;
using MvcSiteMapProvider.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace MvcSiteMapProvider
{
    /// <summary>
    /// Provides overrides of the <see cref="T:MvcSiteMapProvider.SiteMap"/> that track the return values of specific
    /// resource-intensive members in case they are accessed more than one time during a single request.
    /// </summary>
    public class RequestCacheableSiteMap
        : LockableSiteMap
    {
        public RequestCacheableSiteMap(
            ISiteMapPluginProvider pluginProvider,
            IMvcContextFactory mvcContextFactory,
            ISiteMapChildStateFactory siteMapChildStateFactory,
            IUrlPath urlPath,
            ISiteMapSettings siteMapSettings,
            IRequestCache requestCache
            )
            : base(pluginProvider, mvcContextFactory, siteMapChildStateFactory, urlPath, siteMapSettings)
        {
            this.requestCache = requestCache ?? throw new ArgumentNullException(nameof(requestCache));
        }

        private readonly IRequestCache requestCache;

        #region Request Cacheable Members

        public override ISiteMapNode FindSiteMapNode(string rawUrl)
        {
            var key = GetCacheKey("FindSiteMapNode_" + rawUrl);
            var result = requestCache.GetValue<ISiteMapNode>(key);
            if (result == null)
            {
                result = base.FindSiteMapNode(rawUrl);
                if (result != null)
                {
                    requestCache.SetValue<ISiteMapNode>(key, result);
                }
            }
            return result;
        }

        public override ISiteMapNode FindSiteMapNodeFromCurrentContext()
        {
            var key = GetCacheKey("FindSiteMapNodeFromCurrentContext");
            var result = requestCache.GetValue<ISiteMapNode>(key);
            if (result == null)
            {
                result = base.FindSiteMapNodeFromCurrentContext();
                if (result != null)
                {
                    requestCache.SetValue<ISiteMapNode>(key, result);
                }
            }
            return result;
        }

        public override ISiteMapNode FindSiteMapNode(ControllerContext context)
        {
            var key = GetCacheKey("FindSiteMapNode_ControllerContext" + GetDictionaryKey(context.RouteData.Values));
            var result = requestCache.GetValue<ISiteMapNode>(key);
            if (result == null)
            {
                result = base.FindSiteMapNode(context);
                if (result != null)
                {
                    requestCache.SetValue<ISiteMapNode>(key, result);
                }
            }
            return result;
        }

        public override bool IsAccessibleToUser(ISiteMapNode node)
        {
            var key = GetCacheKey("IsAccessibleToUser_" + node.Key);
            var result = requestCache.GetValue<bool?>(key);
            if (result == null)
            {
                // Fix for #272 - Change the context of the URL cache to ensure
                // that the AclModule doesn't prevent manually setting route values
                // from having any effect on the URL. This setting takes effect in
                // the RequestCacheableSiteMapNode.Url property.
                var urlContextKey = this.GetUrlContextKey();
                requestCache.SetValue<string>(urlContextKey, "AclModule");
                result = base.IsAccessibleToUser(node);
                requestCache.SetValue<bool>(key, (bool)result);

                // Restore the URL context.
                requestCache.SetValue<string>(urlContextKey, string.Empty);
            }
            return (bool)result;
        }

        #endregion Request Cacheable Members

        #region Protected Members

        protected virtual string GetCacheKey(string memberName)
        {
            // NOTE: We must include IsReadOnly in the request cache key because we may have a different
            // result when the sitemap is being constructed than when it is being read by the presentation layer.
            return "__MVCSITEMAP_" + CacheKey + "_" + memberName + "_" + IsReadOnly.ToString() + "_";
        }

        protected virtual string GetDictionaryKey(IDictionary<string, object> dictionary)
        {
            var builder = new StringBuilder();
            foreach (var pair in dictionary)
            {
                builder.Append(pair.Key);
                builder.Append("_");
                builder.Append(GetStringFromValue(pair.Value));
                builder.Append("|");
            }
            return builder.ToString();
        }

        protected virtual string GetStringFromValue(object value)
        {
            if (value.GetType().Equals(typeof(string)))
            {
                return value.ToString();
            }
            return value.GetHashCode().ToString();
        }

        #endregion Protected Members
    }
}
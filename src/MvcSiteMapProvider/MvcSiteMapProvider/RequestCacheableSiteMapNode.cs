using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Web;
using MvcSiteMapProvider.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;

namespace MvcSiteMapProvider
{
    /// <summary>
    /// Provides overrides of the <see cref="T:MvcSiteMapProvider.SiteMapNode"/> that track the return values of specific
    /// resource-intensive members in case they are accessed more than one time during a single request. Also stores
    /// values set from specific read-write properties in the request cache for later retrieval.
    /// </summary>
    public class RequestCacheableSiteMapNode
        : LockableSiteMapNode
    {
        private readonly IRequestCache requestCache;

        public RequestCacheableSiteMapNode(
                    ISiteMap siteMap,
            string key,
            bool isDynamic,
            ISiteMapNodePluginProvider pluginProvider,
            IMvcContextFactory mvcContextFactory,
            ISiteMapNodeChildStateFactory siteMapNodeChildStateFactory,
            ILocalizationService localizationService,
            IUrlPath urlPath
            )
            : base(
                siteMap,
                key,
                isDynamic,
                pluginProvider,
                mvcContextFactory,
                siteMapNodeChildStateFactory,
                localizationService,
                urlPath
            )
        {
            if (mvcContextFactory == null)
                throw new ArgumentNullException(nameof(mvcContextFactory));

            requestCache = mvcContextFactory.GetRequestCache();
        }

        public override string CanonicalKey
        {
            get { return GetCachedOrMemberValue<string>(() => base.CanonicalKey, "CanonicalKey", false); }
            set { SetCachedOrMemberValue<string>(x => base.CanonicalKey = x, "CanonicalKey", value); }
        }

        public override string CanonicalUrl
        {
            get { return GetCachedOrMemberValue<string>(() => base.CanonicalUrl, "CanonicalUrl", false); }
            set { SetCachedOrMemberValue<string>(x => base.CanonicalUrl = x, "CanonicalUrl", value); }
        }

        public override string CanonicalUrlHostName
        {
            get { return GetCachedOrMemberValue<string>(() => base.CanonicalUrlHostName, "CanonicalUrlHostName", false); }
            set { SetCachedOrMemberValue<string>(x => base.CanonicalUrlHostName = x, "CanonicalUrlHostName", value); }
        }

        public override string CanonicalUrlProtocol
        {
            get { return GetCachedOrMemberValue<string>(() => base.CanonicalUrlProtocol, "CanonicalUrlProtocol", false); }
            set { SetCachedOrMemberValue<string>(x => base.CanonicalUrlProtocol = x, "CanonicalUrlProtocol", value); }
        }

        public override bool Clickable
        {
            get { return (bool)GetCachedOrMemberValue<bool?>(() => base.Clickable, "Clickable", false); }
            set { SetCachedOrMemberValue<bool>(x => base.Clickable = x, "Clickable", value); }
        }

        public override string Description
        {
            get { return GetCachedOrMemberValue<string>(() => base.Description, "Description", true); }
            set { SetCachedOrMemberValue<string>(x => base.Description = x, "Description", value); }
        }

        public override string HostName
        {
            get { return GetCachedOrMemberValue<string>(() => base.HostName, "HostName", false); }
            set { SetCachedOrMemberValue<string>(x => base.HostName = x, "HostName", value); }
        }

        public override string ImageUrl
        {
            get { return GetCachedOrMemberValue<string>(() => base.ImageUrl, "ImageUrl", false); }
            set { SetCachedOrMemberValue<string>(x => base.ImageUrl = x, "ImageUrl", value); }
        }

        public override string ImageUrlHostName
        {
            get { return GetCachedOrMemberValue<string>(() => base.ImageUrlHostName, "ImageUrlHostName", false); }
            set { SetCachedOrMemberValue<string>(x => base.ImageUrlHostName = x, "ImageUrlHostName", value); }
        }

        public override string ImageUrlProtocol
        {
            get { return GetCachedOrMemberValue<string>(() => base.ImageUrlProtocol, "ImageUrlProtocol", false); }
            set { SetCachedOrMemberValue<string>(x => base.ImageUrlProtocol = x, "ImageUrlProtocol", value); }
        }

        public override int Order
        {
            get { return (int)GetCachedOrMemberValue<int?>(() => base.Order, "Order", false); }
            set { SetCachedOrMemberValue<int>(x => base.Order = x, "Order", value); }
        }

        public override string Protocol
        {
            get { return GetCachedOrMemberValue<string>(() => base.Protocol, "Protocol", false); }
            set { SetCachedOrMemberValue<string>(x => base.Protocol = x, "Protocol", value); }
        }

        public override string Route
        {
            get { return GetCachedOrMemberValue<string>(() => base.Route, "Route", false); }
            set { SetCachedOrMemberValue<string>(x => base.Route = x, "Route", value); }
        }

        public override string TargetFrame
        {
            get { return GetCachedOrMemberValue<string>(() => base.TargetFrame, "TargetFrame", false); }
            set { SetCachedOrMemberValue<string>(x => base.TargetFrame = x, "TargetFrame", value); }
        }

        public override string Title
        {
            get { return GetCachedOrMemberValue<string>(() => base.Title, "Title", true); }
            set { SetCachedOrMemberValue<string>(x => base.Title = x, "Title", value); }
        }

        public override string Url
        {
            get
            {
                // Fix for #272 - Change the context of the URL cache to ensure
                // that the AclModule doesn't prevent manually setting route values
                // from having any effect on the URL.
                var urlContext = requestCache.GetValue<string>(SiteMap.GetUrlContextKey());
                var memberName = "Url" + (string.IsNullOrEmpty(urlContext) ? string.Empty : "_" + urlContext);

                return GetCachedOrMemberValue<string>(() => base.Url, memberName, true);
            }
            set { base.Url = value; }
        }

        public override string UrlResolver
        {
            get { return GetCachedOrMemberValue<string>(() => base.UrlResolver, "UrlResolver", false); }
            set { SetCachedOrMemberValue<string>(x => base.UrlResolver = x, "UrlResolver", value); }
        }

        public override string VisibilityProvider
        {
            get { return GetCachedOrMemberValue<string>(() => base.VisibilityProvider, "VisibilityProvider", false); }
            set { SetCachedOrMemberValue<string>(x => base.VisibilityProvider = x, "VisibilityProvider", value); }
        }

        protected override bool AreRouteParametersPreserved
        {
            get
            {
                var key = GetCacheKey("AreRouteParametersPreserved");
                var result = requestCache.GetValue<bool?>(key) ?? false;
                return (bool)result;
            }
            set
            {
                var key = GetCacheKey("AreRouteParametersPreserved");
                requestCache.SetValue<bool>(key, value);
            }
        }

        public override bool IsVisible(IDictionary<string, object> sourceMetadata)
        {
            var key = GetCacheKey("IsVisible" + GetDictionaryKey(sourceMetadata));
            var result = requestCache.GetValue<bool?>(key);
            if (result == null)
            {
                result = base.IsVisible(sourceMetadata);
                requestCache.SetValue<bool>(key, (bool)result);
            }
            return (bool)result;
        }

        protected virtual T GetCachedOrMemberValue<T>(Func<T> member, string memberName, bool storeInCache)
        {
            var key = GetCacheKey(memberName);
            var result = requestCache.GetValue<T>(key);
            if (result == null)
            {
                result = member.Invoke();
                if (storeInCache)
                {
                    requestCache.SetValue<T>(key, result);
                }
            }
            return result;
        }

        protected string GetCacheKey(string memberName)
        {
            // NOTE: We must include IsReadOnly in the request cache key because we may have a different
            // result when the sitemap is being constructed than when it is being read by the presentation layer.
            return "__MVCSITEMAPNODE_" + SiteMap.CacheKey + "_" + Key + "_" + memberName + "_" + IsReadOnly.ToString() + "_";
        }

        protected override NameValueCollection GetCaseCorrectedQueryString(HttpContextBase httpContext)
        {
            // This method is called twice per node only in the case where there are
            // preserved route parameters, so the memory trade-off is only worth it if
            // we have some configured.
            if (PreservedRouteParameters.Any())
            {
                var key = GetCacheKey("GetCaseCorrectedQueryString_" + httpContext.Request.Url.Query);
                var result = requestCache.GetValue<NameValueCollection>(key);
                if (result == null)
                {
                    result = base.GetCaseCorrectedQueryString(httpContext);
                    requestCache.SetValue<NameValueCollection>(key, result);
                }

                return result;
            }

            return base.GetCaseCorrectedQueryString(httpContext);
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

        protected virtual void SetCachedOrMemberValue<T>(Action<T> member, string memberName, T value)
        {
            if (IsReadOnly)
            {
                var key = GetCacheKey(memberName);
                requestCache.SetValue<T>(key, value);
            }
            else
            {
                member(value);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Web;
using MvcSiteMapProvider.Web.Mvc;

namespace MvcSiteMapProvider;

/// <summary>
///     Provides overrides of the <see cref="T:MvcSiteMapProvider.SiteMapNode" /> that track the return values of specific
///     resource-intensive members in case they are accessed more than one time during a single request. Also stores
///     values set from specific read-write properties in the request cache for later retrieval.
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
        {
            throw new ArgumentNullException(nameof(mvcContextFactory));
        }

        requestCache = mvcContextFactory.GetRequestCache();
    }

    public override int Order
    {
        get
        {
            var value = GetCachedOrMemberValue<int?>(() => base.Order, "Order", false);
            return value ?? 0;
        }
        set { SetCachedOrMemberValue(x => base.Order = x, "Order", value); }
    }

    public override string Title
    {
        get { return GetCachedOrMemberValue(() => base.Title, "Title", true); }
        set { SetCachedOrMemberValue(x => base.Title = x, "Title", value); }
    }

    public override string Description
    {
        get { return GetCachedOrMemberValue(() => base.Description, "Description", true); }
        set { SetCachedOrMemberValue(x => base.Description = x, "Description", value); }
    }

    public override string TargetFrame
    {
        get { return GetCachedOrMemberValue(() => base.TargetFrame, "TargetFrame", false); }
        set { SetCachedOrMemberValue(x => base.TargetFrame = x, "TargetFrame", value); }
    }

    public override string ImageUrl
    {
        get { return GetCachedOrMemberValue(() => base.ImageUrl, "ImageUrl", false); }
        set { SetCachedOrMemberValue(x => base.ImageUrl = x, "ImageUrl", value); }
    }

    public override string ImageUrlProtocol
    {
        get { return GetCachedOrMemberValue(() => base.ImageUrlProtocol, "ImageUrlProtocol", false); }
        set { SetCachedOrMemberValue(x => base.ImageUrlProtocol = x, "ImageUrlProtocol", value); }
    }

    public override string ImageUrlHostName
    {
        get { return GetCachedOrMemberValue(() => base.ImageUrlHostName, "ImageUrlHostName", false); }
        set { SetCachedOrMemberValue(x => base.ImageUrlHostName = x, "ImageUrlHostName", value); }
    }

    public override string VisibilityProvider
    {
        get { return GetCachedOrMemberValue(() => base.VisibilityProvider, "VisibilityProvider", false); }
        set { SetCachedOrMemberValue(x => base.VisibilityProvider = x, "VisibilityProvider", value); }
    }

    public override bool Clickable
    {
        get
        {
            var value = GetCachedOrMemberValue<bool?>(() => base.Clickable, "Clickable", false);
            return value ?? false;
        }
        set { SetCachedOrMemberValue(x => base.Clickable = x, "Clickable", value); }
    }

    public override string UrlResolver
    {
        get { return GetCachedOrMemberValue(() => base.UrlResolver, "UrlResolver", false); }
        set { SetCachedOrMemberValue(x => base.UrlResolver = x, "UrlResolver", value); }
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

            return GetCachedOrMemberValue(() => base.Url, memberName, true);
        }
        set => base.Url = value;
    }

    public override string Protocol
    {
        get { return GetCachedOrMemberValue(() => base.Protocol, "Protocol", false); }
        set { SetCachedOrMemberValue(x => base.Protocol = x, "Protocol", value); }
    }

    public override string HostName
    {
        get { return GetCachedOrMemberValue(() => base.HostName, "HostName", false); }
        set { SetCachedOrMemberValue(x => base.HostName = x, "HostName", value); }
    }

    public override string CanonicalKey
    {
        get { return GetCachedOrMemberValue(() => base.CanonicalKey, "CanonicalKey", false); }
        set { SetCachedOrMemberValue(x => base.CanonicalKey = x, "CanonicalKey", value); }
    }

    public override string CanonicalUrl
    {
        get { return GetCachedOrMemberValue(() => base.CanonicalUrl, "CanonicalUrl", false); }
        set { SetCachedOrMemberValue(x => base.CanonicalUrl = x, "CanonicalUrl", value); }
    }

    public override string CanonicalUrlProtocol
    {
        get
        {
            return GetCachedOrMemberValue(() => base.CanonicalUrlProtocol, "CanonicalUrlProtocol", false);
        }
        set { SetCachedOrMemberValue(x => base.CanonicalUrlProtocol = x, "CanonicalUrlProtocol", value); }
    }

    public override string CanonicalUrlHostName
    {
        get
        {
            return GetCachedOrMemberValue(() => base.CanonicalUrlHostName, "CanonicalUrlHostName", false);
        }
        set { SetCachedOrMemberValue(x => base.CanonicalUrlHostName = x, "CanonicalUrlHostName", value); }
    }

    public override string Route
    {
        get { return GetCachedOrMemberValue(() => base.Route, "Route", false); }
        set { SetCachedOrMemberValue(x => base.Route = x, "Route", value); }
    }

    protected override bool AreRouteParametersPreserved
    {
        get
        {
            var cacheKey = GetCacheKey("AreRouteParametersPreserved");

            return requestCache.GetValue<bool?>(cacheKey) ?? false;
        }
        set
        {
            var cacheKey = GetCacheKey("AreRouteParametersPreserved");
            requestCache.SetValue(cacheKey, value);
        }
    }

    public override bool IsVisible(IDictionary<string, object?> sourceMetadata)
    {
        var cacheKey = GetCacheKey("IsVisible" + GetDictionaryKey(sourceMetadata));
        var result = requestCache.GetValue<bool?>(cacheKey);
        if (result != null)
        {
            return (bool)result;
        }

        result = base.IsVisible(sourceMetadata);
        requestCache.SetValue(cacheKey, (bool)result);

        return (bool)result;
    }

    protected override NameValueCollection GetCaseCorrectedQueryString(HttpContextBase httpContext)
    {
        // This method is called twice per node only in the case where there are 
        // preserved route parameters, so the memory trade-off is only worth it if
        // we have some configured.
        if (!PreservedRouteParameters.Any())
        {
            return base.GetCaseCorrectedQueryString(httpContext);
        }

        var cacheKey = GetCacheKey("GetCaseCorrectedQueryString_" + httpContext.Request.Url?.Query);
        var result = requestCache.GetValue<NameValueCollection>(cacheKey);
        if (result != null)
        {
            return result;
        }

        result = base.GetCaseCorrectedQueryString(httpContext);
        requestCache.SetValue(cacheKey, result);

        return result;

    }

    private string GetCacheKey(string memberName)
    {
        // NOTE: We must include IsReadOnly in the request cache key because we may have a different 
        // result when the sitemap is being constructed than when it is being read by the presentation layer.
        return "__MVCSITEMAPNODE_" + SiteMap.CacheKey + "_" + Key + "_" + memberName + "_" + IsReadOnly + "_";
    }

    protected virtual string GetDictionaryKey(IDictionary<string, object?> dictionary)
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
        return value is string ? value.ToString() : value.GetHashCode().ToString();
    }

    protected virtual T GetCachedOrMemberValue<T>(Func<T> member, string memberName, bool storeInCache)
    {
        var cacheKey = GetCacheKey(memberName);
        var result = requestCache.GetValue<T>(cacheKey);
        if (result != null)
        {
            return result;
        }

        result = member.Invoke();
        if (storeInCache)
        {
            requestCache.SetValue(cacheKey, result);
        }

        return result;
    }

    protected virtual void SetCachedOrMemberValue<T>(Action<T> member, string memberName, T value)
    {
        if (IsReadOnly)
        {
            var cacheKey = GetCacheKey(memberName);
            requestCache.SetValue(cacheKey, value);
        }
        else
        {
            member(value);
        }
    }
}
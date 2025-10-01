using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Web;
using MvcSiteMapProvider.Web.Mvc;
using System;

namespace MvcSiteMapProvider;

/// <summary>
/// An abstract factory that can be used to create new instances of <see cref="T:MvcSiteMapProvider.RequestCacheableSiteMapNode"/>
/// at runtime.
/// </summary>
public class SiteMapNodeFactory 
    : ISiteMapNodeFactory
{
    public SiteMapNodeFactory(
        ISiteMapNodeChildStateFactory siteMapNodeChildStateFactory,
        ILocalizationServiceFactory localizationServiceFactory,
        ISiteMapNodePluginProvider pluginProvider,
        IUrlPath urlPath,
        IMvcContextFactory mvcContextFactory
    ) 
    {
        this.siteMapNodeChildStateFactory = siteMapNodeChildStateFactory ?? throw new ArgumentNullException(nameof(siteMapNodeChildStateFactory));
        this.localizationServiceFactory = localizationServiceFactory ?? throw new ArgumentNullException(nameof(localizationServiceFactory));
        this.pluginProvider = pluginProvider ?? throw new ArgumentNullException(nameof(pluginProvider));
        this.urlPath = urlPath ?? throw new ArgumentNullException(nameof(urlPath));
        this.mvcContextFactory = mvcContextFactory ?? throw new ArgumentNullException(nameof(mvcContextFactory));
    }

    // Services
    protected readonly ISiteMapNodeChildStateFactory siteMapNodeChildStateFactory;
    protected readonly ILocalizationServiceFactory localizationServiceFactory;
    protected readonly ISiteMapNodePluginProvider pluginProvider;
    protected readonly IUrlPath urlPath;
    protected readonly IMvcContextFactory mvcContextFactory;


    #region ISiteMapNodeFactory Members

    public ISiteMapNode Create(ISiteMap siteMap, string key, string? implicitResourceKey)
    {
        return CreateInternal(siteMap, key, implicitResourceKey, false);
    }

    public ISiteMapNode CreateDynamic(ISiteMap siteMap, string key, string implicitResourceKey)
    {
        return CreateInternal(siteMap, key, implicitResourceKey, true);
    }

    protected ISiteMapNode CreateInternal(ISiteMap siteMap, string key, string? implicitResourceKey, bool isDynamic)
    {
        // IMPORTANT: we must create one localization service per node because the service contains its own state that applies to the node
        var localizationService = localizationServiceFactory.Create(implicitResourceKey ?? string.Empty);

        return new RequestCacheableSiteMapNode(
            siteMap,
            key,
            isDynamic,
            pluginProvider,
            mvcContextFactory,
            siteMapNodeChildStateFactory,
            localizationService,
            urlPath);
    }

    #endregion
}
using System;
using MvcSiteMapProvider.Loader;
using MvcSiteMapProvider.Resources;

namespace MvcSiteMapProvider
{
    /// <summary>
    ///     This class is the static entry point where the presentation layer can request a sitemap by calling Current or
    ///     passing a siteMapCacheKey.
    /// </summary>
    public class SiteMaps
    {
        private static ISiteMapLoader loader;

        public static ISiteMapLoader Loader
        {
            set
            {
                if (loader != null)
                {
                    throw new MvcSiteMapException(Messages.SiteMapLoaderAlreadySet);
                }

                loader = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        public static ISiteMap? Current => GetSiteMap();

        public static ISiteMap? GetSiteMap(string siteMapCacheKey)
        {
            ThrowIfLoaderNotInitialized();
            return loader.GetSiteMap(siteMapCacheKey);
        }

        private static ISiteMap? GetSiteMap()
        {
            ThrowIfLoaderNotInitialized();
            return loader.GetSiteMap();
        }

        public static void ReleaseSiteMap(string siteMapCacheKey)
        {
            ThrowIfLoaderNotInitialized();
            loader.ReleaseSiteMap(siteMapCacheKey);
        }

        public static void ReleaseSiteMap()
        {
            ThrowIfLoaderNotInitialized();
            loader.ReleaseSiteMap();
        }

        private static void ThrowIfLoaderNotInitialized()
        {
            if (loader == null)
            {
                throw new MvcSiteMapException(Messages.SiteMapLoaderNotInitialized);
            }
        }
    }
}

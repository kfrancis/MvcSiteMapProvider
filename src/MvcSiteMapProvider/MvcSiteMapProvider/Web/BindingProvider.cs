using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Microsoft.Web.Administration;
using MvcSiteMapProvider.Web.Mvc;

namespace MvcSiteMapProvider.Web
{
    /// <summary>
    ///     Provides bindings from the IIS server, if running on IIS.
    /// </summary>
    /// <remarks>
    ///     To determine if IIS is used, we are using the SERVER_SOFTWARE server variable.
    ///     Do note that it is possible to overwrite this value so prying eyes cannot see what,
    ///     web server you are running. If you overwrite the value so that it does not start with
    ///     Microsoft-IIS, this class will stop functioning.
    ///     This class should be configured as a singleton in the DI container, so
    ///     the settings are only retrieved at application start, rather than on every request.
    /// </remarks>
    public class BindingProvider
        : IBindingProvider
    {
        protected readonly IBindingFactory bindingFactory;
        protected readonly IMvcContextFactory mvcContextFactory;

        public BindingProvider(
            IBindingFactory bindingFactory,
            IMvcContextFactory mvcContextFactory
        )
        {
            this.bindingFactory = bindingFactory ?? throw new ArgumentNullException(nameof(bindingFactory));
            this.mvcContextFactory = mvcContextFactory ?? throw new ArgumentNullException(nameof(mvcContextFactory));
        }

        protected HttpContextBase HttpContext => mvcContextFactory.CreateHttpContext();

        protected virtual bool IsIISServer
        {
            get
            {
                var serverSoftware = HttpContext.Request.ServerVariables["SERVER_SOFTWARE"];
                return string.IsNullOrEmpty(serverSoftware) ? false : serverSoftware.StartsWith("Microsoft-IIS/");
            }
        }

        protected IEnumerable<IBinding>? Bindings { get; set; }

        public IEnumerable<IBinding> GetBindings()
        {
            if (Bindings == null)
            {
                LoadBindings();
            }

            return Bindings!; // Non-null after initialization
        }

        /// <summary>
        ///     Loads the IIS bindings for the current application, if running on IIS.
        /// </summary>
        protected virtual void LoadBindings()
        {
            IList<IBinding> result = new List<IBinding>();

            if (IsIISServer)
            {
                // Get the current Site Name
                var siteName = HostingEnvironment.SiteName;

                // Get the sites section from the AppPool.config
                var sitesSection = WebConfigurationManager.GetSection(null, null, "system.applicationHost/sites");

                var site = sitesSection.GetCollection().FirstOrDefault(x =>
                    string.Equals((string)x["name"], siteName, StringComparison.OrdinalIgnoreCase));

                if (site != null)
                {
                    foreach (var iisBinding in site.GetCollection("bindings"))
                    {
                        var protocol = iisBinding["protocol"] as string;

                        if (iisBinding["bindingInformation"] is not string bindingInfo)
                        {
                            continue;
                        }

                        var parts = bindingInfo.Split(':');
                        if (parts.Length != 3)
                        {
                            continue;
                        }

                        var ip = parts[0]; // May be "*" or the actual IP
                        var port = parts[1]; // Always a port number (even if default port)
                        var hostHeader = parts[2]; // Optional - may be "". We can't rely on this entirely.

                        // Guess what the hostName will be depending on host header/IP address
                        var hostName = string.IsNullOrEmpty(hostHeader) ? ip : hostHeader;

                        result.Add(bindingFactory.Create(hostName, protocol, int.Parse(port)));
                    }
                }
            }

            Bindings = result;
        }
    }
}

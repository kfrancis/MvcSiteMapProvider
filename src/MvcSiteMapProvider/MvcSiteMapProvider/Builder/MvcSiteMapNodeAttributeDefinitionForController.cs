using System;

namespace MvcSiteMapProvider.Builder
{
    /// <summary>
    /// MvcSiteMapNodeAttributeDefinition for Controller
    /// </summary>
    public class MvcSiteMapNodeAttributeDefinitionForController
        : IMvcSiteMapNodeAttributeDefinition
    {
        /// <summary>
        /// Gets or sets the type of the controller.
        /// </summary>
        /// <value>The type of the controller.</value>
        public Type ControllerType { get; set; }

        /// <summary>
        /// Gets or sets the site map node attribute.
        /// </summary>
        /// <value>The site map node attribute.</value>
        public IMvcSiteMapNodeAttribute SiteMapNodeAttribute { get; set; }
    }
}
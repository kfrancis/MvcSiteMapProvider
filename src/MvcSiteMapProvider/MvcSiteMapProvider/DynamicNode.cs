using System;
using System.Collections.Generic;
using System.Linq;

namespace MvcSiteMapProvider
{
    /// <summary>
    /// DynamicNode class
    /// </summary>
    public class DynamicNode
    {
        protected ChangeFrequency changeFrequency = ChangeFrequency.Undefined;
        protected UpdatePriority updatePriority = UpdatePriority.Undefined;

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>The key.</value>
        public virtual string Key { get; set; }

        /// <summary>
        /// Gets or sets the parent key (optional).
        /// </summary>
        /// <value>The parent key.</value>
        public virtual string ParentKey { get; set; }

        /// <summary>
        /// Gets or sets the sort order of the node relative to its sibling nodes (the nodes that have the same parent).
        /// </summary>
        /// <value>The sort order.</value>
        public virtual int? Order { get; set; }

        /// <summary>
        /// Gets or sets the HTTP method (such as GET, POST, or HEAD).
        /// </summary>
        /// <value>
        /// The HTTP method.
        /// </value>
        public virtual string HttpMethod { get; set; }

        // NOTE: Resource key is missing

        /// <summary>
        /// Gets or sets the title (optional).
        /// </summary>
        /// <value>The title.</value>
        public virtual string Title { get; set; }

        /// <summary>
        /// Gets or sets the description (optional).
        /// </summary>
        /// <value>The description.</value>
        public virtual string Description { get; set; }

        /// <summary>
        /// Gets or sets the target frame (optional).
        /// </summary>
        /// <value>The target frame.</value>
        public virtual string TargetFrame { get; set; }

        /// <summary>
        /// Gets or sets the image URL (optional).
        /// </summary>
        /// <value>The image URL.</value>
        public virtual string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the protocol such as http or https to use when resolving the image URL.
        /// Defaults to the protocol of the current request if not provided.
        /// </summary>
        /// <value>The protocol of the image URL.</value>
        public virtual string ImageUrlProtocol { get; set; }

        /// <summary>
        /// Gets or sets the host name such as www.somewhere.com to use when resolving the image URL.
        /// </summary>
        /// <value>The host name of the image URL.</value>
        public virtual string ImageUrlHostName { get; set; }

        /// <summary>
        /// Gets or sets the attributes (optional).
        /// </summary>
        /// <value>The attributes.</value>
        public virtual IDictionary<string, object> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        /// <value>The roles.</value>
        public virtual IList<string> Roles { get; set; }

        /// <summary>
        /// Gets or sets the last modified date.
        /// </summary>
        /// <value>The last modified date.</value>
        public virtual DateTime? LastModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets the change frequency.
        /// </summary>
        /// <value>The change frequency.</value>
        public virtual ChangeFrequency ChangeFrequency
        {
            get { return changeFrequency; }
            set { changeFrequency = value; }
        }

        /// <summary>
        /// Gets or sets the update priority.
        /// </summary>
        /// <value>The update priority.</value>
        public virtual UpdatePriority UpdatePriority
        {
            get { return updatePriority; }
            set { updatePriority = value; }
        }

        /// <summary>
        /// Gets or sets the visibility provider.
        /// </summary>
        /// <value>
        /// The visibility provider.
        /// </value>
        public virtual string VisibilityProvider { get; set; }

        // NOTE: Dynamic node provider is missing (intentionally).

        /// <summary>
        /// Gets or sets whether the node is clickable or just a grouping node.
        /// </summary>
        /// <value>
        /// Is clickable.
        /// </value>
        public virtual bool? Clickable { get; set; }

        /// <summary>
        /// Gets or sets the URL resolver.
        /// </summary>
        /// <value>
        /// The URL resolver.
        /// </value>
        public virtual string UrlResolver { get; set; }

        /// <summary>
        /// Gets or sets the Url (optional).
        /// </summary>
        /// <value>The area.</value>
        public virtual string Url { get; set; }

        /// <summary>
        /// A value indicating to cache the resolved URL. If false, the URL will be
        /// resolved every time it is accessed.
        /// </summary>
        public virtual bool? CacheResolvedUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include ambient request values
        /// (from the RouteValues and/or query string) when resolving URLs.
        /// </summary>
        /// <value><b>true</b> to include ambient values (like MVC does); otherwise <b>false</b>.</value>
        public virtual bool? IncludeAmbientValuesInUrl { get; set; }

        /// <summary>
        /// Gets or sets the protocol (such as http or https) that will be used when resolving the URL.
        /// </summary>
        /// <value>The protocol.</value>
        public virtual string Protocol { get; set; }

        /// <summary>
        /// Gets or sets the host name (such as www.mysite.com) that will be used when resolving the URL.
        /// </summary>
        /// <value>The host name.</value>
        public virtual string HostName { get; set; }

        /// <summary>
        /// Gets or sets the canonical key. The key is used to reference another ISiteMapNode to get the canonical URL.
        /// </summary>
        /// <remarks>May not be used in conjunction with CanonicalUrl. Only 1 canonical value is allowed.</remarks>
        public virtual string CanonicalKey { get; set; }

        /// <summary>
        /// Gets or sets the canonical URL.
        /// </summary>
        /// <remarks>May not be used in conjunction with CanonicalKey. Only 1 canonical value is allowed.</remarks>
        public virtual string CanonicalUrl { get; set; }

        /// <summary>
        /// Gets or sets the protocol that will be used when resolving the canonical URL.
        /// </summary>
        public virtual string CanonicalUrlProtocol { get; set; }

        /// <summary>
        /// Gets or sets the host name that will be used when resolving the canonical URL.
        /// </summary>
        public virtual string CanonicalUrlHostName { get; set; }

        /// <summary>
        /// Gets or sets the robots meta values.
        /// </summary>
        /// <value>The robots meta values.</value>
        public virtual IList<string> MetaRobotsValues { get; set; }

        /// <summary>
        /// Gets or sets the route.
        /// </summary>
        /// <value>The route.</value>
        public virtual string Route { get; set; }

        /// <summary>
        /// Gets or sets the route values.
        /// </summary>
        /// <value>The route values.</value>
        public virtual IDictionary<string, object> RouteValues { get; set; }

        /// <summary>
        /// Gets or sets the preserved route parameter names (= values that will be used from the current request route).
        /// </summary>
        /// <value>The attributes.</value>
        public virtual IList<string> PreservedRouteParameters { get; set; }

        /// <summary>
        /// Gets or sets the area (optional).
        /// </summary>
        /// <value>The area.</value>
        public virtual string Area { get; set; }

        /// <summary>
        /// Gets or sets the controller (optional).
        /// </summary>
        /// <value>The controller.</value>
        public virtual string Controller { get; set; }

        /// <summary>
        /// Gets or sets the action (optional).
        /// </summary>
        /// <value>The action.</value>
        public virtual string Action { get; set; }

        /// <summary>
        /// Copies the values for matching properties on an <see cref="T:MvcSiteMapNodeProvider.ISiteMapNode"/> instance, but
        /// doesn't overwrite any values that are not set in this <see cref="T:MvcSiteMapNodeProvider.DynamicNode"/> instance.
        /// </summary>
        /// <param name="node">The site map node to copy the values into.</param>
        public virtual void SafeCopyTo(ISiteMapNode node)
        {
            if (Order != null)
                node.Order = (int)Order;
            if (!string.IsNullOrEmpty(HttpMethod))
                node.HttpMethod = HttpMethod;
            if (!string.IsNullOrEmpty(Title))
                node.Title = Title;
            if (!string.IsNullOrEmpty(Description))
                node.Description = Description;
            if (!string.IsNullOrEmpty(TargetFrame))
                node.TargetFrame = TargetFrame;
            if (!string.IsNullOrEmpty(ImageUrl))
                node.ImageUrl = ImageUrl;
            if (!string.IsNullOrEmpty(ImageUrlProtocol))
                node.ImageUrlProtocol = ImageUrlProtocol;
            if (!string.IsNullOrEmpty(ImageUrlHostName))
                node.ImageUrlHostName = ImageUrlHostName;
            foreach (var kvp in Attributes)
            {
                node.Attributes[kvp.Key] = kvp.Value;
            }
            if (Roles.Any())
            {
                foreach (var role in Roles)
                {
                    if (!node.Roles.Contains(role))
                    {
                        node.Roles.Add(role);
                    }
                }
            }
            if (LastModifiedDate != null && LastModifiedDate.HasValue)
                node.LastModifiedDate = LastModifiedDate.Value;
            if (ChangeFrequency != ChangeFrequency.Undefined)
                node.ChangeFrequency = ChangeFrequency;
            if (UpdatePriority != UpdatePriority.Undefined)
                node.UpdatePriority = UpdatePriority;
            if (!string.IsNullOrEmpty(VisibilityProvider))
                node.VisibilityProvider = VisibilityProvider;
            if (Clickable != null)
                node.Clickable = (bool)Clickable;
            if (!string.IsNullOrEmpty(UrlResolver))
                node.UrlResolver = UrlResolver;
            if (!string.IsNullOrEmpty(Url))
                node.Url = Url;
            if (CacheResolvedUrl != null)
                node.CacheResolvedUrl = (bool)CacheResolvedUrl;
            if (IncludeAmbientValuesInUrl != null)
                node.IncludeAmbientValuesInUrl = (bool)IncludeAmbientValuesInUrl;
            if (!string.IsNullOrEmpty(Protocol))
                node.Protocol = Protocol;
            if (!string.IsNullOrEmpty(HostName))
                node.HostName = HostName;
            if (!string.IsNullOrEmpty(CanonicalKey))
                node.CanonicalKey = CanonicalKey;
            if (!string.IsNullOrEmpty(CanonicalUrl))
                node.CanonicalUrl = CanonicalUrl;
            if (!string.IsNullOrEmpty(CanonicalUrlProtocol))
                node.CanonicalUrlProtocol = CanonicalUrlProtocol;
            if (!string.IsNullOrEmpty(CanonicalUrlHostName))
                node.CanonicalUrlHostName = CanonicalUrlHostName;
            if (MetaRobotsValues.Any())
            {
                foreach (var value in MetaRobotsValues)
                {
                    if (!node.MetaRobotsValues.Contains(value))
                    {
                        node.MetaRobotsValues.Add(value);
                    }
                }
            }
            if (!string.IsNullOrEmpty(Route))
                node.Route = Route;
            foreach (var kvp in RouteValues)
            {
                node.RouteValues[kvp.Key] = kvp.Value;
            }
            if (PreservedRouteParameters.Any())
            {
                foreach (var p in PreservedRouteParameters)
                {
                    if (!node.PreservedRouteParameters.Contains(p))
                    {
                        node.PreservedRouteParameters.Add(p);
                    }
                }
            }
            if (!string.IsNullOrEmpty(Area))
                node.Area = Area;
            if (!string.IsNullOrEmpty(Controller))
                node.Controller = Controller;
            if (!string.IsNullOrEmpty(Action))
                node.Action = Action;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicNode"/> class.
        /// </summary>
        public DynamicNode()
        {
            RouteValues = new Dictionary<string, object>();
            Attributes = new Dictionary<string, object>();
            PreservedRouteParameters = new List<string>();
            Roles = new List<string>();
            MetaRobotsValues = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicNode"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="title">The title.</param>
        public DynamicNode(string key, string title)
            : this(key, null, title, "")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicNode"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="parentKey">The parent key.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        public DynamicNode(string key, string parentKey, string title, string description)
            : this()
        {
            Key = key;
            ParentKey = parentKey;
            Title = title;
            Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicNode"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="parentKey">The parent key.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="controller">The controller (optional).</param>
        /// <param name="action">The action (optional).</param>
        public DynamicNode(string key, string parentKey, string title, string description, string controller, string action)
            : this(key, parentKey, title, description, null, controller, action)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicNode"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="parentKey">The parent key.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="action">The action (optional).</param>
        public DynamicNode(string key, string parentKey, string title, string description, string action)
            : this(key, parentKey, title, description, null, null, action)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicNode"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="parentKey">The parent key.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <param name="area">The area (optional).</param>
        /// <param name="controller">The controller (optional).</param>
        /// <param name="action">The action (optional).</param>
        public DynamicNode(string key, string parentKey, string title, string description, string area, string controller, string action)
            : this(key, parentKey, title, description)
        {
            Area = area;
            Controller = controller;
            Action = action;
        }
    }
}
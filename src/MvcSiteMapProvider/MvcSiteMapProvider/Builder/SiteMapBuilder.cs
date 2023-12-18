using MvcSiteMapProvider.DI;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Visitor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MvcSiteMapProvider.Builder
{
    /// <summary>
    /// The default implementation of SiteMapBuilder. Builds a <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> tree
    /// based on a <see cref="T:MvcSiteMapProvider.ISiteMapNodeProvider"/> and then runs a <see cref="T:MvcSiteMapProvider.Visitor.ISiteMapNodeVisitor"/>
    /// to optimize the nodes.
    /// </summary>
    [ExcludeFromAutoRegistration]
    public class SiteMapBuilder
        : ISiteMapBuilder
    {
        protected readonly ICultureContextFactory CultureContextFactory;

        protected readonly ISiteMapHierarchyBuilder SiteMapHierarchyBuilder;

        protected readonly ISiteMapNodeHelperFactory SiteMapNodeHelperFactory;

        protected readonly ISiteMapNodeProvider SiteMapNodeProvider;

        protected readonly ISiteMapNodeVisitor SiteMapNodeVisitor;

        public SiteMapBuilder(
                                                    ISiteMapNodeProvider siteMapNodeProvider,
            ISiteMapNodeVisitor siteMapNodeVisitor,
            ISiteMapHierarchyBuilder siteMapHierarchyBuilder,
            ISiteMapNodeHelperFactory siteMapNodeHelperFactory,
            ICultureContextFactory cultureContextFactory
            )
        {
            SiteMapNodeProvider = siteMapNodeProvider ?? throw new ArgumentNullException(nameof(siteMapNodeProvider));
            SiteMapHierarchyBuilder = siteMapHierarchyBuilder ?? throw new ArgumentNullException(nameof(siteMapHierarchyBuilder));
            SiteMapNodeHelperFactory = siteMapNodeHelperFactory ?? throw new ArgumentNullException(nameof(siteMapNodeHelperFactory));
            SiteMapNodeVisitor = siteMapNodeVisitor ?? throw new ArgumentNullException(nameof(siteMapNodeVisitor));
            CultureContextFactory = cultureContextFactory ?? throw new ArgumentNullException(nameof(cultureContextFactory));
        }

        public ISiteMapNode BuildSiteMap(ISiteMap siteMap, ISiteMapNode rootNode)
        {
            // Load the source nodes
            var sourceNodes = new List<ISiteMapNodeToParentRelation>();
            LoadSourceNodes(siteMap, sourceNodes);

            // Add the root node to the sitemap
            var root = GetRootNode(siteMap, sourceNodes);
            if (root != null)
            {
                siteMap.AddNode(root);
            }

            var orphans = SiteMapHierarchyBuilder.BuildHierarchy(siteMap, sourceNodes);

            if (orphans.Any())
            {
                // We have orphaned nodes - filter to remove the matching descendants of the mismatched keys.
                var mismatched = from parent in orphans
                                 where !(from child in orphans
                                         select child.Node.Key)
                                         .Contains(parent.ParentKey)
                                 select parent;

                var names = string.Join(Environment.NewLine + Environment.NewLine, mismatched.Select(x =>
                    string.Format(Resources.Messages.SiteMapNodeFormatWithParentKey, x.ParentKey, x.Node.Controller,
                    x.Node.Action, x.Node.Area, x.Node.Url, x.Node.Key, x.SourceName)).ToArray());
                throw new MvcSiteMapException(string.Format(Resources.Messages.SiteMapBuilderOrphanedNodes, siteMap.CacheKey, names));
            }

            // Run our visitors
            VisitNodes(root);

            // Done!
            return root;
        }

        protected virtual ISiteMapNode GetRootNode(ISiteMap siteMap, IList<ISiteMapNodeToParentRelation> sourceNodes)
        {
            var rootNodes = sourceNodes.Where(x => string.IsNullOrEmpty(x.ParentKey) || x.ParentKey.Trim()?.Length == 0);

            // Check if we have more than one root node defined or no root defined
            if (rootNodes.Count() > 1)
            {
                var names = string.Join(Environment.NewLine + Environment.NewLine, rootNodes.Select(x =>
                    string.Format(Resources.Messages.SiteMapNodeFormatWithParentKey, x.ParentKey, x.Node.Controller,
                    x.Node.Action, x.Node.Area, x.Node.Url, x.Node.Key, x.SourceName)).ToArray());
                throw new MvcSiteMapException(string.Format(Resources.Messages.SiteMapBuilderRootKeyAmbiguous, siteMap.CacheKey, names));
            }
            else if (!rootNodes.Any())
            {
                throw new MvcSiteMapException(string.Format(Resources.Messages.SiteMapBuilderRootNodeNotDefined, siteMap.CacheKey));
            }

            var root = rootNodes.Single();

            // Remove the root node from the sourceNodes
            sourceNodes.Remove(root);

            return root.Node;
        }

        protected virtual void LoadSourceNodes(ISiteMap siteMap, List<ISiteMapNodeToParentRelation> sourceNodes)
        {
            // Temporarily override the current thread's culture with the invariant culture
            // while running the ISiteMapNodeProvider instances.
            using (var cultureContext = CultureContextFactory.CreateInvariant())
            {
                var siteMapNodeHelper = SiteMapNodeHelperFactory.Create(siteMap, cultureContext);
                sourceNodes.AddRange(SiteMapNodeProvider.GetSiteMapNodes(siteMapNodeHelper));
            }
        }

        protected virtual void VisitNodes(ISiteMapNode node)
        {
            SiteMapNodeVisitor.Execute(node);

            if (node.HasChildNodes)
            {
                foreach (var childNode in node.ChildNodes)
                {
                    VisitNodes(childNode);
                }
            }
        }
    }
}

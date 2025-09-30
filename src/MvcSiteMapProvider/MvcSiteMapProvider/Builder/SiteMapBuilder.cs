using System;
using System.Collections.Generic;
using System.Linq;
using MvcSiteMapProvider.DI;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Resources;
using MvcSiteMapProvider.Visitor;

namespace MvcSiteMapProvider.Builder
{
    /// <summary>
    ///     The default implementation of SiteMapBuilder. Builds a <see cref="T:MvcSiteMapProvider.ISiteMapNode" /> tree
    ///     based on a <see cref="T:MvcSiteMapProvider.ISiteMapNodeProvider" /> and then runs a
    ///     <see cref="T:MvcSiteMapProvider.Visitor.ISiteMapNodeVisitor" />
    ///     to optimize the nodes.
    /// </summary>
    [ExcludeFromAutoRegistration]
    public class SiteMapBuilder
        : ISiteMapBuilder
    {
        private readonly ICultureContextFactory _cultureContextFactory;
        private readonly ISiteMapHierarchyBuilder _siteMapHierarchyBuilder;
        private readonly ISiteMapNodeHelperFactory _siteMapNodeHelperFactory;
        private readonly ISiteMapNodeProvider _siteMapNodeProvider;
        private readonly ISiteMapNodeVisitor _siteMapNodeVisitor;

        public SiteMapBuilder(
            ISiteMapNodeProvider siteMapNodeProvider,
            ISiteMapNodeVisitor siteMapNodeVisitor,
            ISiteMapHierarchyBuilder siteMapHierarchyBuilder,
            ISiteMapNodeHelperFactory siteMapNodeHelperFactory,
            ICultureContextFactory cultureContextFactory
        )
        {
            _siteMapNodeProvider = siteMapNodeProvider ?? throw new ArgumentNullException(nameof(siteMapNodeProvider));
            _siteMapHierarchyBuilder = siteMapHierarchyBuilder ??
                                       throw new ArgumentNullException(nameof(siteMapHierarchyBuilder));
            _siteMapNodeHelperFactory = siteMapNodeHelperFactory ??
                                        throw new ArgumentNullException(nameof(siteMapNodeHelperFactory));
            _siteMapNodeVisitor = siteMapNodeVisitor ?? throw new ArgumentNullException(nameof(siteMapNodeVisitor));
            _cultureContextFactory =
                cultureContextFactory ?? throw new ArgumentNullException(nameof(cultureContextFactory));
        }

        public ISiteMapNode? BuildSiteMap(ISiteMap siteMap, ISiteMapNode? rootNode)
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

            var orphans = _siteMapHierarchyBuilder.BuildHierarchy(siteMap, sourceNodes);

            if (orphans.Any())
            {
                // We have orphaned nodes - filter to remove the matching descendants of the mismatched keys.
                var mismatched = from parent in orphans
                    where !(from child in orphans
                            select child.Node.Key)
                        .Contains(parent.ParentKey)
                    select parent;

                var names = string.Join(Environment.NewLine + Environment.NewLine, mismatched.Select(x =>
                    string.Format(Messages.SiteMapNodeFormatWithParentKey, x.ParentKey, x.Node.Controller,
                        x.Node.Action, x.Node.Area, x.Node.Url, x.Node.Key, x.SourceName)).ToArray());
                throw new MvcSiteMapException(string.Format(Messages.SiteMapBuilderOrphanedNodes, siteMap.CacheKey,
                    names));
            }

            // Run our visitors
            VisitNodes(root);

            // Done!
            return root;
        }

        protected virtual void LoadSourceNodes(ISiteMap siteMap, List<ISiteMapNodeToParentRelation> sourceNodes)
        {
            // Temporarily override the current thread's culture with the invariant culture
            // while running the ISiteMapNodeProvider instances.
            using var cultureContext = _cultureContextFactory.CreateInvariant();
            var siteMapNodeHelper = _siteMapNodeHelperFactory.Create(siteMap, cultureContext);
            sourceNodes.AddRange(_siteMapNodeProvider.GetSiteMapNodes(siteMapNodeHelper));
        }

        protected virtual ISiteMapNode? GetRootNode(ISiteMap siteMap, IList<ISiteMapNodeToParentRelation> sourceNodes)
        {
            var rootNodes =
                sourceNodes.Where(x => string.IsNullOrEmpty(x.ParentKey) || x.ParentKey?.Trim() == string.Empty);

            // Check if we have more than one root node defined or no root defined
            if (rootNodes.Count() > 1)
            {
                var names = string.Join(Environment.NewLine + Environment.NewLine, rootNodes.Select(x =>
                    string.Format(Messages.SiteMapNodeFormatWithParentKey, x.ParentKey, x.Node.Controller,
                        x.Node.Action, x.Node.Area, x.Node.Url, x.Node.Key, x.SourceName)).ToArray());
                throw new MvcSiteMapException(string.Format(Messages.SiteMapBuilderRootKeyAmbiguous, siteMap.CacheKey,
                    names));
            }

            if (!rootNodes.Any())
            {
                throw new MvcSiteMapException(
                    string.Format(Messages.SiteMapBuilderRootNodeNotDefined, siteMap.CacheKey));
            }

            var root = rootNodes.Single();

            // Remove the root node from the sourceNodes
            sourceNodes.Remove(root);

            return root.Node;
        }

        protected virtual void VisitNodes(ISiteMapNode? node)
        {
            _siteMapNodeVisitor.Execute(node);

            if (node is not { HasChildNodes: true })
            {
                return;
            }

            foreach (var childNode in node.ChildNodes)
            {
                VisitNodes(childNode);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using MvcSiteMapProvider.Web.Html.Models;
using MvcSiteMapProvider.Tests.Unit.Web.Html; // FakeSiteMap & FakeSiteMapNode

namespace MvcSiteMapProvider.Tests.Unit.Web.Html.Models
{
    [TestFixture]
    public class SiteMapNodeModelTests
    {
        private static FakeSiteMap CreateSiteMap(bool securityTrimming = false, bool visibilityAffectsDescendants = true)
        {
            return new FakeSiteMap(securityTrimming, visibilityAffectsDescendants);
        }

        private static FakeSiteMapNode CreateNode(FakeSiteMap siteMap, string key, string title, bool accessible = true, bool visible = true, bool clickable = true, int order = 0, string? url = null, string meta = "")
        {
            var node = new FakeSiteMapNode(siteMap, key, title, false, accessible, visible, clickable, url ?? ("/" + key), meta) { Order = order };
            return node;
        }

        [Test]
        public void Ctor_NullNode_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new SiteMapNodeModel(null!, new Dictionary<string, object?>()));
        }

        [Test]
        public void Ctor_NullSourceMetadata_Throws()
        {
            var sm = CreateSiteMap();
            var n = CreateNode(sm, "root", "Root");
            Assert.Throws<ArgumentNullException>(() => _ = new SiteMapNodeModel(n, null!));
        }

        [Test]
        public void Ctor_NegativeMaxDepth_Throws()
        {
            var sm = CreateSiteMap();
            var n = CreateNode(sm, "root", "Root");
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new SiteMapNodeModel(n, new Dictionary<string, object?>(), -1, true, false, true));
        }

        [Test]
        public void Properties_IsRootCurrentAndInPath_FlagsSetCorrectly()
        {
            var sm = CreateSiteMap();
            var root = CreateNode(sm, "root", "Root");
            sm.AddNode(root);
            sm.SetCurrentNode(root);
            var model = new SiteMapNodeModel(root, new Dictionary<string, object?>());
            Assert.That(model.IsRootNode, Is.True);
            Assert.That(model.IsCurrentNode, Is.True);
            Assert.That(model.IsInCurrentPath, Is.True);
        }

        [Test]
        public void Children_VisibilityAffectsDescendantsTrue_FiltersInvisible()
        {
            var sm = CreateSiteMap(visibilityAffectsDescendants: true);
            var root = CreateNode(sm, "root", "Root");
            var c1 = CreateNode(sm, "c1", "Child1", visible: true, order: 2);
            var c2 = CreateNode(sm, "c2", "Child2", visible: false, order: 1); // invisible should be skipped
            sm.AddNode(root);
            sm.AddNode(c1, root);
            sm.AddNode(c2, root);
            var model = new SiteMapNodeModel(root, new Dictionary<string, object?>(), 1, true, false, true);
            Assert.That(model.Children.Count, Is.EqualTo(1));
            Assert.That(model.Children[0].Key, Is.EqualTo("c1"));
        }

        [Test]
        public void Children_VisibilityAffectsDescendantsFalse_InvisibleChildReplacedByVisibleGrandchildren()
        {
            var sm = CreateSiteMap(visibilityAffectsDescendants: false);
            var root = CreateNode(sm, "root", "Root");
            var invisible = CreateNode(sm, "inv", "Invisible", visible: false, order: 2);
            var g1 = CreateNode(sm, "g1", "Grand1", visible: true, order: 2);
            var g2 = CreateNode(sm, "g2", "Grand2", visible: true, order: 1);
            var normal = CreateNode(sm, "c1", "Child1", visible: true, order: 3);
            sm.AddNode(root);
            sm.AddNode(invisible, root);
            sm.AddNode(normal, root);
            sm.AddNode(g1, invisible);
            sm.AddNode(g2, invisible);
            var model = new SiteMapNodeModel(root, new Dictionary<string, object?>(), 3, true, false, false);
            var children = model.Children; // triggers loading
            // Expect: invisible node skipped, its visible grandchildren inserted (ordered by Order) before normal
            Assert.That(children.Count, Is.EqualTo(3));
            Assert.That(children[0].Key, Is.EqualTo("g2")); // Order 1
            Assert.That(children[1].Key, Is.EqualTo("g1")); // Order 2
            Assert.That(children[2].Key, Is.EqualTo("c1")); // Order 3
        }

        [Test]
        public void Children_SortingByOrderWhenAnyNonZero()
        {
            var sm = CreateSiteMap();
            var root = CreateNode(sm, "root", "Root");
            var c1 = CreateNode(sm, "c1", "Child1", order: 2);
            var c2 = CreateNode(sm, "c2", "Child2", order: 1);
            sm.AddNode(root);
            sm.AddNode(c1, root);
            sm.AddNode(c2, root);
            var model = new SiteMapNodeModel(root, new Dictionary<string, object?>(), 1, true, false, true);
            Assert.That(model.Children[0].Key, Is.EqualTo("c2"));
            Assert.That(model.Children[1].Key, Is.EqualTo("c1"));
        }

        [Test]
        public void Children_StartingNodeInChildLevelTrue_ReturnsChildrenThenResets()
        {
            var sm = CreateSiteMap();
            var root = CreateNode(sm, "root", "Root");
            var c1 = CreateNode(sm, "c1", "Child1");
            sm.AddNode(root);
            sm.AddNode(c1, root);
            var model = new SiteMapNodeModel(root, new Dictionary<string, object?>(), 1, true, true, true);
            var first = model.Children;
            var second = model.Children; // should be empty after reset
            Assert.That(first.Count, Is.EqualTo(1));
            Assert.That(second.Count, Is.EqualTo(0));
        }

        [Test]
        public void Children_DrillDownToCurrentTrue_AllowsExceedingMaxDepthZeroWhenSiblingInCurrentPath()
        {
            var sm = CreateSiteMap();
            var root = CreateNode(sm, "root", "Root");
            var pathChild = CreateNode(sm, "pc", "PathChild");
            var pathLeaf = CreateNode(sm, "leaf", "Leaf");
            var subject = CreateNode(sm, "subject", "Subject");
            var subjectChild = CreateNode(sm, "subchild", "SubjectChild");
            sm.AddNode(root);
            sm.AddNode(pathChild, root);
            sm.AddNode(subject, root);
            sm.AddNode(pathLeaf, pathChild);
            sm.AddNode(subjectChild, subject);
            sm.SetCurrentNode(pathLeaf); // current path through pathChild
            // maxDepth=0 but drillDownToCurrent=true should still include subject's children because sibling in current path
            var model = new SiteMapNodeModel(subject, new Dictionary<string, object?>(), 0, true, false, true);
            Assert.That(model.Children.Count, Is.EqualTo(1));
            Assert.That(model.Children[0].Key, Is.EqualTo("subchild"));
        }

        [Test]
        public void Parent_ReturnsParentModel()
        {
            var sm = CreateSiteMap();
            var root = CreateNode(sm, "root", "Root");
            var c1 = CreateNode(sm, "c1", "Child1");
            sm.AddNode(root);
            sm.AddNode(c1, root);
            var model = new SiteMapNodeModel(c1, new Dictionary<string, object?>(), 1, true, false, true);
            Assert.That(model.Parent, Is.Not.Null);
            Assert.That(model.Parent!.Key, Is.EqualTo("root"));
        }

        [Test]
        public void Descendants_ReturnsAllDescendants()
        {
            var sm = CreateSiteMap();
            var root = CreateNode(sm, "root", "Root");
            var c1 = CreateNode(sm, "c1", "Child1");
            var c2 = CreateNode(sm, "c2", "Child2");
            var g1 = CreateNode(sm, "g1", "Grand1");
            sm.AddNode(root);
            sm.AddNode(c1, root);
            sm.AddNode(c2, root);
            sm.AddNode(g1, c1);
            var model = new SiteMapNodeModel(root, new Dictionary<string, object?>(), 3, true, false, true);
            var descendants = model.Descendants;
            Assert.That(descendants.Count, Is.EqualTo(3));
            Assert.That(descendants.Select(d => d.Key), Is.EquivalentTo(new[] { "c1", "c2", "g1" }));
        }

        [Test]
        public void Ancestors_ReturnsAllAncestorsOrderedFromImmediateParentUp()
        {
            var sm = CreateSiteMap();
            var root = CreateNode(sm, "root", "Root");
            var c1 = CreateNode(sm, "c1", "Child1");
            var g1 = CreateNode(sm, "g1", "Grand1");
            sm.AddNode(root);
            sm.AddNode(c1, root);
            sm.AddNode(g1, c1);
            var model = new SiteMapNodeModel(g1, new Dictionary<string, object?>(), 3, true, false, true);
            var ancestors = model.Ancestors;
            Assert.That(ancestors.Count, Is.EqualTo(2));
            Assert.That(ancestors[0].Key, Is.EqualTo("c1"));
            Assert.That(ancestors[1].Key, Is.EqualTo("root"));
        }
    }
}

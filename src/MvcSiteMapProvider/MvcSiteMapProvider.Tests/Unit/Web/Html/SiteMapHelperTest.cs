using Moq;
using MvcSiteMapProvider.Collections.Specialized;
using MvcSiteMapProvider.Web.Html;
using NUnit.Framework;
using System.Web.Mvc;

namespace MvcSiteMapProvider.Tests.Unit.Web.Html
{
    [TestFixture]
    public class SiteMapHelperTest
    {
        #region SetUp / TearDown

        private Mock<IViewDataContainer> iView = null;
        private Mock<ViewContext> viewContext = null;

        [SetUp]
        public void Init()
        {
            iView = new Mock<IViewDataContainer>();
            viewContext = new Mock<ViewContext>();
        }

        [TearDown]
        public void Dispose()
        {
            iView = null;
            viewContext = null;
        }

        #endregion

        #region Tests

        [Test]
        public void BuildModel_Case1_Default_ShouldReturnAllNodesAtRootLevel()
        {
            // @Html.MvcSiteMap().SiteMap()

            // Arrange
            var siteMap = HtmlHelperTestCases.CreateFakeSiteMapCase1();
            var startingNode = siteMap.RootNode;
            HtmlHelper helper = new HtmlHelper(this.viewContext.Object, this.iView.Object);
            MvcSiteMapHtmlHelper helperExtension = new MvcSiteMapHtmlHelper(helper, siteMap, false);

            // Act
            var result = SiteMapHelper.BuildModel(
                helper: helperExtension,
                sourceMetadata: new SourceMetadataDictionary(),
                startingNode: startingNode,
                startingNodeInChildLevel: true,
                visibilityAffectsDescendants: true);

            // Assert
            // Flat structure - 3 nodes
            Affirm.That("Home").IsEqualTo(result.Nodes[0].Title);
            Affirm.That("About").IsEqualTo(result.Nodes[1].Title);
            Affirm.That("Contact").IsEqualTo(result.Nodes[2].Title);

            // Check counts
            Affirm.That(3).IsEqualTo(result.Nodes.Count);
            Affirm.That(0).IsEqualTo(result.Nodes[0].Children.Count);
            Affirm.That(0).IsEqualTo(result.Nodes[1].Children.Count);
            Affirm.That(0).IsEqualTo(result.Nodes[2].Children.Count);
        }

        [Test]
        public void BuildModel_Case1_StartingNodeNotInChildLevel_ShouldReturnHierarchicalNodes()
        {
            // @Html.MvcSiteMap().SiteMap(false)

            // Arrange
            var siteMap = HtmlHelperTestCases.CreateFakeSiteMapCase1();
            var startingNode = siteMap.RootNode;
            HtmlHelper helper = new HtmlHelper(this.viewContext.Object, this.iView.Object);
            MvcSiteMapHtmlHelper helperExtension = new MvcSiteMapHtmlHelper(helper, siteMap, false);

            // Act
            var result = SiteMapHelper.BuildModel(
                helper: helperExtension,
                sourceMetadata: new SourceMetadataDictionary(),
                startingNode: startingNode,
                startingNodeInChildLevel: false,
                visibilityAffectsDescendants: true);

            // Assert
            // Tree structure - 3 nodes
            Affirm.That("Home").IsEqualTo(result.Nodes[0].Title);
            Affirm.That("About").IsEqualTo(result.Nodes[0].Children[0].Title);
            Affirm.That("Contact").IsEqualTo(result.Nodes[0].Children[1].Title);

            // Check Counts
            Affirm.That(1).IsEqualTo(result.Nodes.Count);
            Affirm.That(2).IsEqualTo(result.Nodes[0].Children.Count);
            Affirm.That(0).IsEqualTo(result.Nodes[0].Children[0].Children.Count);
            Affirm.That(0).IsEqualTo(result.Nodes[0].Children[1].Children.Count);
        }

        [Test]
        public void BuildModel_Case2_StartingNodeNotInChildLevel_ShouldReturnHierarchicalNodes()
        {
            // @Html.MvcSiteMap().Menu(false)

            // Arrange
            var siteMap = HtmlHelperTestCases.CreateFakeSiteMapCase2();
            var startingNode = siteMap.RootNode;
            HtmlHelper helper = new HtmlHelper(this.viewContext.Object, this.iView.Object);
            MvcSiteMapHtmlHelper helperExtension = new MvcSiteMapHtmlHelper(helper, siteMap, false);

            // Act
            var result = SiteMapHelper.BuildModel(
                helper: helperExtension,
                sourceMetadata: new SourceMetadataDictionary(),
                startingNode: startingNode,
                startingNodeInChildLevel: false,
                visibilityAffectsDescendants: true);

            // Assert
            Affirm.That("Home").IsEqualTo(result.Nodes[0].Title);
            Affirm.That("About").IsEqualTo(result.Nodes[0].Children[0].Title);
            Affirm.That("About Me").IsEqualTo(result.Nodes[0].Children[0].Children[0].Title);
            Affirm.That("About You").IsEqualTo(result.Nodes[0].Children[0].Children[1].Title);

            // "Contact" is inaccessible - should be skipped. So should its child node "ContactSomebody".
            Affirm.That("Categories").IsEqualTo(result.Nodes[0].Children[1].Title);

            Affirm.That("Cameras").IsEqualTo(result.Nodes[0].Children[1].Children[0].Title);
            Affirm.That("Nikon Coolpix 200").IsEqualTo(result.Nodes[0].Children[1].Children[0].Children[0].Title);
            Affirm.That("Canon Ixus 300").IsEqualTo(result.Nodes[0].Children[1].Children[0].Children[1].Title);

            // "Memory Cards" is not visible. None of its children should be visible.
            Affirm.That(1).IsEqualTo(result.Nodes[0].Children[1].Children.Count);
        }

        [Test]
        public void BuildModel_Case2_StartingNodeNotInChildLevel_VisibilyDoesntAffectDescendants_ShouldReturnHierarchialNodes()
        {
            // @Html.MvcSiteMap().SiteMap(null, MvcSiteMapProvider.SiteMaps.Current.RootNode, false, false)

            // Arrange
            var siteMap = HtmlHelperTestCases.CreateFakeSiteMapCase2();
            var startingNode = siteMap.RootNode;
            HtmlHelper helper = new HtmlHelper(this.viewContext.Object, this.iView.Object);
            MvcSiteMapHtmlHelper helperExtension = new MvcSiteMapHtmlHelper(helper, siteMap, false);

            // Act
            var result = SiteMapHelper.BuildModel(
                helper: helperExtension,
                sourceMetadata: new SourceMetadataDictionary(),
                startingNode: startingNode,
                startingNodeInChildLevel: false,
                visibilityAffectsDescendants: false);

            // Assert
            Affirm.That("Home").IsEqualTo(result.Nodes[0].Title);
            Affirm.That("About").IsEqualTo(result.Nodes[0].Children[0].Title);
            Affirm.That("About Me").IsEqualTo(result.Nodes[0].Children[0].Children[0].Title);
            Affirm.That("About You").IsEqualTo(result.Nodes[0].Children[0].Children[1].Title);

            // "Contact" is inaccessible - should be skipped. So should its child node "ContactSomebody".
            Affirm.That("Categories").IsEqualTo(result.Nodes[0].Children[1].Title);

            Affirm.That("Cameras").IsEqualTo(result.Nodes[0].Children[1].Children[0].Title);
            Affirm.That("Nikon Coolpix 200").IsEqualTo(result.Nodes[0].Children[1].Children[0].Children[0].Title);
            Affirm.That("Canon Ixus 300").IsEqualTo(result.Nodes[0].Children[1].Children[0].Children[1].Title);

            // "Memory Cards" is not visible. However its children should be in its place.
            Affirm.That("Kingston 256 GB SD").IsEqualTo(result.Nodes[0].Children[1].Children[1].Title);
            Affirm.That("Sony 256 GB SD").IsEqualTo(result.Nodes[0].Children[1].Children[2].Title);
            Affirm.That("Sony SD Card Reader").IsEqualTo(result.Nodes[0].Children[1].Children[2].Children[0].Title);

            // Check counts
            Affirm.That(1).IsEqualTo( result.Nodes.Count);
            Affirm.That(2).IsEqualTo( result.Nodes[0].Children.Count); // Home
            Affirm.That(2).IsEqualTo( result.Nodes[0].Children[0].Children.Count); // About
            Affirm.That(0).IsEqualTo( result.Nodes[0].Children[0].Children[0].Children.Count); // About Me
            Affirm.That(0).IsEqualTo( result.Nodes[0].Children[0].Children[1].Children.Count); // About You
            Affirm.That(3).IsEqualTo( result.Nodes[0].Children[1].Children.Count); // Categories
            Affirm.That(2).IsEqualTo( result.Nodes[0].Children[1].Children[0].Children.Count); // Cameras
            Affirm.That(0).IsEqualTo( result.Nodes[0].Children[1].Children[0].Children[0].Children.Count); // Nikon Coolpix 200
            Affirm.That(0).IsEqualTo( result.Nodes[0].Children[1].Children[0].Children[1].Children.Count); // Canon Ixus 300
            Affirm.That(0).IsEqualTo( result.Nodes[0].Children[1].Children[1].Children.Count); // Kingston 256 GB SD
            Affirm.That(1).IsEqualTo( result.Nodes[0].Children[1].Children[2].Children.Count); // Sony 256 GB SD
            Affirm.That(0).IsEqualTo(result.Nodes[0].Children[1].Children[2].Children[0].Children.Count); // Sony SD Card Reader
        }



        #endregion
    }
}

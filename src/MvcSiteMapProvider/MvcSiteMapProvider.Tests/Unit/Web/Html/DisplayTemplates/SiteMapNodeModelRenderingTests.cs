using System.Web.Mvc;
using NUnit.Framework;
using MvcSiteMapProvider.Web.Html;

namespace MvcSiteMapProvider.Tests.Unit.Web.Html.DisplayTemplates
{
    [TestFixture]
    public class SiteMapNodeModelRenderingTests
    {
        [Test]
        public void Anchor_Merges_Parsed_HtmlAttributes()
        {
            // Arrange
            var attrs = HtmlAttributeParser.Parse("id=siteMapLogoutLink class=btn");

            var tag = new TagBuilder("a");
            tag.Attributes["href"] = "/Account/LogOff";
            tag.SetInnerText("Logout");
            foreach (var kv in attrs)
            {
                tag.MergeAttribute(kv.Key, kv.Value?.ToString(), true);
            }

            // Act
            var html = tag.ToString();

            // Assert
            Assert.That(html, Does.Contain("id=\"siteMapLogoutLink\""));
            Assert.That(html, Does.Contain("class=\"btn\""));
            Assert.That(html, Does.Contain("href=\"/Account/LogOff\""));
            Assert.That(html, Does.Contain(">Logout<"));
        }
    }
}

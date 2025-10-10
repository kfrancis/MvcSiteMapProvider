using NUnit.Framework;
using System.Collections.Generic;
using MvcSiteMapProvider.Web.Html;

namespace MvcSiteMapProvider.Tests.Unit.Web.Html
{
    [TestFixture]
    public class HtmlAttributeParserTests
    {
        [Test]
        public void Parse_NullOrEmpty_ReturnsEmpty()
        {
            var d1 = HtmlAttributeParser.Parse(null);
            var d2 = HtmlAttributeParser.Parse("");
            Assert.That(d1, Is.Empty);
            Assert.That(d2, Is.Empty);
        }

        [Test]
        public void Parse_SinglePair_WithSpaces_Parses()
        {
            var d = HtmlAttributeParser.Parse("id=siteMapLogoutLink");
            Assert.That(d.ContainsKey("id"), Is.True);
            Assert.That(d["id"], Is.EqualTo("siteMapLogoutLink"));
        }

        [Test]
        public void Parse_MultiplePairs_WithVariousSeparators_ParsesAll()
        {
            var d = HtmlAttributeParser.Parse("id=link1, class=btn-primary;data-x=1 data-y=two");
            Assert.Multiple(() =>
            {
                Assert.That(d["id"], Is.EqualTo("link1"));
                Assert.That(d["class"], Is.EqualTo("btn-primary"));
                Assert.That(d["data-x"], Is.EqualTo("1"));
                Assert.That(d["data-y"], Is.EqualTo("two"));
            });
        }

        [Test]
        public void Parse_IsCaseInsensitive_OnKeys()
        {
            var d = HtmlAttributeParser.Parse("ID=link1");
            Assert.That(d.ContainsKey("id"));
            Assert.That(d["id"], Is.EqualTo("link1"));
        }
    }
}

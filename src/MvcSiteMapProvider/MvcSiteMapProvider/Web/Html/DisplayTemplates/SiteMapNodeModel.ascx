<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl`1[[MvcSiteMapProvider.Web.Html.Models.SiteMapNodeModel,MvcSiteMapProvider]]" %>
<%@ Import Namespace="MvcSiteMapProvider.Web.Html" %>

<%
    var raw = Model.Attributes != null && Model.Attributes.ContainsKey("htmlAttributes") ? (Model.Attributes["htmlAttributes"]?.ToString()) : null;
    var attrs = HtmlAttributeParser.Parse(raw);
%>
<% if (Model.IsCurrentNode && Model.SourceMetadata["HtmlHelper"]?.ToString() != "MvcSiteMapProvider.Web.Html.MenuHelper")  { %>
    <%=Model.Title %>
<% } else if (Model.IsClickable) { %>
    <% if (string.IsNullOrEmpty(Model.Description)) { %>
        <a href="<%=Model.Url%>" <%= string.Join(" ", attrs.Select(kv => kv.Key + "=\"" + kv.Value + "\"")) %>><%=Model.Title %></a>
    <% } else { %>
        <% if (!attrs.ContainsKey("title")) { attrs["title"] = Model.Description; } %>
        <a href="<%=Model.Url%>" <%= string.Join(" ", attrs.Select(kv => kv.Key + "=\"" + kv.Value + "\"")) %>><%=Model.Title %></a>
    <% } %>
<% } else { %>
    <%=Model.Title %>
<% } %>

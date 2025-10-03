using System;
using System.Web;
using System.Web.Mvc;

namespace MvcSiteMapProvider.Web.Mvc;

/// <summary>
///     HttpRequest wrapper.
/// </summary>
public class SiteMapHttpRequest
    : HttpRequestWrapper
{
    private readonly ISiteMapNode? _node;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SiteMapHttpRequest" /> class.
    /// </summary>
    /// <param name="httpRequest">The object that this wrapper class provides access to.</param>
    /// <param name="node">The site map node to fake node access context for or <c>null</c>.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     <paramref name="httpRequest" /> is null.
    /// </exception>
    public SiteMapHttpRequest(HttpRequest httpRequest, ISiteMapNode node)
        : base(httpRequest)
    {
        _node = node;
    }

    /// <summary>
    ///     Gets the virtual path of the application root and makes it relative by using the tilde (~) notation for the
    ///     application root (as in "~/page.aspx").
    /// </summary>
    /// <value></value>
    /// <returns>
    ///     The virtual path of the application root for the current request with the tilde operator added.
    /// </returns>
    public override string AppRelativeCurrentExecutionFilePath
    {
        get => VirtualPathUtility.ToAppRelative(CurrentExecutionFilePath);
    }

    /// <summary>
    ///     Gets the virtual path of the current request.
    /// </summary>
    /// <value></value>
    /// <returns>
    ///     The virtual path of the page handler that is currently executing.
    /// </returns>
    public override string CurrentExecutionFilePath
    {
        get => base.FilePath;
    }

    /// <summary>
    ///     Gets the HTTP data-transfer method (such as GET, POST, or HEAD) that was used by the client.
    /// </summary>
    /// <returns>
    ///     The HTTP data-transfer method that was used by the client.
    /// </returns>
    public override string? HttpMethod
    {
        get
        {
            var useRequest = _node == null ||
                             string.Equals(_node.HttpMethod, "*") ||
                             string.Equals(_node.HttpMethod, "request", StringComparison.OrdinalIgnoreCase);
            if (!useRequest)
            {
                return string.IsNullOrEmpty(_node?.HttpMethod)
                    ? nameof(HttpVerbs.Get).ToUpperInvariant()
                    : _node?.HttpMethod.ToUpperInvariant();
            }

            return base.HttpMethod;
        }
    }
}
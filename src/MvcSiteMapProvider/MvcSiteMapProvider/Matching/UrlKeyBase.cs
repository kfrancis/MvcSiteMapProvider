using MvcSiteMapProvider.Web;
using System;

namespace MvcSiteMapProvider.Matching;

/// <summary>
/// An abstract class containing the logic for comparing 2 IUrlKey instances.
/// </summary>
public abstract class UrlKeyBase
    : IUrlKey
{
    public UrlKeyBase(
        IUrlPath urlPath
    )
    {
        this.urlPath = urlPath ?? throw new ArgumentNullException(nameof(urlPath));
        this.hostName = string.Empty;
        this.rootRelativeUrl = string.Empty;
    }

    protected IUrlPath urlPath;
    protected string hostName;
    protected string rootRelativeUrl;

    public virtual string HostName => this.hostName;

    public virtual string RootRelativeUrl => this.rootRelativeUrl;

    protected virtual void SetUrlValues(string relativeOrAbsoluteUrl)
    {
        if (this.urlPath.IsAbsolutePhysicalPath(relativeOrAbsoluteUrl) || this.urlPath.IsAppRelativePath(relativeOrAbsoluteUrl))
        {
            this.rootRelativeUrl = this.urlPath.ResolveVirtualApplicationToRootRelativeUrl(relativeOrAbsoluteUrl) ?? string.Empty;
        }
        else if (this.urlPath.IsAbsoluteUrl(relativeOrAbsoluteUrl))
        {
            var absoluteUri = new Uri(relativeOrAbsoluteUrl, UriKind.Absolute);

            // NOTE: this will cut off any fragments, but since they are not passed
            // to the server, this is desired.
            this.rootRelativeUrl = absoluteUri.PathAndQuery;
            this.hostName = absoluteUri.Host;
        }
        else
        {
            // We must assume we already have a relative root URL
            this.rootRelativeUrl = relativeOrAbsoluteUrl ?? string.Empty;
        }
    }

    // Source: http://stackoverflow.com/questions/70303/how-do-you-implement-gethashcode-for-structure-with-two-string#21604191
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = 0;

            // String properties
            hashCode = (hashCode * 397) ^ (this.HostName != null ? this.HostName.GetHashCode() : string.Empty.GetHashCode());
            hashCode = (hashCode * 397) ^ (this.RootRelativeUrl != null ? this.RootRelativeUrl.GetHashCode() : string.Empty.GetHashCode());

            //// int properties
            //hashCode = (hashCode * 397) ^ intProperty;

            return hashCode;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is not IUrlKey objB)
        {
            return false;
        }
        if (object.ReferenceEquals(this, obj))
        {
            return true;
        }
        if (!string.Equals(this.RootRelativeUrl, objB.RootRelativeUrl, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        if (!string.Equals(this.HostName, objB.HostName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        return true;
    }

    public override string ToString()
    {
        return $"[HostName: {this.HostName}, RootRelativeUrl: {this.RootRelativeUrl}]";
    }
}
using System;
using System.Web;
using MvcSiteMapProvider.Web.Mvc;

namespace MvcSiteMapProvider.Caching;

/// <summary>
///     Provides type-safe access to <see cref="P:System.Web.HttpContext.Items" />.
/// </summary>
public class RequestCache
    : IRequestCache
{
    private readonly IMvcContextFactory _mvcContextFactory;

    public RequestCache(
        IMvcContextFactory mvcContextFactory
    )
    {
        _mvcContextFactory = mvcContextFactory ?? throw new ArgumentNullException(nameof(mvcContextFactory));
    }

    private HttpContextBase Context => _mvcContextFactory.CreateHttpContext();

    public virtual T? GetValue<T>(string key)
    {
        if (Context.Items.Contains(key))
        {
            return (T)Context.Items[key];
        }

        return default;
    }

    public virtual void SetValue<T>(string key, T value)
    {
        Context.Items[key] = value;
    }
}

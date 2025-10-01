using System;
using System.Globalization;

namespace MvcSiteMapProvider.Globalization;

/// <summary>
/// An abstract factory that can be used to create new instances of <see cref="T:MvcSiteMapProvider.Globalization.ICultureContext"/>
/// at runtime.
/// </summary>
public class CultureContextFactory
    : ICultureContextFactory
{
    public ICultureContext CreateInvariant()
    {
        return new CultureContext(CultureInfo.InvariantCulture, CultureInfo.InvariantCulture);
    }

    public ICultureContext Create(string cultureName, string uiCultureName)
    {
        if (string.IsNullOrEmpty(cultureName))
            throw new ArgumentNullException(nameof(cultureName));
        if (string.IsNullOrEmpty(uiCultureName))
            throw new ArgumentNullException(nameof(uiCultureName));

        return new CultureContext(new CultureInfo(cultureName), new CultureInfo(uiCultureName));
    }

    public ICultureContext Create(CultureInfo culture, CultureInfo uiCulture)
    {
        if (culture == null)
            throw new ArgumentNullException(nameof(culture));
        if (uiCulture == null)
            throw new ArgumentNullException(nameof(uiCulture));

        return new CultureContext(culture, uiCulture);
    }
}
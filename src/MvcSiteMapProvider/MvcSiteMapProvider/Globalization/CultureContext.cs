using MvcSiteMapProvider.DI;
using System;
using System.Globalization;
using System.Threading;

namespace MvcSiteMapProvider.Globalization
{
    /// <summary>
    /// Allows switching the current thread to a new culture in a using block that will automatically
    /// return the culture to its previous state upon completion.
    /// </summary>
    [ExcludeFromAutoRegistration]
    public class CultureContext
        : ICultureContext
    {
        public CultureContext(
            CultureInfo culture,
            CultureInfo uiCulture
            )
        {
            currentThread = Thread.CurrentThread;

            // Record the current culture settings so they can be restored later.
            OriginalCulture = currentThread.CurrentCulture;
            originalUICulture = currentThread.CurrentUICulture;

            // Set both the culture and UI culture for this context.
            currentThread.CurrentCulture = culture ?? throw new ArgumentNullException(nameof(culture));
            currentThread.CurrentUICulture = uiCulture ?? throw new ArgumentNullException(nameof(uiCulture));
        }

        private readonly Thread currentThread;
        private readonly CultureInfo originalUICulture;

        public CultureInfo OriginalCulture { get; }

        public CultureInfo OriginalUICulture
        {
            get { return originalUICulture; }
        }

        public void Dispose()
        {
            // Restore the culture to the way it was before the constructor was called.
            currentThread.CurrentCulture = OriginalCulture;
            currentThread.CurrentUICulture = originalUICulture;
        }
    }
}
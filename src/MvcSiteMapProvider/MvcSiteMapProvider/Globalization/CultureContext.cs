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
            if (culture == null)
                throw new ArgumentNullException(nameof(culture));
            if (uiCulture == null)
                throw new ArgumentNullException(nameof(uiCulture));

            this.currentThread = Thread.CurrentThread;

            // Record the current culture settings so they can be restored later.
            this.OriginalCulture = this.currentThread.CurrentCulture;
            this.OriginalUICulture = this.currentThread.CurrentUICulture;

            // Set both the culture and UI culture for this context.
            this.currentThread.CurrentCulture = culture;
            this.currentThread.CurrentUICulture = uiCulture;
        }

        private readonly Thread currentThread;

        public CultureInfo OriginalCulture { get; }

        public CultureInfo OriginalUICulture { get; }

        public void Dispose()
        {
            // Restore the culture to the way it was before the constructor was called.
            this.currentThread.CurrentCulture = this.OriginalCulture;
            this.currentThread.CurrentUICulture = this.OriginalUICulture;
        }
    }
}

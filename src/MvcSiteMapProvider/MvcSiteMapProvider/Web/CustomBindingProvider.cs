using System;
using System.Collections.Generic;

namespace MvcSiteMapProvider.Web
{
    /// <summary>
    /// Class that can be used to explicitly specify binding configuration by passing
    /// <see cref="T:MvcSiteMapProvider.Web.IBinding"/> instances into the constructor.
    /// </summary>
    public class CustomBindingProvider
        : IBindingProvider
    {
        protected readonly IEnumerable<IBinding> bindings;

        public CustomBindingProvider(
                    IEnumerable<IBinding> bindings
            )
        {
            this.bindings = bindings ?? throw new ArgumentNullException(nameof(bindings));
        }

        public IEnumerable<IBinding> GetBindings()
        {
            return bindings;
        }
    }
}
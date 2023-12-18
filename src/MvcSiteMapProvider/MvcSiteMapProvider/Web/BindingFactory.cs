namespace MvcSiteMapProvider.Web
{
    /// <summary>
    /// An abstract factory that creates new instances of <see cref="T:MvcSiteMapProvider.Web.Binding"/> at runtime.
    /// </summary>
    public class BindingFactory
        : IBindingFactory
    {
        public IBinding Create(string hostName, string protocol, int port)
        {
            return new Binding(hostName, protocol, port);
        }
    }
}
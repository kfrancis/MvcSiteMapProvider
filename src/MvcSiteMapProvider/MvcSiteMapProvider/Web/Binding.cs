using MvcSiteMapProvider.DI;
using System;

namespace MvcSiteMapProvider.Web
{
    /// <summary>
    /// Represents a binding between host name, protocol, and port.
    /// This class can be used to determine the port when generating a URL by
    /// matching the host name and protocol.
    /// </summary>
    [ExcludeFromAutoRegistration]
    public class Binding
        : IBinding
    {
        public Binding(
            string hostName,
            string protocol,
            int port
            )
        {
            if (string.IsNullOrEmpty(hostName))
                throw new ArgumentNullException(nameof(hostName));
            if (string.IsNullOrEmpty(protocol))
                throw new ArgumentNullException(nameof(protocol));

            HostName = hostName;
            Protocol = protocol;
            Port = port;
        }

        public string HostName { get; }

        public int Port { get; }
        public string Protocol { get; }
    }
}
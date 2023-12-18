using System;
using System.IO;
using System.Xml.Linq;

namespace MvcSiteMapProvider.Xml
{
    /// <summary>
    /// Provides an XDocument instance based on an XML file source.
    /// </summary>
    public class FileXmlSource
        : IXmlSource
    {
        protected readonly string fileName;

        /// <summary>
        /// Creates a new instance of FileXmlSource.
        /// </summary>
        /// <param name="fileName">The absolute path to the Xml file.</param>
        public FileXmlSource(
            string fileName
            )
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            this.fileName = fileName;
        }

        public XDocument GetXml()
        {
            if (File.Exists(fileName))
            {
                return XDocument.Load(fileName);
            }
            else
            {
                throw new FileNotFoundException(string.Format(Resources.Messages.XmlFileNotFound, fileName), fileName);
            }
        }
    }
}
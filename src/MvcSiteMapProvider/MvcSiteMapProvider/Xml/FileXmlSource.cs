using System;
using System.IO;
using System.Xml.Linq;
using MvcSiteMapProvider.Resources;

namespace MvcSiteMapProvider.Xml
{
    /// <summary>
    ///     Provides an XDocument instance based on an XML file source.
    /// </summary>
    public class FileXmlSource
        : IXmlSource
    {
        private readonly string _fileName;

        /// <summary>
        ///     Creates a new instance of FileXmlSource.
        /// </summary>
        /// <param name="fileName">The absolute path to the Xml file.</param>
        public FileXmlSource(
            string fileName
        )
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            this._fileName = fileName;
        }

        public XDocument GetXml()
        {
            XDocument? result;
            if (File.Exists(_fileName))
            {
                result = XDocument.Load(_fileName);
            }
            else
            {
                throw new FileNotFoundException(string.Format(Messages.XmlFileNotFound, _fileName), _fileName);
            }

            return result;
        }
    }
}

using System;
using System.Xml;
using System.Xml.Schema;

namespace MvcSiteMapProvider.Xml
{
    /// <summary>
    /// Validates an XML file against an XSD schema. Throws an exception if it fails.
    /// </summary>
    public class SiteMapXmlValidator 
        : ISiteMapXmlValidator
    {
        public void ValidateXml(string xmlPath)
        {
            const string resourceNamespace = "MvcSiteMapProvider.Xml";
            const string resourceFileName = "MvcSiteMapSchema.xsd";

            var xsdPath = resourceNamespace + "." + resourceFileName;
            var xsdStream = this.GetType().Assembly.GetManifestResourceStream(xsdPath);
            using (var xsd = XmlReader.Create(xsdStream))
            {
                var schema = new XmlSchemaSet();
                schema.Add(null, xsd);

                var xmlReaderSettings = new XmlReaderSettings
                {
                    ValidationType = ValidationType.Schema
                };
                xmlReaderSettings.Schemas.Add(schema);
                //xmlReaderSettings.ValidationEventHandler += new ValidationEventHandler(ValidationHandler);

                using (var xmlReader = XmlReader.Create(xmlPath, xmlReaderSettings))
                {
                    try
                    {
                        while (xmlReader.Read()) ;
                    }
                    catch (Exception ex)
                    {
                        throw new MvcSiteMapException(string.Format(Resources.Messages.XmlValidationFailed, xmlPath), ex);
                    }
                }
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;

namespace INZFS.Theme.KeyManagements
{
    public static class SelfSignedCertificateEncryptorExtensions
    {

        public static XElement EncryptIfNecessary(this IXmlEncryptor encryptor, XElement element)
        {
            if (!DoesElementOrDescendentRequireEncryption(element))
            {
                return null;
            }
            var doc = new XDocument(new XElement(element));

            var placeholderReplacements = new Dictionary<XElement, EncryptedXmlInfo>();

            while (true)
            {
                var elementWhichRequiresEncryption = doc.Descendants().FirstOrDefault(DoesSingleElementRequireEncryption);
                if (elementWhichRequiresEncryption == null)
                {
                    break;
                }

                var clonedElementWhichRequiresEncryption = new XElement(elementWhichRequiresEncryption);
                var innerDoc = new XDocument(clonedElementWhichRequiresEncryption);
                var encryptedXmlInfo = encryptor.Encrypt(clonedElementWhichRequiresEncryption);
                var newPlaceholder = new XElement("placeholder");
                placeholderReplacements[newPlaceholder] = encryptedXmlInfo;
                elementWhichRequiresEncryption.ReplaceWith(newPlaceholder);
            }
            foreach (var entry in placeholderReplacements)
            {
                entry.Key.ReplaceWith(
                    new XElement(SelfSignedCertificateEncryptorXmlConstants.EncryptedSecretElementName,
                        new XAttribute(SelfSignedCertificateEncryptorXmlConstants.DecryptorTypeAttributeName, entry.Value.DecryptorType.AssemblyQualifiedName),
                        entry.Value.EncryptedElement));
            }
            return doc.Root;
        }

        private static bool DoesElementOrDescendentRequireEncryption(XElement element)
        {
            return element.DescendantsAndSelf().Any(DoesSingleElementRequireEncryption);
        }

        private static bool DoesSingleElementRequireEncryption(XElement element)
        {
            return element.IsMarkedAsRequiringEncryption();
        }

        internal static bool IsMarkedAsRequiringEncryption(this XElement element)
        {
            return ((bool?)element.Attribute(SelfSignedCertificateEncryptorXmlConstants.RequiresEncryptionAttributeName)).GetValueOrDefault();
        }
    }
}
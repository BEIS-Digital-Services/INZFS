using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace INZFS.Theme.KeyManagements
{
    public class TemporaryDpKeyGeneratorXmlRepository : IXmlRepository
    {
        Dictionary<string, XElement> store = new Dictionary<string, XElement>();
        private readonly KeyManagementOptions _keyManagementOptions;
        public const string _friendlyNameId = "24ed354a-e377-40fb-8105-50db734541f1";

        private readonly string _secret;

        internal static readonly XName KeyElementName = "key";
        internal static readonly XName IdAttributeName = "id";
        internal static readonly XName VersionAttributeName = "version";
        internal static readonly XName CreationDateElementName = "creationDate";
        internal static readonly XName ActivationDateElementName = "activationDate";
        internal static readonly XName ExpirationDateElementName = "expirationDate";
        internal static readonly XName DescriptorElementName = "descriptor";
        internal static readonly XName DeserializerTypeAttributeName = "deserializerType";

        public TemporaryDpKeyGeneratorXmlRepository(KeyManagementOptions keyManagementOptions, string environment, string protectionKey)
        {
            _keyManagementOptions = keyManagementOptions;
            _secret = $"{environment}-{protectionKey}";
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            if (store == null || !store.Any())
            {
                return Enumerable.Empty<XElement>().ToList().AsReadOnly();
            }
            return store.Values.ToList().AsReadOnly();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            var now = DateTimeOffset.UtcNow;
            var xElement = GetElement(element, Guid.Parse(_friendlyNameId), now, now, expirationDate: now + _keyManagementOptions.NewKeyLifetime);
            var encryptElement = _keyManagementOptions.XmlEncryptor?.EncryptIfNecessary(xElement);
            var allElement = encryptElement ?? xElement;

            if (store.ContainsKey(_friendlyNameId))
            {
                store[_friendlyNameId] = allElement;
            }
            else
            {
                store.Add(_friendlyNameId, allElement);
            }
        }

        public XElement GetElement(XElement element, Guid keyId, DateTimeOffset creationDate, DateTimeOffset activationDate, DateTimeOffset expirationDate)
        {
            var newDescriptor = new AuthenticatedEncryptorDescriptor((AuthenticatedEncryptorConfiguration)
                _keyManagementOptions.AuthenticatedEncryptorConfiguration, new Secret(Encoding.UTF8.GetBytes(_secret)));

            var descriptorXmlInfo = newDescriptor.ExportToXml();

            // build the <key> element
            var keyElement = new XElement(KeyElementName,
                new XAttribute(IdAttributeName, keyId),
                new XAttribute(VersionAttributeName, 1),
                new XElement(CreationDateElementName, creationDate),
                new XElement(ActivationDateElementName, activationDate),
                new XElement(ExpirationDateElementName, expirationDate),
                new XElement(DescriptorElementName,
                    new XAttribute(DeserializerTypeAttributeName, descriptorXmlInfo.DeserializerType.AssemblyQualifiedName),
                    descriptorXmlInfo.SerializedDescriptorElement));

            return keyElement;
        }
    }
}
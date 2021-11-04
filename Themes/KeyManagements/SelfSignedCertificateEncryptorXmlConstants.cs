using System.Threading.Tasks;
using System.Xml.Linq;

namespace INZFS.Theme.KeyManagements
{
    internal static class SelfSignedCertificateEncryptorXmlConstants
    {
        private static readonly XNamespace RootNamespace = XNamespace.Get("http://schemas.asp.net/2015/03/dataProtection");
        internal static readonly XName DecryptorTypeAttributeName = "decryptorType";
        internal static readonly XName EncryptedSecretElementName = RootNamespace.GetName("encryptedSecret");
        internal static readonly XName RequiresEncryptionAttributeName = RootNamespace.GetName("requiresEncryption");
    }
}

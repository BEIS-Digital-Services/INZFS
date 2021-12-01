using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;

namespace Galebra.Security.Headers.Csp.Infrastructure
{ 

/// <summary>
/// <inheritdoc cref="INonceGenerator"/>
/// </summary>
internal sealed class NonceGenerator : INonceGenerator
{
    private readonly string _nonce;

    private NonceGenerator(int numOfBytes)
    {
            //One byte can take 256 values (0 to 255)
            //16 bytes gives 256^16=2^128 possibilities, or a 22 long webencoded base64 string
            //8 gives 256^8=2^64 possibilities, or an 11 long base64 string
            //Youtube video ids seem to be 64^11, which is 2^66
            //var bytes = RandomNumberGenerator.GetBytes(9);
            var byteArray = new byte[numOfBytes];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(byteArray);

            _nonce = WebEncoders.Base64UrlEncode(byteArray);
    }

    public NonceGenerator(CspPolicyGroup policyGroup)
        : this(policyGroup.NumberOfNonceBytes)
    {
    }

    public string Nonce => _nonce;
    public string NonceHeader => string.Concat("'nonce-", _nonce, "'");
}

}
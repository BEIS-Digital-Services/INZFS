namespace Galebra.Security.Headers.Csp.Infrastructure
{ 

/// <summary>
/// Used for nonce generators
/// </summary>
public interface INonceGenerator
{
    string Nonce { get; }
    string NonceHeader { get; }
}

}
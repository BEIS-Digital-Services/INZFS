using System.Collections.Generic;

namespace Galebra.Security.Headers.Csp.Infrastructure
{
    public interface ICspNonce
    {
        Dictionary<string, INonceGenerator> Nonces { get; init; }
        public void GenerateNonces();
    }
}
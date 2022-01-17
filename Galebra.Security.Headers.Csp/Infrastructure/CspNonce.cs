using System.Collections.Generic;

namespace Galebra.Security.Headers.Csp.Infrastructure
{
    public class CspNonce : ICspNonce
    {
        public Dictionary<string, INonceGenerator> Nonces { get; init; }
        private readonly IDictionary<string, CspPolicyGroup> _cspPolicyGroups;
        public CspNonce(IDictionary<string, CspPolicyGroup> policyGroups)
        {
                _cspPolicyGroups = policyGroups;
                Nonces = new Dictionary<string, INonceGenerator>();
        }

        public void GenerateNonces()
    {
            Nonces.Clear();
            foreach (var group in _cspPolicyGroups)
        {
            Nonces.Add(group.Key, new NonceGenerator(group.Value));
        }
    }
    }
}
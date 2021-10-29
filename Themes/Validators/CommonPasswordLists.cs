using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace INZFS.Theme.Validators
{
    public class CommonPasswordLists : ICommonPasswordLists
    {
        const string fileName = "INZFS.Theme.Validators>CommonPasswords.txt";

        private readonly ILogger<CommonPasswordLists> _logger;
        private HashSet<string> passwordListHashSet;
        
        public CommonPasswordLists(ILogger<CommonPasswordLists> logger)
        {
            _logger = logger;
        }
        
        public HashSet<string> GetPasswords()
        {
            return passwordListHashSet ??= LoadList(fileName);
        }

        private HashSet<string> LoadList(string resourceName)
        {
            HashSet<string> hashset;
            
            var assembly = typeof(CommonPasswordLists).GetTypeInfo().Assembly;
            using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                using var streamReader = new StreamReader(resourceStream);
                hashset = new HashSet<string>(GetPasswords(streamReader));
            }

            _logger.LogDebug("The {file} is loaded for common passwords and count is {CommonPasswordCount}", resourceName, hashset.Count);

            return new HashSet<string>(hashset.OrderByDescending(e=> e.Length), StringComparer.OrdinalIgnoreCase);
        }

        private IEnumerable<string> GetPasswords(StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                yield return reader.ReadLine(); 
            }
        }
    }
}




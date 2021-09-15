using INZFS.MVC.Models;
using Microsoft.AspNetCore.Http;
using nClam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.VirusScan
{
    public class VirusScanService : IVirusScanService
    {
        private readonly ClamClient _clam;
        public VirusScanService(ClamClient clam)
        {
            _clam = clam;
        }
        public async Task<bool> ScanFile(IFormFile file)
        {
            var log = new List<ScanResult>();
            if (file.Length > 0)
            {
                var extension = file.FileName.Contains('.')
                    ? file.FileName.Substring(file.FileName.LastIndexOf('.'), file.FileName.Length - file.FileName.LastIndexOf('.'))
                    : string.Empty;
                var newfile = new Models.File
                {
                    Name = $"{Guid.NewGuid()}{extension}",
                    Alias = file.FileName,
                    ContentType = file.ContentType,
                    Size = file.Length,
                    Uploaded = DateTime.UtcNow,
                };
                var ping = await _clam.PingAsync();

                if (ping)
                {
                    
                    var result = await _clam.SendAndScanFileAsync(file.OpenReadStream());

                    newfile.ScanResult = result.Result.ToString();
                    newfile.Infected = result.Result == ClamScanResults.VirusDetected;
                    newfile.Scanned = DateTime.UtcNow;
                    if (result.InfectedFiles != null)
                    {
                        foreach (var infectedFile in result.InfectedFiles)
                        {
                            newfile.Viruses.Add(new Virus
                            {
                                Name = infectedFile.VirusName
                            });
                        }
                        return false;
                    }
                    else
                    {
                        var metaData = new Dictionary<string, string>
                        {
                            { "av-status", result.Result.ToString() },
                            { "av-timestamp", DateTime.UtcNow.ToString() },
                            { "alias", newfile.Alias }
                        };

                        var scanResult = new ScanResult()
                        {
                            FileName = file.FileName,
                            Result = result.Result.ToString(),
                            Message = result.InfectedFiles?.FirstOrDefault()?.VirusName,
                            RawResult = result.RawResult
                        };
                        log.Add(scanResult);
                    }
                    return true;
                }
                else
                {
                   
                    return false;
                }

            }
            return false;
        }
    }
}

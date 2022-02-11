using INZFS.MVC.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloudmersive.APIClient.NETCore.VirusScan.Api;
using Cloudmersive.APIClient.NETCore.VirusScan.Client;
using Cloudmersive.APIClient.NETCore.VirusScan.Model;


namespace INZFS.MVC.Services.VirusScan
{
    public class VirusScanService : IVirusScanService
    {
        public string ScanFile(IFormFile file, string cloudmersiveApiKey)
        {
            Configuration.Default.AddApiKey("Apikey", cloudmersiveApiKey);
            var apiInstance = new ScanApi();
            var inputFile = file.OpenReadStream(); 
            var allowExecutables = false;  
            var allowInvalidFiles = false;  
            var allowScripts = false;  
            var allowPasswordProtectedFiles = false;  
            var allowMacros = true;  
            var allowXmlExternalEntities = true;  
            var restrictFileTypes = ".xlsx,.xlx,.pdf,.doc,.docx,.ppt,.pptx, .gif, .jpeg, .png";  

            try
            {
                
                VirusScanAdvancedResult result = apiInstance.ScanFileAdvanced(inputFile, allowExecutables, allowInvalidFiles, allowScripts, allowPasswordProtectedFiles, allowMacros, allowXmlExternalEntities, restrictFileTypes);
                Debug.WriteLine(result);
                if(result.FoundViruses != null)
                {
                    Debug.WriteLine("The following viruses were found: " + result.FoundViruses.ToString());
                    return "Virus";
                   
                }
                else
                {
                    Debug.WriteLine("Virus scanning complete. Your upload is virus-free!");
                    return "Clean";
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception when calling ScanApi.ScanFileAdvanced: " + e.Message);
                return "Exception";
            }
        }
     
    }
}

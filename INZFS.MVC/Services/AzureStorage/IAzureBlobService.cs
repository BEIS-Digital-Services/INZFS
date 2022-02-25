using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.AzureStorage
{
    public interface IAzureBlobService
    {
        public Task<string> AddFileToBlobStorage(FormFile file);
        public Task<bool> AddApplicationToBlobStorage();
    }
}

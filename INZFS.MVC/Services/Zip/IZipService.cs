using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.Zip
{
    public interface IZipService
    {
        public Task<byte[]> GetZipFileBytes(string filetype, string userId, bool includeJsonSummary = false);
        public Task<string> GetApplicationId(string userId);

        public Task<string> GetApplicationCompanyName(string userId);
    }
}

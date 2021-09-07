using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.AntiForgery
{
    public interface IAntiforgeryTokenGenerator
    {
        public string GenerateAntiforgeryToken();
        public void SetAntiforgeryResponseHeader();
    }
}

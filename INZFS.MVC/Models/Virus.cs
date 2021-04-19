using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models
{
    public class Virus
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public string Name { get; set; }

        public File File { get; set; }
    }
}

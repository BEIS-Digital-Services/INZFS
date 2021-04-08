using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Models
{
    public class File
    {
        public File()
        {
            Viruses = new HashSet<Virus>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
        public DateTime Uploaded { get; set; }
        public string ScanResult { get; set; }
        public bool Infected { get; set; }
        public DateTime Scanned { get; set; }

        public ICollection<Virus> Viruses { get; set; }
    }
}

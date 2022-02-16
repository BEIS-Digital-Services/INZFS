using Microsoft.AspNetCore.Mvc;

namespace INZFS.Web.POCO
{
    public class EligibilityCheckerPOCO
    {
        
        public bool Question1 { get; set; } 
       
        public bool Question2 { get; set; } 
       
        public bool Question3 { get; set; } 
        
        public bool Question4 { get; set; } 
       
        public bool Question5 { get; set; } 
   
        public bool Question6 { get; set; } 
        [BindProperty]
        public string RedirectUrl { get; set; }

        public EligibilityCheckerPOCO()
        {
        }
    }
   
}

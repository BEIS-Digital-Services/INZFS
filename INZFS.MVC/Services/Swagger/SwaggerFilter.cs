using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Services.Swagger
{
    public class SwaggerFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
            {
                var nonMobileRoutes = swaggerDoc.Paths
                    .Where(x => !x.Key.ToLower().Contains("api/listapplications"))
                    .ToList();
                nonMobileRoutes.ForEach(x => { swaggerDoc.Paths.Remove(x.Key); });
            }
        
    }
}

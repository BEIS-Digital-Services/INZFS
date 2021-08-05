using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace INZFS.Theme
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static ResourceManifest _manifest;

        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("INZFS.Theme")
                .SetUrl("~/INZFS.Theme/css/govuk-frontend-3.11.0.min.css", "~/INZFS.Theme/css/main.css")

                .SetVersion("1.0.0");
        }

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}

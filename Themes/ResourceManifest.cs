using OrchardCore.ResourceManagement;

namespace INZFS.Theme
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineStyle("INZFS.Theme")
                .SetUrl("~/INZFS.Theme/css/govuk-frontend-3.11.0.min.css", "~/INZFS.Theme/css/main.css")
                .SetVersion("1.0.0");
				
        }
    }
}

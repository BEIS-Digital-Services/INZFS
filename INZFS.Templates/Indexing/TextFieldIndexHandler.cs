using System.Threading.Tasks;
using INZFS.Templates.Fields;
using OrchardCore.Indexing;

namespace INZFS.Templates.Indexing
{
    public class GovTextFieldIndexHandler : ContentFieldIndexHandler<GovTextField>
    {
        public override Task BuildIndexAsync(GovTextField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();

            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key, field.Text, options);
            }

            return Task.CompletedTask;
        }
    }
}
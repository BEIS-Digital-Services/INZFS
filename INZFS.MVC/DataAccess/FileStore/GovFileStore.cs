using OrchardCore.FileStorage;
using OrchardCore.FileStorage.FileSystem;

namespace INZFS.MVC.Services
{

    public interface IGovFileStore : IFileStore
    {
    }

    public class GovFileStore : FileSystemStore, IGovFileStore
    {
        public GovFileStore(string fileSystemPath)
            : base(fileSystemPath)
        {
        }
    }
}
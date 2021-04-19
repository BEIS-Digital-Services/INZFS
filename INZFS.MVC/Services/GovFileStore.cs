using OrchardCore.FileStorage;
using OrchardCore.FileStorage.FileSystem;
using System.Diagnostics.CodeAnalysis;

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
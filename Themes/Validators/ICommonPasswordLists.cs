using System.Collections.Generic;

namespace INZFS.Theme.Validators
{
    public interface ICommonPasswordLists
    {
        HashSet<string> GetPasswords();
    }
}
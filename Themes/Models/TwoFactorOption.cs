namespace INZFS.Theme.Models
{
    public class TwoFactorOption
    {
        public TwoFactorStatus Status { get; set; }
        public string AccountName { get; set; }
    }

    public enum TwoFactorStatus
    {
        Default,
        Optional,
        Disabled
    }

}
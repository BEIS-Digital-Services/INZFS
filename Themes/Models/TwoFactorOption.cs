namespace INZFS.Theme.Models
{
    public class TwoFactorOption
    {
        public TwoFactorStatus Status { get; set; }
        public string AccountName { get; set; }
    }

    public class TimeOutOption
    {
        public int WarningStart { get; set; } = 20;
        public int WarningDuration { get; set; } = 10;
    }
    public enum TwoFactorStatus
    {
        Default,
        Optional,
        Disabled
    }
}
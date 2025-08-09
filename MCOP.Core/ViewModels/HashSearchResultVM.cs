namespace MCOP.Core.ViewModels
{
    public sealed class HashSearchResultVM
    {
        public ulong? MessageId { get; set; } = null;
        public ulong? MessageIdNormalized { get; set; } = null;
        public required byte[] HashToCheck { get; set; }
        public byte[]? HashFound { get; set; } = null;
        public byte[]? HashFoundNormalized { get; set; } = null;
        public double Difference { get; set; } = 0;
        public double DifferenceNormalized { get; set; } = 0;
    }
}

namespace Shared.Domain.Abstractions
{
    public interface IConcurrencyToken
    {
        byte[]? RowVersion { get; }
    }
}

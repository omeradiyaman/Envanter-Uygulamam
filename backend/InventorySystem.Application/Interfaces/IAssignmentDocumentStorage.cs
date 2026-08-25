namespace InventorySystem.Application.Interfaces;

public interface IAssignmentDocumentStorage
{
    Task<string> SaveAsync(byte[] content, string extension, CancellationToken cancellationToken);
    Task<byte[]> ReadAsync(string storedFileName, CancellationToken cancellationToken);
}

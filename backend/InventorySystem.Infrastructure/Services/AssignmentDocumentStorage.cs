using InventorySystem.Application.Common.Exceptions;
using InventorySystem.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace InventorySystem.Infrastructure.Services;

public sealed class AssignmentDocumentStorage : IAssignmentDocumentStorage
{
    private readonly string rootPath;

    public AssignmentDocumentStorage(IConfiguration configuration)
    {
        var configuredPath = configuration["DocumentStorage:RootPath"] ?? "storage/assignment-documents";
        rootPath = Path.GetFullPath(configuredPath, AppContext.BaseDirectory);
        Directory.CreateDirectory(rootPath);
    }

    public async Task<string> SaveAsync(byte[] content, string extension, CancellationToken cancellationToken)
    {
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = ResolveSafePath(storedFileName);
        await using var stream = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            FileOptions.Asynchronous);
        await stream.WriteAsync(content, cancellationToken);
        return storedFileName;
    }

    public async Task<byte[]> ReadAsync(string storedFileName, CancellationToken cancellationToken)
    {
        var fullPath = ResolveSafePath(storedFileName);
        if (!File.Exists(fullPath))
        {
            throw new NotFoundException("Belge dosyası depolama alanında bulunamadı.");
        }

        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }

    private string ResolveSafePath(string storedFileName)
    {
        if (string.IsNullOrWhiteSpace(storedFileName)
            || Path.GetFileName(storedFileName) != storedFileName)
        {
            throw new InvalidOperationException("Geçersiz belge dosya adı.");
        }

        var fullPath = Path.GetFullPath(Path.Combine(rootPath, storedFileName));
        var rootWithSeparator = rootPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Belge yolu depolama alanının dışında olamaz.");
        }

        return fullPath;
    }
}

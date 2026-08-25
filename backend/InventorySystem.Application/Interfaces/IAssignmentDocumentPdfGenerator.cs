using InventorySystem.Application.DTOs;

namespace InventorySystem.Application.Interfaces;

public interface IAssignmentDocumentPdfGenerator
{
    byte[] Generate(AssignmentDocumentPdfModel model);
}

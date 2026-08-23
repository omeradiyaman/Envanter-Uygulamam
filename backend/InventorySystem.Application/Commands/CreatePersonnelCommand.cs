using InventorySystem.Application.DTOs;
using MediatR;

namespace InventorySystem.Application.Commands;

public sealed record CreatePersonnelCommand(
    string SicilNo,
    string Ad,
    string Soyad,
    string Departman,
    string Pozisyon,
    string? ZimmetNo,
    bool AktifMi) : IRequest<PersonnelDetailDto>;

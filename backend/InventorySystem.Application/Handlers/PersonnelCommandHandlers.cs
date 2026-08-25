using InventorySystem.Application.Commands;
using InventorySystem.Application.Common.Exceptions;
using InventorySystem.Application.Common.Audit;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using MediatR;

namespace InventorySystem.Application.Handlers;

public sealed class CreatePersonnelCommandHandler(
    IPersonnelRepository repository,
    IAuditLogService auditLogService)
    : IRequestHandler<CreatePersonnelCommand, PersonnelDetailDto>
{
    public async Task<PersonnelDetailDto> Handle(
        CreatePersonnelCommand request,
        CancellationToken cancellationToken)
    {
        if (await repository.SicilNoExistsAsync(
                request.SicilNo,
                null,
                cancellationToken))
        {
            throw new ConflictException("Bu sicil numarası aktif bir personel tarafından kullanılıyor.");
        }

        var personnel = new Personnel(
            request.SicilNo,
            request.Ad,
            request.Soyad,
            request.Departman,
            request.Pozisyon,
            request.ZimmetNo,
            request.AktifMi);

        await repository.AddAsync(personnel, cancellationToken);
        await auditLogService.RecordAsync(
            AuditActionTypes.PersonnelCreated, AuditEntityTypes.Personnel, personnel.Id,
            $"{personnel.Ad} {personnel.Soyad} personeli eklendi.", null,
            AuditSnapshots.Personnel(personnel), true, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return personnel.ToDetailDto();
    }
}

public sealed class UpdatePersonnelCommandHandler(
    IPersonnelRepository repository,
    IDateTimeProvider dateTimeProvider,
    IAuditLogService auditLogService)
    : IRequestHandler<UpdatePersonnelCommand, PersonnelDetailDto>
{
    public async Task<PersonnelDetailDto> Handle(
        UpdatePersonnelCommand request,
        CancellationToken cancellationToken)
    {
        var personnel = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Personel kaydı bulunamadı.");

        if (await repository.SicilNoExistsAsync(
                request.SicilNo,
                request.Id,
                cancellationToken))
        {
            throw new ConflictException("Bu sicil numarası aktif bir personel tarafından kullanılıyor.");
        }

        var oldValues = AuditSnapshots.Personnel(personnel);
        personnel.Update(
            request.SicilNo,
            request.Ad,
            request.Soyad,
            request.Departman,
            request.Pozisyon,
            request.ZimmetNo,
            request.AktifMi,
            dateTimeProvider.UtcNow);

        await auditLogService.RecordAsync(
            AuditActionTypes.PersonnelUpdated, AuditEntityTypes.Personnel, personnel.Id,
            $"{personnel.Ad} {personnel.Soyad} personeli güncellendi.", oldValues,
            AuditSnapshots.Personnel(personnel), true, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return personnel.ToDetailDto();
    }
}

public sealed class DeletePersonnelCommandHandler(
    IPersonnelRepository repository,
    IDateTimeProvider dateTimeProvider,
    IAuditLogService auditLogService)
    : IRequestHandler<DeletePersonnelCommand>
{
    public async Task Handle(
        DeletePersonnelCommand request,
        CancellationToken cancellationToken)
    {
        var personnel = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Personel kaydı bulunamadı.");

        var oldValues = AuditSnapshots.Personnel(personnel);
        personnel.SoftDelete(dateTimeProvider.UtcNow);
        await auditLogService.RecordAsync(
            AuditActionTypes.PersonnelDeleted, AuditEntityTypes.Personnel, personnel.Id,
            $"{personnel.Ad} {personnel.Soyad} personeli silindi.", oldValues,
            AuditSnapshots.Personnel(personnel), true, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}

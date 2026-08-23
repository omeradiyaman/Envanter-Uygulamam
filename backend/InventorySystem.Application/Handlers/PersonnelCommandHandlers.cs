using InventorySystem.Application.Commands;
using InventorySystem.Application.Common.Exceptions;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using MediatR;

namespace InventorySystem.Application.Handlers;

public sealed class CreatePersonnelCommandHandler(
    IPersonnelRepository repository)
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
        await repository.SaveChangesAsync(cancellationToken);

        return personnel.ToDetailDto();
    }
}

public sealed class UpdatePersonnelCommandHandler(
    IPersonnelRepository repository,
    IDateTimeProvider dateTimeProvider)
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

        personnel.Update(
            request.SicilNo,
            request.Ad,
            request.Soyad,
            request.Departman,
            request.Pozisyon,
            request.ZimmetNo,
            request.AktifMi,
            dateTimeProvider.UtcNow);

        await repository.SaveChangesAsync(cancellationToken);
        return personnel.ToDetailDto();
    }
}

public sealed class DeletePersonnelCommandHandler(
    IPersonnelRepository repository,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<DeletePersonnelCommand>
{
    public async Task Handle(
        DeletePersonnelCommand request,
        CancellationToken cancellationToken)
    {
        var personnel = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Personel kaydı bulunamadı.");

        personnel.SoftDelete(dateTimeProvider.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
    }
}

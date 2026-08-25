using InventorySystem.Application.Commands;
using InventorySystem.Application.Common.Exceptions;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using MediatR;

namespace InventorySystem.Application.Handlers;

public sealed class CreateDeviceCategoryCommandHandler(IDeviceRepository repository)
    : IRequestHandler<CreateDeviceCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateDeviceCategoryCommand request, CancellationToken cancellationToken)
    {
        if (await repository.CategoryNameExistsAsync(request.Name, null, cancellationToken))
            throw new ConflictException("Bu adla bir cihaz kategorisi zaten bulunuyor.");
        var category = new DeviceCategory(request.Name, request.Description);
        await repository.AddCategoryAsync(category, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}

public sealed class UpdateDeviceCategoryCommandHandler(IDeviceRepository repository, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<UpdateDeviceCategoryCommand>
{
    public async Task Handle(UpdateDeviceCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetCategoryByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Cihaz kategorisi bulunamadı.");
        if (await repository.CategoryNameExistsAsync(request.Name, request.Id, cancellationToken))
            throw new ConflictException("Bu adla bir cihaz kategorisi zaten bulunuyor.");
        category.Update(request.Name, request.Description, dateTimeProvider.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
    }
}

public sealed class DeleteDeviceCategoryCommandHandler(IDeviceRepository repository, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<DeleteDeviceCategoryCommand>
{
    public async Task Handle(DeleteDeviceCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetCategoryByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Cihaz kategorisi bulunamadı.");
        if (await repository.CategoryDeviceCountAsync(request.Id, cancellationToken) > 0)
            throw new BusinessRuleException("İçinde cihaz bulunan kategori silinemez. Önce cihazları başka kategoriye taşıyın.");
        category.SoftDelete(dateTimeProvider.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
    }
}

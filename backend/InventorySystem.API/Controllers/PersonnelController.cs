using InventorySystem.Application.Commands;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace InventorySystem.API.Controllers;
[ApiController]
[Route("api/personnel")]
public sealed class PersonnelController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PersonnelListResponse>> GetList([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) => Ok(await sender.Send(new GetPersonnelListQuery(search, page, pageSize), cancellationToken));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PersonnelDetailDto>> GetById(Guid id, CancellationToken cancellationToken) => Ok(await sender.Send(new GetPersonnelByIdQuery(id), cancellationToken));
    [HttpPost]
    public async Task<ActionResult<PersonnelDetailDto>> Create(CreatePersonnelRequest request, CancellationToken cancellationToken) => CreatedAtAction(nameof(GetById), new { id = (await sender.Send(new CreatePersonnelCommand(request.SicilNo, request.Ad, request.Soyad, request.Departman, request.Pozisyon, request.Eposta, request.ZimmetNo, request.AktifMi), cancellationToken)).Id }, request);
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PersonnelDetailDto>> Update(Guid id, UpdatePersonnelRequest request, CancellationToken cancellationToken) => Ok(await sender.Send(new UpdatePersonnelCommand(id, request.SicilNo, request.Ad, request.Soyad, request.Departman, request.Pozisyon, request.Eposta, request.ZimmetNo, request.AktifMi), cancellationToken));
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) { await sender.Send(new DeletePersonnelCommand(id), cancellationToken); return NoContent(); }
}

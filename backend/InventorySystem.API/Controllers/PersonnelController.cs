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
    [ProducesResponseType<IReadOnlyList<PersonnelListItemDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PersonnelListItemDto>>> GetList(
        CancellationToken cancellationToken)
    {
        var personnel = await sender.Send(
            new GetPersonnelListQuery(),
            cancellationToken);

        return Ok(personnel);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<PersonnelDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonnelDetailDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var personnel = await sender.Send(
            new GetPersonnelByIdQuery(id),
            cancellationToken);

        return Ok(personnel);
    }

    [HttpPost]
    [ProducesResponseType<PersonnelDetailDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PersonnelDetailDto>> Create(
        CreatePersonnelRequest request,
        CancellationToken cancellationToken)
    {
        var personnel = await sender.Send(
            new CreatePersonnelCommand(
                request.SicilNo,
                request.Ad,
                request.Soyad,
                request.Departman,
                request.Pozisyon,
                request.ZimmetNo,
                request.AktifMi),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = personnel.Id }, personnel);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<PersonnelDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PersonnelDetailDto>> Update(
        Guid id,
        UpdatePersonnelRequest request,
        CancellationToken cancellationToken)
    {
        var personnel = await sender.Send(
            new UpdatePersonnelCommand(
                id,
                request.SicilNo,
                request.Ad,
                request.Soyad,
                request.Departman,
                request.Pozisyon,
                request.ZimmetNo,
                request.AktifMi),
            cancellationToken);

        return Ok(personnel);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeletePersonnelCommand(id), cancellationToken);
        return NoContent();
    }
}

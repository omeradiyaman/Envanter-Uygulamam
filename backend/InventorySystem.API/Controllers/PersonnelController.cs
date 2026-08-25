using InventorySystem.Application.Commands;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace InventorySystem.API.Controllers;
[ApiController,Route("api/personnel")] public sealed class PersonnelController(ISender sender):ControllerBase {
 [HttpGet] public async Task<ActionResult<PersonnelListResponse>> GetList([FromQuery]string? search,[FromQuery]int page=1,[FromQuery]int pageSize=20,CancellationToken ct=default)=>Ok(await sender.Send(new GetPersonnelListQuery(search,page,pageSize),ct));
 [HttpGet("{id:guid}")] public async Task<ActionResult<PersonnelDetailDto>> GetById(Guid id,CancellationToken ct)=>Ok(await sender.Send(new GetPersonnelByIdQuery(id),ct));
 [HttpPost] public async Task<ActionResult<PersonnelDetailDto>> Create(CreatePersonnelRequest r,CancellationToken ct){var p=await sender.Send(new CreatePersonnelCommand(r.SicilNo,r.Ad,r.Soyad,r.Departman,r.Pozisyon,r.Eposta,r.ZimmetNo,r.AktifMi),ct);return CreatedAtAction(nameof(GetById),new{id=p.Id},p);}
 [HttpPut("{id:guid}")] public async Task<ActionResult<PersonnelDetailDto>> Update(Guid id,UpdatePersonnelRequest r,CancellationToken ct)=>Ok(await sender.Send(new UpdatePersonnelCommand(id,r.SicilNo,r.Ad,r.Soyad,r.Departman,r.Pozisyon,r.Eposta,r.ZimmetNo,r.AktifMi),ct));
 [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id,CancellationToken ct){await sender.Send(new DeletePersonnelCommand(id),ct);return NoContent();}
}

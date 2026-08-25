using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace InventorySystem.Infrastructure.Persistence;
public sealed class PersonnelRepository(ApplicationDbContext dbContext) : IPersonnelRepository {
 public async Task<PersonnelPage> ListAsync(string? search,int page,int pageSize,CancellationToken ct) { page=Math.Max(1,page); pageSize=Math.Clamp(pageSize,1,100); var q=dbContext.Personnel.AsNoTracking(); if(!string.IsNullOrWhiteSpace(search)){var s=search.Trim().ToLower(); q=q.Where(p=>p.SicilNo.ToLower().Contains(s)||p.Ad.ToLower().Contains(s)||p.Soyad.ToLower().Contains(s)||p.Departman.ToLower().Contains(s)||p.Eposta!.ToLower().Contains(s));} var total=await q.CountAsync(ct); var active=await q.CountAsync(p=>p.AktifMi,ct); var devices=await q.CountAsync(p=>p.ZimmetNo!=null,ct); var pages=Math.Max(1,(int)Math.Ceiling(total/(double)pageSize)); page=Math.Min(page,pages); var items=await q.OrderBy(p=>p.Ad).ThenBy(p=>p.Soyad).ThenBy(p=>p.SicilNo).Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct); return new(items,page,pageSize,total,pages,active,devices); }
 public Task<Personnel?> GetByIdAsync(Guid id,CancellationToken ct)=>dbContext.Personnel.FirstOrDefaultAsync(p=>p.Id==id,ct);
 public Task<bool> SicilNoExistsAsync(string s,Guid? id,CancellationToken ct){var n=s.Trim().ToUpperInvariant();return dbContext.Personnel.AnyAsync(p=>p.SicilNo==n&&(!id.HasValue||p.Id!=id.Value),ct);}
 public Task AddAsync(Personnel p,CancellationToken ct)=>dbContext.Personnel.AddAsync(p,ct).AsTask(); public Task SaveChangesAsync(CancellationToken ct)=>dbContext.SaveChangesAsync(ct);
}

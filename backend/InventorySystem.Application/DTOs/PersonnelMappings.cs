using InventorySystem.Domain.Entities;
namespace InventorySystem.Application.DTOs;
internal static class PersonnelMappings {
 public static PersonnelListItemDto ToListItemDto(this Personnel p) => new(p.Id, Masking.Sicil(p.SicilNo), Masking.Name(p.Ad, p.Soyad), Masking.Text(p.Departman), p.Pozisyon, Masking.Email(p.Eposta), p.ZimmetNo is null ? 0 : 1, p.AktifMi);
 public static PersonnelDetailDto ToDetailDto(this Personnel p) => new(p.Id, Masking.Sicil(p.SicilNo), Masking.Name(p.Ad, p.Soyad), Masking.Text(p.Departman), p.Pozisyon, Masking.Email(p.Eposta), p.ZimmetNo is null ? Array.Empty<string>() : new[] { Masking.Text(p.ZimmetNo) }, p.AktifMi, p.CreatedAt, p.UpdatedAt);
}
internal static class Masking {
 public static string Sicil(string v) => v.Length <= 2 ? "••••" : new string('•', Math.Max(2, v.Length - 2)) + v[^2..];
 public static string Name(string a, string s) => $"{Keep(a)} {Keep(s)}";
 public static string Text(string v) => string.IsNullOrWhiteSpace(v) ? "—" : $"{Keep(v)}••";
 public static string Email(string? v) { if (string.IsNullOrWhiteSpace(v)) return "—"; var x=v.Split('@'); return x.Length==2 ? $"{Keep(x[0])}••@{x[1]}" : "••••"; }
 private static string Keep(string v) => string.IsNullOrWhiteSpace(v) ? "—" : v[..1].ToUpperInvariant();
}

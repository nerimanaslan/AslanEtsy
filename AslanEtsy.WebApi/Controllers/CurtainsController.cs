using AslanEtsy.Domain.Entities;
using AslanEtsy.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AslanEtsy.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurtainsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CurtainsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/curtains
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Seed default products if database is empty
        if (!await _context.CurtainProducts.AnyAsync())
        {
            var defaultList = new List<CurtainProduct>
            {
                new()
                {
                    Name = "Kırmızı Çizgili Keten Fon Perde",
                    M2Price = 4000,
                    Fabric = "%100 Saf Pamuk & Keten Karışımı",
                    Note = "Etsy Çok Satan • Rustik Çizgili Kumaş",
                    ImageUrl = "https://images.unsplash.com/photo-1513694203232-719a280e022f?w=600",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Bergonya / Bordo Keten Fon Perde",
                    M2Price = 4500,
                    Fabric = "%100 Doğal Taşlanmış Keten",
                    Note = "Cranberry / Koyu Bordo Şarap Rengi",
                    ImageUrl = "https://images.unsplash.com/photo-1520699049698-acd2fccb8cc8?w=600",
                    CreatedAtUtc = DateTime.UtcNow
                }
            };
            await _context.CurtainProducts.AddRangeAsync(defaultList);
            await _context.SaveChangesAsync();
        }

        var list = await _context.CurtainProducts
            .OrderByDescending(p => p.UpdatedAtUtc ?? p.CreatedAtUtc)
            .ToListAsync();

        return Ok(list);
    }

    // GET: api/curtains/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _context.CurtainProducts.FindAsync(id);
        if (product == null) return NotFound(new { message = "Perde modeli bulunamadı." });
        return Ok(product);
    }

    // POST: api/curtains
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CurtainProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Model adı boş bırakılamaz." });

        var product = new CurtainProduct
        {
            Name = dto.Name.Trim(),
            M2Price = dto.M2Price > 0 ? dto.M2Price : 4000,
            Fabric = dto.Fabric?.Trim(),
            Note = dto.Note?.Trim(),
            ImageUrl = dto.ImageUrl,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _context.CurtainProducts.AddAsync(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // PUT: api/curtains/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CurtainProductDto dto)
    {
        var product = await _context.CurtainProducts.FindAsync(id);
        if (product == null) return NotFound(new { message = "Perde modeli bulunamadı." });

        if (!string.IsNullOrWhiteSpace(dto.Name)) product.Name = dto.Name.Trim();
        if (dto.M2Price > 0) product.M2Price = dto.M2Price;
        if (dto.Fabric != null) product.Fabric = dto.Fabric.Trim();
        if (dto.Note != null) product.Note = dto.Note.Trim();
        if (!string.IsNullOrWhiteSpace(dto.ImageUrl)) product.ImageUrl = dto.ImageUrl;

        product.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(product);
    }

    // DELETE: api/curtains/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.CurtainProducts.FindAsync(id);
        if (product == null) return NotFound(new { message = "Perde modeli bulunamadı." });

        product.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Perde modeli başarıyla silindi." });
    }
}

public class CurtainProductDto
{
    public string Name { get; set; } = string.Empty;
    public decimal M2Price { get; set; }
    public string? Fabric { get; set; }
    public string? Note { get; set; }
    public string? ImageUrl { get; set; }
}

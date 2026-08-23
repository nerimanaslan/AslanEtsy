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
        // Seed default products if database has fewer than 5 items
        if (await _context.CurtainProducts.CountAsync() < 5)
        {
            var existingNames = await _context.CurtainProducts.Select(p => p.Name).ToListAsync();
            var allCurtains = new List<CurtainProduct>
            {
                new()
                {
                    Name = "Organic Thick Bamboo Ruffle Curtains - Custom Size, Sold in Pairs.",
                    M2Price = 4000,
                    Fabric = "%100 Organic Thick Bamboo • Fırfırlı (Ruffle)",
                    Note = "Etsy Özel Sipariş • Beyaz Bambu Kumaş",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/organic_thick_bamboo_ruffle.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Classic Linen Striped Blackout Curtains Organic Fabric - Custom Size",
                    M2Price = 4000,
                    Fabric = "%100 Organic Linen • Çizgili Karartma (Blackout)",
                    Note = "Bordo / Krem Çizgili Rustik Karartma Fon Perde",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/classic_linen_striped_blackout.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Classic Linen Striped Blackout Curtains Organic Fabric - Custom Size (Boydan)",
                    M2Price = 4000,
                    Fabric = "%100 Organic Linen • Boydan Dökümlü Karartma",
                    Note = "Bordo / Bej Çizgili Boydan Görünüm",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/classic_linen_striped_full_length.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Densely Pleated Linen Blackout Curtain Organic Fabric - Custom Size",
                    M2Price = 4000,
                    Fabric = "%100 Organic Linen • Sık Pileli (Densely Pleated)",
                    Note = "Vizon / Taupe Sık Pileli Dökümlü Karartma Keten",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/densely_pleated_linen_blackout.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "American Style Dense Pleated Linen Blackout Curtain in Organic Fabric - Customized Size",
                    M2Price = 4000,
                    Fabric = "%100 Organic Linen • Amerikan / Pinch Pleat Pileli",
                    Note = "Amerikan Pileli Rustik Keten Karartma Fon Perde",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/american_style_dense_pleated_linen.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Decorative Frequent Pleated Linen Blackout Curtain Organic Fabric - Custom Size",
                    M2Price = 4000,
                    Fabric = "%100 Organic Linen • Düğme Detaylı Sık Pileli",
                    Note = "Naturel Bej Keten Düğmeli Dekoratif Karartma",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/decorative_frequent_pleated_linen.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Linen Blackout Lining Curtains, Organic Fabric - Custom Size",
                    M2Price = 4000,
                    Fabric = "%100 Organic Linen • Kuşaklı (Tab Top) Karartma Astarlı",
                    Note = "Bej/Keten Kuşaklı Karartma Astarlı Fon Perde",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/linen_blackout_lining_curtains.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Tufted Liner Linen Blackout Curtains, Organic Fabric - Custom Size",
                    M2Price = 4000,
                    Fabric = "%100 Organic Linen • Yan Püsküllü (Tufted/Tassel)",
                    Note = "Pudra / Toz Gül Keten Püsküllü Karartma Fon Perde",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/tufted_liner_linen_blackout.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Dusty Blue Pompom Trim Linen Curtains, Organic Fabric - Custom Size",
                    M2Price = 4000,
                    Fabric = "%100 Organic Linen • Buz Mavisi Ponponlu Fırfırlı",
                    Note = "Buz Mavisi / Dusty Blue Ponponlu Doğal Keten",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/dusty_blue_pompom_trim_linen.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Suede Velvet Blackout Lining Curtain Organic Fabric - Custom Size",
                    M2Price = 4000,
                    Fabric = "Lüks Süet Kadife • Karartma Astarlı (Blackout Lining)",
                    Note = "Antrasit / Koyu Gri Lüks Kadife Karartma Fon Perde",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/suede_velvet_blackout_lining.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                }
            };

            foreach (var item in allCurtains)
            {
                if (!existingNames.Contains(item.Name))
                {
                    await _context.CurtainProducts.AddAsync(item);
                }
            }
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

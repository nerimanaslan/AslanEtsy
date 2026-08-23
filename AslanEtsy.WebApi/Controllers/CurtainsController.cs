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

    // GET: api/curtains?category=Curtain (or Bedding)
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category = null)
    {
        // Seed default products if database has fewer than 15 items
        if (await _context.CurtainProducts.CountAsync() < 15)
        {
            var existingNames = await _context.CurtainProducts.Select(p => p.Name).ToListAsync();
            var allCurtains = new List<CurtainProduct>
            {
                new()
                {
                    Name = "Organic Thick Bamboo Ruffle Curtains - Custom Size, Sold in Pairs.",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "%100 Organic Thick Bamboo • Fırfırlı (Ruffle)",
                    Note = "Etsy Özel Sipariş • Beyaz Bambu Kumaş",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/organic_thick_bamboo_ruffle.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Classic Linen Striped Blackout Curtains Organic Fabric - Custom Size",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "%100 Organic Linen • Çizgili Karartma (Blackout)",
                    Note = "Bordo / Krem Çizgili Rustik Karartma Fon Perde",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/classic_linen_striped_blackout.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Classic Linen Striped Blackout Curtains Organic Fabric - Custom Size (Boydan)",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "%100 Organic Linen • Boydan Dökümlü Karartma",
                    Note = "Bordo / Bej Çizgili Boydan Görünüm",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/classic_linen_striped_full_length.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Densely Pleated Linen Blackout Curtain Organic Fabric - Custom Size",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "%100 Organic Linen • Sık Pileli (Densely Pleated)",
                    Note = "Vizon / Taupe Sık Pileli Dökümlü Karartma Keten",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/densely_pleated_linen_blackout.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "American Style Dense Pleated Linen Blackout Curtain in Organic Fabric - Customized Size",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "%100 Organic Linen • Amerikan / Pinch Pleat Pileli",
                    Note = "Amerikan Pileli Rustik Keten Karartma Fon Perde",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/american_style_dense_pleated_linen.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Decorative Frequent Pleated Linen Blackout Curtain Organic Fabric - Custom Size",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "%100 Organic Linen • Düğme Detaylı Sık Pileli",
                    Note = "Naturel Bej Keten Düğmeli Dekoratif Karartma",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/decorative_frequent_pleated_linen.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Linen Blackout Lining Curtains, Organic Fabric - Custom Size",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "%100 Organic Linen • Kuşaklı (Tab Top) Karartma Astarlı",
                    Note = "Bej/Keten Kuşaklı Karartma Astarlı Fon Perde",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/linen_blackout_lining_curtains.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Tufted Liner Linen Blackout Curtains, Organic Fabric - Custom Size",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "%100 Organic Linen • Yan Püsküllü (Tufted/Tassel)",
                    Note = "Pudra / Toz Gül Keten Püsküllü Karartma Fon Perde",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/tufted_liner_linen_blackout.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Dusty Blue Pompom Trim Linen Curtains, Organic Fabric - Custom Size",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "%100 Organic Linen • Buz Mavisi Ponponlu Fırfırlı",
                    Note = "Buz Mavisi / Dusty Blue Ponponlu Doğal Keten",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/dusty_blue_pompom_trim_linen.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Suede Velvet Blackout Lining Curtain Organic Fabric - Custom Size",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "Lüks Süet Kadife • Karartma Astarlı (Blackout Lining)",
                    Note = "Antrasit / Koyu Gri Lüks Kadife Karartma Fon Perde",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/suede_velvet_blackout_lining.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Cotton Satin Blackout Lining Curtain, Organic Fabric - Custom Size",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "Lüks Pamuk Saten • İnci/Düğme Pileli Karartma Astarlı",
                    Note = "Şampanya / Bej Parlak Lüks Pamuk Saten",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/cotton_satin_blackout_lining.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Classic Linen Striped Blackout Curtains Organic Fabric - Custom Size, Pom-Pom Curtain",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "%100 Organic Linen • Çizgili Karartma & Pom-Pom / Katmanlı Ruffle",
                    Note = "Bordo / Krem Çizgili Pom-Pom Detaylı",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/classic_linen_striped_pompom.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Suede Velvet Blackout Lining Curtain Organic Fabric - Custom Size (Dusty Rose)",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "Lüks Süet Kadife • Gül Kurusu / Pudra Karartma Astarlı",
                    Note = "Gül Kurusu / Pudra Pembe Lüks Kadife Karartma",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/suede_velvet_pink_blackout.webp",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Off-White Sheer Curtain: Pleated Tulle, Wrinkle-Free, Organic Cotton Fabric",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "Organik Pamuk & Keten Tül (Sheer) • Kırık Beyaz/Ekru Ütü İstemez",
                    Note = "DSN: TTM • VR: EKRU • EN: 300CM • Pileli Tül Perde",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/off_white_sheer_curtain_tulle.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Blackout Curtain With Pom-Poms, Made From 100% Organic Cotton Fabric.",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "%100 Organic Cotton • 3D Ponponlu (Pom-Poms / Tufted Dots) Karartma",
                    Note = "Vizon / Bej 3D Ponpon İşlemeli Karartma Fon Perde",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/blackout_curtain_with_pompoms_cotton.webp",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Linen Blend Curtain Panel – Organic Cotton Curtain with Fringe, Rustic Boho Window Treatment",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "Organik Pamuk & Keten Karışımı • Çiçek Nakışlı & Dantel Saçaklı (Fringe)",
                    Note = "Gül Nakışlı ve Püsküllü Rustik Boho Pencere Perdesi",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/linen_blend_curtain_fringe_rustic_boho.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "100% Linen Blackout Curtains with Valance, Grey Gingham Checkered Window Treatments, Farmhouse Style Nursery Drapes, Custom Size Panel",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "%100 Organic Linen • Gri Pötikare (Gingham Checkered) & Valanslı",
                    Note = "Gri Pötikare Ekoseli Çift Katlı Valanslı Karartma Fon Perde",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/grey_gingham_checkered_linen_valance.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Striped Linen Blackout Curtains with Pom Pom Trim, Ticking Stripe Drapes for Farmhouse Decor, Custom Size Rustic Window Panels, Beige Cream",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "%100 Organic Linen • Bej/Krem İnce Çizgili & Yan Ponponlu (Pom Pom Trim)",
                    Note = "Bej Krem Çizgili Ponpon Kenarlı Farmhouse Karartma",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/striped_linen_pompom_trim_beige_cream.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "100% Organic Linen Blackout Curtain, Pink Striped Linen Drapes, Organic Cotton Lining, Custom Size Drapes",
                    M2Price = 4000,
                    Category = "Curtain",
                    Fabric = "%100 Organic Linen • Pembe / Şeker Pembe Çizgili & Pamuk Astarlı",
                    Note = "Pembe Çizgili Rustik Karartma Keten Fon Perde",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/pink_striped_linen_blackout_drapes.jpg",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Heart Pattern Organic Cotton Bedding Set - Custom Size Duvet Cover",
                    M2Price = 22000,
                    Category = "Bedding",
                    Fabric = "%100 Organic Cotton • Kırmızı Kalp Desenli Nevresim Takımı",
                    Note = "Crib, Toddler, Twin, Double, Queen, King Yatak Ölçüleri",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/bedding/heart_pattern_organic_cotton_bedding.png",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Duvet Cover Set 100% Cotton, Lace, Embroidered Organic Fabric",
                    M2Price = 22000,
                    Category = "Bedding",
                    Fabric = "%100 Organic Cotton • Fransız Güpürlü / Dantelli (Lace) & Nakışlı Pembe Nevresim Takımı",
                    Note = "Pudra Pembe Dantel & Nakış İşlemeli Lüks Pamuk Nevresim Takımı",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/bedding/duvet_cover_set_pink_lace_embroidered.webp",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Ruffled Duvet Cover Set 100% Cotton, Embroidered Organic Fabric",
                    M2Price = 22000,
                    Category = "Bedding",
                    Fabric = "%100 Organic Cotton • Fırfırlı (Ruffled) & Nakışlı Gri / Vizon Nevresim Takımı",
                    Note = "Gri / Vizon Kenarları Fırfırlı Lüks Pamuk Nevresim Takımı",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/bedding/ruffled_duvet_cover_set_grey_embroidered.webp",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Korean Princess Style Ruffles Duvet Cover Set 100% Cotton, Embroidered Organic Fabric",
                    M2Price = 22000,
                    Category = "Bedding",
                    Fabric = "%100 Organic Cotton • Beyaz Kore Prenses Tarzı Fırfırlı (Ruffles) & Pileli Lüks Nevresim Takımı",
                    Note = "Beyaz Prenses Tarzı Fırfır & Pili Detaylı Lüks Pamuk Nevresim Takımı",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/bedding/korean_princess_ruffles_duvet_cover_set.webp",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Ruffled Stripe Patterned Bedding Set, 100% Linen, Embroidered Organic Fabric",
                    M2Price = 22000,
                    Category = "Bedding",
                    Fabric = "%100 Organic Linen • Gül Kurusu / Krem Çizgili & Kenarları Krem Fırfırlı (Ruffled) Lüks Keten Nevresim Takımı",
                    Note = "Gül Kurusu Çizgili Keten Fırfırlı Lüks Nevresim Takımı",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/bedding/ruffled_stripe_patterned_linen_bedding.webp",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Red Heart Duvet Cover Set 100% Cotton, Embroidered Organic Fabric",
                    M2Price = 22000,
                    Category = "Bedding",
                    Fabric = "%100 Organic Cotton • Kırmızı Kalp Desenli (Red Heart) Beyaz Pamuk Nevresim Takımı",
                    Note = "Beyaz Zemin Kırmızı Kalp Desenli Lüks Pamuk Nevresim Takımı",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/bedding/red_heart_duvet_cover_set_cotton.webp",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Ruffled Duvet Cover Set 100% Cotton, Embroidered Organic Fabric (Beige/White)",
                    M2Price = 22000,
                    Category = "Bedding",
                    Fabric = "%100 Organic Cotton • Bej / Vizon & Beyaz Çift Renk Fırfırlı (Ruffled) Lüks Nevresim Takımı",
                    Note = "Vizon Bej & Beyaz Katmanlı Fırfırlı Lüks Pamuk Nevresim Takımı",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/bedding/ruffled_duvet_cover_set_beige_white.webp",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Name = "Personalized Duvet Cover Set 100% Cotton, Custom Embroidered Bedding Set, Organic Cotton Ruffled Bedspread, Minimalist Bedroom Decor Gift",
                    M2Price = 22000,
                    Category = "Bedding",
                    Fabric = "%100 Organic Cotton • Kişiye Özel İsim Nakışlı (Personalized) & Gri Fırfırlı / Degrade Lüks Nevresim Takımı",
                    Note = "İsim Nakışlı Gri Fırfırlı Degrade Lüks Pamuk Nevresim Takımı",
                    ImageUrl = "https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/bedding/personalized_embroidered_ruffled_bedding.webp",
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

        var query = _context.CurtainProducts.AsQueryable();
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category);
        }

        var list = await query
            .OrderByDescending(p => p.UpdatedAtUtc ?? p.CreatedAtUtc)
            .ToListAsync();

        return Ok(list);
    }

    // GET: api/curtains/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _context.CurtainProducts.FindAsync(id);
        if (product == null) return NotFound(new { message = "Ürün modeli bulunamadı." });
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
            Category = string.IsNullOrWhiteSpace(dto.Category) ? "Curtain" : dto.Category.Trim(),
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
        if (product == null) return NotFound(new { message = "Ürün modeli bulunamadı." });

        if (!string.IsNullOrWhiteSpace(dto.Name)) product.Name = dto.Name.Trim();
        if (dto.M2Price > 0) product.M2Price = dto.M2Price;
        if (!string.IsNullOrWhiteSpace(dto.Category)) product.Category = dto.Category.Trim();
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
        if (product == null) return NotFound(new { message = "Ürün modeli bulunamadı." });

        product.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Ürün modeli başarıyla silindi." });
    }
}

public class CurtainProductDto
{
    public string Name { get; set; } = string.Empty;
    public decimal M2Price { get; set; }
    public string? Category { get; set; } = "Curtain";
    public string? Fabric { get; set; }
    public string? Note { get; set; }
    public string? ImageUrl { get; set; }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. Consultar listado de categorías
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        return await _context.Categories.ToListAsync();
    }

    // 2. Consultar listado de productos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        var products = await _context.Products.Include(p => p.Category)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                ImageUrl = p.ImageUrl // Incluir ImageUrl
            }).ToListAsync();

        return Ok(products);
    }

    // 3. Consultar detalle de un producto
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                ImageUrl = p.ImageUrl // Incluir ImageUrl
            })
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }
        return Ok(product); // Devuelve el DTO
    }

    // 4. Agregar un producto como “producto deseado”
    [HttpPost("wishlist")]
    public async Task<ActionResult<WishlistItem>> AddToWishlist(WishlistItem wishlistItem)
    {
        _context.WishlistItems.Add(wishlistItem);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetWishlist), new { id = wishlistItem.Id }, wishlistItem);
    }

    // 5. Eliminar un producto deseado
    [HttpDelete("wishlist/{id}")]
    public async Task<IActionResult> RemoveFromWishlist(int id)
    {
        var wishlistItem = await _context.WishlistItems.FindAsync(id);
        if (wishlistItem == null)
        {
            return NotFound();
        }
        _context.WishlistItems.Remove(wishlistItem);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // 6. Consultar listado de productos deseados de un usuario
    [HttpGet("wishlist/{userId}")]
    public async Task<ActionResult<IEnumerable<WishlistItem>>> GetWishlist(int userId)
    {
        return await _context.WishlistItems.Include(w => w.Product).Where(w => w.UserId == userId).ToListAsync();
    }

    // Prueba de conexión de la base
    [HttpGet("test")]
    public IActionResult TestConnection()
    {
        var isConnected = _context.Database.CanConnect();
        return Ok(new { IsConnected = isConnected });
    }
}

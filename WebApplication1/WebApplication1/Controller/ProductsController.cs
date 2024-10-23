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
    public async Task<ActionResult<WishlistItem>> AddToWishlist([FromBody] WishlistItem wishlistItem)
    {
        // Obtiene el producto
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == wishlistItem.ProductId);

        if (product == null)
        {
            return NotFound(new { Message = "Product not found." });
        }

        // Verifica si el producto ya está en la lista de deseos
        var existingWishlistItem = await _context.WishlistItems
            .FirstOrDefaultAsync(wi => wi.UserId == wishlistItem.UserId && wi.ProductId == wishlistItem.ProductId);

        if (existingWishlistItem != null)
        {
            return Conflict(new { Message = "Product already in wishlist." });
        }

        // Establece las propiedades del wishlistItem
        wishlistItem.Product = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            CategoryId = product.CategoryId,
            ImageUrl = product.ImageUrl,
            Category = new Category
            {
                Id = product.Category.Id,
                Name = product.Category.Name
            }
        };

        _context.WishlistItems.Add(wishlistItem);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWishlist), new { userId = wishlistItem.UserId }, wishlistItem);
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
        var wishlistItems = await _context.WishlistItems
            .Where(wi => wi.UserId == userId)
            .Include(wi => wi.Product)
            .ThenInclude(p => p.Category)
            .ToListAsync();

        return Ok(wishlistItems);
    }

    // 7. Consultar productos por categoría
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId)
    {
        var products = await _context.Products
            .Where(p => p.CategoryId == categoryId)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                ImageUrl = p.ImageUrl // Incluir ImageUrl
            })
            .ToListAsync();

        if (products == null || products.Count == 0)
        {
            return NotFound(new { Message = "No products found in this category." });
        }

        return Ok(products);
    }
    // 8. Consultar una categoría por ID
    [HttpGet("categoryID/{id}")]
    public async Task<ActionResult<Category>> GetCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound(new { Message = "Category not found." });
        }

        return Ok(category);
    }
    // Prueba de conexión de la base
    [HttpGet("test")]
    public IActionResult TestConnection()
    {
        var isConnected = _context.Database.CanConnect();
        return Ok(new { IsConnected = isConnected });
    }
}

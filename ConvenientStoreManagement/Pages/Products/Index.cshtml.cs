using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.Pages.Products
{
  public class IndexModel : PageModel
  {
    private readonly StoreDbContext _context;

    public IndexModel(StoreDbContext context)
    {
      _context = context;
    }

    public IList<Product> Products { get; set; } = default!;

    public async Task OnGetAsync()
    {
      Products = await _context.Products.Include(p => p.Category).ToListAsync();
    }
  }
}

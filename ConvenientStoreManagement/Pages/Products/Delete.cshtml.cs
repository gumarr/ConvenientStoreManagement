using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.Pages.Products
{
  public class DeleteModel : PageModel
  {
    private readonly StoreDbContext _context;

    public DeleteModel(StoreDbContext context)
    {
      _context = context;
    }

    [BindProperty]
    public Product Product { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      Product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(m => m.ProductId == id);
      if (Product == null)
      {
        return NotFound();
      }
      return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      Product = await _context.Products.FindAsync(id);
      if (Product != null)
      {
        _context.Products.Remove(Product);
        await _context.SaveChangesAsync();
      }

      return RedirectToPage("./Index");
    }
  }
}

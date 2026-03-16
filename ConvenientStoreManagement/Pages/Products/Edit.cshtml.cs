using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.Pages.Products
{
  public class EditModel : PageModel
  {
    private readonly StoreDbContext _context;

    public EditModel(StoreDbContext context)
    {
      _context = context;
    }

    [BindProperty]
    public Product Product { get; set; } = default!;

    public SelectList CategoryList { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      Product = await _context.Products.FirstOrDefaultAsync(m => m.ProductId == id);
      if (Product == null)
      {
        return NotFound();
      }

      CategoryList = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "CategoryName");
      return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
      if (!ModelState.IsValid)
      {
        CategoryList = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "CategoryName");
        return Page();
      }

      Product.UpdatedDate = DateTime.Now;
      _context.Attach(Product).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!ProductExists(Product.ProductId))
        {
          return NotFound();
        }
        else
        {
          throw;
        }
      }

      return RedirectToPage("./Index");
    }

    private bool ProductExists(int id)
    {
      return _context.Products.Any(e => e.ProductId == id);
    }
  }
}

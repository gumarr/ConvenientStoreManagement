using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.Pages.Sales
{
  public class DeleteModel : PageModel
  {
    private readonly StoreDbContext _context;

    public DeleteModel(StoreDbContext context)
    {
      _context = context;
    }

    [BindProperty]
    public Sale Sale { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      Sale = await _context.Sales.Include(s => s.Customer).FirstOrDefaultAsync(m => m.SaleId == id);
      if (Sale == null)
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

      Sale = await _context.Sales.FindAsync(id);
      if (Sale != null)
      {
        _context.Sales.Remove(Sale);
        await _context.SaveChangesAsync();
      }

      return RedirectToPage("./Index");
    }
  }
}

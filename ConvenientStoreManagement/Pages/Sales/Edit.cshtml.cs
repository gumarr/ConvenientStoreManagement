using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.Pages.Sales
{
  public class EditModel : PageModel
  {
    private readonly StoreDbContext _context;

    public EditModel(StoreDbContext context)
    {
      _context = context;
    }

    [BindProperty]
    public Sale Sale { get; set; } = default!;

    public SelectList CustomerList { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      Sale = await _context.Sales.FirstOrDefaultAsync(m => m.SaleId == id);
      if (Sale == null)
      {
        return NotFound();
      }

      CustomerList = new SelectList(await _context.Customers.Where(c => c.IsActive).ToListAsync(), "CustomerId", "CustomerName");
      return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
      if (!ModelState.IsValid)
      {
        CustomerList = new SelectList(await _context.Customers.Where(c => c.IsActive).ToListAsync(), "CustomerId", "CustomerName");
        return Page();
      }

      _context.Attach(Sale).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!SaleExists(Sale.SaleId))
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

    private bool SaleExists(int id)
    {
      return _context.Sales.Any(e => e.SaleId == id);
    }
  }
}

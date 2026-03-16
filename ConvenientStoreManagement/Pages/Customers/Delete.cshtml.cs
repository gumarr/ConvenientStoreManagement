using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.Pages.Customers
{
  public class DeleteModel : PageModel
  {
    private readonly StoreDbContext _context;

    public DeleteModel(StoreDbContext context)
    {
      _context = context;
    }

    [BindProperty]
    public Customer Customer { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      Customer = await _context.Customers.FirstOrDefaultAsync(m => m.CustomerId == id);
      if (Customer == null)
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

      Customer = await _context.Customers.FindAsync(id);
      if (Customer != null)
      {
        _context.Customers.Remove(Customer);
        await _context.SaveChangesAsync();
      }

      return RedirectToPage("./Index");
    }
  }
}

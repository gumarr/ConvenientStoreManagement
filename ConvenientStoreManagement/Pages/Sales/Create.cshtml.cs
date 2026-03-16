using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.Pages.Sales
{
  public class CreateModel : PageModel
  {
    private readonly StoreDbContext _context;

    public CreateModel(StoreDbContext context)
    {
      _context = context;
    }

    [BindProperty]
    public Sale Sale { get; set; } = default!;

    public SelectList CustomerList { get; set; } = default!;

    public async Task OnGetAsync()
    {
      CustomerList = new SelectList(await _context.Customers.Where(c => c.IsActive).ToListAsync(), "CustomerId", "CustomerName");
    }

    public async Task<IActionResult> OnPostAsync()
    {
      if (!ModelState.IsValid)
      {
        CustomerList = new SelectList(await _context.Customers.Where(c => c.IsActive).ToListAsync(), "CustomerId", "CustomerName");
        return Page();
      }

      Sale.SaleDate = DateTime.Now;
      _context.Sales.Add(Sale);
      await _context.SaveChangesAsync();

      return RedirectToPage("./Index");
    }
  }
}

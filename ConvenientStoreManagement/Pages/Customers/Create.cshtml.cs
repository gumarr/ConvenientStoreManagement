using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.Pages.Customers
{
  public class CreateModel : PageModel
  {
    private readonly StoreDbContext _context;

    public CreateModel(StoreDbContext context)
    {
      _context = context;
    }

    [BindProperty]
    public Customer Customer { get; set; } = default!;

    public IActionResult OnGet()
    {
      return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
      if (!ModelState.IsValid)
      {
        return Page();
      }

      Customer.CreatedDate = DateTime.Now;
      _context.Customers.Add(Customer);
      await _context.SaveChangesAsync();

      return RedirectToPage("./Index");
    }
  }
}

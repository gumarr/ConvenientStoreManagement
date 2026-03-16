using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.Pages.Customers
{
  public class IndexModel : PageModel
  {
    private readonly StoreDbContext _context;

    public IndexModel(StoreDbContext context)
    {
      _context = context;
    }

    public IList<Customer> Customers { get; set; } = default!;

    public async Task OnGetAsync()
    {
      Customers = await _context.Customers.ToListAsync();
    }
  }
}

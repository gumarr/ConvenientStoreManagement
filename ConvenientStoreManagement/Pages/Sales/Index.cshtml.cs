using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.Pages.Sales
{
  public class IndexModel : PageModel
  {
    private readonly StoreDbContext _context;

    public IndexModel(StoreDbContext context)
    {
      _context = context;
    }

    public IList<Sale> Sales { get; set; } = default!;

    public async Task OnGetAsync()
    {
      Sales = await _context.Sales.Include(s => s.Customer).OrderByDescending(s => s.SaleDate).ToListAsync();
    }
  }
}

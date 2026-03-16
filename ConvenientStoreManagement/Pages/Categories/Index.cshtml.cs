using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.Pages.Categories
{
  public class IndexModel : PageModel
  {
    private readonly StoreDbContext _context;

    public IndexModel(StoreDbContext context)
    {
      _context = context;
    }

    public IList<Category> Categories { get; set; } = default!;

    public async Task OnGetAsync()
    {
      Categories = await _context.Categories.ToListAsync();
    }
  }
}

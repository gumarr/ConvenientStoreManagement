using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.Pages.Categories
{
  public class DeleteModel : PageModel
  {
    private readonly StoreDbContext _context;

    public DeleteModel(StoreDbContext context)
    {
      _context = context;
    }

    [BindProperty]
    public Category Category { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      Category = await _context.Categories.FirstOrDefaultAsync(m => m.CategoryId == id);
      if (Category == null)
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

      Category = await _context.Categories.FindAsync(id);
      if (Category != null)
      {
        _context.Categories.Remove(Category);
        await _context.SaveChangesAsync();
      }

      return RedirectToPage("./Index");
    }
  }
}

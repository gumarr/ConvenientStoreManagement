using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.Pages.Categories
{
  public class CreateModel : PageModel
  {
    private readonly StoreDbContext _context;

    public CreateModel(StoreDbContext context)
    {
      _context = context;
    }

    [BindProperty]
    public Category Category { get; set; } = default!;

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

      Category.CreatedDate = DateTime.Now;
      _context.Categories.Add(Category);
      await _context.SaveChangesAsync();

      return RedirectToPage("./Index");
    }
  }
}

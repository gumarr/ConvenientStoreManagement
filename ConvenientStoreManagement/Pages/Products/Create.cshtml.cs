using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ConvenientStoreManagement.Data;
using ConvenientStoreManagement.Models;

namespace ConvenientStoreManagement.Pages.Products
{
  public class CreateModel : PageModel
  {
    private readonly StoreDbContext _context;

    public CreateModel(StoreDbContext context)
    {
      _context = context;
    }

    [BindProperty]
    public Product Product { get; set; } = default!;

    public SelectList CategoryList { get; set; } = default!;

    public async Task OnGetAsync()
    {
      CategoryList = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "CategoryName");
    }

    public async Task<IActionResult> OnPostAsync()
    {
      if (!ModelState.IsValid)
      {
        CategoryList = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "CategoryName");
        return Page();
      }

      Product.CreatedDate = DateTime.Now;
      _context.Products.Add(Product);
      await _context.SaveChangesAsync();

      return RedirectToPage("./Index");
    }
  }
}

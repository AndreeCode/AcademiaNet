using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Academic.Pages.Coordinador;

[Authorize(Roles = "Coordinador")]
public class DashboardModel : PageModel
{
    public void OnGet() { }
}

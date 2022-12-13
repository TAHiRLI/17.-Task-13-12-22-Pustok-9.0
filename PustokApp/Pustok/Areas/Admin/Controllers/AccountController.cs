using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pustok.Models;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Pustok.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task<IActionResult> CreateAdmin()
        {
            AppUser user = new AppUser
            {
                Fullname = "Tahir Tahirli",
                Email = "TahirTahirli@gmail.com",
                UserName = "TahirAdmin"
            };

            var result = await _userManager.CreateAsync(user, "Tahir123");
            if (!result.Succeeded)
            {
                return Ok(result.Errors);
            }


            return Ok(_userManager.FindByNameAsync("TahirAdmin"));
        }

        public async Task<IActionResult> Login()
        {

            return View();

        }

        [HttpPost]
        public async Task<IActionResult> Login()
        {

             
            return RedirectToAction("Index", "Dashboard");
        }
    }
}

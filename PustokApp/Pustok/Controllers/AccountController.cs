using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using Pustok.DAL;
using Pustok.Models;
using Pustok.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Pustok.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(MemberRegisterViewModel memberVm)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }


            if (await _userManager.FindByNameAsync(memberVm.Username) != null)
            {
                ModelState.AddModelError("Username", "User already exists");
                return RedirectToAction("Login");
            }
            if (await _userManager.FindByEmailAsync(memberVm.Email) != null)
            {
                ModelState.AddModelError("Email", "Email already exists");
            }


            AppUser appUser = new AppUser
            {
                Email = memberVm.Email,
                Fullname = memberVm.Fullname,
                UserName = memberVm.Username
            };

            var result = await _userManager.CreateAsync(appUser, memberVm.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }

              await _userManager.AddToRoleAsync(appUser, "Member");



            return RedirectToAction("Login", "Account");
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(MemberLoginViewModel memberLoginVm, string returnUrl)
        {
            if (memberLoginVm.Password == null)
                ModelState.AddModelError("Password", "This field Is required");
            if (memberLoginVm.Username == null)
                ModelState.AddModelError("Username", "This field Is required");
            if (!ModelState.IsValid)
            {
                return View();
            }

            AppUser appUser = await _userManager.FindByNameAsync(memberLoginVm.Username);
            if (appUser == null)
            {
                ModelState.AddModelError("", "Username or Password is incorrect !");
                return View();
            }
            // vm de required yazmagima baxmayaraq field null olanda modelstate.isvalid true olur



            var roles = await _userManager.GetRolesAsync(appUser);
            if (!roles.Contains("Member"))
            {
                ModelState.AddModelError("", "Username or Password is incorrect !");
                return View();
            }



           



           
            var result = await _signInManager.PasswordSignInAsync(appUser, memberLoginVm.Password, true, true);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Too many attempts, please wait 5 minutes");
                return View();
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Username or Password is incorrect !");
                return View();
            }
            if (returnUrl != null)
                return Redirect(returnUrl);


            return RedirectToAction("Index", "Home");
        }

        //public IActionResult Show()
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        return Content(User.Identity.Name);
        //    }
        //    return Content("User Is logged Out");
        //}
        [Authorize(Roles ="Member")]
        public async Task<IActionResult> Profile()
        {
            AppUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);
            MemberEditViewModel memberEditVm = new MemberEditViewModel();
            memberEditVm.Fullname = appUser.Fullname;
            memberEditVm.UserName = appUser.UserName;
            memberEditVm.Email = appUser.Email;

            return View(memberEditVm);
        }
        [HttpPost]
        public async Task<IActionResult> Profile(MemberEditViewModel memberEditVm)
        {
            AppUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (!ModelState.IsValid)
            {
                return View(memberEditVm);
            }
            if (memberEditVm.Fullname != appUser.Fullname)
                appUser.Fullname = memberEditVm.Fullname;

            if (memberEditVm.Email != appUser.Email)
            {
                if (await _userManager.FindByEmailAsync(memberEditVm.Email) == null)
                    appUser.Email = memberEditVm.Email;
                else
                {
                    ModelState.AddModelError("Email", "This email already exists");
                    return View(memberEditVm);

                }
            }

            if (memberEditVm.UserName != appUser.UserName)
            {

                if (await _userManager.FindByNameAsync(memberEditVm.UserName) == null)
                    appUser.UserName = memberEditVm.UserName;
                else
                {

                    ModelState.AddModelError("Username", "This Username already exists");
                    return View(memberEditVm);
                }

            }
            if (memberEditVm.CurrentPassword != null || memberEditVm.NewPassword != null)
            {


                if (memberEditVm.CurrentPassword != null)
                {
                    var isUpdated = await _userManager.ChangePasswordAsync(appUser, memberEditVm.CurrentPassword, memberEditVm.NewPassword);
                    if (!isUpdated.Succeeded)
                    {
                        foreach (var error in isUpdated.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(memberEditVm);
                    }
                }
                

            }




            var result = await _userManager.UpdateAsync(appUser);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(memberEditVm);
            }

            return RedirectToAction("index", "home");
        }
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "home");
        }



    }
}

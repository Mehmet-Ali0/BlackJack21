using BlackJack21.Models;
using BlackJack21.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

//AI slop boilerplate

namespace BlackJack21.Controllers
{
    public class AccountController : Controller
    {
        // These are built-in Microsoft Identity tools
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ------------------- REGISTRATION -------------------

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create the new user object
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Balance = 1000 // Give them their starting chips!
                };

                // Attempt to save them to the AspNetUsers table
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Automatically log them in after registering
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "BlackJack"); // Send them to the lobby
                }

                // If password was too weak, etc., show the errors
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // ------------------- LOGIN -------------------

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to sign in
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "BlackJack"); // Send them to the lobby
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        // ------------------- LOGOUT -------------------

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "BlackJack");
        }
    }
}
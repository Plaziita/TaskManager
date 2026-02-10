
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TaskManager.Models;

namespace TaskManager.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; }

        public class LoginInputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; }

            [Required, DataType(DataType.Password)]
            public string Password { get; set; }
        }

        [BindProperty]
        public string ReturnUrl { get; set; }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? "/Dashboard/Dashboard";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _signInManager.PasswordSignInAsync(
                Input.Email,
                Input.Password,
                isPersistent: true,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                return LocalRedirect(ReturnUrl ?? "/Dashboard/Dashboard");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }
}

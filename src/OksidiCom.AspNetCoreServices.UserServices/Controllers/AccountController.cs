using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OksidiCom.AspNetCoreServices.UserServices.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OksidiCom.AspNetCoreServices.UserServices.Controllers
{
    [Authorize]
    [Route("connect/[controller]")]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public class LoginViewModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; } = false;

            public string ReturnUrl { get; set; } = "";
        }
        // GET: /Account/Login
        [AllowAnonymous]
        [HttpGet("[action]")]
        public IActionResult Login(string returnUrl)
        {
            //ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel()
            {
                Email = "test@example.com",
                ReturnUrl = returnUrl
            });
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    //_logger.LogInformation(1, "User logged in.");
                    return RedirectToLocal(model.ReturnUrl);
                }
                //if (result.RequiresTwoFactor)
                //{
                //    return RedirectToAction(nameof(SendCode), new { ReturnUrl = model.ReturnUrl, RememberMe = model.RememberLogin });
                //}
                //if (result.IsLockedOut)
                //{
                //    _logger.LogWarning(2, "User account locked out.");
                //    return View("Lockout");
                //}
                //else
                //{
                    return Login(model.ReturnUrl);
                //}
            }

            // If we got this far, something failed, redisplay form
            return Login(model.ReturnUrl);
        }

        // POST: /Account/ExternalLogin
        [HttpPost("[action]")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        //
        // GET: /Account/ExternalLoginCallback
        [HttpGet("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                //_logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            return new ContentResult()
            {
                Content = "External login failure?"
            };
            //if (result.RequiresTwoFactor)
            //{
            //    return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl });
            //}
            //if (result.IsLockedOut)
            //{
            //    return View("Lockout");
            //}
            //else
            //{
                // If the user does not have an account, then ask the user to create an account.
                //ViewData["ReturnUrl"] = returnUrl;
                //ViewData["LoginProvider"] = info.LoginProvider;
                //var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                //return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
            //}
        }

        [HttpGet("[action]")]
        public IActionResult LoggedIn()
        {
            return new ContentResult()
            {
                Content = User.Identity.Name
            };
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect("/Account/LoggedIn");
            }
        }

        #endregion

    }
}

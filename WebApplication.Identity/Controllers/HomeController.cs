using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication.Identity.Models;

namespace WebApplication.Identity.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<MyUser> _userManager;
        private readonly IUserClaimsPrincipalFactory<MyUser> _UserClaimsPrincipalFactory;
        private readonly SignInManager<MyUser> _signInManager;

        public HomeController(ILogger<HomeController> logger, UserManager<MyUser> userManager, IUserClaimsPrincipalFactory<MyUser> UserClaimsPrincipalFactory, SignInManager<MyUser> signInManager)
        {


            _logger = logger;
            _userManager = userManager;
            _UserClaimsPrincipalFactory = UserClaimsPrincipalFactory;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {

                var user = await _userManager.FindByNameAsync(model.UserName);


                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError("", "E-mail não está Válida!");
                        return View();
                    }
                    //var identity = new ClaimsIdentity("Identity.Application");
                    //identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                    //identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

                    if (await _userManager.GetTwoFactorEnabledAsync(user))
                    {
                        var validator = await _userManager.GetValidTwoFactorProvidersAsync(user);

                        if (validator.Contains("Email"))
                        {
                            var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                            System.IO.File.WriteAllText("email2sv.txt", token);

                            await HttpContext.SignInAsync(IdentityConstants.TwoFactorUserIdScheme,
                                Store2FA(user.Id, "Email"));

                            return RedirectToAction("TwoFactor");
                        }
                    }

                    var principal = await _UserClaimsPrincipalFactory.CreateAsync(user);

                    //await HttpContext.SignInAsync("Identity.Application", new ClaimsPrincipal(identity));
                    await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);
                    //var signInResult = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                    //if (signInResult.Succeeded)
                    //{


                    return RedirectToAction("About");
                }



                ModelState.AddModelError("", "Usuário ou senha errado");
            }
            return View();
        }
        public ClaimsPrincipal Store2FA(string userId, string provider)
        {

            var identity = new ClaimsIdentity(new List<Claim> {

            new  Claim("sub", userId),
            new  Claim("amr", provider)
            }, IdentityConstants.TwoFactorUserIdScheme);
            return new ClaimsPrincipal(identity);

        }
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();

        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> About()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Success()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            // Podemos fazer validação do data annotation assim
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);

                if (user == null)
                {
                    user = new MyUser()
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = model.UserName,
                        PasswordHash = model.Password,
                        Email = model.UserName


                    };

                    var result = await _userManager.CreateAsync(
                        user, model.Password);

                    if (result.Succeeded)
                    {
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var confirmationEmail = Url.Action("ConfirmEmailAddress", "Home",
                            new { token = token, email = user.Email }, Request.Scheme);

                        System.IO.File.WriteAllText("confirmedEmail.txt", confirmationEmail);
                    }
                    else
                    {
                        foreach (var item in result.Errors)
                        {
                            ModelState.AddModelError("", item.Description);
                        }
                        return View();
                    }

                }
                return View("Success");
            }
            return View();

        }


        [HttpGet]
        public async Task<IActionResult> ConfirmEmailAddress(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);

                if (result.Succeeded)
                {
                    return View("Success");
                }
            }
            return View("Error");
        }


        [HttpGet]
        public async Task<IActionResult> ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var resetURL = Url.Action("ResetPassword", "Home",
                        new { token = token, email = model.Email }, Request.Scheme);

                    System.IO.File.WriteAllText("resetPassword.txt", resetURL);

                    return View("Success");
                }
                else
                {

                }

            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {

            return View(new ResetPasswordModel { Token = token, Email = email });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

                    if (!result.Succeeded)
                    {
                        foreach (var item in result.Errors)
                        {
                            ModelState.AddModelError("", item.Description);
                        }
                        return View();
                    }
                    return View("Success");
                }
                ModelState.AddModelError("", "Request Invalid");
            }
            return View();
        }

        [HttpGet]
        public IActionResult TwoFactor()
        {
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> TwoFactor(TwoFactorModel model)
        {
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.TwoFactorUserIdScheme);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Seu token Expirou");
                return View();
            }
            if (ModelState.IsValid)
            {

                var user = await _userManager.FindByIdAsync(result.Principal.FindFirstValue("sub"));
                if (user != null)
                {
                    var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, result.Principal.FindFirstValue("amr"), model.Token);
                    if (isValid)
                    {
                        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                        var claimsPrincipal = await _UserClaimsPrincipalFactory.CreateAsync(user);
                        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal);

                        return RedirectToAction("About");
                    }

                    ModelState.AddModelError("", "Invalid Token");
                    return View();

                }
                ModelState.AddModelError("", "Invalid Request");
                return View();

            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

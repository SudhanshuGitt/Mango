using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Mango.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;

        public AuthController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }

        [HttpGet]
        public IActionResult Login()
        {
            LoginRequestDto loginRequestDto = new();
            return View(loginRequestDto);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
        {
            if (ModelState.IsValid)
            {
                ResponseDto? response = await _authService.LoginAsync(loginRequestDto);

                if (response != null && response.IsSuccess)
                {

                    LoginResponseDto loginResponseDto = JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(response.Result));

                    await SingnInUser(loginResponseDto);
                    _tokenProvider.SetToken(loginResponseDto.Token);

                    TempData["success"] = "Login successfull!";
                    // redirect to index action of home controller
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["error"] = response.Message;
                }

            }

            return View(loginRequestDto);
        }

        [HttpGet]
        public IActionResult Register()
        {
            var roleList = new List<SelectListItem>
            {
                new SelectListItem{Text= SD.Role.ADMIN.ToString(),Value=((int)SD.Role.ADMIN).ToString()},
                new SelectListItem{Text= SD.Role.CUSTOMER.ToString(),Value=((int)SD.Role.CUSTOMER).ToString()},
            };

            ViewBag.RoleList = roleList;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequestDto registrationRequestDto)
        {
            if (ModelState.IsValid)
            {
                ResponseDto? response = await _authService.RegisterAsync(registrationRequestDto);
                //ResponseDto? assingRole;

                if (response != null && response.IsSuccess)
                {

                    if (string.IsNullOrEmpty(registrationRequestDto.Role))
                    {
                        registrationRequestDto.Role = ((int)SD.Role.ADMIN).ToString();
                    }

                    ResponseDto? assingRole = await _authService.AssignRoleAsync(registrationRequestDto);

                    if (response != null && response.IsSuccess)
                    {
                        TempData["success"] = "Registration successfull!";
                    }

                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    TempData["error"] = response?.Message;
                }

            }
            var roleList = new List<SelectListItem>
            {
                new SelectListItem{Text= SD.Role.ADMIN.ToString(),Value=((int)SD.Role.ADMIN).ToString()},
                new SelectListItem{Text= SD.Role.CUSTOMER.ToString(),Value=((int)SD.Role.CUSTOMER).ToString()},
            };
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();

            return RedirectToAction("Index", "Home");
        }

        // signin the user using builtin .net identity
        private async Task SingnInUser(LoginResponseDto loginResponseDto)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(loginResponseDto.Token);

            //we need to extract all the claims we added from the token
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email,
                jwt.Claims.FirstOrDefault(d => d.Type == JwtRegisteredClaimNames.Email).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub,
                jwt.Claims.FirstOrDefault(d => d.Type == JwtRegisteredClaimNames.Sub).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name,
                jwt.Claims.FirstOrDefault(d => d.Type == JwtRegisteredClaimNames.Name).Value));

            identity.AddClaim(new Claim(ClaimTypes.Name,
              jwt.Claims.FirstOrDefault(d => d.Type == JwtRegisteredClaimNames.Email).Value));
            identity.AddClaim(new Claim(ClaimTypes.Role,
              jwt.Claims.FirstOrDefault(d => d.Type == "role").Value));

            // we can pas the identity in that we need to specify claim types
            var principal = new ClaimsPrincipal(identity);
            // signinasyn expect a claim principle we can also specify authentication scheme
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

    }
}

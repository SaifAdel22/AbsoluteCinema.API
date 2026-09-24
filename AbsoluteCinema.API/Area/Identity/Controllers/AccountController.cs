using AbsoluteCinema.API.DTOs.Requests;
using AbsoluteCinema.API.DTOs.Responses;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text;
using ForgetPasswordRequest = AbsoluteCinema.API.DTOs.Requests.ForgetPasswordRequest;
using LoginRequest = AbsoluteCinema.API.DTOs.Requests.LoginRequest;
using RegisterRequest = AbsoluteCinema.API.DTOs.Requests.RegisterRequest;
using ResendEmailConfirmationRequest = AbsoluteCinema.API.DTOs.Requests.ResendEmailConfirmationRequest;

namespace AbsoluteCinema.API.Areas.Identity.Controllers
{
    [ApiController]
    [Area(AreaConstants.IDENTITY_AREA)]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender, IUnitOfWork unitOfWork,
            IRepository<ApplicationUserOTP> applicationUserOTPRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
            _applicationUserOTPRepository = applicationUserOTPRepository;
        }


        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Data = [registerRequest]
                });
            }


            ApplicationUser user = registerRequest.Adapt<ApplicationUser>();

            var result = await _userManager.CreateAsync(user, registerRequest.Password);

            ModelStateDictionary modelState = new();

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    modelState.AddModelError(string.Empty, item.Description);
                }

                return BadRequest(new ErrorResponse
                {
                    Data = [modelState]
                });
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(Confirm), ControllerConstants.ACCOUNT_CONTROLLER, new { area = AreaConstants.IDENTITY_AREA, user.Id, token }, Request.Scheme);
            string body = $"<h1>Please confirm your account by clicking <b><a href='{link}'>here</a></b></h1>";

            await _emailSender.SendEmailAsync(user.Email!, "Confirm Your Account", body);

            var SUCCESS_NOTIFICATION = "Create Account Successfully, please verify your account";

            return Created($"{Request.Scheme}://{Request.Host}/Identity/account/login", new SuccessResponse
            {
                SuccessNotification = SUCCESS_NOTIFICATION
            });
        }

        [HttpPost("Confirm")]

        public async Task<IActionResult> Confirm(string id, string token)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return BadRequest(new ErrorResponse
                { ErrorNotification = "User not Found" });


            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                StringBuilder msg = new();

                foreach (var item in result.Errors)
                {
                    msg.Append(item.Description);
                }

                var ERROR_NOTIFICATION = msg.ToString();

                return BadRequest(new ErrorResponse()
                {
                    ErrorNotification = ERROR_NOTIFICATION
                });
            }

            var SUCCESS_NOTIFICATION = "Confirm Account successfully, please login";

            return Ok(new SuccessResponse()
            {
                SuccessNotification = SUCCESS_NOTIFICATION
            });
        }



        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Data = [loginRequest]
                });
            }


            var user = await _userManager.FindByEmailAsync(loginRequest.EmailOrUserName) ??
                                    await _userManager.FindByNameAsync(loginRequest.EmailOrUserName);
            var modelState = new ModelStateDictionary();
            if (user is null)
            {
                modelState.AddModelError(nameof(LoginRequest.EmailOrUserName), "Invalid User Name or Email");
                modelState.AddModelError(nameof(LoginRequest.Password), "Invalid Password");

                return BadRequest(new ErrorResponse
                {
                    Data = [modelState]
                });
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, loginRequest.Password, loginRequest.Remember, lockoutOnFailure: true);

            if (signInResult.IsLockedOut)
            {
                return BadRequest(new ErrorResponse
                {
                    ErrorNotification = "Too many attempts, please try again later"
                });
            }
            if (signInResult.IsNotAllowed)
            {
                return BadRequest(new ErrorResponse
                {
                    ErrorNotification = "Please verify your account!"
                });
            }

            if (!signInResult.Succeeded)
            {
                var modelstate = new ModelStateDictionary();

                modelstate.AddModelError(nameof(loginRequest.EmailOrUserName), "Invalid User Name or Email");
                modelstate.AddModelError(nameof(loginRequest.Password), "Invalid Password");

                return BadRequest(new ErrorResponse()
                {
                    Data = [modelstate]
                });
            }

            var SUCCESS_NOTIFICATION = $"Welcome Back {user.FirstName} {user.LastName}";


            return Created($"{Request.Scheme}://{Request.Host}/customer/home", new SuccessResponse
            {
                SuccessNotification = SUCCESS_NOTIFICATION
            });
        }



        [HttpPost("ResendConfirmation")]
        public async Task<IActionResult> ResendConfirmation(ResendEmailConfirmationRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse
                {
                    Data = [model]
                });

            if (model.EmailOrUserName is null)
            {
                var emailOrUN = new ModelStateDictionary();
                emailOrUN.AddModelError(nameof(model.EmailOrUserName), "Email Or UserName is required");
                return BadRequest(new ErrorResponse
                {
                    Data = [emailOrUN]
                });
            }

            var user = await _userManager.FindByEmailAsync(model.EmailOrUserName) ??
                                    await _userManager.FindByNameAsync(model.EmailOrUserName);
            if (user is null)
            {
                var userNull = new ModelStateDictionary();

                userNull.AddModelError(string.Empty, "If your email is registered, a confirmation link has been sent.");
                return BadRequest(new ErrorResponse
                {
                    Data = [userNull]
                });
            }
            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                var confirmed = new ModelStateDictionary();

                confirmed.AddModelError(string.Empty, "Your email is already confirmed.");
                return BadRequest(new ErrorResponse
                {
                    Data = [confirmed]
                });
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(Confirm), ControllerConstants.ACCOUNT_CONTROLLER, new { area = AreaConstants.IDENTITY_AREA, user.Id, token }, Request.Scheme);
            string body = $"<h1>Please confirm your account by clicking <b><a href='{link}'>here</a></b></h1>";

            await _emailSender.SendEmailAsync(user.Email!, "Confirm Your Account", body);

            var SUCCESS_NOTIFICATION = "Confirmation email resent. Please check your inbox.";

            return Ok(new SuccessResponse()
            {
                SuccessNotification = SUCCESS_NOTIFICATION
            });
        }

        [HttpPost("Logout")]

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            var SUCCESS_NOTIFICATION = "Logout successfully";
            return Ok(new SuccessResponse()
            {
                SuccessNotification = SUCCESS_NOTIFICATION
            });
        }



        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest forgetPasswordRequest, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse()
                {
                    Data = [forgetPasswordRequest]
                });


            var user = await _userManager.FindByEmailAsync(forgetPasswordRequest.EmailOrUserName) ??
                                    await _userManager.FindByNameAsync(forgetPasswordRequest.EmailOrUserName);

            if (user is null)
            {
                ModelStateDictionary modelState = new();

                modelState.AddModelError(nameof(forgetPasswordRequest.EmailOrUserName), "Invalid User Name or Email");

                return BadRequest(new ErrorResponse()
                {
                    Data = [modelState]
                });
            }

            string otp = new Random().Next(1000, 9999).ToString();

            await _applicationUserOTPRepository.CreateAsync(new()
            {
                ApplicationUserId = user.Id,
                OTP = otp,
            }, ct);
            await _applicationUserOTPRepository.CommitAsync(ct);

            string body = $"<h1>Your otp number is: {otp}. don't share it.</h1>";
            await _emailSender.SendEmailAsync(user.Email!, "Reset your account", body);

            Response.Cookies.Append("userId", user.Id);

            return Ok(new SuccessResponse()
            {
                Data = [new { RedirectToValidateOTP = Guid.NewGuid() }]
            });
        }


        [HttpPost("ValidateOTP")]
        public async Task<IActionResult> ValidateOTP(ValidateOTPRequest validateOTPRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse()
                {
                    Data = [validateOTPRequest]
                });

            var userId = Request.Cookies["userId"];
            if (userId is null) return NotFound(new ErrorResponse { ErrorNotification = "User session not found in cookies" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound(new ErrorResponse { ErrorNotification = "User not found" });

            var otpInDB = _applicationUserOTPRepository
                .Get(e => e.ApplicationUserId == userId && !e.IsUsed && e.ValidTo >= DateTime.UtcNow)
                .OrderBy(e => e.CreateAt)
                .LastOrDefault();

            if (otpInDB is null || otpInDB.OTP is null || validateOTPRequest.OTP != otpInDB.OTP)
            {
                return BadRequest(new ErrorResponse()
                {
                    ErrorNotification = "Invalid OTP"
                });
            }

            otpInDB.IsUsed = true;
            await _applicationUserOTPRepository.CommitAsync();

            return Ok(new SuccessResponse()
            {
                SuccessNotification = "Valid OTP, you can now change your password",
                Data = [new { RedirectToNewPassword = Guid.NewGuid() }]
            });
        }


        [HttpPost("NewPassword")]
        public async Task<IActionResult> NewPassword(NewPasswordRequest newPasswordRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse()
                {
                    Data = [newPasswordRequest]
                });

            var userId = Request.Cookies["userId"];
            if (userId is null) return NotFound(new ErrorResponse { ErrorNotification = "User session not found in cookies" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound(new ErrorResponse { ErrorNotification = "User not found" });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPasswordRequest.Password);

            if (!result.Succeeded)
            {
                ModelStateDictionary modelState = new();
                foreach (var item in result.Errors)
                {
                    modelState.AddModelError(string.Empty, item.Description);
                }

                return BadRequest(new ErrorResponse()
                {
                    Data = [modelState]
                });
            }

            Response.Cookies.Delete("userId");

            return Ok(new SuccessResponse()
            {
                SuccessNotification = "Reset Password successfully"
            });
        }

        /* public IActionResult AccessDenied()
         {
             return View();
         }

         public IActionResult ExternalLogin()
         {
             return View();
         }*/
    }
}
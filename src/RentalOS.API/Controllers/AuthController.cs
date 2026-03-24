using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Modules.Auth.Commands.Login;
using RentalOS.Application.Modules.Auth.Commands.Register;
using RentalOS.Application.Modules.Auth.Commands.ChangePassword;
using RentalOS.Application.Modules.Auth.Commands.ForgotPassword;
using RentalOS.Application.Modules.Auth.Commands.ResetPassword;
using RentalOS.Application.Modules.Auth.DTOs;
using MediatR;

namespace RentalOS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResultDto>> Register([FromBody] RegisterCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResultDto>> Refresh()
    {
        return BadRequest("Chưa triển khai logic Refresh Token.");
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var result = await mediator.Send(command);
        return result ? Ok("Yêu cầu khôi phục mật khẩu đã được xử lý.") : BadRequest("Email không tồn tại.");
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await mediator.Send(command);
        return result ? Ok("Mật khẩu đã được khôi phục.") : BadRequest("Token không hợp lệ hoặc đã hết hạn.");
    }

    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var result = await mediator.Send(command);
        return result ? Ok("Đổi mật khẩu thành công.") : BadRequest("Mật khẩu hiện tại không đúng.");
    }
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Modules.Settings.Commands.UpdateMoMoSettings;
using RentalOS.Application.Modules.Settings.Commands.UploadLogo;
using RentalOS.Application.Modules.Settings.Commands.TestMoMoConnection;
using RentalOS.Application.Modules.Settings.Queries.GetAllSettings;
using RentalOS.Application.Modules.Settings.Dtos;

namespace RentalOS.API.Controllers;

[Authorize(Roles = "owner")]
[ApiController]
[Route("api/v1/[controller]")]
public class SettingsController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AllSettingsDto>> GetAll()
    {
        return Ok(await mediator.Send(new GetAllSettingsQuery()));
    }

    [HttpPut("momo")]
    public async Task<IActionResult> UpdateMoMo(MoMoSettingsDto settings)
    {
        return Ok(await mediator.Send(new UpdateMoMoSettingsCommand(settings)));
    }

    [HttpPost("momo/test")]
    public async Task<ActionResult<TestConnectionResult>> TestMoMo()
    {
        return Ok(await mediator.Send(new TestMoMoConnectionCommand()));
    }

    [HttpPost("logo")]
    public async Task<ActionResult<string>> UploadLogo(IFormFile file)
    {
        return Ok(await mediator.Send(new UploadLogoCommand(file)));
    }

    // placeholder for remaining as per spec
    [HttpPut("vnpay")] public async Task<IActionResult> UpdateVNPay() => Ok();
    [HttpPost("vnpay/test")] public async Task<IActionResult> TestVNPay() => Ok();
    [HttpPut("bank")] public async Task<IActionResult> UpdateBank() => Ok();
    [HttpPut("zalo")] public async Task<IActionResult> UpdateZalo() => Ok();
    [HttpPut("sms")] public async Task<IActionResult> UpdateSms() => Ok();
    [HttpPut("email")] public async Task<IActionResult> UpdateEmail() => Ok();
    [HttpPut("billing")] public async Task<IActionResult> UpdateBilling() => Ok();
    [HttpPut("company")] public async Task<IActionResult> UpdateCompany() => Ok();
    [HttpGet("{key}")] public async Task<IActionResult> GetByKey(string key) => Ok();
}
 Eskom core endpoints implemented with 12 placeholders total. Eskom strict owner access.

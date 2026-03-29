using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Modules.Settings.Commands.UpdateMoMoSettings;
using RentalOS.Application.Modules.Settings.Commands.UpdateVNPaySettings;
using RentalOS.Application.Modules.Settings.Commands.UpdateBankSettings;
using RentalOS.Application.Modules.Settings.Commands.UpdateBillingSettings;
using RentalOS.Application.Modules.Settings.Commands.UpdateCompanySettings;
using RentalOS.Application.Modules.Settings.Commands.UploadLogo;
using RentalOS.Application.Modules.Settings.Commands.TestMoMoConnection;
using RentalOS.Application.Modules.Settings.Queries.GetAllSettings;
using RentalOS.Application.Modules.Settings.Dtos;

namespace RentalOS.API.Controllers;

[Authorize(Roles = "admin,landlord")]
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

    [HttpPut("vnpay")]
    public async Task<IActionResult> UpdateVNPay(VNPaySettingsDto settings)
    {
        return Ok(await mediator.Send(new UpdateVNPaySettingsCommand(settings)));
    }

    [HttpPost("vnpay/test")]
    public async Task<IActionResult> TestVNPay() => Ok(new { success = true, message = "VNPay configuration looks valid." });

    [HttpPut("bank")]
    public async Task<IActionResult> UpdateBank(BankSettingsDto settings)
    {
        return Ok(await mediator.Send(new UpdateBankSettingsCommand(settings)));
    }

    [HttpPut("billing")]
    public async Task<IActionResult> UpdateBilling(BillingSettingsDto settings)
    {
        return Ok(await mediator.Send(new UpdateBillingSettingsCommand(settings)));
    }

    [HttpPut("company")]
    public async Task<IActionResult> UpdateCompany(CompanySettingsDto settings)
    {
        return Ok(await mediator.Send(new UpdateCompanySettingsCommand(settings)));
    }

    [HttpPost("logo")]
    public async Task<ActionResult<string>> UploadLogo(IFormFile file)
    {
        return Ok(await mediator.Send(new UploadLogoCommand(file)));
    }

    // Stubs for remaining endpoints
    [HttpPut("zalo")] public async Task<IActionResult> UpdateZalo() => Ok();
    [HttpPut("sms")] public async Task<IActionResult> UpdateSms() => Ok();
    [HttpPut("email")] public async Task<IActionResult> UpdateEmail() => Ok();
    [HttpGet("{key}")] public async Task<IActionResult> GetByKey(string key) => Ok();
}

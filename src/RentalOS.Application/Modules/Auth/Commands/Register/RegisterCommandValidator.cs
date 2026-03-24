using FluentValidation;

namespace RentalOS.Application.Modules.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.OwnerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OwnerEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone)
            .NotEmpty()
            .Matches(@"^0\d{9}$")
            .WithMessage("Sđt VN phải bắt đầu bằng 0 và có 10 chữ số.");
        RuleFor(x => x.TenantName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"[A-Za-z]")
            .Matches(@"[0-9]")
            .Matches(@"[^a-zA-Z0-9]")
            .WithMessage("Mật khẩu tối thiểu 8 ký tự, bao gồm chữ, số và ký tự đặc biệt.");
    }
}

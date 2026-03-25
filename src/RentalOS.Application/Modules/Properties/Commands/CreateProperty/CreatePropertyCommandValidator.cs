using FluentValidation;

namespace RentalOS.Application.Modules.Properties.Commands.CreateProperty;

public class CreatePropertyCommandValidator : AbstractValidator<CreatePropertyCommand>
{
    public CreatePropertyCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Tên nhà trọ không được để trống.")
            .MinimumLength(2).WithMessage("Tên nhà trọ phải từ 2 ký tự trở lên.")
            .MaximumLength(200).WithMessage("Tên nhà trọ không được quá 200 ký tự.");

        RuleFor(v => v.Address)
            .NotEmpty().WithMessage("Địa chỉ không được để trống.");
    }
}

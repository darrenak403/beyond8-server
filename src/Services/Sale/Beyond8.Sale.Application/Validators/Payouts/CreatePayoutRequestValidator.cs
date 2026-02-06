using Beyond8.Sale.Application.Dtos.Payouts;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators.Payouts;

public class CreatePayoutRequestValidator : AbstractValidator<CreatePayoutRequest>
{
    public CreatePayoutRequestValidator()
    {
        RuleFor(x => x.InstructorId)
            .NotEmpty()
            .WithMessage("InstructorId không được để trống");

        // Per BR-19: Minimum payout amount is 500,000 VND
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(500000)
            .WithMessage("Số tiền rút tối thiểu là 500.000 VND");

        RuleFor(x => x.BankAccountNumber)
            .NotEmpty()
            .WithMessage("Số tài khoản ngân hàng không được để trống")
            .MaximumLength(50)
            .WithMessage("Số tài khoản ngân hàng không được vượt quá 50 ký tự");

        RuleFor(x => x.BankAccountName)
            .NotEmpty()
            .WithMessage("Tên chủ tài khoản không được để trống")
            .MaximumLength(200)
            .WithMessage("Tên chủ tài khoản không được vượt quá 200 ký tự");

        RuleFor(x => x.BankName)
            .NotEmpty()
            .WithMessage("Tên ngân hàng không được để trống")
            .MaximumLength(100)
            .WithMessage("Tên ngân hàng không được vượt quá 100 ký tự");

        RuleFor(x => x.Note)
            .MaximumLength(1000)
            .WithMessage("Ghi chú không được vượt quá 1000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Note));
    }
}
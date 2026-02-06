using Beyond8.Sale.Application.Dtos.Payouts;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators.Payouts
{
    public class CreatePayoutRequestValidator : AbstractValidator<CreatePayoutRequest>
    {
        public CreatePayoutRequestValidator()
        {
            RuleFor(x => x.InstructorId)
                .NotEmpty()
                .WithMessage("InstructorId không được để trống");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Số tiền thanh toán phải lớn hơn 0");

            RuleFor(x => x.BankAccount)
                .NotEmpty()
                .WithMessage("Số tài khoản ngân hàng không được để trống")
                .MaximumLength(50)
                .WithMessage("Số tài khoản ngân hàng không được vượt quá 50 ký tự");

            RuleFor(x => x.BankName)
                .NotEmpty()
                .WithMessage("Tên ngân hàng không được để trống")
                .MaximumLength(100)
                .WithMessage("Tên ngân hàng không được vượt quá 100 ký tự");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .WithMessage("Ghi chú không được vượt quá 500 ký tự")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}
using Beyond8.Sale.Application.Dtos.Transactions;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators.Transactions
{
    public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
    {
        public CreateTransactionRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId không được để trống");

            RuleFor(x => x.Type)
                .NotEmpty()
                .WithMessage("Loại giao dịch không được để trống")
                .Must(type => new[] { "Payment", "Refund", "Payout", "Commission" }.Contains(type))
                .WithMessage("Loại giao dịch không hợp lệ. Các giá trị hợp lệ: Payment, Refund, Payout, Commission");

            RuleFor(x => x.Amount)
                .NotEqual(0)
                .WithMessage("Số tiền giao dịch không được bằng 0");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Mô tả giao dịch không được để trống")
                .MaximumLength(500)
                .WithMessage("Mô tả giao dịch không được vượt quá 500 ký tự");

            RuleFor(x => x.TransactionId)
                .MaximumLength(100)
                .WithMessage("Mã giao dịch không được vượt quá 100 ký tự")
                .When(x => !string.IsNullOrEmpty(x.TransactionId));
        }
    }
}
using Beyond8.Sale.Application.Dtos.Transactions;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators.Transactions;

public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty()
            .WithMessage("WalletId không được để trống");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Loại giao dịch không hợp lệ");

        RuleFor(x => x.Amount)
            .NotEqual(0)
            .WithMessage("Số tiền giao dịch không được bằng 0");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Mô tả giao dịch không được vượt quá 500 ký tự");

        RuleFor(x => x.ReferenceType)
            .MaximumLength(50)
            .WithMessage("ReferenceType không được vượt quá 50 ký tự")
            .When(x => !string.IsNullOrEmpty(x.ReferenceType));
    }
}
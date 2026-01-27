using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Users;
using FluentValidation;

namespace Beyond8.Identity.Application.Validators.User
{
    public class UpdateUserForAdminRequestValidator : AbstractValidator<UpdateUserForAdminRequest>
    {
        private static readonly string[] ValidRoleCodes =
        [
            Role.Student,
            Role.Instructor,
            Role.Staff,
            Role.Admin
        ];

        public UpdateUserForAdminRequestValidator()
        {
            RuleFor(x => x.Roles)
                .NotEmpty().WithMessage("Phải có ít nhất một vai trò")
                .Must(roles => roles != null && roles.Count > 0).WithMessage("Phải có ít nhất một vai trò");

            RuleForEach(x => x.Roles)
                .Must(roleCode => ValidRoleCodes.Contains(roleCode))
                .WithMessage(roleCode => $"Vai trò '{roleCode}' không hợp lệ. Các vai trò hợp lệ: {string.Join(", ", ValidRoleCodes)}");
        }
    }
}

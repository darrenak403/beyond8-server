using Beyond8.Identity.Application.Dtos.Instructors;
using FluentValidation;

namespace Beyond8.Identity.Application.Validators.Instructor;

public class CreateInstructorProfileRequestValidator : AbstractValidator<CreateInstructorProfileRequest>
{
    public CreateInstructorProfileRequestValidator()
    {
        // Bio validation
        RuleFor(x => x.Bio)
            .NotEmpty().WithMessage("Bio không được để trống")
            .MaximumLength(300).WithMessage("Bio không được vượt quá 300 ký tự");

        // Headline validation
        RuleFor(x => x.Headline)
            .NotEmpty().WithMessage("Headline không được để trống")
            .MaximumLength(200).WithMessage("Headline không được vượt quá 200 ký tự");

        // ExpertiseAreas validation
        RuleFor(x => x.ExpertiseAreas)
            .NotEmpty().WithMessage("Lĩnh vực chuyên môn không được để trống")
            .Must(x => x.Count > 0).WithMessage("Phải có ít nhất một lĩnh vực chuyên môn")
            .ForEach(expertise =>
            {
                expertise.NotEmpty().WithMessage("Lĩnh vực chuyên môn không được để trống")
                     .MaximumLength(100).WithMessage("Lĩnh vực chuyên môn không được vượt quá 100 ký tự");
            });

        // Education validation
        RuleFor(x => x.Education)
            .NotEmpty().WithMessage("Thông tin học vấn không được để trống")
            .Must(x => x.Count > 0).WithMessage("Phải có ít nhất một thông tin học vấn")
            .ForEach(education =>
            {
                education.ChildRules(child =>
                {
                    child.RuleFor(e => e.School)
                        .NotEmpty().WithMessage("Trường học không được để trống")
                        .MaximumLength(200).WithMessage("Tên trường không được vượt quá 200 ký tự");

                    child.RuleFor(e => e.Degree)
                        .NotEmpty().WithMessage("Bằng cấp không được để trống")
                        .MaximumLength(100).WithMessage("Bằng cấp không được vượt quá 100 ký tự");

                    child.RuleFor(e => e.Start)
                        .GreaterThan(1900).WithMessage("Năm bắt đầu phải lớn hơn 1900")
                        .LessThanOrEqualTo(DateTime.Now.Year).WithMessage("Năm bắt đầu không được lớn hơn năm hiện tại");

                    child.RuleFor(e => e.End)
                        .GreaterThanOrEqualTo(e => e.Start).WithMessage("Năm kết thúc phải sau hoặc bằng năm bắt đầu")
                        .LessThanOrEqualTo(DateTime.Now.Year + 10).WithMessage($"Năm kết thúc không được lớn hơn {DateTime.Now.Year + 10}");
                });
            });

        // WorkExperience validation (optional)
        When(x => x.WorkExperience != null && x.WorkExperience.Any(), () =>
        {
            RuleFor(x => x.WorkExperience)
                .ForEach(work =>
                {
                    work.ChildRules(child =>
                    {
                        child.RuleFor(w => w.Company)
                            .NotEmpty().WithMessage("Công ty không được để trống")
                            .MaximumLength(200).WithMessage("Tên công ty không được vượt quá 200 ký tự");

                        child.RuleFor(w => w.Role)
                            .NotEmpty().WithMessage("Chức vụ không được để trống")
                            .MaximumLength(100).WithMessage("Chức vụ không được vượt quá 100 ký tự");

                        child.RuleFor(w => w.From)
                            .NotEmpty().WithMessage("Thời gian bắt đầu không được để trống")
                            .MaximumLength(50).WithMessage("Thời gian bắt đầu không được vượt quá 50 ký tự");

                        child.RuleFor(w => w.To)
                            .MaximumLength(50).WithMessage("Thời gian kết thúc không được vượt quá 50 ký tự");
                    });
                });
        });

        // SocialLinks validation (optional)
        When(x => x.SocialLinks != null, () =>
        {
            RuleFor(x => x.SocialLinks!.Website)
                .Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Website phải là URL hợp lệ");

            RuleFor(x => x.SocialLinks!.LinkedIn)
                .Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("LinkedIn phải là URL hợp lệ");

            RuleFor(x => x.SocialLinks!.Facebook)
                .Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Facebook phải là URL hợp lệ");
        });

        // BankInfo validation
        RuleFor(x => x.BankInfo)
            .NotEmpty().WithMessage("Thông tin ngân hàng không được để trống")
            .MaximumLength(500).WithMessage("Thông tin ngân hàng không được vượt quá 500 ký tự");

        // TaxId validation (optional)
        RuleFor(x => x.TaxId)
            .MaximumLength(20).WithMessage("Mã số thuế không được vượt quá 20 ký tự")
            .When(x => !string.IsNullOrEmpty(x.TaxId))
            .Matches(@"^[0-9\-]+$").WithMessage("Mã số thuế chỉ được chứa số và dấu gạch ngang");

        // IdentityDocuments validation
        RuleFor(x => x.IdentityDocuments)
            .NotEmpty().WithMessage("Giấy tờ tùy thân không được để trống")
            .Must(x => x.Count > 0).WithMessage("Phải có ít nhất một giấy tờ tùy thân")
            .ForEach(document =>
            {
                document.ChildRules(child =>
                {
                    child.RuleFor(d => d.Type)
                        .NotEmpty().WithMessage("Loại giấy tờ không được để trống")
                        .MaximumLength(50).WithMessage("Loại giấy tờ không được vượt quá 50 ký tự");

                    child.RuleFor(d => d.Number)
                        .NotEmpty().WithMessage("Số giấy tờ không được để trống")
                        .MaximumLength(50).WithMessage("Số giấy tờ không được vượt quá 50 ký tự");

                    child.RuleFor(d => d.IssuedDate)
                        .NotEmpty().WithMessage("Ngày cấp không được để trống")
                        .MaximumLength(50).WithMessage("Ngày cấp không được vượt quá 50 ký tự");

                    child.RuleFor(d => d.FrontImg)
                        .NotEmpty().WithMessage("Ảnh mặt trước không được để trống")
                        .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                        .WithMessage("URL ảnh mặt trước phải hợp lệ");

                    child.RuleFor(d => d.BackImg)
                        .NotEmpty().WithMessage("Ảnh mặt sau không được để trống")
                        .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                        .WithMessage("URL ảnh mặt sau phải hợp lệ");
                });
            });

        // Certificates validation (optional)
        When(x => x.Certificates != null && x.Certificates.Any(), () =>
        {
            RuleFor(x => x.Certificates)
                .ForEach(certificate =>
                {
                    certificate.ChildRules(child =>
                    {
                        child.RuleFor(c => c.Name)
                            .NotEmpty().WithMessage("Tên chứng chỉ không được để trống")
                            .MaximumLength(200).WithMessage("Tên chứng chỉ không được vượt quá 200 ký tự");

                        child.RuleFor(c => c.Issuer)
                            .NotEmpty().WithMessage("Tổ chức cấp không được để trống")
                            .MaximumLength(200).WithMessage("Tên tổ chức không được vượt quá 200 ký tự");

                        child.RuleFor(c => c.Year)
                            .GreaterThan(1900).WithMessage("Năm cấp phải lớn hơn 1900")
                            .LessThanOrEqualTo(DateTime.Now.Year + 10).WithMessage($"Năm cấp không được lớn hơn {DateTime.Now.Year + 10}");

                        child.RuleFor(c => c.Url)
                            .NotEmpty().WithMessage("URL chứng chỉ không được để trống")
                            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                            .WithMessage("URL chứng chỉ phải hợp lệ");
                    });
                });
        });
    }
}

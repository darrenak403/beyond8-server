using FluentValidation;
using Beyond8.Identity.Application.Dtos.Instructors;
using Beyond8.Identity.Domain.JSONFields;

namespace Beyond8.Identity.Application.Validators.Instructor
{
    public class UpdateInstructorProfileRequestValidator : AbstractValidator<UpdateInstructorProfileRequest>
    {
        public UpdateInstructorProfileRequestValidator()
        {
            RuleFor(x => x.Bio)
                .MaximumLength(1000)
                .WithMessage("Tiểu sử không được vượt quá 1000 ký tự");

            RuleFor(x => x.Headline)
                .MaximumLength(200)
                .WithMessage("Tiêu đề không được vượt quá 200 ký tự");

            RuleFor(x => x.ExpertiseAreas)
                .Custom((value, context) =>
                {
                    if (value != null && value.Any(string.IsNullOrWhiteSpace))
                    {
                        context.AddFailure("ExpertiseAreas", "Lĩnh vực chuyên môn không được chứa giá trị trống");
                    }
                });

            RuleFor(x => x.Education)
                .Custom((value, context) =>
                {
                    if (value != null && !ValidateEducationList(value, context))
                    {
                        context.AddFailure("Education", "Thông tin giáo dục phải có tất cả các trường bắt buộc");
                    }
                });

            RuleFor(x => x.WorkExperience)
                .Custom((value, context) =>
                {
                    if (value != null && !ValidateWorkExperienceList(value, context))
                    {
                        context.AddFailure("WorkExperience", "Thông tin kinh nghiệm làm việc phải có tất cả các trường bắt buộc");
                    }
                });

            RuleFor(x => x.SocialLinks)
                .Custom((value, context) =>
                {
                    if (value != null)
                    {
                        ValidateSocialLinks(value, context);
                    }
                });

            // BankInfo validation (optional)
            When(x => x.BankInfo != null, () =>
            {
                RuleFor(x => x.BankInfo)
                    .ChildRules(bank =>
                    {
                        bank.RuleFor(b => b!.BankName)
                            .NotEmpty().WithMessage("Tên ngân hàng không được để trống")
                            .MaximumLength(200).WithMessage("Tên ngân hàng không được vượt quá 200 ký tự");

                        bank.RuleFor(b => b!.AccountNumber)
                            .NotEmpty().WithMessage("Số tài khoản không được để trống")
                            .MaximumLength(50).WithMessage("Số tài khoản không được vượt quá 50 ký tự")
                            .Matches(@"^[0-9]+$").WithMessage("Số tài khoản chỉ được chứa số");

                        bank.RuleFor(b => b!.AccountHolderName)
                            .NotEmpty().WithMessage("Tên chủ tài khoản không được để trống")
                            .MaximumLength(200).WithMessage("Tên chủ tài khoản không được vượt quá 200 ký tự");
                    });
            });

            // TeachingLanguages validation (optional)
            When(x => x.TeachingLanguages != null, () =>
            {
                RuleFor(x => x.TeachingLanguages)
                    .Must(x => x!.Count > 0).WithMessage("Phải có ít nhất một ngôn ngữ giảng dạy")
                    .ForEach(language =>
                    {
                        language.NotEmpty().WithMessage("Ngôn ngữ giảng dạy không được để trống")
                             .MaximumLength(20).WithMessage("Mã ngôn ngữ không được vượt quá 20 ký tự");
                    });
            });

            // IntroVideoUrl validation (optional)
            RuleFor(x => x.IntroVideoUrl)
                .Must(uri => string.IsNullOrEmpty(uri) || IsValidUrl(uri))
                .WithMessage("URL video giới thiệu phải hợp lệ")
                .MaximumLength(500).WithMessage("URL video giới thiệu không được vượt quá 500 ký tự");
        }

        /// <summary>
        /// Validate education list has required fields
        /// </summary>
        private static bool ValidateEducationList(List<EducationInfo> educationList, ValidationContext<UpdateInstructorProfileRequest> context)
        {
            return !educationList.Any(edu => string.IsNullOrWhiteSpace(edu.Degree) || string.IsNullOrWhiteSpace(edu.School));
        }

        /// <summary>
        /// Validate work experience list has required fields
        /// </summary>
        private static bool ValidateWorkExperienceList(List<WorkInfo> workList, ValidationContext<UpdateInstructorProfileRequest> context)
        {
            return !workList.Any(work => string.IsNullOrWhiteSpace(work.Role) || string.IsNullOrWhiteSpace(work.Company));
        }

        /// <summary>
        /// Validate all social media URLs
        /// </summary>
        private static void ValidateSocialLinks(SocialInfo socialLinks, ValidationContext<UpdateInstructorProfileRequest> context)
        {
            ValidateSocialUrl(socialLinks.LinkedIn, "LinkedIn", context);
            ValidateSocialUrl(socialLinks.Facebook, "Facebook", context);
            ValidateSocialUrl(socialLinks.Website, "Website", context);
        }

        /// <summary>
        /// Validate a single social media URL
        /// </summary>
        private static void ValidateSocialUrl(string? url, string platform, ValidationContext<UpdateInstructorProfileRequest> context)
        {
            if (!string.IsNullOrWhiteSpace(url) && !IsValidUrl(url))
            {
                context.AddFailure($"SocialLinks.{platform}", $"{platform} URL không hợp lệ");
            }
        }

        /// <summary>
        /// Check if URL is valid
        /// </summary>
        private static bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
using Beyond8.Integration.Application.Dtos.VnptEkyc;
using FluentValidation;

namespace Beyond8.Integration.Application.Validators.VnptEkyc
{
    public class LivenessRequestValidator : AbstractValidator<LivenessRequest>
    {
        public LivenessRequestValidator()
        {
            RuleFor(x => x.Img)
                .NotEmpty().WithMessage("Image hash không được để trống")
                .Must(BeValidHash).WithMessage("Image hash không hợp lệ");

            RuleFor(x => x.ClientSession)
                .NotEmpty().WithMessage("Client session không được để trống")
                .MaximumLength(500).WithMessage("Client session không được vượt quá 500 ký tự")
                .Must(BeValidClientSession).WithMessage("Client session không đúng định dạng. Định dạng: <IOS/ANDROID>_<model>_<OS/API>_<Device/Simulator>_<SDK version>_<Device id>_<Timestamp>");
        }

        private bool BeValidHash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return false;

            var parts = hash.Split('/');
            return parts.Length >= 3 && hash.StartsWith("idg-");
        }

        private bool BeValidClientSession(string clientSession)
        {
            if (string.IsNullOrWhiteSpace(clientSession))
                return false;

            var parts = clientSession.Split('_');
            if (parts.Length < 7)
                return false;

            var platform = parts[0].ToUpper();
            return platform == "IOS" || platform == "ANDROID";
        }
    }
}

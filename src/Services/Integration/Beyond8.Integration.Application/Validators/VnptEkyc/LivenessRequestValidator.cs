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

            // Basic validation for Minio hash format
            // Format: idg-{guid}/{guid}/{date}/{filename}
            // Example: idg-bbee2cbb-b05c-4857-9788-6ed9c3f74391/397894be-02aa-4a4f-8458-a912ef72167c/20200331/IDG01_622c2972-730a-11ea-9632-8fd0d0c7fe71
            var parts = hash.Split('/');
            return parts.Length >= 3 && hash.StartsWith("idg-");
        }

        private bool BeValidClientSession(string clientSession)
        {
            if (string.IsNullOrWhiteSpace(clientSession))
                return false;

            // Validate client session format
            // Format: <IOS/ANDROID>_<model>_<OS/API>_<Device/Simulator>_<SDK version>_<Device id>_<Timestamp>
            var parts = clientSession.Split('_');
            if (parts.Length < 7)
                return false;

            var platform = parts[0].ToUpper();
            return platform == "IOS" || platform == "ANDROID";
        }
    }
}

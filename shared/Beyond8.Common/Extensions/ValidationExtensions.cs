using Microsoft.AspNetCore.Http;
using FluentValidation;
using Beyond8.Common.Utilities;

namespace Beyond8.Common.Extensions
{
    public static class ValidationExtensions
    {
        public static bool ValidateRequest<T>(this T request, IValidator<T> validator, out IResult? result) where T : class
        {
            result = null;

            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

                var errorMessage = string.Join(", ", errors);
                result = Results.BadRequest(ApiResponse<object>.FailureResponse(errorMessage));
                return false;
            }

            return true;
        }
    }
}

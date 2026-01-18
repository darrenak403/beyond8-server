using Microsoft.AspNetCore.Http;
using FluentValidation;
using Beyond8.Common.Utilities;

namespace Beyond8.Common.Extensions;

public static class ValidationExtensions
{
    /// <summary>
    /// Validates a request object using FluentValidation and returns BadRequest with ApiResponse if validation fails
    /// </summary>
    /// <typeparam name="T">The type of request object to validate</typeparam>
    /// <param name="request">The request object to validate</param>
    /// <param name="validator">The FluentValidation validator instance</param>
    /// <param name="result">Output IResult containing validation errors if validation failed</param>
    /// <returns>True if validation passed, false if validation failed (check result parameter)</returns>
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

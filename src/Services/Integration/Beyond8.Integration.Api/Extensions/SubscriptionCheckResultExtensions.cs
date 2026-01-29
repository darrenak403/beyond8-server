using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Helpers.AiService;

namespace Beyond8.Integration.Api.Extensions
{
    public static class SubscriptionCheckResultExtensions
    {
        public static IResult ToDeniedResult<T>(this SubscriptionCheckResult check)
        {
            return Results.BadRequest(ApiResponse<T>.FailureResponse(check.Message, check.Metadata));
        }
    }
}

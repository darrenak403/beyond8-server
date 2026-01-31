namespace Beyond8.Assessment.Api.Apis
{
    public static class AssignmentSubmissionApis
    {
        public static IEndpointRouteBuilder MapAssignmentSubmissionApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/assignment-submissions")
                .MapAssignmentSubmissionRoutes()
                .WithTags("Assignment Submission Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        private static RouteGroupBuilder MapAssignmentSubmissionRoutes(this RouteGroupBuilder group)
        {

            return group;
        }
    }
}
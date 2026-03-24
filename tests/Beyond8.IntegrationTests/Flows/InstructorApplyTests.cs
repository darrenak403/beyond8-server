using Beyond8.Common.Events.Identity;
using Beyond8.Identity.Application.Dtos.Instructors;
using Beyond8.Identity.Domain.Enums;
using Beyond8.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Beyond8.IntegrationTests.Flows;

public partial class InstructorApplyTests(IdentityTestFixture fixture)
    : IClassFixture<IdentityTestFixture>
{
    [Fact]
    public async Task SubmitInstructorProfile_WithValidData_ShouldCreatePendingProfile()
    {
        using var context = fixture.CreateInstructorContext();
        var request = TestDataFactory.BuildValidCreateInstructorRequest();

        var result = await context.Service.SubmitInstructorProfileAsync(request, context.StudentUserId);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.VerificationStatus.Should().Be(VerificationStatus.Pending);

        var profile = await context.DbContext.InstructorProfiles
            .FirstOrDefaultAsync(x => x.UserId == context.StudentUserId);

        profile.Should().NotBeNull();
        profile!.VerificationStatus.Should().Be(VerificationStatus.Pending);

        context.PublishEndpointMock.Verify(
            x => x.Publish(It.IsAny<InstructorProfileSubmittedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ApproveInstructorProfile_ByAdmin_ShouldSetVerifiedAndAssignInstructorRole()
    {
        using var context = fixture.CreateInstructorContext();

        var submit = await context.Service.SubmitInstructorProfileAsync(
            TestDataFactory.BuildValidCreateInstructorRequest(),
            context.StudentUserId);
        submit.IsSuccess.Should().BeTrue();
        var profileId = submit.Data!.Id;

        var approve = await context.Service.ApproveInstructorProfileAsync(profileId, context.AdminUserId);

        approve.IsSuccess.Should().BeTrue();
        approve.Data.Should().NotBeNull();
        approve.Data!.VerificationStatus.Should().Be(VerificationStatus.Verified);

        var profile = await context.DbContext.InstructorProfiles.FirstAsync(x => x.Id == profileId);
        profile.VerificationStatus.Should().Be(VerificationStatus.Verified);
        profile.VerifiedBy.Should().Be(context.AdminUserId);
        profile.VerifiedAt.Should().NotBeNull();

        var user = await context.DbContext.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstAsync(x => x.Id == context.StudentUserId);

        user.UserRoles.Should().Contain(ur => ur.Role.Code == "ROLE_INSTRUCTOR" && ur.RevokedAt == null);

        context.PublishEndpointMock.Verify(
            x => x.Publish(It.IsAny<InstructorApprovalEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SubmitInstructorProfile_WhenPendingProfileAlreadyExists_ShouldRejectDuplicateApply()
    {
        using var context = fixture.CreateInstructorContext();
        var request = TestDataFactory.BuildValidCreateInstructorRequest();

        var firstApply = await context.Service.SubmitInstructorProfileAsync(request, context.StudentUserId);
        var secondApply = await context.Service.SubmitInstructorProfileAsync(request, context.StudentUserId);

        firstApply.IsSuccess.Should().BeTrue();
        secondApply.IsSuccess.Should().BeFalse();
        secondApply.Message.Should().ContainEquivalentOf("đang chờ duyệt");
    }

    [Fact]
    public async Task ApproveInstructorProfile_WhenProfileDoesNotExist_ShouldFail()
    {
        using var context = fixture.CreateInstructorContext();

        var result = await context.Service.ApproveInstructorProfileAsync(Guid.NewGuid(), context.AdminUserId);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().ContainEquivalentOf("không tồn tại");
    }

    [Fact]
    public async Task ApproveInstructorProfile_WhenEkycFailed_ShouldNotApproveAndShouldNotAssignInstructorRole()
    {
        using var context = fixture.CreateInstructorContext();

        var submit = await context.Service.SubmitInstructorProfileAsync(
            TestDataFactory.BuildValidCreateInstructorRequest(),
            context.StudentUserId);
        submit.IsSuccess.Should().BeTrue();

        var reject = await context.Service.NotApproveInstructorProfileAsync(
            submit.Data!.Id,
            new NotApproveInstructorProfileRequest
            {
                VerificationStatus = VerificationStatus.RequestUpdate,
                NotApproveReason = "eKYC failed: face mismatch score below threshold."
            },
            context.AdminUserId);

        reject.IsSuccess.Should().BeTrue();
        reject.Data!.VerificationStatus.Should().Be(VerificationStatus.RequestUpdate);

        var profile = await context.DbContext.InstructorProfiles.FirstAsync(x => x.Id == submit.Data.Id);
        profile.VerificationNotes.Should().ContainEquivalentOf("eKYC");

        var user = await context.DbContext.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstAsync(x => x.Id == context.StudentUserId);

        user.UserRoles.Should().NotContain(ur => ur.Role.Code == "ROLE_INSTRUCTOR" && ur.RevokedAt == null);
    }

    [Fact]
    public async Task ApproveInstructorProfile_WhenAiProfileScoreBelowThreshold_ShouldNotApproveAndShouldNotAssignInstructorRole()
    {
        using var context = fixture.CreateInstructorContext();

        var submit = await context.Service.SubmitInstructorProfileAsync(
            TestDataFactory.BuildValidCreateInstructorRequest(),
            context.StudentUserId);
        submit.IsSuccess.Should().BeTrue();

        var reject = await context.Service.NotApproveInstructorProfileAsync(
            submit.Data!.Id,
            new NotApproveInstructorProfileRequest
            {
                VerificationStatus = VerificationStatus.RequestUpdate,
                NotApproveReason = "AI profile score 0.42 < pass threshold 0.70."
            },
            context.AdminUserId);

        reject.IsSuccess.Should().BeTrue();
        reject.Data!.VerificationStatus.Should().Be(VerificationStatus.RequestUpdate);

        var profile = await context.DbContext.InstructorProfiles.FirstAsync(x => x.Id == submit.Data.Id);
        profile.VerificationNotes.Should().ContainEquivalentOf("AI profile score");

        var user = await context.DbContext.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstAsync(x => x.Id == context.StudentUserId);

        user.UserRoles.Should().NotContain(ur => ur.Role.Code == "ROLE_INSTRUCTOR" && ur.RevokedAt == null);
    }

}

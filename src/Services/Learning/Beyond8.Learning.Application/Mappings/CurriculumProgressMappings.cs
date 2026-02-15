using Beyond8.Learning.Application.Dtos.Catalog;
using Beyond8.Learning.Application.Dtos.Progress;
using Beyond8.Learning.Domain.Entities;
using Beyond8.Learning.Domain.Enums;

namespace Beyond8.Learning.Application.Mappings;

public static class CurriculumProgressMappings
{
    public static CurriculumProgressResponse ToCurriculumProgressResponse(
        this Enrollment enrollment,
        CourseStructureResponse structure,
        IReadOnlyDictionary<Guid, LessonProgress> lessonProgressByLessonId,
        IReadOnlyDictionary<Guid, SectionProgress> sectionProgressBySectionId)
    {
        var sections = structure.Sections
            .OrderBy(s => s.Order)
            .Select(s => s.ToSectionProgressItem(
                lessonProgressByLessonId,
                sectionProgressBySectionId.GetValueOrDefault(s.Id)))
            .ToList();

        // Compute completed/total from actual lesson progress so summary matches section detail
        var allLessonIds = structure.Sections.SelectMany(s => s.Lessons).Select(l => l.Id).ToList();
        var totalLessons = allLessonIds.Count;
        var completedLessons = allLessonIds.Count(lessonId =>
        {
            var progress = lessonProgressByLessonId.GetValueOrDefault(lessonId);
            var status = progress?.Status;
            return status is LessonProgressStatus.Completed or LessonProgressStatus.Failed;
        });
        var progressPercent = totalLessons > 0
            ? Math.Round((decimal)completedLessons * 100 / totalLessons, 2)
            : 0;

        return new CurriculumProgressResponse
        {
            EnrollmentId = enrollment.Id,
            CourseId = enrollment.CourseId,
            CourseTitle = enrollment.CourseTitle,
            ProgressPercent = progressPercent,
            CompletedLessons = completedLessons,
            TotalLessons = totalLessons,
            Sections = sections
        };
    }

    public static SectionProgressItem ToSectionProgressItem(
        this SectionStructureItem section,
        IReadOnlyDictionary<Guid, LessonProgress> lessonProgressByLessonId,
        SectionProgress? sectionProgress)
    {
        var lessons = section.Lessons
            .OrderBy(l => l.Order)
            .Select(l => l.ToLessonProgressItem(lessonProgressByLessonId.GetValueOrDefault(l.Id)))
            .ToList();

        var allLessonsCompleted = section.Lessons.Count > 0 && lessons.TrueForAll(l => l.IsCompleted);

        return new SectionProgressItem
        {
            SectionId = section.Id,
            Title = section.Title,
            Order = section.Order,
            IsCompleted = allLessonsCompleted,
            AssignmentSubmitted = sectionProgress?.AssignmentSubmitted ?? false,
            AssignmentGrade = sectionProgress?.AssignmentGrade,
            AssignmentPassed = sectionProgress?.AssignmentPassed ?? false,
            AssignmentSubmittedAt = sectionProgress?.AssignmentSubmittedAt,
            AssignmentGradedAt = sectionProgress?.AssignmentGradedAt,
            Lessons = lessons
        };
    }

    public static LessonProgressItem ToLessonProgressItem(
        this LessonStructureItem lesson,
        LessonProgress? progress)
    {
        var status = progress?.Status;
        var isCompleted = status is LessonProgressStatus.Completed or LessonProgressStatus.Failed;
        var isPassed = status == LessonProgressStatus.Completed;

        return new LessonProgressItem
        {
            LessonId = lesson.Id,
            Title = lesson.Title,
            Order = lesson.Order,
            IsCompleted = isCompleted,
            IsPassed = isPassed
        };
    }
}

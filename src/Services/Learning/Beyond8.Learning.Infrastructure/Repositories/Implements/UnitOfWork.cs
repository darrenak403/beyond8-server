using Beyond8.Learning.Domain.Repositories.Interfaces;
using Beyond8.Learning.Infrastructure.Data;
using Beyond8.Common.Data.Implements;

namespace Beyond8.Learning.Infrastructure.Repositories.Implements;

public class UnitOfWork(LearningDbContext context) : BaseUnitOfWork<LearningDbContext>(context), IUnitOfWork
{
    private IEnrollmentRepository? _enrollmentRepository;
    private ILessonProgressRepository? _lessonProgressRepository;
    private ISectionProgressRepository? _sectionProgressRepository;
    private ICertificateRepository? _certificateRepository;
    private ICourseReviewRepository? _courseReviewRepository;

    public IEnrollmentRepository EnrollmentRepository =>
        _enrollmentRepository ??= new EnrollmentRepository(context);

    public ILessonProgressRepository LessonProgressRepository =>
        _lessonProgressRepository ??= new LessonProgressRepository(context);

    public ISectionProgressRepository SectionProgressRepository =>
        _sectionProgressRepository ??= new SectionProgressRepository(context);

    public ICertificateRepository CertificateRepository =>
        _certificateRepository ??= new CertificateRepository(context);

    public ICourseReviewRepository CourseReviewRepository =>
        _courseReviewRepository ??= new CourseReviewRepository(context);
}

using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Catalog.Infrastructure.Data;
using Beyond8.Common.Data.Implements;

namespace Beyond8.Catalog.Infrastructure.Repositories.Implements
{
    public class UnitOfWork(CatalogDbContext context) : BaseUnitOfWork<CatalogDbContext>(context), IUnitOfWork
    {
        private ICategoryRepository? _categoryRepository;
        private ICourseRepository? _courseRepository;
        private ISectionRepository? _sectionRepository;
        private ILessonRepository? _lessonRepository;
        private ICourseDocumentRepository? _courseDocumentRepository;
        private ILessonDocumentRepository? _lessonDocumentRepository;
        public ICategoryRepository CategoryRepository => _categoryRepository ??= new CategoryRepository(context);
        public ICourseRepository CourseRepository => _courseRepository ??= new CourseRepository(context);
        public ISectionRepository SectionRepository => _sectionRepository ??= new SectionRepository(context);
        public ILessonRepository LessonRepository => _lessonRepository ??= new LessonRepository(context);
        public ICourseDocumentRepository CourseDocumentRepository => _courseDocumentRepository ??= new CourseDocumentRepository(context);
        public ILessonDocumentRepository LessonDocumentRepository => _lessonDocumentRepository ??= new LessonDocumentRepository(context);
    }
}

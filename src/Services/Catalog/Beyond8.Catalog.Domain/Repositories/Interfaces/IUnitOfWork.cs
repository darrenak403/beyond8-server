using System;
using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Catalog.Domain.Repositories.Interfaces;

public interface IUnitOfWork : IBaseUnitOfWork
{
    ICategoryRepository CategoryRepository { get; }
    ICourseRepository CourseRepository { get; }
    ISectionRepository SectionRepository { get; }
    ILessonRepository LessonRepository { get; }
    ICourseDocumentRepository CourseDocumentRepository { get; }
    ILessonDocumentRepository LessonDocumentRepository { get; }
}

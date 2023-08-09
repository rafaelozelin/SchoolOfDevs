using Microsoft.EntityFrameworkCore;
using SchoolOfDevs.Entities;
using SchoolOfDevs.Exceptions;
using SchoolOfDevs.Helpers;

namespace SchoolOfDevs.Services
{
    public interface ICourseService
    {
        public Task<List<Course>> GetAll();
        public Task<Course> GetById(int id);
        public Task<Course> Create(Course course);
        public Task Update(Course courseIn, int id);
        public Task Delete(int id);
    }

    public class CourseService : ICourseService
    {
        private readonly DataContext _context;

        public CourseService(DataContext context)
        {
            _context = context;
        }
        
        public async Task Delete(int id)
        {
            Course courseDb = await _context.Courses
                 .SingleOrDefaultAsync(c => c.Id == id);

            if (courseDb is null)
                throw new KeyNotFoundException($"Course {id} not found.");

            _context.Courses.Remove(courseDb);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Course>> GetAll()
        {
            return await _context.Courses.ToListAsync();
        }

        public async Task<Course> GetById(int id)
        {
            Course courseDb = await _context.Courses
                .SingleOrDefaultAsync(c => c.Id == id);

            if (courseDb is null)
                throw new KeyNotFoundException($"Course {id} not found.");

            return courseDb;
        }

        public async Task<Course> Create(Course course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return course;
        }

        public async Task Update(Course courseIn, int id)
        {
            if (courseIn.Id != id)
                throw new BadRequestException("Route id differs User id");

            Course courseDb = await _context.Courses
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Id == id);

            if (courseDb is null)
                throw new KeyNotFoundException($"Course {id} not found.");

            courseIn.CreatedAt = courseDb.CreatedAt;

            _context.Entry(courseIn).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}

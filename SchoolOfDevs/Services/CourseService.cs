﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SchoolOfDevs.Dto.Course;
using SchoolOfDevs.Entities;
using SchoolOfDevs.Exceptions;
using SchoolOfDevs.Helpers;

namespace SchoolOfDevs.Services
{
    public interface ICourseService
    {
        public Task<List<CourseResponse>> GetAll();
        public Task<CourseResponse> GetById(int id);
        public Task<CourseResponse> Create(CourseRequest course);
        public Task Update(CourseRequest courseIn, int id);
        public Task Delete(int id);
    }

    public class CourseService : ICourseService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public CourseService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

        public async Task<List<CourseResponse>> GetAll()
        {
            var courses = await _context.Courses.ToListAsync();

            return courses.Select(c => _mapper.Map<CourseResponse>(c)).ToList(); 
        }

        public async Task<CourseResponse> GetById(int id)
        {
            Course courseDb = await _context.Courses
                .Include(c => c.Teacher)
                .SingleOrDefaultAsync(c => c.Id == id);

            if (courseDb is null)
                throw new KeyNotFoundException($"Course {id} not found.");

            return _mapper.Map<CourseResponse>(courseDb);
        }

        public async Task<CourseResponse> Create(CourseRequest courseRequest)
        {
            var course = _mapper.Map<Course>(courseRequest);
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return _mapper.Map<CourseResponse>(course);
        }

        public async Task Update(CourseRequest courseRequest, int id)
        {
            if (courseRequest.Id != id)
                throw new BadRequestException("Route id differs User id");

            Course courseDb = await _context.Courses
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Id == id);

            if (courseDb is null)
                throw new KeyNotFoundException($"Course {id} not found.");

            courseDb = _mapper.Map<Course>(courseRequest);

            _context.Entry(courseRequest).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SchoolOfDevs.Dto.User;
using SchoolOfDevs.Entities;
using SchoolOfDevs.Enums;
using SchoolOfDevs.Exceptions;
using SchoolOfDevs.Helpers;
using BC = BCrypt.Net.BCrypt;

namespace SchoolOfDevs.Services
{
    public interface IUserService
    {
        public Task<AuthenticateResponse> Authenticate(AuthenticateRequest request);
        public Task<List<UserResponse>> GetAll();
        public Task<UserResponse> GetById(int id);
        public Task<UserResponse> Create(UserRequest user);
        public Task Update(UserRequestUpdate userIn, int id);
        public Task Delete(int id);
    }

    public class UserService : IUserService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;

        public UserService(DataContext context, IMapper mapper, IJwtService jwtService)
        {
            _context = context;
            _mapper = mapper;
            _jwtService = jwtService;
        }
        
        public async Task Delete(int id)
        {
            User userDb = await _context.Users
                 .SingleOrDefaultAsync(u => u.Id == id);

            if (userDb is null)
                throw new KeyNotFoundException($"User {id} not found.");

            _context.Users.Remove(userDb);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserResponse>> GetAll()
        {
            List<User> users = await _context.Users.ToListAsync();

            return users.Select(e => _mapper.Map<UserResponse>(e)).ToList();
        }

        public async Task<UserResponse> GetById(int id)
        {
            User userDb = await _context.Users
                .Include(e => e.CoursesStuding)
                .Include(e => e.CoursesTeaching)
                .SingleOrDefaultAsync(u => u.Id == id);

            if (userDb is null)
                throw new KeyNotFoundException($"User {id} not found.");

            return _mapper.Map<UserResponse>(userDb);
        }

        public async Task<UserResponse> Create(UserRequest userRequest)
        {
            if (!userRequest.Password.Equals(userRequest.ConfirmPassword))
                throw new BadRequestException("Password does not match ConfirmPassword");

            User userDb = await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserName == userRequest.UserName);

            if (userDb is not null)
                throw new BadRequestException($"UserName {userRequest.UserName} already exist.");

            User user = _mapper.Map<User>(userRequest);

            if(user.TypeUser != TypeUser.Teacher && userRequest.CoursesStudentIds.Any())
            {
                user.CoursesStuding = new List<Course>();
                List<Course> courses = await _context.Courses
                    .Where(e => userRequest.CoursesStudentIds.Contains(e.Id))
                    .ToListAsync();

                foreach (Course course in courses)
                    user.CoursesStuding.Add(course);
            }

            user.Password = BC.HashPassword(user.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserResponse>(user);
        }

        public async Task Update(UserRequestUpdate userRequest, int id)
        {
            if (userRequest.Id != id)
                throw new BadRequestException("Route id differs User id");
            else if(!userRequest.Password.Equals(userRequest.ConfirmPassword))
                throw new BadRequestException("Password does not match ConfirmPassword");

            User userDb = await _context.Users
                .Include(e => e.CoursesStuding)
                .SingleOrDefaultAsync(u => u.Id == id);

            if (userDb is null)
                throw new KeyNotFoundException($"User {id} not found.");
            else if (!BC.Verify(userRequest.CurrentPassword, userDb.Password))
                throw new BadRequestException("Incorrect Password");

            await AddOrRemoveCourse(userDb, userRequest.CoursesStudentIds);

            userDb = _mapper.Map<User>(userRequest);
            userDb.Password = BC.HashPassword(userRequest.Password);

            _context.Entry(userDb).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        private async Task AddOrRemoveCourse(User userDb, int[] coursesIds)
        {
            int[] removeIds = userDb.CoursesStuding
                .Where(e => !coursesIds.Contains(e.Id))
                .Select(e => e.Id)
                .ToArray();

            int[] addedIds = coursesIds
                .Where(e => !userDb.CoursesStuding.Select(u => u.Id).ToArray().Contains(e))
                .ToArray();

            if(!removeIds.Any() && !addedIds.Any())
            {
                _context.Entry(userDb).State = EntityState.Detached;
                return;
            }

            List<Course> tempCourse = await _context.Courses
                .Where(e => removeIds.Contains(e.Id) || addedIds.Contains(e.Id))
                .ToListAsync();

            List<Course> coursesToBeRemoved = tempCourse.Where(e => removeIds.Contains(e.Id)).ToList();
            foreach (Course course in coursesToBeRemoved)
                userDb.CoursesStuding.Remove(course);

            List<Course> coursesToBeAdded = tempCourse.Where(e => addedIds.Contains(e.Id)).ToList();
            foreach (Course course in coursesToBeAdded)
                userDb.CoursesStuding.Add(course);

            await _context.SaveChangesAsync();
            _context.Entry(userDb).State = EntityState.Detached;
        }

        public async Task<AuthenticateResponse> Authenticate(AuthenticateRequest request)
        {
            User userDb = await _context.Users
                .SingleOrDefaultAsync(u => u.UserName == request.UserName);

            if (userDb is null)
                throw new KeyNotFoundException($"User {request.UserName} not found.");
            else if (!BC.Verify(request.Password, userDb.Password))
                throw new BadRequestException("Incorrect Password");

            string token = _jwtService.GenerateJwtToken(userDb);
         
            return new AuthenticateResponse(userDb, token);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using SchoolOfDevs.Authorization;
using SchoolOfDevs.Dto.Course;
using SchoolOfDevs.Enums;
using SchoolOfDevs.Services;

namespace SchoolOfDevs.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _service;

        public CourseController(ICourseService service)
        {
            _service = service;
        }

        [Authorize(TypeUser.Teacher, TypeUser.Both)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CourseRequest courseRequest)
        {
            return Ok(await _service.Create(courseRequest));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _service.GetById(id));
        }

        [Authorize(TypeUser.Teacher, TypeUser.Both)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] CourseRequest courseRequest, int id)
        {
            await _service.Update(courseRequest, id);

            return NoContent();
        }

        [Authorize(TypeUser.Teacher, TypeUser.Both)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.Delete(id);

            return NoContent();
        }
    }
}
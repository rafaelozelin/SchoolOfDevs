using Microsoft.AspNetCore.Mvc;
using SchoolOfDevs.Dto.Note;
using SchoolOfDevs.Services;

namespace SchoolOfDevs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NoteController : ControllerBase
    {
        private readonly INoteService _service;

        public NoteController(INoteService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NoteRequest noteRequest)
        {
            return Ok(await _service.Create(noteRequest));
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] NoteRequest noteRequest, int id)
        {
            await _service.Update(noteRequest, id);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.Delete(id);

            return NoContent();
        }
    }
}
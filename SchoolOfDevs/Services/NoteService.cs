using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SchoolOfDevs.Dto.Note;
using SchoolOfDevs.Entities;
using SchoolOfDevs.Exceptions;
using SchoolOfDevs.Helpers;

namespace SchoolOfDevs.Services
{
    public interface INoteService
    {
        public Task<List<NoteResponse>> GetAll();
        public Task<NoteResponse> GetById(int id);
        public Task<NoteResponse> Create(NoteRequest note);
        public Task Update(NoteRequest noteIn, int id);
        public Task Delete(int id);
    }

    public class NoteService : INoteService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public NoteService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        
        public async Task Delete(int id)
        {
            Note noteDb = await _context.Notes
                 .SingleOrDefaultAsync(u => u.Id == id);

            if (noteDb is null)
                throw new KeyNotFoundException($"Note {id} not found.");

            _context.Notes.Remove(noteDb);
            await _context.SaveChangesAsync();
        }

        public async Task<List<NoteResponse>> GetAll()
        {
            var notes = await _context.Notes.ToListAsync();
            return notes.Select(e => _mapper.Map<NoteResponse>(e)).ToList();
        }

        public async Task<NoteResponse> GetById(int id)
        {
            Note noteDb = await _context.Notes
                .SingleOrDefaultAsync(u => u.Id == id);

            if (noteDb is null)
                throw new KeyNotFoundException($"Note {id} not found.");

            return _mapper.Map<NoteResponse>(noteDb);
        }

        public async Task<NoteResponse> Create(NoteRequest noteRequest)
        {
            Note note = _mapper.Map<Note>(noteRequest);

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            return _mapper.Map<NoteResponse>(note);
        }

        public async Task Update(NoteRequest noteRequest, int id)
        {
            if (noteRequest.Id != id)
                throw new BadRequestException("Route id differs Note id");

            Note noteDb = await _context.Notes
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Id == id);

            if (noteDb is null)
                throw new KeyNotFoundException($"Note {id} not found.");

            noteDb = _mapper.Map<Note>(noteRequest);

            _context.Entry(noteRequest).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}

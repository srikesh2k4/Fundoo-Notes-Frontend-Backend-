using BusinessLayer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.DTOs.Notes;
using ModelLayer.Responses;

namespace FundooNotes.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly INoteService _noteService;
        private readonly ILogger<NotesController> _logger;

        public NotesController(INoteService noteService, ILogger<NotesController> logger)
        {
            _noteService = noteService;
            _logger = logger;
        }
        /// Get all notes for logged-in user
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NoteResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            _logger.LogInformation("Fetching all notes for user {UserId}", userId);

            var notes = await _noteService.GetAllAsync(userId);

            return Ok(ApiResponse<IEnumerable<NoteResponseDto>>.SuccessResponse(
                notes, $"Retrieved {notes.Count()} notes"));
        }


        /// Get note by ID

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<NoteResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            _logger.LogInformation("Fetching note {NoteId} for user {UserId}", id, userId);

            var note = await _noteService.GetByIdAsync(id, userId);

            if (note == null)
                return NotFound(new ErrorResponse("Note not found", "NOT_FOUND"));

            return Ok(ApiResponse<NoteResponseDto>.SuccessResponse(note, "Note retrieved successfully"));
        }


        /// Create a new note

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<NoteResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateNoteDto dto)
        {
            var userId = GetUserId();
            _logger.LogInformation("Creating note for user {UserId}", userId);

            var note = await _noteService.CreateAsync(dto, userId);

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<NoteResponseDto>.SuccessResponse(note, "Note created successfully"));
        }

   
        /// Update a note
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<NoteResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateNoteDto dto)
        {
            var userId = GetUserId();
            _logger.LogInformation("Updating note {NoteId} for user {UserId}", id, userId);

            var note = await _noteService.UpdateAsync(id, dto, userId);

            return Ok(ApiResponse<NoteResponseDto>.SuccessResponse(note, "Note updated successfully"));
        }

        /// Delete a note (soft delete)
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            _logger.LogInformation("Deleting note {NoteId} for user {UserId}", id, userId);

            await _noteService.DeleteAsync(id, userId);

            return Ok(ApiResponse.SuccessResponse("Note deleted successfully"));
        }


        /// Search notes

        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NoteResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var userId = GetUserId();
            _logger.LogInformation("Searching notes for user {UserId} with query: {Query}", userId, query);

            var dto = new SearchNotesDto { Query = query };
            var notes = await _noteService.SearchAsync(dto, userId);

            return Ok(ApiResponse<IEnumerable<NoteResponseDto>>.SuccessResponse(
                notes, $"Found {notes.Count()} notes"));
        }


        /// Toggle pin status

        [HttpPatch("{id}/pin")]
        [ProducesResponseType(typeof(ApiResponse<NoteResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> TogglePin(int id)
        {
            var userId = GetUserId();
            _logger.LogInformation("Toggling pin for note {NoteId}", id);

            var note = await _noteService.TogglePinAsync(id, userId);
            return Ok(ApiResponse<NoteResponseDto>.SuccessResponse(note, "Pin status updated"));
        }


        /// Toggle archive status

        [HttpPatch("{id}/archive")]
        [ProducesResponseType(typeof(ApiResponse<NoteResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ToggleArchive(int id)
        {
            var userId = GetUserId();
            _logger.LogInformation("Toggling archive for note {NoteId}", id);

            var note = await _noteService.ToggleArchiveAsync(id, userId);
            return Ok(ApiResponse<NoteResponseDto>.SuccessResponse(note, "Archive status updated"));
        }


        /// Update note color

        [HttpPatch("{id}/color")]
        [ProducesResponseType(typeof(ApiResponse<NoteResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateColor(int id, [FromBody] UpdateNoteColorDto dto)
        {
            var userId = GetUserId();
            _logger.LogInformation("Updating color for note {NoteId}", id);

            var note = await _noteService.UpdateColorAsync(id, dto, userId);
            return Ok(ApiResponse<NoteResponseDto>.SuccessResponse(note, "Color updated successfully"));
        }


        /// Toggle trash status
        [HttpPatch("{id}/trash")]
        [ProducesResponseType(typeof(ApiResponse<NoteResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ToggleTrash(int id)
        {
            var userId = GetUserId();
            _logger.LogInformation("Toggling trash for note {NoteId}", id);

            var note = await _noteService.ToggleTrashAsync(id, userId);
            return Ok(ApiResponse<NoteResponseDto>.SuccessResponse(note, "Note moved to/from trash"));
        }


        /// Get all trashed notes
        [HttpGet("trash")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NoteResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTrashed()
        {
            var userId = GetUserId();
            _logger.LogInformation("Getting trashed notes for user {UserId}", userId);

            var notes = await _noteService.GetTrashedAsync(userId);
            return Ok(ApiResponse<IEnumerable<NoteResponseDto>>.SuccessResponse(notes, "Trashed notes retrieved"));
        }


        /// Restore note from trash
        [HttpPost("{id}/restore")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Restore(int id)
        {
            var userId = GetUserId();
            _logger.LogInformation("Restoring note {NoteId} from trash", id);

            await _noteService.RestoreFromTrashAsync(id, userId);
            return Ok(ApiResponse.SuccessResponse("Note restored successfully"));
        }


        /// Permanently delete a note
        [HttpDelete("{id}/permanent")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeletePermanently(int id)
        {
            var userId = GetUserId();
            _logger.LogInformation("Permanently deleting note {NoteId}", id);

            await _noteService.DeletePermanentlyAsync(id, userId);
            return Ok(ApiResponse.SuccessResponse("Note permanently deleted"));
        }


        /// Empty trash
        [HttpDelete("trash/empty")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> EmptyTrash()
        {
            var userId = GetUserId();
            _logger.LogInformation("Emptying trash for user {UserId}", userId);

            await _noteService.EmptyTrashAsync(userId);
            return Ok(ApiResponse.SuccessResponse("Trash emptied successfully"));
        }


        /// Bulk delete notes
        [HttpPost("bulk-delete")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteDto dto)
        {
            var userId = GetUserId();
            _logger.LogInformation("Bulk deleting notes for user {UserId}", userId);

            await _noteService.BulkDeleteAsync(dto, userId);
            return Ok(ApiResponse.SuccessResponse("Notes deleted successfully"));
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}

using BusinessLayer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.DTOs.Collaborators;
using ModelLayer.Responses;

namespace FundooNotes.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CollaboratorsController : ControllerBase
    {
        private readonly ICollaboratorService _collaboratorService;
        private readonly ILogger<CollaboratorsController> _logger;

        public CollaboratorsController(
            ICollaboratorService collaboratorService,
            ILogger<CollaboratorsController> logger)
        {
            _collaboratorService = collaboratorService;
            _logger = logger;
        }


        /// Get all collaborators for a note

        [HttpGet("note/{noteId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CollaboratorResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByNoteId(int noteId)
        {
            var userId = GetUserId();
            _logger.LogInformation("Fetching collaborators for note {NoteId}", noteId);

            var collaborators = await _collaboratorService.GetByNoteIdAsync(noteId, userId);

            return Ok(ApiResponse<IEnumerable<CollaboratorResponseDto>>.SuccessResponse(
                collaborators, $"Retrieved {collaborators.Count()} collaborators"));
        }


        /// Add a collaborator to a note

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CollaboratorResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Add([FromBody] AddCollaboratorDto dto)
        {
            var userId = GetUserId();
            _logger.LogInformation("Adding collaborator to note {NoteId}", dto.NoteId);

            var collaborator = await _collaboratorService.AddAsync(dto, userId);

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<CollaboratorResponseDto>.SuccessResponse(
                    collaborator, "Collaborator added successfully"));
        }


        /// Update collaborator permission

        [HttpPut("{id}/permission")]
        [ProducesResponseType(typeof(ApiResponse<CollaboratorResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePermission(int id, [FromBody] UpdatePermissionDto dto)
        {
            var userId = GetUserId();
            _logger.LogInformation("Updating permission for collaborator {CollaboratorId}", id);

            var collaborator = await _collaboratorService.UpdatePermissionAsync(id, dto, userId);

            return Ok(ApiResponse<CollaboratorResponseDto>.SuccessResponse(
                collaborator, "Permission updated successfully"));
        }


        /// Remove a collaborator
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remove(int id)
        {
            var userId = GetUserId();
            _logger.LogInformation("Removing collaborator {CollaboratorId}", id);

            await _collaboratorService.RemoveAsync(id, userId);

            return Ok(ApiResponse.SuccessResponse("Collaborator removed successfully"));
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}
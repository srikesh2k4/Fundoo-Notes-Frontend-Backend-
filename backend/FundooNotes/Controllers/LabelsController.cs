// ========================================
// FILE: FundooNotes/Controllers/LabelsController.cs (FIXED)
// ========================================
using BusinessLayer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.DTOs.Labels;
using ModelLayer.Responses;

namespace FundooNotes.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LabelsController : ControllerBase
    {
        private readonly ILabelService _labelService;
        private readonly ILogger<LabelsController> _logger;

        public LabelsController(ILabelService labelService, ILogger<LabelsController> logger)
        {
            _labelService = labelService;
            _logger = logger;
        }

        /// <summary>
        /// Get all labels for logged-in user
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LabelResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            _logger.LogInformation("Fetching all labels for user {UserId}", userId);

            var labels = await _labelService.GetByUserAsync(userId);

            return Ok(ApiResponse<IEnumerable<LabelResponseDto>>.SuccessResponse(
                labels, $"Retrieved {labels.Count()} labels"));
        }

        /// <summary>
        /// Get label by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<LabelResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            _logger.LogInformation("Fetching label {LabelId} for user {UserId}", id, userId);

            var label = await _labelService.GetByIdAsync(id, userId);

            if (label == null)
                return NotFound(new ErrorResponse("Label not found", "NOT_FOUND"));

            return Ok(ApiResponse<LabelResponseDto>.SuccessResponse(label, "Label retrieved successfully"));
        }

        /// <summary>
        /// Create a new label
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LabelResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateLabelDto dto)
        {
            var userId = GetUserId();
            _logger.LogInformation("Creating label for user {UserId}", userId);

            var label = await _labelService.CreateAsync(dto, userId);

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<LabelResponseDto>.SuccessResponse(label, "Label created successfully"));
        }

        /// <summary>
        /// Update a label
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<LabelResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLabelDto dto)
        {
            var userId = GetUserId();
            _logger.LogInformation("Updating label {LabelId} for user {UserId}", id, userId);

            var label = await _labelService.UpdateAsync(id, dto, userId);

            return Ok(ApiResponse<LabelResponseDto>.SuccessResponse(label, "Label updated successfully"));
        }

        /// <summary>
        /// Delete a label
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            _logger.LogInformation("Deleting label {LabelId} for user {UserId}", id, userId);

            await _labelService.DeleteAsync(id, userId);

            return Ok(ApiResponse.SuccessResponse("Label deleted successfully"));
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}
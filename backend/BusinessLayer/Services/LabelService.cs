using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces.Services;
using BusinessLayer.Rules;
using DataBaseLayer.Entities;
using DataBaseLayer.Interfaces;
using ModelLayer.DTOs.Labels;

namespace BusinessLayer.Services
{
    public class LabelService : ILabelService
    {
        private readonly ILabelRepository _labelRepository;

        public LabelService(ILabelRepository labelRepository)
        {
            _labelRepository = labelRepository;
        }

        public async Task<IEnumerable<LabelResponseDto>> GetByUserAsync(int userId)
        {
            // ✅ Add validation
            if (userId <= 0)
                throw new ValidationException("Invalid user ID");

            var labels = await _labelRepository.GetByUserAsync(userId);

            return labels
                .Select(l => new LabelResponseDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    CreatedAt = l.CreatedAt,
                })
                .ToList(); // ✅ Materialize the query
        }

        public async Task<LabelResponseDto?> GetByIdAsync(int labelId, int userId)
        {
            // ✅ Add validation
            if (labelId <= 0)
                throw new ValidationException("Invalid label ID");

            if (userId <= 0)
                throw new ValidationException("Invalid user ID");

            var label = await _labelRepository.GetByIdAsync(labelId);

            if (label == null)
                return null;

            if (label.UserId != userId)
                throw new UnauthorizedException("Access denied to this label");

            return new LabelResponseDto
            {
                Id = label.Id,
                Name = label.Name,
                CreatedAt = label.CreatedAt, // ✅ FIXED: Changed from l.CreatedAt to label.CreatedAt
            };
        }

        public async Task<LabelResponseDto> CreateAsync(CreateLabelDto dto, int userId)
        {
            // ✅ Add null check
            if (dto == null)
                throw new ValidationException("Label data is required");

            // ✅ Add user validation
            if (userId <= 0)
                throw new ValidationException("Invalid user ID");

            // Validate and trim name
            var trimmedName = dto.Name?.Trim() ?? string.Empty;
            LabelRules.ValidateName(trimmedName);

            // Check for duplicate label name for this user
            if (await _labelRepository.ExistsForUserAsync(trimmedName, userId))
                throw new ValidationException("A label with this name already exists");

            var label = new Label
            {
                Name = trimmedName,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
            };

            await _labelRepository.AddAsync(label);
            await _labelRepository.SaveAsync();

            return new LabelResponseDto
            {
                Id = label.Id,
                Name = label.Name,
                CreatedAt = label.CreatedAt,
            };
        }

        public async Task<LabelResponseDto> UpdateAsync(int labelId, UpdateLabelDto dto, int userId)
        {
            // ✅ Add null check
            if (dto == null)
                throw new ValidationException("Label data is required");

            // ✅ Add validation
            if (labelId <= 0)
                throw new ValidationException("Invalid label ID");

            if (userId <= 0)
                throw new ValidationException("Invalid user ID");

            // Validate and trim name
            var trimmedName = dto.Name?.Trim() ?? string.Empty;
            LabelRules.ValidateName(trimmedName);

            var label =
                await _labelRepository.GetByIdAsync(labelId)
                ?? throw new NotFoundException("Label not found");

            if (label.UserId != userId)
                throw new UnauthorizedException("Access denied to this label");

            // ✅ Check if name actually changed
            if (label.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase))
            {
                // Name unchanged, return existing label
                return new LabelResponseDto
                {
                    Id = label.Id,
                    Name = label.Name,
                    CreatedAt = label.CreatedAt,
                };
            }

            // Check for duplicate name (excluding current label)
            if (await _labelRepository.ExistsForUserAsync(trimmedName, userId, labelId))
                throw new ValidationException("A label with this name already exists");

            label.Name = trimmedName;

            await _labelRepository.SaveAsync();

            return new LabelResponseDto
            {
                Id = label.Id,
                Name = label.Name,
                CreatedAt = label.CreatedAt,
            };
        }

        public async Task DeleteAsync(int labelId, int userId)
        {
            // ✅ Add validation
            if (labelId <= 0)
                throw new ValidationException("Invalid label ID");

            if (userId <= 0)
                throw new ValidationException("Invalid user ID");

            var label =
                await _labelRepository.GetByIdAsync(labelId)
                ?? throw new NotFoundException("Label not found");

            if (label.UserId != userId)
                throw new UnauthorizedException("Access denied to this label");

            await _labelRepository.DeleteAsync(label);
            await _labelRepository.SaveAsync();
        }
    }
}

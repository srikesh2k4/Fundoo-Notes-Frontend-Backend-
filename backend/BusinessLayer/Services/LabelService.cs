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
            var labels = await _labelRepository.GetByUserAsync(userId);
            return labels.Select(l => new LabelResponseDto
            {
                Id = l.Id,
                Name = l.Name,
                CreatedAt = l.CreatedAt
            });
        }

        public async Task<LabelResponseDto?> GetByIdAsync(int labelId, int userId)
        {
            var label = await _labelRepository.GetByIdAsync(labelId);

            if (label == null)
                return null;

            if (label.UserId != userId)
                throw new UnauthorizedException("Access denied to this label");

            return new LabelResponseDto
            {
                Id = label.Id,
                Name = label.Name,
                CreatedAt = label.CreatedAt
            };
        }

        public async Task<LabelResponseDto> CreateAsync(CreateLabelDto dto, int userId)
        {
            LabelRules.ValidateName(dto.Name);

            // Check for duplicate label name for this user
            if (await _labelRepository.ExistsForUserAsync(dto.Name, userId))
                throw new ValidationException("A label with this name already exists");

            var label = new Label
            {
                Name = dto.Name.Trim(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _labelRepository.AddAsync(label);
            await _labelRepository.SaveAsync();

            return new LabelResponseDto
            {
                Id = label.Id,
                Name = label.Name,
                CreatedAt = label.CreatedAt
            };
        }

        public async Task<LabelResponseDto> UpdateAsync(int labelId, UpdateLabelDto dto, int userId)
        {
            LabelRules.ValidateName(dto.Name);

            var label = await _labelRepository.GetByIdAsync(labelId)
                ?? throw new NotFoundException("Label not found");

            if (label.UserId != userId)
                throw new UnauthorizedException("Access denied to this label");

            // Check for duplicate name (excluding current label)
            if (await _labelRepository.ExistsForUserAsync(dto.Name, userId, labelId))
                throw new ValidationException("A label with this name already exists");

            label.Name = dto.Name.Trim();

            await _labelRepository.SaveAsync();

            return new LabelResponseDto
            {
                Id = label.Id,
                Name = label.Name,
                CreatedAt = label.CreatedAt
            };
        }

        public async Task DeleteAsync(int labelId, int userId)
        {
            var label = await _labelRepository.GetByIdAsync(labelId)
                ?? throw new NotFoundException("Label not found");

            if (label.UserId != userId)
                throw new UnauthorizedException("Access denied to this label");

            await _labelRepository.DeleteAsync(label);
            await _labelRepository.SaveAsync();
        }
    }
}
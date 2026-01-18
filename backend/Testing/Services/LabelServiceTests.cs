// ========================================
// FILE: Testing/Services/LabelServiceTests.cs
// ========================================
using BusinessLayer.Exceptions;
using BusinessLayer.Services;
using DataBaseLayer.Entities;
using DataBaseLayer.Interfaces;
using FluentAssertions;
using ModelLayer.DTOs.Labels;
using Moq;
using NUnit.Framework;

namespace Testing.Services
{
    [TestFixture]
    public class LabelServiceTests
    {
        private Mock<ILabelRepository> _labelRepositoryMock = null!;
        private LabelService _labelService = null!;

        [SetUp]
        public void Setup()
        {
            _labelRepositoryMock = new Mock<ILabelRepository>();
            _labelService = new LabelService(_labelRepositoryMock.Object);
        }

        [Test]
        public async Task CreateAsync_ValidLabel_ReturnsLabelResponse()
        {
            var dto = new CreateLabelDto { Name = "Work" };
            var userId = 1;

            _labelRepositoryMock.Setup(x => x.ExistsForUserAsync(dto.Name, userId, null))
                .ReturnsAsync(false);

            _labelRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Label>()))
                .Returns(Task.CompletedTask);

            _labelRepositoryMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await _labelService.CreateAsync(dto, userId);

            result.Should().NotBeNull();
            result.Name.Should().Be(dto.Name);
            _labelRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Label>()), Times.Once);
        }

        [Test]
        public async Task CreateAsync_DuplicateName_ThrowsValidationException()
        {
            var dto = new CreateLabelDto { Name = "Work" };
            var userId = 1;

            _labelRepositoryMock.Setup(x => x.ExistsForUserAsync(dto.Name, userId, null))
                .ReturnsAsync(true);

            Func<Task> act = async () => await _labelService.CreateAsync(dto, userId);
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("A label with this name already exists");
        }

        [Test]
        public async Task CreateAsync_EmptyName_ThrowsValidationException()
        {
            var dto = new CreateLabelDto { Name = "" };
            var userId = 1;

            Func<Task> act = async () => await _labelService.CreateAsync(dto, userId);
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Label name is required");
        }

        [Test]
        public async Task GetByUserAsync_UserHasLabels_ReturnsAllLabels()
        {
            var userId = 1;
            var labels = new List<Label>
            {
                new Label { Id = 1, Name = "Work", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Label { Id = 2, Name = "Personal", UserId = userId, CreatedAt = DateTime.UtcNow }
            };

            _labelRepositoryMock.Setup(x => x.GetByUserAsync(userId))
                .ReturnsAsync(labels);

            var result = await _labelService.GetByUserAsync(userId);

            result.Should().HaveCount(2);
            result.Should().Contain(l => l.Name == "Work");
            result.Should().Contain(l => l.Name == "Personal");
        }

        [Test]
        public async Task GetByIdAsync_ValidLabel_ReturnsLabel()
        {
            var labelId = 1;
            var userId = 1;
            var label = new Label
            {
                Id = labelId,
                Name = "Work",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _labelRepositoryMock.Setup(x => x.GetByIdAsync(labelId))
                .ReturnsAsync(label);

            var result = await _labelService.GetByIdAsync(labelId, userId);

            result.Should().NotBeNull();
            result!.Id.Should().Be(labelId);
            result.Name.Should().Be(label.Name);
        }

        [Test]
        public async Task UpdateAsync_ValidUpdate_ReturnsUpdatedLabel()
        {
            var labelId = 1;
            var userId = 1;
            var label = new Label
            {
                Id = labelId,
                Name = "Old Name",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            var dto = new UpdateLabelDto { Name = "New Name" };

            _labelRepositoryMock.Setup(x => x.GetByIdAsync(labelId))
                .ReturnsAsync(label);

            _labelRepositoryMock.Setup(x => x.ExistsForUserAsync(dto.Name, userId, labelId))
                .ReturnsAsync(false);

            _labelRepositoryMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await _labelService.UpdateAsync(labelId, dto, userId);

            result.Should().NotBeNull();
            result.Name.Should().Be(dto.Name);
            label.Name.Should().Be(dto.Name);
        }

        [Test]
        public async Task DeleteAsync_ValidLabel_DeletesLabel()
        {
            var labelId = 1;
            var userId = 1;
            var label = new Label
            {
                Id = labelId,
                Name = "Work",
                UserId = userId
            };

            _labelRepositoryMock.Setup(x => x.GetByIdAsync(labelId))
                .ReturnsAsync(label);

            _labelRepositoryMock.Setup(x => x.DeleteAsync(label))
                .Returns(Task.CompletedTask);

            _labelRepositoryMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            await _labelService.DeleteAsync(labelId, userId);

            _labelRepositoryMock.Verify(x => x.DeleteAsync(label), Times.Once);
            _labelRepositoryMock.Verify(x => x.SaveAsync(), Times.Once);
        }
    }
}

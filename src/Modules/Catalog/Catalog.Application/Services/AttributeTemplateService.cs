using AutoMapper;
using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Catalog.Application.Services.Interface;
using Catalog.Domain.Entities;
using FluentValidation;
using Shared.Application.Interfaces;
using Shared.Application.Models;
using System.Text.Json;

namespace Catalog.Application.Services
{
    public class AttributeTemplateService : IAttributeTemplateService
    {
        private readonly IAttributeTemplateRepository _templateRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateAttributeTemplateDto> _createValidator;
        private readonly IValidator<UpdateAttributeTemplateDto> _updateValidator;

        public AttributeTemplateService(
            IAttributeTemplateRepository templateRepository,
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateAttributeTemplateDto> createValidator,
            IValidator<UpdateAttributeTemplateDto> updateValidator)
        {
            _templateRepository = templateRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<Result<IEnumerable<AttributeTemplateDto>>> GetAllAsync(
            CancellationToken ct = default)
        {
            var templates = await _templateRepository.GetAllAsync(ct);
            return Result<IEnumerable<AttributeTemplateDto>>.Success(
                _mapper.Map<IEnumerable<AttributeTemplateDto>>(templates));
        }

        public async Task<Result<AttributeTemplateDto>> GetByIdAsync(
            int id, CancellationToken ct = default)
        {
            var template = await _templateRepository.GetByIdWithItemsAsync(id, ct);
            if (template is null || template.IsDeleted)
                return Result<AttributeTemplateDto>.Failure(
                    $"Attribute template {id} not found.", "TEMPLATE_NOT_FOUND");

            return Result<AttributeTemplateDto>.Success(
                _mapper.Map<AttributeTemplateDto>(template));
        }

        public async Task<Result<AttributeTemplateDto>> GetByCategoryIdAsync(
            int categoryId, CancellationToken ct = default)
        {
            var template = await _templateRepository.GetByCategoryIdAsync(categoryId, ct);
            if (template is null)
                return Result<AttributeTemplateDto>.Failure(
                    $"No attribute template found for category {categoryId}.",
                    "TEMPLATE_NOT_FOUND");

            return Result<AttributeTemplateDto>.Success(
                _mapper.Map<AttributeTemplateDto>(template));
        }

        public async Task<Result<AttributeTemplateDto>> CreateAsync(
            CreateAttributeTemplateDto dto, Guid adminId, CancellationToken ct = default)
        {
            // 1. Validate
            var validation = await _createValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<AttributeTemplateDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            // 2. Check category exists
            if (!await _categoryRepository.ExistsAsync(dto.CategoryId, ct))
                return Result<AttributeTemplateDto>.Failure(
                    $"Category {dto.CategoryId} not found.", "CATEGORY_NOT_FOUND");

            // 3. Check no existing template for this category
            if (await _templateRepository.ExistsForCategoryAsync(dto.CategoryId, ct))
                return Result<AttributeTemplateDto>.Failure(
                    $"An attribute template already exists for category {dto.CategoryId}.",
                    "TEMPLATE_EXISTS");

            // 4. Create via factory
            var template = AttributeTemplate.Create(
                categoryId: dto.CategoryId,
                name: dto.Name,
                createdBy: adminId);

            // 5. Add items
            foreach (var item in dto.Items.OrderBy(i => i.SortOrder))
            {
                // Serialize options list to JSON string for storage
                var optionsJson = item.Options.Count > 0
                    ? JsonSerializer.Serialize(item.Options)
                    : null;

                template.AddItem(
                    attributeName: item.AttributeName,
                    inputType: item.InputType,
                    isRequired: item.IsRequired,
                    sortOrder: item.SortOrder,
                    options: optionsJson);
            }

            await _templateRepository.AddAsync(template, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // Reload with items for mapping
            var created = await _templateRepository.GetByIdWithItemsAsync(template.Id, ct);
            return Result<AttributeTemplateDto>.Success(
                _mapper.Map<AttributeTemplateDto>(created!));
        }

        public async Task<Result<AttributeTemplateDto>> UpdateAsync(
            int id, UpdateAttributeTemplateDto dto, Guid adminId, CancellationToken ct = default)
        {
            // 1. Validate
            var validation = await _updateValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<AttributeTemplateDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            // 2. Find template with items
            var template = await _templateRepository.GetByIdWithItemsAsync(id, ct);
            if (template is null || template.IsDeleted)
                return Result<AttributeTemplateDto>.Failure(
                    $"Attribute template {id} not found.", "TEMPLATE_NOT_FOUND");

            // 3. Update name and active state via domain method
            template.Update(dto.Name, adminId);

            if (dto.IsActive && !template.IsActive)
                template.Activate(adminId);
            else if (!dto.IsActive && template.IsActive)
                template.Deactivate(adminId);

            // 4. Replace items — clear existing and re-add
            // Infrastructure handles this via EF Core owned collection
            template.ReplaceItems(
                dto.Items.Select(item => (
                    attributeName: item.AttributeName,
                    inputType: item.InputType,
                    isRequired: item.IsRequired,
                    sortOrder: item.SortOrder,
                    options: item.Options.Count > 0
                        ? JsonSerializer.Serialize(item.Options)
                        : null
                )).ToList(),
                adminId);

            _templateRepository.Update(template);
            await _unitOfWork.SaveChangesAsync(ct);

            var updated = await _templateRepository.GetByIdWithItemsAsync(id, ct);
            return Result<AttributeTemplateDto>.Success(
                _mapper.Map<AttributeTemplateDto>(updated!));
        }

        public async Task<Result> DeleteAsync(
            int id, Guid adminId, CancellationToken ct = default)
        {
            var template = await _templateRepository.GetByIdAsync(id, ct);
            if (template is null || template.IsDeleted)
                return Result.Failure(
                    $"Attribute template {id} not found.", "TEMPLATE_NOT_FOUND");

            template.SoftDelete(adminId);
            _templateRepository.Update(template);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
    }

}

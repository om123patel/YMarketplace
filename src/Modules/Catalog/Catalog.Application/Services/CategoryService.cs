using AutoMapper;
using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Catalog.Application.Services.Interface;
using Catalog.Domain.Entities;
using FluentValidation;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Catalog.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateCategoryDto> _createValidator;
        private readonly IValidator<UpdateCategoryDto> _updateValidator;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateCategoryDto> createValidator,
            IValidator<UpdateCategoryDto> updateValidator)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<Result<CategoryDto>> GetByIdAsync(
            int id, CancellationToken ct = default)
        {
            var category = await _categoryRepository.GetByIdWithChildrenAsync(id, ct);
            if (category is null || category.IsDeleted)
                return Result<CategoryDto>.Failure(
                    $"Category {id} not found.", "CATEGORY_NOT_FOUND");

            return Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category));
        }

        public async Task<Result<IEnumerable<CategoryDto>>> GetAllActiveAsync(
            CancellationToken ct = default)
        {
            var categories = await _categoryRepository.GetAllActiveAsync(ct);
            return Result<IEnumerable<CategoryDto>>.Success(
                _mapper.Map<IEnumerable<CategoryDto>>(categories));
        }

        public async Task<Result<IEnumerable<CategoryDto>>> GetRootCategoriesAsync(
            CancellationToken ct = default)
        {
            var categories = await _categoryRepository.GetRootCategoriesAsync(ct);
            return Result<IEnumerable<CategoryDto>>.Success(
                _mapper.Map<IEnumerable<CategoryDto>>(categories));
        }

        public async Task<Result<IEnumerable<CategoryDto>>> GetChildrenAsync(
            int parentId, CancellationToken ct = default)
        {
            if (!await _categoryRepository.ExistsAsync(parentId, ct))
                return Result<IEnumerable<CategoryDto>>.Failure(
                    $"Category {parentId} not found.", "CATEGORY_NOT_FOUND");

            var children = await _categoryRepository.GetChildrenAsync(parentId, ct);
            return Result<IEnumerable<CategoryDto>>.Success(
                _mapper.Map<IEnumerable<CategoryDto>>(children));
        }

        public async Task<Result<CategoryDto>> CreateAsync(
            CreateCategoryDto dto, Guid adminId, CancellationToken ct = default)
        {
            // 1. Validate
            var validation = await _createValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<CategoryDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            // 2. Check slug uniqueness
            if (await _categoryRepository.SlugExistsAsync(dto.Slug, ct))
                return Result<CategoryDto>.Failure(
                    $"Slug '{dto.Slug}' is already taken.", "SLUG_EXISTS");

            // 3. Validate parent exists if provided
            if (dto.ParentId.HasValue &&
                !await _categoryRepository.ExistsAsync(dto.ParentId.Value, ct))
                return Result<CategoryDto>.Failure(
                    $"Parent category {dto.ParentId} not found.", "PARENT_NOT_FOUND");

            // 4. Create via factory
            var category = Category.Create(
                name: dto.Name,
                slug: dto.Slug,
                createdBy: adminId,
                parentId: dto.ParentId,
                description: dto.Description,
                imageUrl: dto.ImageUrl,
                iconUrl: dto.IconUrl,
                sortOrder: dto.SortOrder);

            await _categoryRepository.AddAsync(category, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category));
        }

        public async Task<Result<CategoryDto>> UpdateAsync(
            int id, UpdateCategoryDto dto, Guid adminId, CancellationToken ct = default)
        {
            // 1. Validate
            var validation = await _updateValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<CategoryDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            // 2. Find category
            var category = await _categoryRepository.GetByIdAsync(id, ct);
            if (category is null || category.IsDeleted)
                return Result<CategoryDto>.Failure(
                    $"Category {id} not found.", "CATEGORY_NOT_FOUND");

            // 3. Check slug uniqueness — exclude current
            if (await _categoryRepository.SlugExistsExceptAsync(dto.Slug, id, ct))
                return Result<CategoryDto>.Failure(
                    $"Slug '{dto.Slug}' is already taken.", "SLUG_EXISTS");

            // 4. Validate parent — cannot set itself as parent
            if (dto.ParentId.HasValue)
            {
                if (dto.ParentId.Value == id)
                    return Result<CategoryDto>.Failure(
                        "A category cannot be its own parent.", "INVALID_PARENT");

                if (!await _categoryRepository.ExistsAsync(dto.ParentId.Value, ct))
                    return Result<CategoryDto>.Failure(
                        $"Parent category {dto.ParentId} not found.", "PARENT_NOT_FOUND");
            }

            // 5. Update via domain method
            category.Update(
                name: dto.Name,
                slug: dto.Slug,
                updatedBy: adminId,
                parentId: dto.ParentId,
                description: dto.Description,
                imageUrl: dto.ImageUrl,
                iconUrl: dto.IconUrl,
                sortOrder: dto.SortOrder);

            // 6. Handle IsActive change
            if (dto.IsActive && !category.IsActive)
                category.Activate(adminId);
            else if (!dto.IsActive && category.IsActive)
                category.Deactivate(adminId);

            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category));
        }

        public async Task<Result> DeleteAsync(
            int id, Guid adminId, CancellationToken ct = default)
        {
            var category = await _categoryRepository.GetByIdAsync(id, ct);
            if (category is null || category.IsDeleted)
                return Result.Failure($"Category {id} not found.", "CATEGORY_NOT_FOUND");

            // Cannot delete if it has children
            if (await _categoryRepository.HasChildrenAsync(id, ct))
                return Result.Failure(
                    "Cannot delete a category that has subcategories. Remove subcategories first.",
                    "HAS_CHILDREN");

            // Cannot delete if it has products
            if (await _categoryRepository.HasProductsAsync(id, ct))
                return Result.Failure(
                    "Cannot delete a category that has products assigned to it.",
                    "HAS_PRODUCTS");

            category.SoftDelete(adminId);
            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        public async Task<Result> ActivateAsync(
            int id, Guid adminId, CancellationToken ct = default)
        {
            var category = await _categoryRepository.GetByIdAsync(id, ct);
            if (category is null || category.IsDeleted)
                return Result.Failure($"Category {id} not found.", "CATEGORY_NOT_FOUND");

            category.Activate(adminId);
            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        public async Task<Result> DeactivateAsync(
            int id, Guid adminId, CancellationToken ct = default)
        {
            var category = await _categoryRepository.GetByIdAsync(id, ct);
            if (category is null || category.IsDeleted)
                return Result.Failure($"Category {id} not found.", "CATEGORY_NOT_FOUND");

            category.Deactivate(adminId);
            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}

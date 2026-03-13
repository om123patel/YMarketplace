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
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateBrandDto> _createValidator;
        private readonly IValidator<UpdateBrandDto> _updateValidator;

        public BrandService(
            IBrandRepository brandRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateBrandDto> createValidator,
            IValidator<UpdateBrandDto> updateValidator)
        {
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<Result<BrandDto>> GetByIdAsync(
            int id, CancellationToken ct = default)
        {
            var brand = await _brandRepository.GetByIdAsync(id, ct);
            if (brand is null || brand.IsDeleted)
                return Result<BrandDto>.Failure(
                    $"Brand {id} not found.", "BRAND_NOT_FOUND");

            return Result<BrandDto>.Success(_mapper.Map<BrandDto>(brand));
        }

        public async Task<Result<IEnumerable<BrandDto>>> GetAllActiveAsync(
            CancellationToken ct = default)
        {
            var brands = await _brandRepository.GetAllActiveAsync(ct);
            return Result<IEnumerable<BrandDto>>.Success(
                _mapper.Map<IEnumerable<BrandDto>>(brands));
        }

        public async Task<Result<BrandDto>> CreateAsync(
            CreateBrandDto dto, Guid adminId, CancellationToken ct = default)
        {
            // 1. Validate
            var validation = await _createValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<BrandDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            // 2. Check name uniqueness
            if (await _brandRepository.NameExistsAsync(dto.Name, ct))
                return Result<BrandDto>.Failure(
                    $"Brand name '{dto.Name}' already exists.", "NAME_EXISTS");

            // 3. Check slug uniqueness
            if (await _brandRepository.SlugExistsAsync(dto.Slug, ct))
                return Result<BrandDto>.Failure(
                    $"Slug '{dto.Slug}' is already taken.", "SLUG_EXISTS");

            // 4. Create via factory
            var brand = Brand.Create(
                name: dto.Name,
                slug: dto.Slug,
                createdBy: adminId,
                description: dto.Description,
                logoUrl: dto.LogoUrl,
                websiteUrl: dto.WebsiteUrl);

            await _brandRepository.AddAsync(brand, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<BrandDto>.Success(_mapper.Map<BrandDto>(brand));
        }

        public async Task<Result<BrandDto>> UpdateAsync(
            int id, UpdateBrandDto dto, Guid adminId, CancellationToken ct = default)
        {
            // 1. Validate
            var validation = await _updateValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<BrandDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            // 2. Find brand
            var brand = await _brandRepository.GetByIdAsync(id, ct);
            if (brand is null || brand.IsDeleted)
                return Result<BrandDto>.Failure(
                    $"Brand {id} not found.", "BRAND_NOT_FOUND");

            // 3. Check name uniqueness — exclude current
            if (await _brandRepository.NameExistsExceptAsync(dto.Name, id, ct))
                return Result<BrandDto>.Failure(
                    $"Brand name '{dto.Name}' already exists.", "NAME_EXISTS");

            // 4. Check slug uniqueness — exclude current
            if (await _brandRepository.SlugExistsExceptAsync(dto.Slug, id, ct))
                return Result<BrandDto>.Failure(
                    $"Slug '{dto.Slug}' is already taken.", "SLUG_EXISTS");

            // 5. Update via domain method
            brand.Update(
                name: dto.Name,
                slug: dto.Slug,
                updatedBy: adminId,
                description: dto.Description,
                logoUrl: dto.LogoUrl,
                websiteUrl: dto.WebsiteUrl);

            // 6. Handle IsActive change
            if (dto.IsActive && !brand.IsActive)
                brand.Activate(adminId);
            else if (!dto.IsActive && brand.IsActive)
                brand.Deactivate(adminId);

            _brandRepository.Update(brand);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<BrandDto>.Success(_mapper.Map<BrandDto>(brand));
        }

        public async Task<Result> DeleteAsync(
            int id, Guid adminId, CancellationToken ct = default)
        {
            var brand = await _brandRepository.GetByIdAsync(id, ct);
            if (brand is null || brand.IsDeleted)
                return Result.Failure($"Brand {id} not found.", "BRAND_NOT_FOUND");

            if (await _brandRepository.HasProductsAsync(id, ct))
                return Result.Failure(
                    "Cannot delete a brand that has products assigned to it.",
                    "HAS_PRODUCTS");

            brand.SoftDelete(adminId);
            _brandRepository.Update(brand);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        public async Task<Result> ActivateAsync(
            int id, Guid adminId, CancellationToken ct = default)
        {
            var brand = await _brandRepository.GetByIdAsync(id, ct);
            if (brand is null || brand.IsDeleted)
                return Result.Failure($"Brand {id} not found.", "BRAND_NOT_FOUND");

            brand.Activate(adminId);
            _brandRepository.Update(brand);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        public async Task<Result> DeactivateAsync(
            int id, Guid adminId, CancellationToken ct = default)
        {
            var brand = await _brandRepository.GetByIdAsync(id, ct);
            if (brand is null || brand.IsDeleted)
                return Result.Failure($"Brand {id} not found.", "BRAND_NOT_FOUND");

            brand.Deactivate(adminId);
            _brandRepository.Update(brand);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
    }

}

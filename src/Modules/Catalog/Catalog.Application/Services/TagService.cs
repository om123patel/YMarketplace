using AutoMapper;
using Catalog.Application.DTOs.Tags;
using Catalog.Application.Interfaces;
using Catalog.Application.Services.Interface;
using Catalog.Domain.Entities;
using FluentValidation;
using Shared.Application.Interfaces;
using Shared.Application.Models;

namespace Catalog.Application.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateTagDto> _createValidator;
        private readonly IValidator<UpdateTagDto> _updateValidator;

        public TagService(
            ITagRepository tagRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateTagDto> createValidator,
            IValidator<UpdateTagDto> updateValidator)
        {
            _tagRepository = tagRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<Result<IEnumerable<TagDto>>> GetAllAsync(
            CancellationToken ct = default)
        {
            var tags = await _tagRepository.GetAllAsync(ct);
            return Result<IEnumerable<TagDto>>.Success(
                _mapper.Map<IEnumerable<TagDto>>(tags));
        }

        public async Task<Result<TagDto>> GetByIdAsync(
            int id, CancellationToken ct = default)
        {
            var tag = await _tagRepository.GetByIdAsync(id, ct);
            if (tag is null || tag.IsDeleted)
                return Result<TagDto>.Failure(
                    $"Tag {id} not found.", "TAG_NOT_FOUND");

            return Result<TagDto>.Success(_mapper.Map<TagDto>(tag));
        }

        public async Task<Result<TagDto>> CreateAsync(
            CreateTagDto dto, Guid adminId, CancellationToken ct = default)
        {
            // 1. Validate
            var validation = await _createValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<TagDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            // 2. Check name uniqueness
            if (await _tagRepository.NameExistsAsync(dto.Name, ct))
                return Result<TagDto>.Failure(
                    $"Tag name '{dto.Name}' already exists.", "NAME_EXISTS");

            // 3. Check slug uniqueness
            if (await _tagRepository.SlugExistsAsync(dto.Slug, ct))
                return Result<TagDto>.Failure(
                    $"Slug '{dto.Slug}' is already taken.", "SLUG_EXISTS");

            // 4. Create via factory
            var tag = Tag.Create(dto.Name, dto.Slug, adminId);

            await _tagRepository.AddAsync(tag, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<TagDto>.Success(_mapper.Map<TagDto>(tag));
        }

        public async Task<Result<TagDto>> UpdateAsync(
            int id, UpdateTagDto dto, Guid adminId, CancellationToken ct = default)
        {
            // 1. Validate
            var validation = await _updateValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return Result<TagDto>.Failure(
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)),
                    "VALIDATION_FAILED");

            // 2. Find tag
            var tag = await _tagRepository.GetByIdAsync(id, ct);
            if (tag is null || tag.IsDeleted)
                return Result<TagDto>.Failure(
                    $"Tag {id} not found.", "TAG_NOT_FOUND");

            // 3. Check name uniqueness — exclude current
            if (await _tagRepository.NameExistsExceptAsync(dto.Name, id, ct))
                return Result<TagDto>.Failure(
                    $"Tag name '{dto.Name}' already exists.", "NAME_EXISTS");

            // 4. Check slug uniqueness — exclude current
            if (await _tagRepository.SlugExistsExceptAsync(dto.Slug, id, ct))
                return Result<TagDto>.Failure(
                    $"Slug '{dto.Slug}' is already taken.", "SLUG_EXISTS");

            // 5. Update — Tag entity needs Update method
            // Add this to Tag.cs domain entity:
            // public void Update(string name, string slug, Guid updatedBy)
            // { Name = name; Slug = slug.ToLowerInvariant(); SetUpdatedBy(updatedBy); }
            tag.Update(dto.Name, dto.Slug, adminId);

            _tagRepository.Update(tag);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<TagDto>.Success(_mapper.Map<TagDto>(tag));
        }

        public async Task<Result> DeleteAsync(
            int id, Guid adminId, CancellationToken ct = default)
        {
            var tag = await _tagRepository.GetByIdAsync(id, ct);
            if (tag is null || tag.IsDeleted)
                return Result.Failure($"Tag {id} not found.", "TAG_NOT_FOUND");

            tag.SoftDelete(adminId);
            _tagRepository.Update(tag);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        public async Task<Result<PagedList<TagDto>>> GetPagedAsync(TagFilterRequest filter, CancellationToken ct = default)
        {
            var paged = await _tagRepository.GetPagedAsync(filter, ct);

            var mapped = new PagedList<TagDto>(
                _mapper.Map<List<TagDto>>(paged.Items),
                paged.Page,
                paged.PageSize,
                paged.TotalCount);

            return Result<PagedList<TagDto>>.Success(mapped);
        }
    }

}

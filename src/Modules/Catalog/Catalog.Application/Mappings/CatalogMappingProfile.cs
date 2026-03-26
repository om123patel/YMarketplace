using AutoMapper;
using Catalog.Application.DTOs.Attributes;
using Catalog.Application.DTOs.Brands;
using Catalog.Application.DTOs.Categories;
using Catalog.Application.DTOs.Products;
using Catalog.Application.DTOs.Tags;
using Catalog.Domain.Entities;
using System.Text.Json;

namespace Catalog.Application.Mappings
{
   

    public class CatalogMappingProfile : Profile
    {
        public CatalogMappingProfile()
        {
            CreateCategoryMappings();
            CreateBrandMappings();
            CreateTagMappings();
            CreateAttributeTemplateMappings();
            CreateProductMappings();
            CreateProductStatusHistoryMappings();
        }

        // ══════════════════════════════════════════════════════
        // CATEGORY
        // ══════════════════════════════════════════════════════
        private void CreateCategoryMappings()
        {
            CreateMap<Category, CategoryDto>()
                .ForMember(d => d.ParentName, o => o.MapFrom(s =>
                    s.Parent != null ? s.Parent.Name : null))
                .ForMember(d => d.ProductCount, o => o.MapFrom(s =>
                    s.Products != null ? s.Products.Count : 0))
                .ForMember(d => d.Children, o => o.MapFrom(s =>
                    s.Children != null ? s.Children : new List<Category>()));

            // CreateCategoryDto → Category not needed
            // Category is created via factory method: Category.Create(...)
            // We never map DTOs directly to Domain entities
        }

        // ══════════════════════════════════════════════════════
        // BRAND
        // ══════════════════════════════════════════════════════
        private void CreateBrandMappings()
        {
            CreateMap<Brand, BrandDto>()
                .ForMember(d => d.ProductCount, o => o.MapFrom(s =>
                    s.Products != null ? s.Products.Count : 0));
        }

        // ══════════════════════════════════════════════════════
        // TAG
        // ══════════════════════════════════════════════════════
        private void CreateTagMappings()
        {
            CreateMap<Tag, TagDto>()
                .ForMember(d => d.ProductCount, o => o.MapFrom(s =>
                    s.Products != null ? s.Products.Count : 0));
        }

        // ══════════════════════════════════════════════════════
        // ATTRIBUTE TEMPLATE
        // ══════════════════════════════════════════════════════
        private void CreateAttributeTemplateMappings()
        {
            CreateMap<AttributeTemplateItem, AttributeTemplateItemDto>()
                .ForMember(d => d.ParsedOptions, o => o.MapFrom(s =>
                    ParseOptions(s.Options)));

            CreateMap<AttributeTemplate, AttributeTemplateDto>()
                .ForMember(d => d.CategoryName, o => o.MapFrom(s =>
                    s.Category != null ? s.Category.Name : string.Empty))
                .ForMember(d => d.Items, o => o.MapFrom(s =>
                    s.Items.OrderBy(i => i.SortOrder).ToList()));
        }

        // ══════════════════════════════════════════════════════
        // PRODUCT STATUS HISTORY
        // ══════════════════════════════════════════════════════
        private void CreateProductStatusHistoryMappings()
        {
            CreateMap<ProductStatusHistory, ProductStatusHistoryDto>()
                .ForMember(d => d.FromStatus, o => o.MapFrom(s =>
                    s.FromStatus.HasValue ? s.FromStatus.Value.ToString() : null))
                .ForMember(d => d.ToStatus, o => o.MapFrom(s =>
                    s.ToStatus.ToString()));
        }

        // ══════════════════════════════════════════════════════
        // PRODUCT
        // ══════════════════════════════════════════════════════
        private void CreateProductMappings()
        {
            // ProductAttribute → ProductAttributeDto
            CreateMap<ProductAttribute, ProductAttributeDto>();

            // ProductVariant → ProductVariantDto
            CreateMap<ProductVariant, ProductVariantDto>()
                .ForMember(d => d.Price, o => o.MapFrom(s =>
                    s.Price.Amount))
                .ForMember(d => d.CurrencyCode, o => o.MapFrom(s =>
                    s.Price.CurrencyCode))
                .ForMember(d => d.CompareAtPrice, o => o.MapFrom(s =>
                    s.CompareAtPrice != null ? s.CompareAtPrice.Amount : (decimal?)null))
                .ForMember(d => d.Attributes, o => o.MapFrom(s =>
                    s.Attributes.OrderBy(a => a.SortOrder).ToList()));

            // ProductImage → ProductImageDto
            CreateMap<ProductImage, ProductImageDto>();

            // ProductSeo → ProductSeoDto
            CreateMap<ProductSeo, ProductSeoDto>();

            // Product → ProductDto (full detail)
            CreateMap<Product, ProductDto>()
                // Ownership
                .ForMember(d => d.CreatorType, o => o.MapFrom(s =>
                    s.CreatorType.ToString()))
                // Pricing — unwrap Money value object
                .ForMember(d => d.BasePrice, o => o.MapFrom(s =>
                    s.BasePrice.Amount))
                .ForMember(d => d.CurrencyCode, o => o.MapFrom(s =>
                    s.BasePrice.CurrencyCode))
                .ForMember(d => d.CompareAtPrice, o => o.MapFrom(s =>
                    s.CompareAtPrice != null ? s.CompareAtPrice.Amount : (decimal?)null))
                // Status — enum to string
                .ForMember(d => d.Status, o => o.MapFrom(s =>
                    s.Status.ToString()))
                // Navigation — Category
                .ForMember(d => d.CategoryName, o => o.MapFrom(s =>
                    s.Category != null ? s.Category.Name : string.Empty))
                // Navigation — Brand
                .ForMember(d => d.BrandName, o => o.MapFrom(s =>
                    s.Brand != null ? s.Brand.Name : null))
                // Navigation — Images (ordered by SortOrder)
                .ForMember(d => d.Images, o => o.MapFrom(s =>
                    s.Images.OrderBy(i => i.SortOrder).ToList()))
                // Navigation — Variants (ordered by SortOrder)
                .ForMember(d => d.Variants, o => o.MapFrom(s =>
                    s.Variants
                        .Where(v => !v.IsDeleted)
                        .OrderBy(v => v.SortOrder)
                        .ToList()))
                // Navigation — Tags
                .ForMember(d => d.Tags, o => o.MapFrom(s =>
                    s.Tags.ToList()))
                // Navigation — SEO
                .ForMember(d => d.Seo, o => o.MapFrom(s => s.Seo));

            // Product → ProductListItemDto (lightweight for tables/lists)
            CreateMap<Product, ProductListItemDto>()
                .ForMember(d => d.BasePrice, o => o.MapFrom(s =>
                    s.BasePrice.Amount))
                .ForMember(d => d.CurrencyCode, o => o.MapFrom(s =>
                    s.BasePrice.CurrencyCode))
                .ForMember(d => d.Status, o => o.MapFrom(s =>
                    s.Status.ToString()))
                .ForMember(d => d.CreatorType, o => o.MapFrom(s =>
                    s.CreatorType.ToString()))
                .ForMember(d => d.PrimaryImageUrl, o => o.MapFrom(s =>
                    s.Images
                        .Where(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault() ??
                    s.Images
                        .OrderBy(i => i.SortOrder)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault()))
                .ForMember(d => d.VariantCount, o => o.MapFrom(s =>
                    s.Variants.Count(v => !v.IsDeleted)))
                // these fields are not on ProductListItemDto — ignore
                .ForMember(d => d.CategoryName, o => o.MapFrom(s =>
                    s.Category != null ? s.Category.Name : string.Empty))
                .ForMember(d => d.BrandName, o => o.MapFrom(s =>
                    s.Brand != null ? s.Brand.Name : null));
        }

        // ══════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ══════════════════════════════════════════════════════

        // Parses the JSON options string stored in AttributeTemplateItem
        // e.g. '["Red","Blue","Green"]' → ["Red", "Blue", "Green"]
        private static List<string> ParseOptions(string? options)
        {
            if (string.IsNullOrWhiteSpace(options))
                return [];

            try
            {
                return JsonSerializer.Deserialize<List<string>>(options) ?? [];
            }
            catch
            {
                return [];
            }
        }
    }


}

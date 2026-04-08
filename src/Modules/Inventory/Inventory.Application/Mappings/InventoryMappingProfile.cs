using AutoMapper;
using Inventory.Application.DTOs;
using Inventory.Domain.Entities;

namespace Inventory.Application.Mappings
{
    public class InventoryMappingProfile : Profile
    {
        public InventoryMappingProfile()
        {
            CreateMap<Stock, StockDto>()
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.AvailableQuantity,
                    o => o.MapFrom(s => s.AvailableQuantity))
                .ForMember(d => d.IsLowStock,
                    o => o.MapFrom(s => s.IsLowStock));

            CreateMap<Stock, StockListItemDto>()
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.AvailableQuantity,
                    o => o.MapFrom(s => s.AvailableQuantity))
                .ForMember(d => d.IsLowStock,
                    o => o.MapFrom(s => s.IsLowStock))
                // ProductName and VariantName resolved via join in repo query
                .ForMember(d => d.ProductName, o => o.Ignore())
                .ForMember(d => d.VariantName, o => o.Ignore())
                .ForMember(d => d.ProductImageUrl, o => o.Ignore());
        }
    }
}
using AutoMapper;
using Orders.Application.DTOs.Cart;
using Orders.Application.DTOs.Dispute;
using Orders.Application.DTOs.Orders;
using Orders.Domain.Entities;

namespace Orders.Application.Mappings
{
    public class OrdersMappingProfile : Profile
    {
        public OrdersMappingProfile()
        {
            // Order → OrderDto
            CreateMap<Order, OrderDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.PaymentStatus, o => o.MapFrom(s => s.PaymentStatus.ToString()))
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items.ToList()))
                .ForMember(d => d.Disputes, o => o.MapFrom(s => s.Disputes.ToList()));

            // Order → OrderListItemDto
            CreateMap<Order, OrderListItemDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.PaymentStatus, o => o.MapFrom(s => s.PaymentStatus.ToString()))
                .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.Items.Count));

            // OrderItem → OrderItemDto
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(d => d.LineTotal, o => o.MapFrom(s => s.LineTotal));

            // Dispute → DisputeDto
            CreateMap<Dispute, DisputeDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

            // Cart → CartDto
            CreateMap<Cart, CartDto>()
                .ForMember(d => d.TotalAmount, o => o.MapFrom(s => s.TotalAmount))
                .ForMember(d => d.TotalItems, o => o.MapFrom(s => s.TotalItems))
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items.ToList()));

            // CartItem → CartItemDto
            CreateMap<CartItem, CartItemDto>()
                .ForMember(d => d.LineTotal, o => o.MapFrom(s => s.LineTotal));
        }
    }

}

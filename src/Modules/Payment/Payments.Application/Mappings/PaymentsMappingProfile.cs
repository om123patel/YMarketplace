using AutoMapper;
using Payments.Application.DTOs.CommissionRules;
using Payments.Application.DTOs.Payouts;
using Payments.Application.DTOs.Transactions;
using Payments.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Payments.Application.Mappings
{
    public class PaymentsMappingProfile : Profile
    {
        public PaymentsMappingProfile()
        {
            // Transaction → TransactionDto
            CreateMap<Transaction, TransactionDto>()
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Method,
                    o => o.MapFrom(s => s.Method.ToString()));

            // Transaction → TransactionListItemDto
            CreateMap<Transaction, TransactionListItemDto>()
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Method,
                    o => o.MapFrom(s => s.Method.ToString()));

            // Payout → PayoutDto
            CreateMap<Payout, PayoutDto>()
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.SellerName,
                    o => o.Ignore());   // enriched by service from identity

            // Payout → PayoutListItemDto
            CreateMap<Payout, PayoutListItemDto>()
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.SellerName,
                    o => o.Ignore());

            // CommissionRule → CommissionRuleDto
            CreateMap<CommissionRule, CommissionRuleDto>()
                .ForMember(d => d.CategoryName,
                    o => o.Ignore());   // enriched by service if needed
        }
    }
}
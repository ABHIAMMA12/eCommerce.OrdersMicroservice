

using AutoMapper;
using BusinessLogicLayer.DTO;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Mappers
{
    public class OrderItemToOrderItemResponseProfile : Profile
    {
        public OrderItemToOrderItemResponseProfile()
        {
            CreateMap<OrderItem, OrderItemResponse>()
                .ForMember(temp => temp.ProductID, src => src.MapFrom(opt => opt.ProductID))
                .ForMember(temp => temp.Quantity, src => src.MapFrom(opt => opt.Quantity))
                .ForMember(temp => temp.UnitPrice, src => src.MapFrom(opt => opt.UnitPrice))
                .ForMember(temp => temp.TotalPrice, src => src.MapFrom(opt => opt.TotalPrice));       
        }
    }
}

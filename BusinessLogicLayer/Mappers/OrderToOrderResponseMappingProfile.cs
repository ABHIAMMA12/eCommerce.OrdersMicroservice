using AutoMapper;
using BusinessLogicLayer.DTO;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Mappers
{
    public class OrderToOrderResponseMappingProfile : Profile
    {
        public OrderToOrderResponseMappingProfile()
        {
            CreateMap<Order, OrderResponse>()
                .ForMember(temp => temp.OrderID, opt => opt.MapFrom(src => src.OrderID))
                .ForMember(temp => temp.UserID, opt => opt.MapFrom(src => src.UserID))
                .ForMember(temp => temp.OrderDate, opt => opt.MapFrom(src => src.OrderDate))
                .ForMember(temp => temp.OrderItems, opt => opt.MapFrom(src => src.OrderItems))
                .ForMember(temp => temp.TotalBill, opt => opt.MapFrom(src => src.TotalBill));
        }
    }
}

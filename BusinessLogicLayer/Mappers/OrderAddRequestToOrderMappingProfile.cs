using AutoMapper;
using BusinessLogicLayer.DTO;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Mappers
{
    public class OrderAddRequestToOrderMappingProfile : Profile
    {
        public OrderAddRequestToOrderMappingProfile()
        {
            CreateMap<OrderAddRequest, Order>()
                .ForMember(temp => temp.UserID, src => src.MapFrom(opt => opt.UserID))
                .ForMember(temp => temp.OrderDate, src => src.MapFrom(opt => opt.OrderDate))
                .ForMember(temp => temp.OrderItems, src => src.MapFrom(opt => opt.OrderItems))
                .ForMember(temp => temp.OrderID, src => src.Ignore())
                .ForMember(temp => temp._id, src => src.Ignore())
                .ForMember(temp => temp.TotalBill, src => src.Ignore());
                
        }
    }
}

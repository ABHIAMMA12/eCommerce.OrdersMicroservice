﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.DTO
{
    public record OrderResponse(Guid OrderID, Guid UserID, decimal TotalBill, DateTime OrderDate,
        List<OrderItemResponse> OrderItems,string? UserPersonName,string? UserPersonEmail)
    {
        public OrderResponse() : this(default, default, default, default, default,default,default)
        {
        }
    }
}

﻿using BusinessLogicLayer.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Validators
{
    public class OrderUpdateRequestValidator : AbstractValidator<OrderUpdateRequest>
    {
        public OrderUpdateRequestValidator()
        {
            //OrderID
            RuleFor(temp => temp.OrderID)
              .NotEmpty().WithErrorCode("Order ID can't be blank");

            //UserID
            RuleFor(temp => temp.UserID)
              .NotEmpty().WithErrorCode("User ID can't be blank");

            //OrderDate
            RuleFor(temp => temp.OrderDate)
              .NotEmpty().WithErrorCode("Order Date can't be blank");

            //OrderItems
            RuleFor(temp => temp.OrderItems)
              .NotEmpty().WithErrorCode("Order Items can't be blank");
        }
    }
}

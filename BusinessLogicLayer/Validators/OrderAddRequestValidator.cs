using BusinessLogicLayer.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Validators
{
    public class OrderAddRequestValidator : AbstractValidator<OrderAddRequest>
    {
        public OrderAddRequestValidator()
        {
            RuleFor(x=>x.UserID).NotEmpty().WithErrorCode("UserId should not be Empty");
            RuleFor(temp => temp.OrderDate).NotEmpty().WithErrorCode("Order Date can't be blank");
            RuleFor(temp => temp.OrderItems).NotEmpty().WithErrorCode("Order Items can't be blank");
        }
    }
}

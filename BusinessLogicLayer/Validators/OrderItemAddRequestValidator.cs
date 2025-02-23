using BusinessLogicLayer.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Validators
{
    public class OrderItemAddRequestValidator : AbstractValidator<OrderItemAddRequest>
    {
        public OrderItemAddRequestValidator()
        {
            RuleFor(x=>x.ProductID).NotEmpty().WithErrorCode("ProductId should not be empty");
            RuleFor(x => x.UnitPrice).NotEmpty().WithErrorCode("Unit Price Shouldn't be Empty")
                .GreaterThan(0).WithErrorCode("Unit Price can't be less than or equal to zero");
            RuleFor(temp => temp.Quantity)
              .NotEmpty().WithErrorCode("Quantity can't be blank")
              .GreaterThan(0).WithErrorCode("Quantity can't be less than or equal to zero");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.DTO
{
    public record ProductDTO(Guid ProductId,string? ProductName,string? Category,double UnitPrice,int QuantityInStock);
}

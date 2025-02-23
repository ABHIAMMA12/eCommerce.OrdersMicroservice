using BusinessLogicLayer.DTO;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Services;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrdersMicroservice.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: api/orders
        [HttpGet("all")]
        public async Task<IEnumerable<OrderResponse>> GetAllOrders()
        {
            return await _orderService.GetOrders();
        }

        // GET: api/orders/{orderId}
        [HttpGet("search/orderid/{orderId:Guid}")]
        public async Task<ActionResult<OrderResponse?>> GetOrderByOrderId(Guid orderId)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderId);
            var order = await _orderService.GetOrderByCondition(filter);
            if (order == null)
            {
                return NotFound($"Order with ID {orderId} not found.");
            }
            return Ok(order);
        }
        [HttpGet("search/productid/{productId}")]
        public async Task<IEnumerable<OrderResponse?>> GetOrdersByProductId(Guid productId)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.ElemMatch(te => te.OrderItems,
                Builders<OrderItem>.Filter.Eq(tem => tem.ProductID, productId));
            List<OrderResponse?> orders = await _orderService.GetOrdersByCondition(filter);
            return orders;
        }

        [HttpGet("search/orderDate/{orderDate:datetime}")]
        public async Task<IEnumerable<OrderResponse?>> GetOrderByOrderDate(DateTime orderDate)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderDate.ToString("yyyy-MM-dd"), orderDate.ToString("yyyy-MM-dd"));

            List<OrderResponse?> orders = await _orderService.GetOrdersByCondition(filter);
            return orders;
        }

        [HttpPost]
        public async Task<IActionResult> AddOrder(OrderAddRequest orderAddRequest)
        {
            if (orderAddRequest == null)
            {
                return BadRequest("Invalid order data");
            }
            OrderResponse? orderResponse = await _orderService.AddOrder(orderAddRequest);

            if (orderResponse == null)
            {
                return Problem("Error in adding product");
            }
            return Created($"api/Orders/search/orderid/{orderResponse?.OrderID}", orderResponse);

        }
        //we need to pass the needed orderid to update
        [HttpPut("{orderID}")]
        public async Task<IActionResult> UpdateOrder(Guid orderID, OrderUpdateRequest orderUpdateRequest)
        {
            if (orderUpdateRequest == null)
            {
                return BadRequest("Invalid order data");
            }

            if (orderID != orderUpdateRequest.OrderID)
            {
                return BadRequest("OrderID in the URL doesn't match with the OrderID in the Request body");
            }

            OrderResponse? orderResponse = await _orderService.UpdateOrder(orderUpdateRequest);

            if (orderResponse == null)
            {
                return Problem("Error in adding product");
            }
            return Ok(orderResponse);
        }

        [HttpDelete("{orderID}")]
        public async Task<IActionResult> Delete(Guid orderID)
        {
            if (orderID == Guid.Empty)
            {
                return BadRequest("Invalid order ID");
            }
            bool isDeleted = await _orderService.DeleteOrder(orderID);
            if (!isDeleted)
            {
                return Problem("Error in Deleting product");
            }
            return Ok($"Order with Id : {orderID} is Deleted Successfully");
        }

    }
}
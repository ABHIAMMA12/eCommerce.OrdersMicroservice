﻿using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using MongoDB.Driver;
using ZstdSharp.Unsafe;

namespace DataAccessLayer.Repositories
{
    public class OrderRepository : IOrdersRepository
    {
        private readonly IMongoCollection<Order> _orders;
        private readonly string collectionName = "orders";
        public OrderRepository(IMongoDatabase mongo)
        {
            _orders=mongo.GetCollection<Order>(collectionName);
        }

        public async Task<Order> AddOrder(Order order)
        {
            order.OrderID =  Guid.NewGuid();
            order._id = Guid.NewGuid();
            foreach(OrderItem orderItem in order.OrderItems)
            {
                orderItem._id = Guid.NewGuid(); 
            }
            await _orders.InsertOneAsync(order);
            return order;
        }

        public async Task<bool> DeleteOrder(Guid orderId)
        {
            //creatoing the filter only it not executes any thing 
            FilterDefinition<Order> filter  = Builders<Order>.Filter.Eq(temp=>temp.OrderID,orderId);
            DeleteResult deleteResult = await _orders.DeleteOneAsync(filter);
            return deleteResult.DeletedCount > 0;
        }

        public async Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter)
        {
            return (await _orders.FindAsync(filter)).FirstOrDefault();
        }

        public async Task<IEnumerable<Order>> GetOrders()
        {
            return (await _orders.FindAsync(Builders<Order>.Filter.Empty)).ToList();
        }

        public async Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter)
        {
            return (await _orders.FindAsync(filter)).ToList();
        }

        public async Task<Order> UpdateOrder(Order order)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, order.OrderID);
            Order? existingOrder = (await _orders.FindAsync(filter)).FirstOrDefault();
            if (existingOrder == null) {
                return null!;
            }
            order._id = existingOrder._id;
            ReplaceOneResult replace = await _orders.ReplaceOneAsync(filter,order);
            return order;

        }
    }
}

using AutoMapper;
using BusinessLogicLayer.DTO;
using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
        private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
        private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
        private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
        private readonly UsersMicroserviceClient _usersMicroserviceClient;
        private readonly ProdcutsMicroserviceClient _prodcutsMicroserviceClient;

        public OrderService(IOrdersRepository ordersRepository, IMapper mapper, IValidator<OrderAddRequest> orderAddRequestValidator, IValidator<OrderItemAddRequest> orderItemAddRequestValidator, IValidator<OrderUpdateRequest> orderUpdateRequestValidator, IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator, UsersMicroserviceClient usersMicroserviceClient, ProdcutsMicroserviceClient prodcutsMicroserviceClient)
        {
            _ordersRepository = ordersRepository;
            _mapper = mapper;
            _orderAddRequestValidator = orderAddRequestValidator;
            _orderItemAddRequestValidator = orderItemAddRequestValidator;
            _orderUpdateRequestValidator = orderUpdateRequestValidator;
            _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
            _usersMicroserviceClient = usersMicroserviceClient;
            _prodcutsMicroserviceClient = prodcutsMicroserviceClient;
        }

        public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
        {
            if (orderAddRequest == null)
            {
                throw new ArgumentNullException(nameof(orderAddRequest));
            }
            ValidationResult validation = await _orderAddRequestValidator.ValidateAsync(orderAddRequest);
            if (!validation.IsValid)
            {
                string errors = string.Join(",", validation.Errors.Select(x => x.ErrorMessage));
                throw new ValidationException(errors);
            }
            //now iterate through exach irder item in order addrequet for the validation
            List<ProductDTO?> products = new List<ProductDTO?>();
            foreach (OrderItemAddRequest orderItemAddRequest in orderAddRequest.OrderItems)
            {
                ValidationResult result = await _orderItemAddRequestValidator.ValidateAsync(orderItemAddRequest);
                if (!result.IsValid)
                {
                    string errors = string.Join(",", result.Errors.Select(x => x.ErrorMessage));
                    throw new ValidationException(errors);
                }
                ProductDTO? product = await _prodcutsMicroserviceClient.GetProductByProductId(orderItemAddRequest.ProductID);
                if (product == null)
                {
                    throw new ArgumentException("Invalid Product ID");
                }
                products.Add(product);
            }


            //check prodcutID id exists in products microservice or not


            //TO DO: Add logic for checking if UserID exists in Users microservice

            UserDTO? user=await _usersMicroserviceClient.GetUserByUserId(orderAddRequest.UserID);
            if (user == null)
            {
                throw new ArgumentException("Invalid UserID");
            }
            Order orderInput = _mapper.Map<Order>(orderAddRequest);
            //Genrat3e values for bill cost

            foreach (OrderItem orderItem in orderInput.OrderItems)
            {
                orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;
            }
            //Gives the total bill for the ordered items 
            orderInput.TotalBill = orderInput.OrderItems.Sum(tem => tem.TotalPrice);

            //Invoke Repository

            Order? addedOrder = await _ordersRepository.AddOrder(orderInput);
            if (addedOrder == null)
            {
                return null;
            }
            OrderResponse addedOrderResponnse = _mapper.Map<OrderResponse>(addedOrder);
            if (addedOrderResponnse != null)
            {
                foreach(OrderItemResponse orderItemResponse in addedOrderResponnse.OrderItems)
                {
                    //we dont get product name and category before that, by this wee will get the product name anfd category in ordersdproducts items
                    ProductDTO? product =  products.Where(temp => temp.ProductId == orderItemResponse.ProductID).FirstOrDefault();
                    if (product == null)
                    {
                        continue;
                    }
                    _mapper.Map<ProductDTO, OrderItemResponse>(product, orderItemResponse);
                }
            }
            if (addedOrderResponnse != null)
            {
                UserDTO? userDTo = await _usersMicroserviceClient.GetUserByUserId(addedOrderResponnse.UserID);
                if (userDTo != null)
                {
                    _mapper.Map<UserDTO, OrderResponse>(userDTo, addedOrderResponnse);
                }
            }
            return addedOrderResponnse;
        }

        public async Task<bool> DeleteOrder(Guid orderId)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderId);
            Order? order = await _ordersRepository.GetOrderByCondition(filter);
            if (order == null)
            {
                return false;
            }
            bool isDeleted = await _ordersRepository.DeleteOrder(orderId);
            return true;
        }

        public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
        {
            Order? order = await _ordersRepository.GetOrderByCondition(filter);
            if (order == null)
            {
                return null!;
            }
            OrderResponse orderResponse = _mapper.Map<OrderResponse>(order);
            if (orderResponse != null)
            {
                foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
                {
                    //we dont get product name and category before that, by this wee will get the product name anfd category in ordersdproducts items
                    ProductDTO? product = await _prodcutsMicroserviceClient.GetProductByProductId(orderItemResponse.ProductID);
                    if (product == null)
                    {
                        continue;
                    }
                    _mapper.Map<ProductDTO, OrderItemResponse>(product, orderItemResponse);
                }

            }
            if (orderResponse != null)
            {
                UserDTO? userDTo = await _usersMicroserviceClient.GetUserByUserId(orderResponse.UserID);
                if(userDTo != null)
                {
                    _mapper.Map<UserDTO, OrderResponse>(userDTo, orderResponse);
                }
            }
            return orderResponse;
        }

        public async Task<List<OrderResponse>> GetOrders()
        {
            IEnumerable<Order?> orders = await _ordersRepository.GetOrders();
            //Load Product Name and Category in each order
            
            IEnumerable<OrderResponse> ordersResponses = _mapper.Map<IEnumerable<OrderResponse>>(orders);
            foreach (OrderResponse or in ordersResponses)
            {
                if (or == null)
                {
                    continue;
                }
                //mapping each product with their name and category based on the order
                foreach(OrderItemResponse orderItemResponse in or.OrderItems)
                {
                    //we dont get product name and category before that, by this wee will get the product name anfd category in ordersdproducts items
                    ProductDTO? product = await _prodcutsMicroserviceClient.GetProductByProductId(orderItemResponse.ProductID);
                    if(product == null)
                    {
                        continue;
                    }
                    _mapper.Map<ProductDTO,OrderItemResponse>(product,orderItemResponse);
                }
                //Load UserName and Email from the Users Microservice
                UserDTO? user = await _usersMicroserviceClient.GetUserByUserId(or.UserID);
                if (user != null)
                {
                    _mapper.Map<UserDTO, OrderResponse>(user, or);
                }
            }
            return ordersResponses.ToList();
        }

        public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
        {
            IEnumerable<Order?> orders = await _ordersRepository.GetOrdersByCondition(filter);
            IEnumerable<OrderResponse?> orderResponses = _mapper.Map<IEnumerable<OrderResponse>>(orders);
            foreach (OrderResponse or in orderResponses)
            {
                if (or == null)
                {
                    continue;
                }
                //mapping each product with their name and category based on the order
                foreach (OrderItemResponse orderItemResponse in or.OrderItems)
                {
                    ProductDTO? product = await _prodcutsMicroserviceClient.GetProductByProductId(orderItemResponse.ProductID);
                    if (product == null)
                    {
                        continue;
                    }
                    _mapper.Map<ProductDTO, OrderItemResponse>(product, orderItemResponse);
                }
                UserDTO? user = await _usersMicroserviceClient.GetUserByUserId(or.UserID);
                if (user != null)
                {
                    _mapper.Map<UserDTO, OrderResponse>(user, or);
                }
            }
                return orderResponses.ToList();
        }

        public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
        {
            if (orderUpdateRequest == null)
            {
                throw new ArgumentNullException(nameof(orderUpdateRequest));
            }
            ValidationResult validation = await _orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);
            if (!validation.IsValid)
            {
                string errors = string.Join(",", validation.Errors.Select(x => x.ErrorMessage));
                throw new ValidationException(errors);
            }
            List<ProductDTO?> products = new List<ProductDTO?>();
            //now iterate through each order item in order Updaterequet for the validation
            //Validate order items using Fluent Validation
            foreach (OrderItemUpdateRequest orderItemUpdateRequest in orderUpdateRequest.OrderItems)
            {
                ValidationResult orderItemUpdateRequestValidationResult = await _orderItemUpdateRequestValidator.ValidateAsync(orderItemUpdateRequest);

                if (!orderItemUpdateRequestValidationResult.IsValid)
                {
                    string errors = string.Join(", ", orderItemUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
                    throw new ArgumentException(errors);
                }
                ProductDTO? product = await _prodcutsMicroserviceClient.GetProductByProductId(orderItemUpdateRequest.ProductID);
                if (product == null)
                {
                    throw new ArgumentException("Invalid Product ID");
                }
                products.Add(product);
            }
            //TO DO: Add logic for checking if UserID exists in Users microservice

            UserDTO? user = await _usersMicroserviceClient.GetUserByUserId(orderUpdateRequest.UserID);
            if (user == null)
            {
                throw new ArgumentException("Invalid UserID");
            }
            Order orderInput = _mapper.Map<Order>(orderUpdateRequest);
            foreach (OrderItem orderItem in orderInput.OrderItems)
            {
                orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
            }
            orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);

            //Invoke Repository
            Order? orderResponse = await _ordersRepository.UpdateOrder(orderInput);
            if (orderResponse == null)
            {
                return null;
            }

            OrderResponse UpdatedOrderResposnse = _mapper.Map<OrderResponse>(orderResponse); //Map addedOrder ('Order' type) into 'OrderResponse' type (it invokes OrderToOrderResponseMappingProfile).
            if (UpdatedOrderResposnse != null)
            {
                foreach (OrderItemResponse orderItemResponse in UpdatedOrderResposnse.OrderItems)
                {
                    //we dont get product name and category before that, by this wee will get the product name anfd category in ordersdproducts items
                    ProductDTO? product = products.Where(temp => temp!.ProductId == orderItemResponse.ProductID).FirstOrDefault();
                    if (product == null)
                    {
                        continue;
                    }
                    _mapper.Map<ProductDTO, OrderItemResponse>(product, orderItemResponse);

                }
                
            }
            if (UpdatedOrderResposnse != null)
            {
                UserDTO? userDTO = await _usersMicroserviceClient.GetUserByUserId(UpdatedOrderResposnse.UserID);
                if (user != null)
                {
                    _mapper.Map<UserDTO, OrderResponse>(user, UpdatedOrderResposnse);
                }
            }
            //Loding the User Person Details
            return UpdatedOrderResposnse;
        }
    }
}

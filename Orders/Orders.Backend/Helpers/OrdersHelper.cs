using Orders.Backend.UnitsOfWork.Interfaces;
using Orders.Shared.Entities;
using Orders.Shared.Enums;
using Orders.Shared.Responses;

namespace Orders.Backend.Helpers
{
    public class OrdersHelper : IOrdersHelper
    {
        private readonly IUsersUnitOfWork _usersUnitOfWork;
        private readonly ITemporalOrdersUnitOfWork _temporalOrdersUnitOfWork;
        private readonly IProductsUnitOfWork _productsUnitOfWork;
        private readonly IOrdersUnitOfWork _ordersUnitOfWork;

        public OrdersHelper(IUsersUnitOfWork usersUnitOfWork, ITemporalOrdersUnitOfWork temporalOrdersUnitOfWork, IProductsUnitOfWork productsUnitOfWork, IOrdersUnitOfWork ordersUnitOfWork)
        {
            _usersUnitOfWork = usersUnitOfWork;
            _temporalOrdersUnitOfWork = temporalOrdersUnitOfWork;
            _productsUnitOfWork = productsUnitOfWork;
            _ordersUnitOfWork = ordersUnitOfWork;
        }

        public async Task<ActionResponse<bool>> ProcessOrderAsync(string email, string remarks)
        {
            var user = await _usersUnitOfWork.GetUserAsync(email);
            if (user == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Usuario no válido"
                };
            }

            var actionTemporalOrders = await _temporalOrdersUnitOfWork.GetAsync(email);
            if (!actionTemporalOrders.WasSuccess)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No hay detalle en la orden"
                };
            }

            var temporalOrders = actionTemporalOrders.Result as List<TemporalOrder>;
            var response = await CheckInventoryAsync(temporalOrders!);
            if (!response.WasSuccess)
            {
                return response;
            }

            var order = new Order
            {
                Date = DateTime.UtcNow,
                User = user,
                Remarks = remarks,
                OrderDetails = new List<OrderDetail>(),
                OrderStatus = OrderStatus.New
            };

            foreach (var temporalOrder in temporalOrders!)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    Product = temporalOrder.Product,
                    Quantity = temporalOrder.Quantity,
                    Remarks = temporalOrder.Remarks,
                });

                var actionProduct = await _productsUnitOfWork.GetAsync(temporalOrder.Product!.Id);
                if (actionProduct.WasSuccess)
                {
                    var product = actionProduct.Result;
                    if (product != null)
                    {
                        product.Stock -= temporalOrder.Quantity;
                        await _productsUnitOfWork.UpdateAsync(product);
                    }
                }

                await _temporalOrdersUnitOfWork.DeleteAsync(temporalOrder.Id);
            }

            await _ordersUnitOfWork.AddAsync(order);
            return response;
        }

        private async Task<ActionResponse<bool>> CheckInventoryAsync(List<TemporalOrder> temporalOrders)
        {
            var response = new ActionResponse<bool>() { WasSuccess = true };
            foreach (var temporalOrder in temporalOrders)
            {
                var actionProduct = await _productsUnitOfWork.GetAsync(temporalOrder.Product!.Id);
                if (!actionProduct.WasSuccess)
                {
                    response.WasSuccess = false;
                    response.Message = $"El producto {temporalOrder.Product!.Id}, ya no está disponible";
                    return response;
                }

                var product = actionProduct.Result;
                if (product == null)
                {
                    response.WasSuccess = false;
                    response.Message = $"El producto {temporalOrder.Product!.Id}, ya no está disponible";
                    return response;
                }

                if (product.Stock < temporalOrder.Quantity)
                {
                    response.WasSuccess = false;
                    response.Message = $"Lo sentimos no tenemos existencias suficientes del producto {temporalOrder.Product!.Name}, para tomar su pedido. Por favor disminuir la cantidad o sustituirlo por otro.";
                    return response;
                }
            }
            return response;
        }
    }
}
﻿using Herbg.Infrastructure.Interfaces;
using Herbg.Models;
using Herbg.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Herbg.ViewModels.Order;


namespace Herbg.Services.Services;

public class OrderService(IRepositroy<Cart> cart, IRepositroy<Order>order) : IOrderService
{
    private readonly IRepositroy<Order> _order = order;
    private readonly IRepositroy<Cart> _cart = cart;
    public async Task<CheckoutViewModel> GetCheckout(string clientId,string cartId)
    {
        var clientCart = await _cart
            .GetAllAttachedAsync()
            .Where(c => c.Id == cartId)
            .Include(c => c.Client)
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync();

        if (clientCart == null)
        {
            return null;
        }

        var totalProductPrice = clientCart.CartItems.Sum(c => c.Price * c.Quantity);

        var checkoutView = new ViewModels.Order.CheckoutViewModel
        {
            Address = clientCart.Client.Address!,
            CartItems = clientCart.CartItems.Select(c => new ViewModels.Cart.CartItemViewModel
            {
                ProductId = c.ProductId,
                ImagePath = c.Product.ImagePath,
                Name = c.Product.Name,
                Price = c.Price,
                Quantity = c.Quantity
            }).ToList(),
            Subtotal = totalProductPrice,
            ShippingCost = 10,
            Total = totalProductPrice + 10
        };

        return checkoutView;
    }

    public async Task<string> GetOrderConfirmed(string clientId, CheckoutViewModel model)
    {
        // Fetch the cart and associated items
        var cartToRemove = await _cart
            .GetAllAttachedAsync()
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product) // Ensure Product details are included if needed for validation
            .FirstOrDefaultAsync(c => c.ClientId == clientId);

        if (cartToRemove == null)
        {
            return null;
        }

        // Calculate the total price dynamically from cart items
        decimal calculatedTotal = cartToRemove.CartItems.Sum(item => item.Price * item.Quantity);

        // Create the new order
        var newOrder = new Order
        {
            ClientId = clientId,
            Address = model.Address,
            PaymentMethod = model.PaymentMethod,
            TotalAmount = calculatedTotal,
            ProductOrders = new List<ProductOrder>()
        };

        // Convert cart items to ProductOrder entries
        foreach (var item in cartToRemove.CartItems)
        {
            var newItem = new ProductOrder
            {
                ProductId = item.ProductId,
                Price = item.Price,
                Quantity = item.Quantity
            };

            newOrder.ProductOrders.Add(newItem);
        }

        // Save the new order and remove the cart
        await _order.AddAsync(newOrder);
        await _cart.DeleteAsync(cartToRemove);
        return newOrder.Id;
    }
}

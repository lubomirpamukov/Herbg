﻿using Herbg.Models;
using Herbg.Services.Interfaces;
using Herbg.ViewModels.Cart;
using Herbg.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Herbg.Services.Services;

public class CartService(IRepository<Cart> cart, IRepository<Product> product, IRepository<Wishlist>wishlist) : ICartService
{
    private readonly IRepository<Cart> _cart = cart;
    private readonly IRepository<Product> _product = product;
    private readonly IRepository<Wishlist> _wishlist = wishlist;

    public async Task<bool> AddItemToCartAsync(string clientId, int productId, int quantity)
    {
        var productToAdd = await _product.FindByIdAsync(productId);
        if (productToAdd == null) 
        {
            return false;
        }

        var clientCart = await _cart
               .GetAllAttached()
               .Include(c => c.CartItems)
               .FirstOrDefaultAsync(c => c.ClientId == clientId);

        if (clientCart == null)
        {
            var newCart = new Cart
            {
                ClientId = clientId!
            };

            await _cart.AddAsync(newCart);
            clientCart = newCart;
        }

        var existingCartItem = clientCart.CartItems.FirstOrDefault(ci => ci.ProductId == productToAdd.Id);

        if (existingCartItem != null)
        {
            // If the product is already in the cart, increase the quantity
            existingCartItem.Quantity += quantity;
        }
        else
        {
            // Add new item to the cart
            var cartItemToAdd = new CartItem
            {
                CartId = clientCart.Id,
                ProductId = productToAdd.Id,
                Quantity = quantity,
                Price = productToAdd.Price
            };

            clientCart.CartItems.Add(cartItemToAdd);
        }

        await _cart.UpdateAsync(clientCart);

        return true;

    }

    public async Task<int> GetCartItemsCountAsync(string clientId)
    {
        var cart = await _cart.GetAllAttached()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

        if (cart == null)
        {
            var newCart = new Cart
            {
                ClientId = clientId!
            };

           await _cart.AddAsync(newCart);
        }

        var cartItemCount = cart?.CartItems?.Sum(ci => ci.Quantity) ?? 0;

        return cartItemCount;
    }

    public async Task<CartViewModel> GetUserCartAsync(string clientId)
    {
        var clientCart = await _cart.GetAllAttached()
              .Where(c => c.ClientId == clientId)
              .Select(c => new CartViewModel
              {
                  Id = c.Id,
                  CartItems = c.CartItems.Select(ci => new CartItemViewModel
                  {
                      ProductId = ci.ProductId,
                      ImagePath = ci.Product.ImagePath,
                      Name = ci.Product.Name,
                      Price = ci.Price,
                      Quantity = ci.Quantity
                  }).ToList()
              })
              .FirstOrDefaultAsync();

        if (clientCart == null)
        {
            var newCart = new Cart
            {
                ClientId = clientId!
            };

            await _cart.AddAsync(newCart);
            clientCart = new CartViewModel { Id = newCart.Id , CartItems = new List<CartItemViewModel>()};
        }

        return clientCart!;
    }

    public async Task<bool> MoveCartItemToWishListAsync(string clinetId, int productId)
    {
        var clientCart = await _cart.GetAllAttached()
                .Where(c => c.ClientId == clinetId)
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync();
        if (clientCart == null)
        {
            //There is no client card
            return false;
        }

        var cartItem = clientCart.CartItems.Where(ci => ci.ProductId == productId).FirstOrDefault();

        if (cartItem == null) 
        {
            //Client cart is empty
            return false;
        }

        //Add to wishlist
        var newWishlistItem = new Wishlist { ClientId = clinetId, ProductId = cartItem.ProductId };
        await _wishlist.AddAsync(newWishlistItem);
        //Remove from cart
        clientCart.CartItems.Remove(cartItem);
        await _cart.UpdateAsync(clientCart);
        return true;
    }

    public async Task<bool> RemoveCartItemAsync(string clientId, int productId)
    {
        // Load the cart with its items in a single query
        var clientCart = await _cart.GetAllAttached()
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.ClientId == clientId);

        if (clientCart == null)
        {
            return false;
        }

        var productToRemove = clientCart.CartItems.FirstOrDefault(ci => ci.ProductId.Equals(productId));

        if (productToRemove == null)
        {
            return false;
        }

        clientCart.CartItems.Remove(productToRemove);
        await _cart.UpdateAsync(clientCart);
        return true;
    }

    public async Task<bool> UpdateCartItemQuantityAsync(string clientId, int productId, int quantity)
    {
        var clientCart = await _cart
            .GetAllAttached()
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(ci => ci.ClientId == clientId && ci.CartItems.Any(p => p.ProductId == productId));

        if (clientCart == null)
        {
            return false;
        }

        var itemToUpdate = clientCart.CartItems.FirstOrDefault(c => c.ProductId == productId);

        itemToUpdate!.Quantity = quantity;
        await _cart.UpdateAsync(clientCart);

        return true;
    }
}

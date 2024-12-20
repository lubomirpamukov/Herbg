﻿using Herbg.Infrastructure.Interfaces;
using Herbg.Models;
using Herbg.Services.Interfaces;
using Herbg.ViewModels.Wishlist;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Herbg.Services.Services;

public class WishlistService(IRepository<Wishlist>wishlist,IRepository<Product>products,IRepository<Cart>carts) : IWishlistService
{
    private readonly IRepository<Wishlist> _wishlist = wishlist;
    private readonly IRepository<Product> _products = products;
    private readonly IRepository<Cart> _carts = carts;

    public async Task<bool> AddToWishlistAsync(string clientId, int productId)
    {
        //Check if wishlist item exist
        var wishlistItem = await _wishlist
            .GetAllAttached()
            .FirstOrDefaultAsync(c => c.ClientId == clientId && c.ProductId == productId);

        if (wishlistItem == null)
        {
            var newWishlistItem = new Wishlist { ClientId = clientId, ProductId = productId };
            await _wishlist.AddAsync(newWishlistItem);
        }

        return true;
    }

    public async Task<IEnumerable<WishlistItemViewModel>> GetClientWishlistAsync(string clientId)
    {
        //Check client wishlist
        var clientWishlists = await _wishlist
            .GetAllAttached()
            .Where(w => w.ClientId == clientId)
            .Include(w => w.Product)
            .Select(c => new WishlistItemViewModel
            {
                Id = c.ProductId,
                Description = c.Product.Description,
                ImagePath = c.Product.ImagePath,
                Name = c.Product.Name,
                Price = c.Product.Price
            })
            .ToArrayAsync();

        return clientWishlists;
    }

    public async Task<int> GetWishlistItemCountAsync(string clientId)
    {
        var wishlist = await _wishlist
            .GetAllAttached()
            .Where(w => w.ClientId == clientId)
            .ToArrayAsync();

        var wishlistCount = wishlist?.Count() ?? 0;

        return wishlistCount;
    }

    //Make that method return enum
    public async Task<bool> MoveToCartAsync(string clientId, int productId)
    {
        // Check if the product exists
        var productToAdd = await _products.FindByIdAsync(productId);
            
        if (productToAdd == null)
        {
            return false; // no product with that id
        }

        // Check if client has a cart
        var clientCart = await _carts
            .GetAllAttached()
            .Include(c => c.CartItems).ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.ClientId == clientId);

        if (clientCart == null)
        {
            clientCart = new Cart { ClientId = clientId, CartItems = new List<CartItem>() };
            await _carts.AddAsync(clientCart); //Creates car for the client
        }

        // Check if the product is already in the cart
        var existingCartItem = clientCart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
        if (existingCartItem == null)
        {
            // Add product to cart
            var cartItem = new CartItem
            {
                ProductId = productId,
                CartId = clientCart.Id,
                Price = productToAdd.Price,
                Quantity = 1
            };
            clientCart.CartItems.Add(cartItem);
            await _carts.UpdateAsync(clientCart);
        }

        // Remove product from wishlist
        var removeProductFromWishlist = await RemoveFromWishlistAsync(clientId, productId);

        return true;
    }

    public async Task<bool> RemoveFromWishlistAsync(string clientId, int productId)
    {
        //Check if wishlist item exist
        var wishlistItem = await _wishlist
            .GetAllAttached()
            .FirstOrDefaultAsync(c => c.ClientId == clientId && c.ProductId == productId);

        if (wishlistItem != null)
        {
            await _wishlist.DeleteAsync(wishlistItem);
        }

        return true;
    }
}

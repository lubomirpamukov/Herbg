﻿using Herbg.Infrastructure.Interfaces;
using Herbg.Models;
using Herbg.Services.Interfaces;
using Herbg.ViewModels.Product;
using Herbg.ViewModels.Review;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Herbg.Services.Services;

public class ProductService(IRepositroy<Product> product) : IProductService
{
    private readonly IRepositroy<Product> _product = product;

    public async Task<ICollection<ProductCardViewModel>> GetAllProductsAsync()
    {
        var products = await _product
             .GetAllAttachedAsync()
             .Where(p => p.IsDeleted == false)
             .Select(p => new ProductCardViewModel
             {
                 Id = p.Id,
                 Name = p.Name,
                 Description = p.Description,
                 ImagePath = p.ImagePath,
                 Price = p.Price
             })
             .ToArrayAsync(); 
        return products;
    }

    public async Task<Product> GetProductByIdAsync(int productId)
    {
        var productToAdd = await _product.FindByIdAsync(productId);

        return productToAdd!;
    }

    public async Task<ProductDetailsViewModel> GetProductDetailsAsync(int productId)
    {
        var product = await _product
            .GetAllAttachedAsync()
            .Where(p => p.Id == productId)
            .Include(p => p.Category)
            .Include(p => p.Manufactorer)
            .Include(p => p.Reviews)
            .ThenInclude(r => r.Client)
            .Select(p => new ProductDetailsViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category.Name,
                Description = p.Description,
                ImagePath = p.ImagePath,
                Price = p.Price,
                Manufactorer = p.Manufactorer.Name,
                Reviews = p.Reviews.Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    Description = r.Description!,
                    Rating = r.Rating,
                    ReviewerName = r.Client.UserName!

                }).ToList()
            })
            .FirstOrDefaultAsync();

        return product;
    }
}

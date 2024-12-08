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

public class ProductService(IRepository<Product> product) : IProductService
{
    private readonly IRepository<Product> _product = product;

    public async Task<ICollection<ProductCardViewModel>> GetAllProductsAsync(string? searchQuery = null)
    {
        var products = _product
            .GetAllAttached()
            .Where(p => !p.IsDeleted); // Always filter out deleted products

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            searchQuery = searchQuery.ToLower().Trim();
            products = products.Where(p => p.Name.ToLower().Contains(searchQuery));
        }

        var productView = await products
            .Select(p => new ProductCardViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ImagePath = p.ImagePath ?? "default-image-path.jpg", // Provide default value if needed
                Price = p.Price
            })
            .ToArrayAsync();

        return productView;
    }


    public async Task<Product> GetProductByIdAsync(int productId)
    {
        var productToAdd = await _product.FindByIdAsync(productId);

        return productToAdd!;
    }

    

    public async Task<ProductDetailsViewModel> GetProductDetailsAsync(int productId)
    {
        var product = await _product
            .GetAllAttached()
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
                ImagePath = p.ImagePath!,
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

    public async Task<ICollection<ProductCardViewModel>> GetProductsByCategoryAsync(int categoryId)
    {
        var productsByCategory = await _product
            .GetAllAttached()
            .Where(p => p.CategoryId == categoryId && p.IsDeleted == false)
            .ToArrayAsync();

        var viewModelCollection = new List<ProductCardViewModel>();
        foreach (var product in productsByCategory)
        {
            var newProduct = new ProductCardViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                ImagePath = product.ImagePath!,
                Price = product.Price
            };

            viewModelCollection.Add(newProduct);
        }
        return viewModelCollection;
    }
}

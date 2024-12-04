﻿using Herbg.Infrastructure.Interfaces;
using Herbg.Models;
using Herbg.Services.Interfaces;
using Herbg.ViewModels.Category;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Herbg.Services.Services;

public class CategoryService(IRepositroy<Category> category) : ICategoryService
{
    private readonly IRepositroy<Category> _category = category;
    public async Task<ICollection<CategoryCardViewModel>> GetAllCategoriesAsync()
    {
        var categories = await _category
            .GetAllAttachedAsync()
            .Select(c => new CategoryCardViewModel
            {
                Id = c.Id,
                Name = c.Name,
                ImagePath = c.ImagePath,
                Description = c.Description
            })
            .ToArrayAsync();

        return categories;
    }
}

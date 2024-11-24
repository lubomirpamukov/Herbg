﻿namespace Herbg.Common.ValidationConstants;

public static class Category
{
    public const int CategoryNameMaxLength = 80;
    public const int CategoryNameMinLength = 2;
    public const string CategoryNameErrorMessage = "Category name must be between 2 and 80 characters";

    public const int CategoryDescriptionMaxLength = 30;
    public const int CategoryDescriptionMinLength = 4;
    public const string CategoryDescriptionLengthErrorMessage = "The description must be between 4 and 30 characters long";

}

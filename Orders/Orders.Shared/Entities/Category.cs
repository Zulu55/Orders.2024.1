﻿using System.ComponentModel.DataAnnotations;
using Orders.Shared.Interfaces;

namespace Orders.Shared.Entities
{
    public class Category : IEntityWithName
    {
        public int Id { get; set; }

        [Display(Name = "Categoría")]
        [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public string Name { get; set; } = null!;

        public ICollection<ProductCategory>? ProductCategories { get; set; }

        [Display(Name = "Productos")]
        public int ProductCategoriesNumber => ProductCategories == null || ProductCategories.Count == 0 ? 0 : ProductCategories.Count;
    }
}
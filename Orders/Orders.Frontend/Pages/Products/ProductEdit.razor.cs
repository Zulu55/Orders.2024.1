using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Orders.Frontend.Repositories;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;

namespace Orders.Frontend.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public partial class ProductEdit
    {
        private ProductDTO productDTO = new()
        {
            ProductCategoryIds = new List<int>(),
            ProductImages = new List<string>()
        };

        private ProductForm? productForm;
        private List<Category> selectedCategories = new();
        private List<Category> nonSelectedCategories = new();
        private bool loading = true;
        private Product? product;
        [Parameter] public int ProductId { get; set; }
        [Inject] private IRepository Repository { get; set; } = null!;
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            await LoadProductAsync();
            await LoadCategoriesAsync();
        }

        private async Task AddImageAsync()
        {
            if (productDTO.ProductImages is null || productDTO.ProductImages.Count == 0)
            {
                return;
            }

            var imageDTO = new ImageDTO
            {
                ProductId = ProductId,
                Images = productDTO.ProductImages!
            };

            var httpActionResponse = await Repository.PostAsync<ImageDTO, ImageDTO>("/api/products/addImages", imageDTO);
            if (httpActionResponse.Error)
            {
                var message = await httpActionResponse.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            productDTO.ProductImages = httpActionResponse.Response!.Images;
            var toast = SweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.BottomEnd,
                ShowConfirmButton = true,
                Timer = 3000
            });
            await toast.FireAsync(icon: SweetAlertIcon.Success, message: "Imagenes agregadas con éxito.");
        }

        private async Task RemoveImageAsyc()
        {
            if (productDTO.ProductImages is null || productDTO.ProductImages.Count == 0)
            {
                return;
            }

            var imageDTO = new ImageDTO
            {
                ProductId = ProductId,
                Images = productDTO.ProductImages!
            };

            var httpActionResponse = await Repository.PostAsync<ImageDTO, ImageDTO>("/api/products/removeLastImage", imageDTO);
            if (httpActionResponse.Error)
            {
                var message = await httpActionResponse.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            productDTO.ProductImages = httpActionResponse.Response!.Images;
            var toast = SweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.BottomEnd,
                ShowConfirmButton = true,
                Timer = 3000
            });
            await toast.FireAsync(icon: SweetAlertIcon.Success, message: "Imagén eliminada con éxito.");
        }

        private async Task LoadProductAsync()
        {
            loading = true;
            var httpActionResponse = await Repository.GetAsync<Product>($"/api/products/{ProductId}");

            if (httpActionResponse.Error)
            {
                loading = false;
                var message = await httpActionResponse.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            product = httpActionResponse.Response!;
            productDTO = ToProductDTO(product);
            loading = false;
        }

        private ProductDTO ToProductDTO(Product product)
        {
            return new ProductDTO
            {
                Description = product.Description,
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
                ProductCategoryIds = product.ProductCategories!.Select(x => x.CategoryId).ToList(),
                ProductImages = product.ProductImages!.Select(x => x.Image).ToList()
            };
        }

        private async Task LoadCategoriesAsync()
        {
            loading = true;
            var httpActionResponse = await Repository.GetAsync<List<Category>>("/api/categories/combo");

            if (httpActionResponse.Error)
            {
                loading = false;
                var message = await httpActionResponse.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            var categories = httpActionResponse.Response!;
            foreach (var category in categories!)
            {
                var found = product!.ProductCategories!.FirstOrDefault(x => x.CategoryId == category.Id);
                if (found == null)
                {
                    nonSelectedCategories.Add(category);
                }
                else
                {
                    selectedCategories.Add(category);
                }
            }
            loading = false;
        }

        private async Task SaveChangesAsync()
        {
            var httpActionResponse = await Repository.PutAsync("/api/products/full", productDTO);
            if (httpActionResponse.Error)
            {
                var message = await httpActionResponse.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            Return();
        }

        private void Return()
        {
            productForm!.FormPostedSuccessfully = true;
            NavigationManager.NavigateTo($"/products");
        }
    }
}
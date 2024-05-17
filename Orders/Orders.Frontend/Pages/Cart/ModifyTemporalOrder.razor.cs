using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Orders.Frontend.Repositories;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;

namespace Orders.Frontend.Pages.Cart
{
    [Authorize(Roles = "Admin, User")]
    public partial class ModifyTemporalOrder
    {
        private List<string>? categories;
        private List<string>? images;
        private bool loading = true;
        private Product? product;
        private TemporalOrderDTO? temporalOrderDTO;

        [Inject] private NavigationManager NavigationManager { get; set; } = null!;
        [Inject] private IRepository Repository { get; set; } = null!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;
        [Parameter] public int TemporalOrderId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadTemporalOrderAsync();
        }

        private async Task LoadTemporalOrderAsync()
        {
            loading = true;
            var httpResponse = await Repository.GetAsync<TemporalOrder>($"/api/temporalOrders/{TemporalOrderId}");

            if (httpResponse.Error)
            {
                loading = false;
                var message = await httpResponse.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            var temporalOrder = httpResponse.Response!;
            temporalOrderDTO = new TemporalOrderDTO
            {
                Id = temporalOrder.Id,
                ProductId = temporalOrder.ProductId,
                Remarks = temporalOrder.Remarks!,
                Quantity = temporalOrder.Quantity
            };
            product = temporalOrder.Product;
            categories = product!.ProductCategories!.Select(x => x.Category.Name).ToList();
            images = product.ProductImages!.Select(x => x.Image).ToList();
            loading = false;
        }

        public async Task UpdateCartAsync()
        {
            var httpResponse = await Repository.PutAsync("/api/temporalOrders/full", temporalOrderDTO);
            if (httpResponse.Error)
            {
                var message = await httpResponse.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            var toast2 = SweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.BottomEnd,
                ShowConfirmButton = true,
                Timer = 3000
            });
            await toast2.FireAsync(icon: SweetAlertIcon.Success, message: "Producto modificado en el de compras.");
            NavigationManager.NavigateTo("/");
        }
    }
}

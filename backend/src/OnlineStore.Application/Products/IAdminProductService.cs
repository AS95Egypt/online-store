namespace OnlineStore.Application.Products;

public interface IAdminProductService
{
    Task<PagedResult<AdminProductListItemDto>> GetPagedAsync(
        AdminProductListQuery query,
        CancellationToken cancellationToken);

    Task<AdminProductDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<AdminProductDetailsDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);
    Task<AdminProductDetailsDto?> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken);
    Task<AdminProductDetailsDto?> SetStatusAsync(int id, bool isActive, CancellationToken cancellationToken);
}

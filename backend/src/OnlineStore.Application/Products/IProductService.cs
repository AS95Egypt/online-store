namespace OnlineStore.Application.Products;

public interface IProductService
{
    Task<PagedResult<ProductListItemDto>> GetActiveProductsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<ProductDetailsDto?> GetActiveProductByIdAsync(
        int id,
        CancellationToken cancellationToken);
}

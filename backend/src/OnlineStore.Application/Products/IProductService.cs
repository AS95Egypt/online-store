namespace OnlineStore.Application.Products;

public interface IProductService
{
    Task<PagedResult<ProductListItemDto>> GetActiveProductsAsync(
        ProductListQuery query,
        CancellationToken cancellationToken);

    Task<ProductDetailsDto?> GetActiveProductByIdAsync(
        int id,
        CancellationToken cancellationToken);
}

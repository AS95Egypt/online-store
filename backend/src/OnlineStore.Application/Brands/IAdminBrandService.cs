namespace OnlineStore.Application.Brands;

public interface IAdminBrandService
{
    Task<IReadOnlyList<AdminBrandDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<AdminBrandDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<AdminBrandDto> CreateAsync(CreateBrandRequest request, CancellationToken cancellationToken);
    Task<AdminBrandDto?> UpdateAsync(int id, UpdateBrandRequest request, CancellationToken cancellationToken);
}

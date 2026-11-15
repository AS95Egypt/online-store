namespace OnlineStore.Application.Categories;

public interface IAdminCategoryService
{
    Task<IReadOnlyList<AdminCategoryDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<AdminCategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<AdminCategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken);
    Task<AdminCategoryDto?> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken);
    Task<AdminCategoryDto?> SetStatusAsync(int id, bool isActive, CancellationToken cancellationToken);
}

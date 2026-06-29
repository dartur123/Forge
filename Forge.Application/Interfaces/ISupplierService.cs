using Forge.Application.Requests;
using Forge.Application.Responses;

namespace Forge.Application.Interfaces
{
    public interface ISupplierService
    {
        Task<SupplierResult> GetSupplierAsync(int supplierId);
        Task<List<SupplierResult>> GetAllSuppliersAsync();
        Task<SupplierResult> CreateSupplierAsync(PostSupplierRequest request);
        Task<SupplierResult> UpdateSupplierAsync(int id, PostSupplierRequest request);
        Task DeactivateSupplierAsync(int supplierId);
    }
}

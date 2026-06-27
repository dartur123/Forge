using Forge.Application.Requests;
using Forge.Application.Responses;
using Forge.Domain.Enums;

namespace Forge.Application.Interfaces
{
    public interface IMaterialService
    {
        Task<MaterialResult> GetMaterialAsync(int materialId);
        Task<List<MaterialResult>> GetMaterialByTypeAsync(MaterialType materialType);
        Task<List<MaterialResult>> GetAllMaterialsAsync();
        Task<MaterialResult> CreateMaterialAsync(PostMaterialRequest request);
        Task<MaterialResult> UpdateMaterialAsync(int id, PostMaterialRequest request);
        Task DeactivateMaterialAsync(int materialId);
    }
}

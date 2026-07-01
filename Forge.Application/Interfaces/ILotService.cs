using Forge.Application.Requests;
using Forge.Application.Responses;

namespace Forge.Application.Interfaces
{
    public interface ILotService
    {
        Task<LotResult> CreateLotAsync(PostLotRequest request);
        Task<LotResult> UpdateLotAsync(int lotId, PostLotRequest request);
        Task<LotResult> GetLotByIdAsync(int lotId);
        Task<List<LotResult>> GetAllLotsAsync();
        Task DeactivateLotAsync(int lotId);
    }
}

using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IEncodingProfileRepository
    {
        Task SaveAsync(EncodingProfile data);
        Task<EncodingProfile?> GetByIdAsync(int id);
        Task<List<EncodingProfile>> GetAllAsync(int pageNumber, int pageSize);
        Task<int> GetTotalCountAsync();
    }

}
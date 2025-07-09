// Services/IEncodingProfileService.cs


using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IEncodingProfileService
    {
        Task<EncodingProfile> CreateProfileAsync(EncodingProfile profile);
        Task<(List<EncodingProfile> Profiles, int TotalCount)> GetAllProfilesAsync(int pageNumber, int pageSize);

    }
}

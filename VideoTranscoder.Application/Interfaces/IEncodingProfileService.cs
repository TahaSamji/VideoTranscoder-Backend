// Services/IEncodingProfileService.cs

using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IEncodingProfileService
    {
        Task<EncodingProfile> CreateProfileAsync(EncodingProfile profile); 
        // Creates a new encoding profile and returns the created entity

        Task<(List<EncodingProfile> Profiles, int TotalCount)> GetAllProfilesAsync(int pageNumber, int pageSize); 
        // Retrieves a paginated list of encoding profiles along with total count for UI pagination

        Task<EncodingProfile?> UpdateProfileAsync(int id, EncodingProfile updatedProfile); 
        // Updates an existing encoding profile by ID and returns the updated entity, or null if not found

        Task<bool> DeleteProfileAsync(int id); 
        // Deletes an encoding profile by ID and returns true if successful
    }
}

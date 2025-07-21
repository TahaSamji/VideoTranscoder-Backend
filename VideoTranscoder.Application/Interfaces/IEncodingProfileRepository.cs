using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IEncodingProfileRepository
    {
        Task SaveAsync(EncodingProfile data); // Save a new or updated encoding profile to the database

        Task<EncodingProfile?> GetByIdAsync(int id); // Retrieve a single encoding profile by its ID

        Task<List<EncodingProfile>> GetAllAsync(int pageNumber, int pageSize); // Get a paginated list of all encoding profiles

        Task<int> GetTotalCountAsync(); // Get total number of encoding profiles 

        Task<List<EncodingProfile>> GetSelectedProfilesUpToHeightAsync(int maxHeight); // Get all profiles with resolution height <= specified max

        Task<bool> DeleteAsync(int id); // Soft Delete an encoding profile by ID, returns true if successful

        // Task UpdateAdminSelectionAsync(int profileId, bool isSelected);
        Task UpdateAdminSelectionAsync(int profileId, bool isSelected);
    }
}

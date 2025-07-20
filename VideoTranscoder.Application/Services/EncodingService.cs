using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Services
{
    public class EncodingProfileService : IEncodingProfileService
    {
        private readonly IEncodingProfileRepository _repository;

        // Inject the encoding profile repository
        public EncodingProfileService(IEncodingProfileRepository repository)
        {
            _repository = repository;
        }

        // Creates a new encoding profile after parsing resolution
        public async Task<EncodingProfile> CreateProfileAsync(EncodingProfile profile)
        {
            // Extract width and height from resolution string (e.g., "1920x1080")
            var resolutionParts = profile.Resolution.Split('x');

            // Validate the format and parse width and height
            if (resolutionParts.Length != 2 ||
                !int.TryParse(resolutionParts[0], out int width) ||
                !int.TryParse(resolutionParts[1], out int height))
            {
                throw new ArgumentException("❌ Invalid resolution format. Expected format: 'WIDTHxHEIGHT'");
            }

            // Create a new encoding profile entity with parsed dimensions
            var entity = new EncodingProfile
            {
                Name = profile.Name,
                FfmpegArgs = profile.FfmpegArgs,
                Resolution = profile.Resolution,
                Bitrate = profile.Bitrate,
                FormatType = profile.FormatType,
                CreatedAt = profile.CreatedAt,
                Width = width,
                Height = height,
                BrowserType = profile.BrowserType

            };

            // Save the new profile to the database
            await _repository.SaveAsync(entity);

            return entity;
        }

        //Soft Delete the encoding Profile with the given id
        public async Task<bool> DeleteProfileAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return false;

            await _repository.DeleteAsync(id);
            return true;
        }
        // Update the encoding Profile
        public async Task<EncodingProfile?> UpdateProfileAsync(int id, EncodingProfile updatedProfile)
        {
            if (id != updatedProfile.Id)
                return null;

            await _repository.SaveAsync(updatedProfile);

            return updatedProfile;
        }

        // Retrieves paginated list of encoding profiles along with total count
        public async Task<(List<EncodingProfile> Profiles, int TotalCount)> GetAllProfilesAsync(int pageNumber, int pageSize)
        {
            var profiles = await _repository.GetAllAsync(pageNumber, pageSize); // Fetch paginated profiles
            var totalCount = await _repository.GetTotalCountAsync();            // Fetch total profile count

            return (profiles, totalCount); // Return both in a tuple
        }
    }
}

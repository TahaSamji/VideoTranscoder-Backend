
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Services
{
    public class EncodingProfileService : IEncodingProfileService
    {
        private readonly IEncodingProfileRepository _repository;

        public EncodingProfileService(IEncodingProfileRepository repository)
        {
            _repository = repository;
        }

        public async Task<EncodingProfile> CreateProfileAsync(EncodingProfile profile)
        {
            var entity = new EncodingProfile
            {
                Name = profile.Name,
                FfmpegArgs = profile.FfmpegArgs,
                Resolution = profile.Resolution,
                Bitrate = profile.Bitrate,
                FormatType = profile.FormatType,
                CreatedAt = profile.CreatedAt
            };

            await _repository.SaveAsync(entity);
            return entity;
        }
        public async Task<(List<EncodingProfile> Profiles, int TotalCount)> GetAllProfilesAsync(int pageNumber, int pageSize)
        {
            var profiles = await _repository.GetAllAsync(pageNumber, pageSize);
            var totalCount = await _repository.GetTotalCountAsync();
            return (profiles, totalCount);
        }
    }
}

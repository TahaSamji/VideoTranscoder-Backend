
using VideoTranscoder.VideoTranscoder.Domain.Entities;
using VideoTranscoder.VideoTranscoder.Application.DTOs;
using AutoMapper;

public class ThumbnailMapper : Profile
{
    public ThumbnailMapper()
    {
        CreateMap<Thumbnail, ThumbnailDto>();
    }
}

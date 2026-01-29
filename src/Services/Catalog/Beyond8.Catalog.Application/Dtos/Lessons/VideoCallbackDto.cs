using System;
using Beyond8.Catalog.Domain.JSONFields;

namespace Beyond8.Catalog.Application.Dtos.Lessons;

public class VideoCallbackDto
{
    public string OriginalKey { get; set; } = null!;
    public TranscodingsRequest TranscodingData { get; set; } = null!;
}

public class TranscodingsRequest
{
    public List<Variants> Variants { get; set; } = [];
}

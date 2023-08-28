using Newtonsoft.Json;

namespace ClipsService.Dtos;
public class UpdateClipUriRequestDto
{
    [JsonProperty("uri")]
    public Uri? Uri { get; set; }

    [JsonProperty("converted")]
    public bool Converted { get; set; }
}

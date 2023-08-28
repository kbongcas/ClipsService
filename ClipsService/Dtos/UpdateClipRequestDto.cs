using Newtonsoft.Json;

namespace ClipsService.Dtos;
public class UpdateClipRequestDto
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("uri")]
    public Uri? Uri { get; set; }

    [JsonProperty("converted")]
    public bool Converted { get; set; }
}

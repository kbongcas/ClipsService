using Newtonsoft.Json;

namespace ClipsService.Dtos;

public class UpdateMyClipRequestDto
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }
}

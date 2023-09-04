using Newtonsoft.Json;

namespace ClipsService.Dtos;
public class UpdateClipRequestDto
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("public")]
    public bool Public { get; set; }
}

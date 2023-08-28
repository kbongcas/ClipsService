using Newtonsoft.Json;
using System.Text.Json;

namespace ClipsService.Dtos;

public class AddClipRequestDto
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("uri")]
    public string Uri { get; set; }

    [JsonProperty("converted")]
    public bool Converted { get; set; }
}

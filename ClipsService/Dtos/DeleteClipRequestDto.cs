using Newtonsoft.Json;

namespace ClipsService.Dtos;
public class DeleteClipRequestDto
{
    [JsonProperty("id")]
    public string Name { get; set; }
}

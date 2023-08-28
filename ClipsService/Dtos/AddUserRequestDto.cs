using Newtonsoft.Json;

namespace ClipsService.Dtos;
public class AddUserRequestDto
{
    [JsonProperty("id")]
    public string Id { get; set; }
}

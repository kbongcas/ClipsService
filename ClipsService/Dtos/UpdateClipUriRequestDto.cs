using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ClipsService.Dtos;
public class UpdateClipUriRequestDto
{
    [Required]
    [JsonProperty("uri")]
    public string Uri { get; set; }

    [Required]
    [JsonProperty("converted")]
    public bool Converted { get; set; }
}

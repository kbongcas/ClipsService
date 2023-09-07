using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace ClipsService.Dtos;

public class AddClipRequestDto
{
    [Required, StringLength(60, MinimumLength = 1)]
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    [StringLength(280)]
    public string? Description { get; set; }

    [JsonProperty("uri")]
    public string? Uri { get; set; }

    [JsonProperty("converted")]
    public bool Converted { get; set; }

    [JsonProperty("Public")]
    public bool Public { get; set; }
}

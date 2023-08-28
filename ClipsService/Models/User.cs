using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClipsService.Models;
public class User
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("clips")]
    public List<Clip> Clips { get; set; }
}

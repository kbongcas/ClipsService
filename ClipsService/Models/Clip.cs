
using Newtonsoft.Json;
using System;
using System.Text.Json;

namespace ClipsService.Models;

public class Clip
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("dateCreated")]
    public DateTime DateCreated { get; set; }

    [JsonProperty("dateModified")]
    public DateTime DateModified { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("uri")]
    public string Uri { get; set; }

    [JsonProperty("converted")]
    public bool Converted { get; set; }

    [JsonProperty("public")]
    public bool Public { get; set; }

    [JsonProperty("userId")]
    public string UserId { get; set; }

    public Clip()
    {
        DateModified = DateTime.Now;
        DateCreated = DateTime.Now;
    }
}

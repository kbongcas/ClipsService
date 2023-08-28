
using Newtonsoft.Json;
using System;
using System.Text.Json;

namespace ClipsService.Models;

public class Clip
{
    [JsonProperty("id")]
    public string Id { get;}

    [JsonProperty("name")]
    public string Name {  get;}

    [JsonProperty("description")]
    public string Description {  get;}

    [JsonProperty("uri")]
    public Uri? Uri { get; }

    [JsonProperty("converted")]
    public bool Converted { get; }

    public Clip(string id, string name, string description, Uri? uri, bool converted)
    {
        if (String.IsNullOrEmpty(name)) throw new ArgumentException("Name must not be empty");
        Id = id;
        Name = name;
        Description = description;
        Uri = uri;
        Converted = converted;
    }
}

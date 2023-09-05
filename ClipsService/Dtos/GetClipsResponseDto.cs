using ClipsService.Models;

namespace ClipsService.Dtos;

public class GetClipsResponseDto
{
    public int Count { get; set; }
    public List<Clip> Clips { get; set; }
}

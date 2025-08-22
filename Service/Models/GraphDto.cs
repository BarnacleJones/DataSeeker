namespace Service.Models;

public class GraphDto
{
    public List<NodeDto> Nodes { get; set; } = new();
    public List<LinkDto> Links { get; set; } = new();
}

public class NodeDto
{
    public string Id { get; set; } = string.Empty;
    public string? Label { get; set; }
}

public class LinkDto
{
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public int Value { get; set; } = 1;
}
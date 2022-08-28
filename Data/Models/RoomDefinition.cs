using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

public class RoomLink
{
    private Regex _matchPatternExpression;
    [JsonIgnore] public Regex MatchPatternExpression => _matchPatternExpression = _matchPatternExpression ?? new Regex(MatchPattern);
    public string MatchPattern { get; set; }
    public string UniqueId { get; set; }
    public string LinkedRoomId { get; set; }
    public string DisplayName { get; set; }
    public string FullDescription { get; set; }
    public string ShortDescription { get; set; }
}
public class RoomDefinition
{
    public string UniqueId { get; set; }
    public string DisplayName { get; set; }
    public string ShortDescription { get; set; }
    public string FullDescription { get; set; }
    public IList<string> Aliases { get; set; } = new List<string>();
    public IList<RoomLink> RoomLinks { get; set; } = new List<RoomLink>();
}
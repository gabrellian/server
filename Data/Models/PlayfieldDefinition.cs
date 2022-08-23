using System.Text.Json.Serialization;

public class PlayfieldDefinition
{
    public string UniqueId { get; set; }
    public string DisplayName { get; set; }
    public IList<RoomDefinition> Rooms { get; set; } = new List<RoomDefinition>();

}
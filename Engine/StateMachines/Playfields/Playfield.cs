using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Data.Models;
using Engine.Net;
using Engine.Utils;

public interface IPlayfieldService
{
    Task Initialize();
    Task<(Playfield Playfield, Room Room)> GetRoom(string path);
    IReadOnlyList<GameSession> Players { get; }
}
public class PlayfieldService : IPlayfieldService
{
    private List<Playfield> _playfields = new List<Playfield>();
    private Task _eventLoopTask;

    public IReadOnlyList<GameSession> Players
    {
        get
        {
            var players = new List<GameSession>();
            foreach (var pf in _playfields)
            {
                foreach (var rm in pf.Rooms)
                {
                    players.AddRange(rm.Players);
                }
            }
            return players;
        }
    }

    public PlayfieldService()
    {
        _eventLoopTask = new Task(async () => await EventLoop(), TaskCreationOptions.LongRunning);
    }

    public async Task EventLoop()
    {
        var sw = new Stopwatch();
        sw.Start();

        while (true)
        {
            foreach (var pf in _playfields)
            {
                await pf.OnTick(sw.Elapsed);
                sw.Restart();
            }
        }
    }

    public Task<(Playfield Playfield, Room Room)> GetRoom(string path)
    {
        string pfId = path.Split('/').Skip(1).FirstOrDefault();
        string rmId = path.Split('/').Skip(2).FirstOrDefault();

        foreach (var p in _playfields)
        {
            if (p.UniqueId.Equals(pfId, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var r in p.Rooms)
                {
                    if (r.UniqueId.Equals(rmId, StringComparison.OrdinalIgnoreCase))
                    {
                        return Task.FromResult((p, r));
                    }
                }
            }
        }
        throw new Exception($"Room '{path}' not found.");
    }

    // Load up playfields
    public async Task Initialize()
    {
        // TODO: Load from configuration the playfields that will be initially loaded
        var tempPF = @"
        {
            ""UniqueId"": ""albrecht"",
            ""DisplayName"": ""The Albrecht"",
            ""Rooms"": [
                {
                    ""UniqueId"": ""apartment-bedroom"",
                    ""DisplayName"": ""Apartment - Bedroom"",
                    ""ShortDescription"": ""A dissheveled bedroom"",
                    ""FullDescription"": ""A dimly lit dissheveled bedroom about 8 feet by 10 feet in size. The walls stand warily plastered with a vintage art deco wallpaper that's begun to peel along the walls edges. The full sized bed in the center of the room is unmade and looks to have been slept in. The green shag carpet is stained and worn from what one would assume is many years of use and abuse. A room lamp is placed on an ornate but worn wooden end table, casting dim shadows across the room."",
                    ""Aliases"": [
                        ""bedroom""
                    ],
                    ""RoomLinks"": [
                        {
                            ""UniqueId"": ""exit"",
                            ""LinkedRoomId"": ""/albrecht/apartment-livingroom"",
                            ""DisplayName"": ""Door"",
                            ""ShortDescription"": ""A standard door"",
                            ""MatchPattern"": ""(leave|exit|x|w)$""
                        }
                    ]
                },
                {
                    ""UniqueId"": ""apartment-livingroom"",
                    ""DisplayName"": ""Apartment - Main Room"",
                    ""ShortDescription"": ""An ordinary living room and kitchenette."",
                    ""FullDescription"": ""An ordinary living room and kitchenette. A worn leather couch is placed in the center of the living room, stained and misshapen from years of use. Along the rear wall a computer terminal sits with it's dim green terminal screen illuminating the room. The kitchenette is dated with old and rusted appliances, not often used and even less often cleaned."",
                    ""Aliases"": [
                        ""living room"",
                        ""kitchen""
                    ],
                    ""RoomLinks"": [
                        {
                            ""UniqueId"": ""bedroom-door"",
                            ""LinkedRoomId"": ""/albrecht/apartment-bedroom"",
                            ""DisplayName"": ""Door"",
                            ""ShortDescription"": ""A standard door"",
                            ""MatchPattern"": ""(east|e)$""
                        }
                    ]
                }
            ]    
        }
        ";

        _playfields.Add(JsonSerializer.Deserialize<Playfield>(tempPF));

        await Task.CompletedTask;
    }

}
public class Playfield
{
    #region Public Properties
    [JsonInclude] public string UniqueId { get; private set; }
    [JsonInclude] public string DisplayName { get; private set; }
    [JsonInclude] public IEnumerable<Room> Rooms { get; private set; } = new List<Room>();
    #endregion

    #region Private/Protected Properties
    protected List<IStatefulContext> _contexts = new List<IStatefulContext>();
    #endregion

    #region Public Public Interface
    public Playfield() { }

    public virtual async Task OnCommand(string rawCommand)
    {
        // Pass commands onto PF/Room specific logic and 
        foreach (var ctx in _contexts) await ctx.OnCommand(rawCommand);
        foreach (var room in Rooms) await room.OnCommand(rawCommand);
    }
    public virtual async Task OnTick(TimeSpan delta)
    {
        // Pass commands onto PF/Room specific logic and 
        foreach (var ctx in _contexts) await ctx.OnTick(delta);
        foreach (var room in Rooms) await room.OnTick(delta);
    }
    #endregion
}
public class RoomLink
{
    [JsonInclude] public string MatchPattern { get; private set; }
    [JsonInclude] public string UniqueId { get; private set; }
    [JsonInclude] public string LinkedRoomId { get; private set; }
    [JsonInclude] public string DisplayName { get; private set; }
    [JsonInclude] public string FullDescription { get; private set; }
    [JsonInclude] public string ShortDescription { get; private set; }
}
public class Room
{
    [JsonInclude] public string UniqueId { get; private set; }
    [JsonInclude] public string DisplayName { get; private set; }
    [JsonInclude] public string ShortDescription { get; private set; }
    [JsonInclude] public string FullDescription { get; private set; }
    [JsonInclude] public IEnumerable<string> Aliases { get; private set; } = new List<string>();
    [JsonInclude] public IEnumerable<RoomLink> RoomLinks { get; private set; } = new List<RoomLink>();

    [JsonIgnore] private List<GameSession> _players = new List<GameSession>();
    [JsonIgnore] public IReadOnlyList<GameSession> Players => _players;

    protected List<IStatefulContext> _contexts = new List<IStatefulContext>();

    public Room() { }

    public virtual async Task OnCommand(string rawCommand)
    {
        // Pass commands onto PF/Room specific logic and 
        foreach (var ctx in _contexts) await ctx.OnCommand(rawCommand);
    }

    public virtual async Task OnTick(TimeSpan delta)
    {
        // Pass commands onto PF/Room specific logic and 
        foreach (var ctx in _contexts) await ctx.OnTick(delta);
        foreach (var player in _players) await player.OnTick(delta);
    }

    public async Task AddPlayer(GameSession pc)
    {
        _players.Add(pc);

        pc.SendLine(FullDescription);

    }
}
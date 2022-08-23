using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Data.Models;
using Engine.Net;
using Engine.Utils;

public interface IPlayfieldService
{
    Task Initialize();
    Task<(PlayfieldInstance Playfield, RoomInstance Room)> GetRoom(string path);
    IReadOnlyList<GameSession> Players { get; }
}
public class PlayfieldService : IPlayfieldService
{
    private List<PlayfieldInstance> _playfields = new List<PlayfieldInstance>();
    private IPlayfieldDefinitionRepo _pfRepo;
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

    public PlayfieldService(IPlayfieldDefinitionRepo pfRepo)
    {
        _pfRepo = pfRepo;
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

    public Task<(PlayfieldInstance Playfield, RoomInstance Room)> GetRoom(string path)
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

        var def = JsonSerializer.Deserialize<PlayfieldDefinition>(tempPF);
        await _pfRepo.SavePlayfieldDefinition(def);
        _playfields.Add(new PlayfieldInstance(def));

        await Task.CompletedTask;
    }

}
public class PlayfieldInstance
{
    #region Public Properties
    public string UniqueId { get; private set; }
    public string DisplayName { get; private set; }
    public IEnumerable<RoomInstance> Rooms { get; private set; } = new List<RoomInstance>();
    #endregion

    #region Private/Protected Properties
    protected List<IStatefulContext> _contexts = new List<IStatefulContext>();
    #endregion

    #region Public Public Interface
    public PlayfieldInstance(PlayfieldDefinition def)
    {
        DisplayName = def.DisplayName;
        UniqueId = def.UniqueId;
        Rooms = def.Rooms.Select(rd => new RoomInstance(rd)).ToList();
    }

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
public class RoomInstance
{
    public string UniqueId { get; private set; }
    public string DisplayName { get; private set; }
    public string ShortDescription { get; private set; }
    public string FullDescription { get; private set; }
    public IEnumerable<string> Aliases { get; private set; } = new List<string>();
    public IEnumerable<RoomLink> RoomLinks { get; private set; } = new List<RoomLink>();

    private List<GameSession> _players = new List<GameSession>();
    public IReadOnlyList<GameSession> Players => _players;

    protected List<IStatefulContext> _contexts = new List<IStatefulContext>();

    public RoomInstance() { }
    public RoomInstance(RoomDefinition def)
    {
        this.Aliases = def.Aliases;
        this.DisplayName = def.DisplayName;
        this.FullDescription = def.FullDescription;
        this.ShortDescription = def.ShortDescription;
        this.RoomLinks = def.RoomLinks;
        this.UniqueId = def.UniqueId;
    }

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

        pc.SendLook();
    }
    public async Task RemovePlayer(GameSession pc)
    {
        _players.Remove(pc);
    }
}
namespace Data.Models;

public class PlayerCharacter
{
    public string Id { get; set; }
    public string Nickname { get; set; }

    public StatBlock Stats {get;set;} = new StatBlock();
}

public class StatBlock {
    
}

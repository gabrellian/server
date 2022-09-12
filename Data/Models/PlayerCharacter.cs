namespace Data.Models;

public class PlayerCharacter
{
    public string Id { get; set; }
    public string Nickname { get; set; }

    public StatBlock Stats { get; set; } = new StatBlock();
}

public class StatBlock
{
    protected Dictionary<string, object> _values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    public StatBlock() { }
    public T Get<T>(string stat, object defaultValue = null) => (T)(_values.ContainsKey(stat) ? _values[stat] : _values[stat] = defaultValue);
    public void Set<T>(string stat, object value = null)
    {
        if (_values.ContainsKey(stat))
        {
            _values[stat] = value;
        }
        else
        {
            _values.Add(stat, value);
        }
    }
}

public class StatDescriptionAttribute : Attribute
{
    public string Description { get; set; }
    public bool IsHidden { get; set; }
    public StatDescriptionAttribute(string description = "", bool isHidden = false)
    {
        Description = description;
        IsHidden = isHidden;
    }
}
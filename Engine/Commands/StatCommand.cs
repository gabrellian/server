
using Data.Models;
using Engine.Commands;
using Engine.Extendable;
using Spectre.Console;
using System.Text.Json;

public class StatCommand : BaseCommand, IStatCommand
{
    private JsonSerializerOptions _jsonOptions;

    public string Stat { get; set; }
    public StatCommand(JsonSerializerOptions jsonOptions)
    {
        _jsonOptions = jsonOptions;
    }

    public override async Task Handle()
    {
        var stats = Session.CurrentPlayer.Stats;
        var props = stats.GetType().GetProperties();
        if (string.IsNullOrEmpty(Stat))
        {
            var tbl = new Table();
            tbl.AddColumn("Stat");
            tbl.AddColumn("Value");
            tbl.AddColumn("Description");
            foreach (var prop in props)
            {
                var statDesc = prop.GetCustomAttributes(typeof(StatDescriptionAttribute), false).FirstOrDefault() as StatDescriptionAttribute;
                if (statDesc != null && !statDesc.IsHidden)
                {
                    tbl.AddRow(new[] {
                        new Markup(prop.Name),
                        new Markup(prop.GetValue(stats).ToString()),
                        new Markup(statDesc.Description ?? "") });
                }
                else if (statDesc == null)
                {
                    tbl.AddRow(new[] {
                        new Markup(prop.Name),
                        new Markup(prop.GetValue(stats).ToString()),
                        new Markup("-") });
                }
            }
            Session.SendLine(tbl);
        }
        else
        {
            var prop = props.FirstOrDefault(p => p.Name.Equals(Stat, StringComparison.OrdinalIgnoreCase));
            var statDesc = prop.GetCustomAttributes(typeof(StatDescriptionAttribute), false).FirstOrDefault();

            if (prop == null)
            {
                Session.SendLine($"Unknown stat '{Stat}'");
                return;
            }
            Session.SendLine($"{prop.Name}: {prop.GetValue(stats).ToString()}");
        }
    }
}
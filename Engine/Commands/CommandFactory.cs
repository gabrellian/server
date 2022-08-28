using System.Text.RegularExpressions;
using Engine.Net;
using Microsoft.Extensions.DependencyInjection;

namespace Engine.Commands;

public interface ICommandFactory
{
    Task<bool> Match(string commandString, GameSession session);
}
public class CommandFactory : ICommandFactory
{
    private IServiceProvider _services;

    public CommandFactory(IServiceProvider services)
    {
        _services = services;
    }
    public virtual Dictionary<Regex, Type> Matchers { get; } = new Dictionary<Regex, Type>()
    {
        { new Regex(@"^who$"), typeof(WhoCommand) },
        { new Regex(@"^(look|l)$"), typeof(LookCommand) },
        { new Regex(@"^(help|h)"), typeof(HelpCommand) },
        { new Regex(@"^(say|sl|global|sg)"), typeof(SayCommand) },
        { new Regex(@"^(character|c)$"), typeof(CharacterCommand) }
    };

    public async Task<bool> Match(string commandString, GameSession session)
    {
        if (session.CurrentRoom != null)
        {
            foreach (var roomLink in session.CurrentRoom.RoomLinks)
            {
                if (roomLink.MatchPatternExpression.IsMatch(commandString))
                {
                    await session.ChangeRoom(roomLink.LinkedRoomId);
                    return true;
                }
            }
        }
        foreach (var matcher in Matchers)
        {
            var commandType = matcher.Value;
            var commandPattern = matcher.Key;

            var matches = commandPattern.Matches(commandString);
            if (matches.Count > 0)
            {
                var command = _services.GetService(commandType) as BaseCommand;
                command.RawCommand = commandString;
                command.Session = session;

                foreach (Match match in matches)
                {
                    foreach (Group group in match.Groups)
                    {
                        if (!int.TryParse(group.Name, out _))
                        {
                            var prop = command.GetType().GetProperty(group.Name);
                            if (prop.PropertyType == typeof(string))
                            {
                                prop.SetValue(command, group.Value);
                            }
                            if (prop.PropertyType == typeof(int))
                            {
                                prop.SetValue(command, Convert.ToInt32(group.Value));
                            }
                        }
                    }
                }

                await command.Handle();
                return true;
            }
        }
        return false;
    }
}

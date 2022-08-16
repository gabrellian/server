using Engine.Net;

namespace Engine.Commands;

public abstract class BaseCommand
{
    public GameSession Session { get; set; }
    public virtual Task Handle() => throw new NotImplementedException();
}

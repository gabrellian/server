using ConsoleHost.Net;
using Microsoft.Extensions.Configuration;

namespace ConsoleHost.Utils;

public interface IState
{
    Task<IState> OnCommand(string rawCommand);
}
public class BaseState : IState
{
    protected GameSession _session;

    public virtual Task<IState> OnCommand(string rawCommand) => Task.FromResult(this as IState);
    public BaseState(GameSession session) => _session = session;
}

public class StatefulContext
{
    protected IState _state;
    private GameSession _session;

    public IState CurrentState => _state;

    public void SetState(IState state) => _state = state;

    public StatefulContext(GameSession session)
    {
        _session = session;
    }
}

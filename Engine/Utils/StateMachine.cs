using Engine.Net;
using Microsoft.Extensions.Configuration;

namespace Engine.Utils;

public interface IState
{
    Task<IState> OnCommand(string rawCommand);
    Task<IState> OnTick(TimeSpan delta);
}
public class BaseState : IState
{
    protected GameSession _session;

    public virtual Task<IState> OnCommand(string rawCommand) => Task.FromResult(this as IState);
    public virtual Task<IState> OnTick(TimeSpan delta) => Task.FromResult(this as IState);
    public BaseState(GameSession session) => _session = session;
}
public interface IStatefulContext
{
    Task OnCommand(string rawCommand);
    Task OnTick(TimeSpan delta);
}
public class StatefulContext : IStatefulContext
{
    protected IState _state;
    private GameSession _session;

    public IState CurrentState => _state;

    public void SetState(IState state) => _state = state;

    public StatefulContext(GameSession session)
    {
        _session = session;
    }

    public virtual async Task OnCommand(string rawCommand) => SetState(await CurrentState.OnCommand(rawCommand));
    public virtual async Task OnTick(TimeSpan delta) => SetState(await CurrentState.OnTick(delta));
}

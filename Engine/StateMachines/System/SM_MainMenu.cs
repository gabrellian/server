using System.Text.RegularExpressions;
using Engine.Net;
using Engine.Core;
using Data;
using Data.Models;
using Spectre.Console;
using Microsoft.Extensions.Configuration;

namespace Engine.StateMachines;

public interface IMainMenu : IStatefulContext { }
public class SM_MainMenu : StatefulContext, IMainMenu, IStatefulContext
{
    private IPlayfieldService _playfields;

    public SM_MainMenu(GameSession session, IPlayfieldService playfields, IConfiguration config) : base(session)
    {
        _playfields = playfields;
        _state = new Unauthenticated(config, session);
    }

    // Initial state - not logged into a specific character
    private class Unauthenticated : BaseState
    {
        private Regex _matchLogin = new Regex(@"login$");
        private Regex _matchExit = new Regex(@"exit$");
        private Regex _matchCreate = new Regex(@"(new|create|start)$");

        public Unauthenticated(IConfiguration config, GameSession session) : base(session)
        {
            session.SendLine(new FigletText(config.GetValue<string>("ServerDisplayName", "Welcome")).ToAnsi());
            session.SendLine(
                new Table().HideHeaders().AddColumns("", "Description")
                .AddRow("login", "Log into an existing character")
                .AddRow("create", "Create a new character")
                .AddRow("exit", "Create a new character")
                .ToAnsi()
            );
            session.Send($"Enter a command to get started: ");
        }

        public async override Task<IState> OnCommand(string rawCommand)
        {
            if (_matchLogin.IsMatch(rawCommand))
            {
                return await Task.FromResult(new LoggingIn(_session));
            }
            else if (_matchCreate.IsMatch(rawCommand))
            {
                return await Task.FromResult(new Creating_Name(_session, new PlayerCharacter()) as IState);
            }
            else if (_matchExit.IsMatch(rawCommand))
            {
                _session.Disconnect();
            }
            return await Task.FromResult(this as IState);
        }
    }

    // Logged into a character
    private class Authenticated : BaseState
    {
        private PlayerCharacter _pc;
        private IPlayfieldService _playfields;

        public Authenticated(GameSession session, IPlayfieldService playfields, PlayerCharacter pc) : base(session)
        {
            _pc = pc;
            _playfields = playfields;
            _session.AttachPlayer(_pc);
            session.SendLine($"Hello '{_pc.Nickname}', you are now signed in..");
            _session.ChangeRoom("/albrecht/apartment-bedroom").Wait();
        }
        
        public async override Task<IState> OnCommand(string rawCommand)
        {
            if (rawCommand.Equals("logout", StringComparison.OrdinalIgnoreCase))
            {
                _session.SendLine($"Goodbye!");

                await _session.DetachPlayer();

                return new Unauthenticated(_session.GetService<IConfiguration>(), _session);
            }
            return this;
        }
    }

    // Logging into a character
    private class LoggingIn : BaseState
    {
        private PlayerCharacter _pc;

        public LoggingIn(GameSession session) : base(session)
        {
            SendPrompt();
        }

        private void SendPrompt() => _session.Send("What is your characters name? ");


        public async override Task<IState> OnCommand(string rawCommand)
        {
            var pcRepo = _session.GetService<IPlayerCharacterRepo>();
            var sessions = _session.GetService<GameSessions>();

            if (rawCommand.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                return new Unauthenticated(_session.GetService<IConfiguration>(), _session);
            }
            else
            {
                var pc = await pcRepo.GetPlayer(rawCommand);

                if (pc == null)
                {
                    _session.SendLine($"Unknown character '{rawCommand}'");
                    SendPrompt();
                    return this;
                }

                if (sessions.ActiveSessions.Any(s => s.CurrentPlayer.Nickname.Equals(rawCommand, StringComparison.OrdinalIgnoreCase)))
                {
                    _session.SendLine($"Already signed in to '{rawCommand}'");
                    return new Unauthenticated(_session.GetService<IConfiguration>(), _session);
                }

                return new Authenticated(_session, _session.GetService<IPlayfieldService>(), pc);
            }
        }
    }

    // Character ancestry selection prompt
    //private class Creating_AncestrySelection : BaseState { }

    // Character class selection prompt
    //private class Creating_ClassSelection : BaseState { }

    // Character naming
    private class Creating_Name : BaseState
    {
        private PlayerCharacter _pc;
        private void SendPrompt() => _session.Send($"What is your characters name? ");
        public Creating_Name(GameSession session, PlayerCharacter newPc) : base(session)
        {
            _pc = newPc;
            SendPrompt();
        }
        public override async Task<IState> OnCommand(string rawCommand)
        {
            // Check to see if name is taken .. if not then proceed
            var pcRepo = _session.GetService<IPlayerCharacterRepo>();
            if (rawCommand.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                return new Unauthenticated(_session.GetService<IConfiguration>(), _session);
            }
            else if (!await pcRepo.IsValidPlayerName(rawCommand))
            {
                _session.SendLine($"The name '{rawCommand}' is not a valid name.");
                SendPrompt();
                return this;
            }

            _pc.Nickname = rawCommand;

            _pc = await pcRepo.SavePlayer(_pc);

            return new Authenticated(_session, _session.GetService<IPlayfieldService>(), _pc);
        }
    }

    // Character email
    //private class Creating_Email : BaseState { }

    // Character password
    //private class Creating_Password : BaseState { }
}

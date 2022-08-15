using System.Text.RegularExpressions;
using ConsoleHost.Net;
using ConsoleHost.Utils;
using Data;
using Data.Models;

namespace ConsoleHost.StateMachines;

public class SM_MainMenu : StatefulContext
{
    public SM_MainMenu(GameSession session) : base(session)
    {
        _state = new Unauthenticated(session);
    }

    // Initial state - not logged into a specific character
    private class Unauthenticated : BaseState
    {
        private Regex _matchLogin = new Regex(@"login$");
        private Regex _matchExit = new Regex(@"exit$");
        private Regex _matchCreate = new Regex(@"(new|create|start)$");

        public Unauthenticated(GameSession session) : base(session)
        {
            session.SendLine("");
            session.SendLine($"Welcome!");
            session.SendLine($"--------");
            session.SendLine($"login    Log into an existing character");
            session.SendLine($"create   Log into an existing character");
            session.SendLine($"exit     Disconnect");
            session.SendLine($"");
            session.Send($"Enter a command to get started: ");
        }

        public async override Task<IState> OnCommand(string rawCommand)
        {
            if (_matchLogin.IsMatch(rawCommand))
            {
                return new LoggingIn(_session);
            }
            else if (_matchCreate.IsMatch(rawCommand))
            {
                return new Creating_Name(_session, new PlayerCharacter()) as IState;
            }
            else if (_matchExit.IsMatch(rawCommand))
            {
                _session.Disconnect();
            }
            return this as IState;
        }
    }

    // Logged into a character
    private class Authenticated : BaseState
    {
        private PlayerCharacter _pc;

        public Authenticated(GameSession session, PlayerCharacter pc) : base(session)
        {
            _pc = pc;
            _session.AttachPlayer(_pc);
            session.SendLine($"Hello '{_pc.Nickname}', you are now signed in..");
        }
        public async override Task<IState> OnCommand(string rawCommand)
        {
            if (rawCommand.Equals("logout", StringComparison.OrdinalIgnoreCase))
            {
                _session.SendLine($"Goodbye!");

                _session.DetachPlayer();

                return new Unauthenticated(_session);
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

            var pc = await pcRepo.GetPlayer(rawCommand);

            if (pc == null)
            {
                _session.SendLine($"Unknown character '{rawCommand}'");
                SendPrompt();
                return this;
            }

            return new Authenticated(_session, pc);
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

            if (!await pcRepo.IsValidPlayerName(rawCommand))
            {
                _session.SendLine($"The name '{rawCommand}' is not a valid name.");
                SendPrompt();
                return this;
            }

            _pc.Nickname = rawCommand;

            _pc = await pcRepo.SavePlayer(_pc);

            return new Authenticated(_session, _pc);
        }
    }

    // Character email
    //private class Creating_Email : BaseState { }

    // Character password
    //private class Creating_Password : BaseState { }
}

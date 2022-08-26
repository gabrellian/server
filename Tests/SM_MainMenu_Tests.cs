using Data;
using Data.Models;
using Engine.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCoreServer;

namespace Tests;

[TestClass]
public class SM_MainMenu_Tests
{
    [TestMethod]
    [Description("Main Menu Flow")]
    public async Task Main_Menu_Flow()
    {
        var name = new Random().Next(100,999).ToString();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string>("ServerDisplayName", "Test Server")
        }).Build();
        var pcrepo = new Mock<IPlayerCharacterRepo>();

        pcrepo.Setup(p => p.IsValidPlayerName(It.IsAny<string>())).Returns(Task.FromResult(true));
        pcrepo.Setup(p => p.SavePlayer(It.IsAny<PlayerCharacter>())).Returns(Task.FromResult(new PlayerCharacter()));

        var session = new Mock<GameSession>(new Mock<TcpServer>("127.0.0.1", 3000).Object);
        session.Setup(s => s.ChangeRoom(It.IsAny<string>())).Returns(Task.CompletedTask);
        session.Setup(s => s.DetachPlayer()).Returns(Task.CompletedTask);

        session.Setup(s => s.GetService<IPlayerCharacterRepo>()).Returns(pcrepo.Object);
        session.Setup(s => s.GetService<IConfiguration>()).Returns(config);
        session.Setup(s => s.GetService<GameSession>()).Returns(session.Object);

        var menu = new SM_MainMenu(session.Object, new Mock<IPlayfieldService>().Object, config);

        session.Verify(s => s.SendLine(It.IsAny<string>(), It.IsAny<bool>()), Times.AtLeastOnce(), "Should send initial greeting");

        Assert.AreEqual("Unauthenticated", menu.CurrentState.GetType().Name, "Should start as unauthenticated");

        await menu.OnCommand("create");

        Assert.AreEqual("Creating_Name", menu.CurrentState.GetType().Name, "Should transition to char creation name selection");

        await menu.OnCommand(name);

        session.Verify(s => s.ChangeRoom(It.IsAny<string>()), Times.Once, "Should change rooms on login");
        Assert.AreEqual("Authenticated", menu.CurrentState.GetType().Name, "Should transition to authenticated if valid character");

        await menu.OnCommand("logout");
        session.Verify(s => s.DetachPlayer(), Times.Once, "Should detach player from session");
        Assert.AreEqual("Unauthenticated", menu.CurrentState.GetType().Name, "Should transition to logged out state on logout");

        await menu.OnCommand("login");

        Assert.AreEqual("LoggingIn", menu.CurrentState.GetType().Name, "Should transition to logging in state");
    }

}
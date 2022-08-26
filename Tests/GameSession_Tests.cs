using Data.Models;
using Engine.Net;
using NetCoreServer;

namespace Tests;

[TestClass]
public class GameSession_Tests
{
    [TestMethod]
    public void SendLine()
    {
        var session = new Mock<GameSession>(new Mock<TcpServer>("127.0.0.1", 3000).Object);
        session.Setup(s => s.SendLine(It.IsAny<string>(), It.IsAny<bool>())).CallBase();

        session.Object.SendLine("SendLine Test", true);

        session.Verify(s => s.Send(It.Is<string>(x => x.EndsWith("\r\n"))), Times.Once, "Should call Send on base session");
        session.Verify(s => s.SendPrompt(), Times.Once, "Should send the prompt when required");


        session.Reset();
        session.Setup(s => s.SendLine(It.IsAny<string>(), It.IsAny<bool>())).CallBase();

        session.Object.SendLine("SendLine Test", false);
        session.Verify(s => s.SendPrompt(), Times.Never, "Should not send the prompt when specified");

    }
    [TestMethod]
    public void OnTick()
    {
        var context = new Mock<Engine.Core.IStatefulContext>();
        var contexts = new List<Engine.Core.IStatefulContext>()
        {
            context.Object
        };
        var session = new Mock<GameSession>(new Mock<TcpServer>("127.0.0.1", 3000).Object);
        session.Setup(s => s.OnTick(It.IsAny<TimeSpan>())).CallBase();
        session.SetupGet(s => s.StatefulContexts).Returns(contexts);

        session.Object.OnTick(TimeSpan.FromSeconds(99)).Wait();

        context.Verify(c => c.OnTick(It.Is<TimeSpan>(t => t.TotalSeconds == 99)), Times.Once,
            "Should call tick on all stateful contexts attached to session");
    }
    [TestMethod]
    public void AttachPlayer()
    {
        var context = new Mock<Engine.Core.IStatefulContext>();
        var contexts = new List<Engine.Core.IStatefulContext>()
        {
            context.Object
        };
        var session = new Mock<GameSession>(new Mock<TcpServer>("127.0.0.1", 3000).Object);
        session.Setup(s => s.AttachPlayer(It.IsAny<PlayerCharacter>())).CallBase();
        session.SetupGet(s => s.CurrentPlayer).CallBase();

        session.Object.AttachPlayer(new PlayerCharacter() { Id = "1234" });

        Assert.AreEqual("1234", session.Object.CurrentPlayer.Id, "Should set the current player");
    }
    [TestMethod]
    public void ChangeRoom()
    {
        var context = new Mock<Engine.Core.IStatefulContext>();
        var contexts = new List<Engine.Core.IStatefulContext>()
        {
            context.Object
        };
        var rm = new Mock<RoomInstance>();
        var pf = new Mock<PlayfieldInstance>(new PlayfieldDefinition() { UniqueId = "123" });

        var pfSvc = new Mock<IPlayfieldService>();
        pfSvc.Setup(p => p.GetRoom("some/test/room")).Returns(Task.FromResult((pf.Object, rm.Object)));
        var session = new Mock<GameSession>(new Mock<TcpServer>("127.0.0.1", 3000).Object);
        session.Setup(s => s.GetService<IPlayfieldService>()).Returns(pfSvc.Object);
        session.Setup(s => s.ChangeRoom(It.IsAny<string>())).CallBase();
        session.SetupGet(s => s.CurrentPlayfield).CallBase();
        session.SetupGet(s => s.CurrentRoom).CallBase();
        rm.Setup(r => r.AddPlayer(It.Is<GameSession>(s => s == session.Object))).Returns(Task.CompletedTask);

        session.Object.ChangeRoom("some/test/room").Wait();

        pfSvc.Verify(p => p.GetRoom("some/test/room"), Times.Once, "Should call pf svc for room instance");
        rm.Verify(r => r.AddPlayer(It.Is<GameSession>(s => s == session.Object)), Times.Once, 
            "Should add player to room");

        Assert.AreEqual(rm.Object, session.Object.CurrentRoom, "Should set current room");
        Assert.AreEqual(pf.Object, session.Object.CurrentPlayfield, "Should set current pf");
    }
}
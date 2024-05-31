using Discord.WebSocket;
using Timer = XDiscordBotLib.Timers.Timer;

namespace LibTestBot;

public class TestTimer : Timer
{
    public TestTimer(DiscordSocketClient client, int offset = 0) : base(client, offset)
    {
    }

    protected override TimeSpan period => TimeSpan.FromSeconds(10);

    protected override void callback()
    {
        throw new NotImplementedException();
    }
}
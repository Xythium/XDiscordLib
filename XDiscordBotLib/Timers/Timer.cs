using System;
using Discord.WebSocket;
using XDiscordBotLib.Utils;

namespace XDiscordBotLib.Timers;

public abstract class Timer
{
    protected System.Threading.Timer timer;

    protected abstract TimeSpan period { get; }

    protected DiscordSocketClient socketClient;

    protected Timer(DiscordSocketClient client, int offset = 0)
    {
        socketClient = client;
        Logging.Console.Verbose("New Timer: {Name}, offset: {Offset}", GetType().Name, offset);
        timer = new System.Threading.Timer(callback, null, offset, (int) period.TotalMilliseconds);
    }

    private void callback(object? state)
    {
        try
        {
            callback();
        }
        catch (Exception ex)
        {
            Logging.Error.Error(ex, "Error in timer");
        }
    }

    protected abstract void callback();
}
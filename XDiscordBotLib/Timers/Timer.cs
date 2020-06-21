using System;
using Discord.WebSocket;
using XDiscordBotLib.Utils;

namespace XDiscordBotLib.Timers
{
    public abstract class Timer
    {
        protected System.Threading.Timer timer;

        protected abstract TimeSpan period { get; }

        protected DiscordSocketClient socketClient;

        protected Timer(DiscordSocketClient client, int offset = 0)
        {
            socketClient = client;
            Logging.Console.Verbose($"New Timer: {GetType().Name}, offset: {offset}");
            timer = new System.Threading.Timer(callback, null, offset, (int) period.TotalMilliseconds);
        }

        protected abstract void callback(object state);
    }
}
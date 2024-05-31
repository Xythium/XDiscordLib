using Discord;
using Discord.WebSocket;
using Hangfire;
using Hangfire.Storage.SQLite;
using XDiscordBotLib.Utils;

namespace LibTestBot;

class Program
{
    private static void Main(string[] args)
    {
        Logging.Setup();
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    async Task MainAsync()
    {
        var token = File.ReadAllText("bottoken");
        var bot = new TestBot(token, "<", new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.GuildMessages | GatewayIntents.Guilds | GatewayIntents.MessageContent
        });
        await bot.RunAsync();
    }
}

internal class TestBot : Bot
{
    private TestTimer timer;

    public TestBot(string token, string commandPrefix, DiscordSocketConfig socketConfig) : base(token, commandPrefix, socketConfig)
    {

    }

    protected override Task setupTimers()
    {
        timer = new TestTimer(socketClient);
        return base.setupTimers();
    }
}
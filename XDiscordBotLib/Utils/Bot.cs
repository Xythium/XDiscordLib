using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Serilog;
using XDiscordBotLib.Commands;

namespace XDiscordBotLib.Utils
{
    public class Bot
    {
        protected readonly DiscordSocketClient socketClient;
        protected readonly CommandService commands;

        protected readonly string token;
        protected readonly string commandPrefix;

        public Bot(string token) : this(token, ".s") { }

        public Bot(string token, string commandPrefix) : this(token, commandPrefix, new DiscordSocketConfig()) { }

        public Bot(string token, string commandPrefix, DiscordSocketConfig socketConfig)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token is null or whitespace", nameof(token));

            this.token = token;
            this.commandPrefix = commandPrefix;
            if (socketConfig == null)
                socketConfig = new DiscordSocketConfig();
            socketClient = new DiscordSocketClient(socketConfig);
            commands = new CommandService();
        }

        protected void setupLogging() { socketClient.Log += SocketClient_Log; }

        protected async Task setupCommands()
        {
            socketClient.MessageReceived += SocketClient_MessageReceived;
            await commands.AddModulesAsync(Assembly.GetAssembly(typeof(GeneralModule)), null);
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        protected virtual void setupTimers() { }

        protected Task SocketClient_Log(LogMessage arg)
        {
            switch (arg.Severity)
            {
                case LogSeverity.Critical:
                    Logging.Error.Fatal(arg.Exception, arg.Message);
                    break;

                case LogSeverity.Error:
                    Logging.Error.Error(arg.Exception, arg.Message);
                    break;

                case LogSeverity.Warning:
                    Logging.Console.Warning(arg.Message);
                    break;

                case LogSeverity.Info:
                    Logging.Console.Information(arg.Message);
                    break;

                case LogSeverity.Verbose:
                    Logging.Console.Verbose(arg.Message);
                    break;

                case LogSeverity.Debug:
                    Logging.Console.Debug(arg.Message);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Task.CompletedTask;
        }

        protected async Task SocketClient_MessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message))
                return;

            DateTimeOffset messageReceivedTimestamp = DateTimeOffset.UtcNow;

            var server = socketClient.GetGuild(message.Channel.Id);

            if (message.EditedTimestamp != null)
            {
                Logging.Chat.Information("[{serverName} #{channelName}] {author}'s Message was edited to {newContent} at {date}", server.Name, message.Channel.Name, message.Author.ToString(), message.Content, message.EditedTimestamp.Value);
            }
            else
            {
                Logging.Chat.Information("[{serverName} #{channelName}] {author} posted Message {content} at {date}", server?.Name ?? "SERVER NAME", message.Channel.Name, message.Author.ToString(), message.Content, message.CreatedAt);
            }

            foreach (var embed in message.Embeds)
            {
                Logging.Chat.Information("[{serverName} #{channelName}] Message {id} has embed {embed}", server?.Name, message.Channel.Name, message.Id, embed);
            }

            if (message.Content == "$ping")
            {
                await message.Channel.SendMessageAsync($"Pong! Time between receiving message and sending it back: {DateTimeOffset.UtcNow.Subtract(messageReceivedTimestamp).TotalMilliseconds}ms");
                return;
            }

            var argPos = 0;
            if (!(message.HasStringPrefix(commandPrefix, ref argPos) || message.HasMentionPrefix(socketClient.CurrentUser, ref argPos)))
                return;

            var context = new CommandContext(socketClient, message);
            using var typing = context.Channel.EnterTypingState();
            var result = await commands.ExecuteAsync(context, argPos, null);

            if (!result.IsSuccess)
            {
                Log.Error("ERROR: {@e}", result);
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        public async Task RunAsync()
        {
            setupLogging();
            await setupCommands();
            setupTimers();

            await socketClient.LoginAsync(TokenType.Bot, token);
            await socketClient.StartAsync();

            await Task.Delay(-1);
        }
    }
}
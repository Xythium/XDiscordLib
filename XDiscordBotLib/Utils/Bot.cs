using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using XDiscordBotLib.Commands;
using XDiscordBotLib.Interactions;

namespace XDiscordBotLib.Utils;

public abstract class Bot
{
    protected readonly DiscordSocketClient socketClient;
    protected readonly CommandService commands;
    protected InteractionService interaction;
    protected ServiceProvider serviceProvider;
    protected IServiceCollection serviceCollection;

    protected readonly string token;
    protected readonly string commandPrefix;

    public Bot(string token) : this(token, ".s")
    {
    }

    public Bot(string token, string commandPrefix) : this(token, commandPrefix, new DiscordSocketConfig())
    {
    }

    public Bot(string token, string commandPrefix, DiscordSocketConfig? socketConfig)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token is null or whitespace", nameof(token));

        this.token = token;
        this.commandPrefix = commandPrefix;
        socketConfig ??= new DiscordSocketConfig();
        socketClient = new DiscordSocketClient(socketConfig);
        commands = new CommandService();

        serviceCollection = new ServiceCollection();
    }

    protected virtual void setupLogging()
    {
        socketClient.Log += SocketClient_Log;
    }

    protected virtual void setupServices()
    {
        serviceProvider = serviceCollection.BuildServiceProvider();
    }

    protected virtual async Task setupCommands()
    {
        commands.Log += SocketClient_Log;
        socketClient.MessageReceived += SocketClient_MessageReceived;

        await commands.AddModulesAsync(Assembly.GetAssembly(typeof(GeneralModule)), serviceProvider);
        await commands.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);
    }

    protected virtual async Task setupInteractions()
    {
        try
        {
            interaction = new InteractionService(socketClient, new InteractionServiceConfig
            {
            });
            interaction.Log += SocketClient_Log;

            await interaction.AddModulesAsync(Assembly.GetAssembly(typeof(GeneralInteractionModule)), serviceProvider);
            await interaction.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);
            await interaction.RegisterCommandsGloballyAsync(true);

            socketClient.InteractionCreated += async inter =>
            {
                var scope = serviceProvider.CreateScope();
                var context = new SocketInteractionContext(socketClient, inter);
                Logging.Console.Verbose("{a}", context.Channel);

                await interaction.ExecuteCommandAsync(context, scope.ServiceProvider);
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "error");
        }

    }

    protected virtual Task setupTimers()
    {
        return Task.CompletedTask;
    }

    protected virtual Task SocketClient_Log(LogMessage logMessage)
    {
        switch (logMessage.Severity)
        {
            case LogSeverity.Critical:
                Logging.Error.Fatal(logMessage.Exception, logMessage.Message);
                break;

            case LogSeverity.Error:
                Logging.Error.Error(logMessage.Exception, logMessage.Message);
                break;

            case LogSeverity.Warning:
                Logging.Console.Warning(logMessage.Message);
                break;

            case LogSeverity.Info:
                Logging.Console.Information(logMessage.Message);
                break;

            case LogSeverity.Verbose:
                Logging.Console.Verbose(logMessage.Message);
                break;

            case LogSeverity.Debug:
                Logging.Console.Debug(logMessage.Message);
                break;

            default: throw new ArgumentOutOfRangeException();
        }

        return Task.CompletedTask;
    }

    protected virtual async Task SocketClient_MessageReceived(SocketMessage arg)
    {
        if (arg is not SocketUserMessage message)
            return;

        if (message.Content == "$ping")
        {
            var messageReceived = message.Timestamp;
            var messageProcessed = DateTimeOffset.UtcNow;

            var receiveTime = messageReceived.Subtract(messageProcessed);
            var processingTime = DateTimeOffset.UtcNow.Subtract(messageProcessed);

            await message.Channel.SendMessageAsync($"Pong! Message timestamp: {receiveTime.TotalMilliseconds}ms ago and received: {processingTime.TotalMilliseconds}ms ago");
            return;
        }

        var server = (message.Channel as SocketGuildChannel)?.Guild;
        var serverName = server?.Name;
        var author = message.Author.ToString();

        if (message.EditedTimestamp != null)
        {
            Logging.Chat.Information("[{ServerName} #{ChannelName}] {Author}'s Message was edited to {NewContent} at {Date}", serverName, message.Channel.Name, author, message.Content, message.EditedTimestamp.Value);
        }
        else
        {
            Logging.Chat.Information("[{ServerName} #{ChannelName}] {Author} posted Message {Content} at {Date}", serverName, message.Channel.Name, author, message.Content, message.CreatedAt);
        }

        foreach (var embed in message.Embeds)
        {
            Logging.Chat.Information("[{ServerName} #{ChannelName}] Message {Id} has embed {Embed}", serverName, message.Channel.Name, message.Id, embed);
        }

        var argPos = 0;
        if (!(message.HasStringPrefix(commandPrefix, ref argPos) || message.HasMentionPrefix(socketClient.CurrentUser, ref argPos)))
            return;

        var context = new CommandContext(socketClient, message);
        using var typing = context.Channel.EnterTypingState();
        var result = await commands.ExecuteAsync(context, argPos, serviceProvider);

        if (!result.IsSuccess)
        {
            Log.Error("ERROR: {@Error}", result);
            await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }

    public async Task RunAsync()
    {
        setupLogging();
        setupServices();
        await setupCommands();
        socketClient.Ready += setupInteractions;
        socketClient.Ready += setupTimers;

        await socketClient.LoginAsync(TokenType.Bot, token);
        await socketClient.StartAsync();

        await Task.Delay(-1);
    }
}
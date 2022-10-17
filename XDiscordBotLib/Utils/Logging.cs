using Serilog;

namespace XDiscordBotLib.Utils;

public class Logging
{
    public static ILogger Console { get; private set; }

    public static ILogger Chat { get; private set; }

    public static ILogger Error { get; private set; }

    public static void Setup(string consoleTemplate = "[{Timestamp:ddd MMM dd HH':'mm':'ss yyyy} {Level:u3}] {Message}{NewLine}{Exception}", string chatTemplate = "[{Timestamp:ddd MMM dd HH':'mm':'ss yyyy} {Level:u3}] {Message}{NewLine}{Exception}", string errorTemplate = "[{Timestamp:ddd MMM dd HH':'mm':'ss yyyy} {Level:u3}] {Message}{NewLine}{Exception}")
    {
        SetupConsole(consoleTemplate);
        SetupChat(chatTemplate);
        SetupError(errorTemplate);
    }

    public static void SetupConsole(string template)
    {
#if DEBUG
        Console = Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose()
            .WriteTo.Console(outputTemplate: template)
            .WriteTo.RollingFile("Logs\\General-Debug.log", outputTemplate: template, retainedFileCountLimit: null)
            .CreateLogger();
#else
             Console =
                Log.Logger =
                    new LoggerConfiguration().MinimumLevel.Information()
                        .WriteTo.Console(outputTemplate: template)
                        .WriteTo.RollingFile("Logs\\General-Release.log", outputTemplate: template, retainedFileCountLimit: null)
                        .CreateLogger();
#endif
        Console.Information("Console Logging Setup Complete");
    }

    public static void SetupChat(string template)
    {
#if DEBUG
        Chat = new LoggerConfiguration().MinimumLevel.Verbose()
            .WriteTo.Console(outputTemplate: template)
            .WriteTo.RollingFile("Logs\\Chat-Debug.log", outputTemplate: template, retainedFileCountLimit: null)
            .CreateLogger();
#else
             Chat =
                new LoggerConfiguration().MinimumLevel.Information()
                        .WriteTo.LiterateConsole(outputTemplate: template)
                        .WriteTo.RollingFile("Logs\\Chat-Release.log", outputTemplate: template, retainedFileCountLimit: null)
                        .CreateLogger();
#endif
        Chat.Information("Chat Logging Setup Complete");
    }

    public static void SetupError(string template)
    {
#if DEBUG
        Error = new LoggerConfiguration().MinimumLevel.Verbose()
            .WriteTo.Console(outputTemplate: template)
            .WriteTo.RollingFile("Logs\\Error-Debug.log", outputTemplate: template, retainedFileCountLimit: null)
            .CreateLogger();
#else
              Error =
                new LoggerConfiguration().MinimumLevel.Information()
                        .WriteTo.LiterateConsole(outputTemplate: template)
                        .WriteTo.RollingFile("Logs\\Error-Release.log", outputTemplate: template, retainedFileCountLimit: null)
                        .CreateLogger();
#endif
        Error.Information("Error Logging Setup Complete");
    }
}
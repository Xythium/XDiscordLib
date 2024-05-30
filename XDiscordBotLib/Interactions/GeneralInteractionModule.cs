﻿using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using XDiscordBotLib.Utils;

namespace XDiscordBotLib.Interactions;

public class GeneralInteractionModule : InteractionModuleBase
{
    private readonly InteractionService _service;

    public GeneralInteractionModule(InteractionService service) { _service = service; }

    [SlashCommand("help", "List of commands")]
    public async Task Help([Remainder] string command = "")
    {
        var builder = new EmbedBuilder();

        if (string.IsNullOrWhiteSpace(command))
        {
            builder.Description = "These are the commands you can use";

            foreach (var module in _service.Modules)
            {
                var description = new StringBuilder();
                var commands = module.SlashCommands.OrderBy(c => c.Name)
                    .ToArray();

                foreach (var cmd in commands)
                {
                    if (!(await cmd.CheckPreconditionsAsync(Context, null)).IsSuccess)
                        continue;

                    description.Append("**");
                    //description.Append(cmd.Aliases.First());
                    description.Append(cmd.Name);
                    /*var aliases = cmd.Aliases.Skip(1)
                        .OrderBy(a => a)
                        .Select(a => $"${a}")
                        .ToArray();
                    if (aliases.Length > 0)
                        description.Append($" ({string.Join(", ", aliases)})");*/
                    description.Append("**");
                    if (!string.IsNullOrWhiteSpace(cmd.Description))
                        description.Append($" - _{cmd.Description}_");
                    description.AppendLine();
                }

                if (description.Length < 1)
                    continue;

                var name = module.Name.Replace("InteractionModule", "").Replace("Module", "");
                builder.AddField(x =>
                {
                    x.Name = name;
                    x.Value = description.ToString();
                    x.IsInline = false;
                });
            }

            await RespondAsync("", new[]
            {
                builder.Build()
            });
        }
        /* else
         {
    
             var result = _service.Search(Context, command);
 
             if (!result.IsSuccess)
             {
                 await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                 return;
             }
 
             builder.Description = $"Here are some commands like **{command}**";
 
             var commands = result.Commands.Select(m => m.Command)
                 .OrderBy(c => c.Name)
                 .ToArray();
 
             foreach (var cmd in commands)
             {
                 var parameters = new StringBuilder("**Parameters:** ");
 
                 foreach (var parameter in cmd.Parameters)
                 {
                     parameters.AppendLine();
                     parameters.Append(parameter.IsOptional ? "[" : "<");
                     parameters.Append($"{parameter.Name} : {parameter.Type.Name}");
                     parameters.Append(parameter.IsOptional ? $"='{parameter.DefaultValue}']" : ">");
                     parameters.Append($" {parameter.Summary}");
                 }
 
                 parameters.AppendLine();
                 parameters.AppendLine();
 
                 builder.AddField(x =>
                 {
                     x.Name = cmd.Module.Name.Replace("Module", "");
                     x.Value = string.Join(", ", cmd.Aliases.Select(a => $"_**{a}**_")) + "\r\n\r\n" + parameters + $"**Summary:** {cmd.Summary}";
                     x.IsInline = false;
                 });
             }
 
             await ReplyAsync("", false, builder.Build());
         }*/
    }

    [SlashCommand("about", "About the bot")]
    public async Task About()
    {
        var library = typeof(Bot).Assembly.GetName();
        var discord = typeof(IChannel).Assembly.GetName();
        var entry = Assembly.GetEntryAssembly()
            .GetName();

        await RespondAsync($"I'm {NameVersion(entry)}, I'm running on {NameVersion(library)} and {NameVersion(discord)}");
    }

    private static string NameVersion(AssemblyName name) { return $"{name.Name} {Version(name.Version)}"; }

    private static string Version(Version? version)
    {
        if (version == null)
            return "";

        if (version.Revision != 0)
        {
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        return $"{version.Major}.{version.Minor}.{version.Build}";
    }
}
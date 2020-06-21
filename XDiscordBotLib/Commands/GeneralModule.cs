﻿using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace XDiscordBotLib.Commands
{
    public class GeneralModule : ModuleBase<CommandContext>
    {
        private readonly CommandService service;

        public GeneralModule(CommandService service) { this.service = service; }

        [Command("help"), Alias("commands", "cmds", "command"), Summary("oh my god wtf please help me")]
        public async Task Help(string command = "''")
        {
            var builder = new EmbedBuilder();

            if (string.IsNullOrWhiteSpace(command))
            {
                builder.Description = "These are the commands you can use";

                foreach (var module in service.Modules)
                {
                    if (module.Name == "BotOwnerModule" || module.Name == "VersionModule")
                        continue;

                    var description = new StringBuilder();
                    var commands = module.Commands.OrderBy(c => c.Name)
                        .ToArray();

                    foreach (var cmd in commands)
                    {
                        if (!(await cmd.CheckPreconditionsAsync(Context)).IsSuccess)
                            continue;

                        description.Append("**");
                        description.Append(cmd.Aliases.First());
                        var aliases = cmd.Aliases.Skip(1)
                            .OrderBy(a => a)
                            .Select(a => $"${a}")
                            .ToArray();
                        if (aliases.Length > 0)
                            description.Append($" ({string.Join(", ", aliases)})");
                        description.Append("**");
                        if (!string.IsNullOrWhiteSpace(cmd.Summary))
                            description.Append($" - _{cmd.Summary}_");
                        description.AppendLine();
                    }

                    var name = module.Name.Replace("Module", "");
                    builder.AddField(x =>
                    {
                        x.Name = name;
                        x.Value = description.ToString();
                        x.IsInline = false;
                    });
                }

                await ReplyAsync("", false, builder.Build());
            }
            else
            {
                var result = service.Search(Context, command);

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
                        parameters.Append(parameter.IsOptional ? $"=`{parameter.DefaultValue}`]" : ">");
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
            }
        }

        /* [Command("ping")]
         public async Task Ping()
         {
             await ReplyAsync($"Pong {Context.Message.Timestamp.Subtract(now).TotalMilliseconds}ms");
         }*/
    }
}
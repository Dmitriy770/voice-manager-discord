using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    internal class VoiceManager
    {
        private DiscordSocketClient _client;
        private ulong ID_VOICE_CREATE_CHANNEL;
        private ulong ID_GUILD;

        public VoiceManager(DiscordSocketClient client)
        {
            ID_VOICE_CREATE_CHANNEL = ulong.Parse(Environment.GetEnvironmentVariable("createVoiceChannelId"));
            ID_GUILD = ulong.Parse(Environment.GetEnvironmentVariable("guildId"));
            _client = client;
            _client.Ready += CreateSlashCommands;
            _client.SlashCommandExecuted += SlashCommandHandler;
            _client.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
        }

        private async Task CreateSlashCommands()
        {
            var guild = _client.GetGuild(ID_GUILD);



            var guildCommand = new SlashCommandBuilder()
                .WithName("set-voice-name")
                .WithDescription("Set name of our voice.")
                .AddOption("name", ApplicationCommandOptionType.String, "Name of our voice channel", isRequired: true, minLength: 0,maxLength: 15);

            var guildCommandLimit = new SlashCommandBuilder()
                .WithName("set-voice-limit")
                .WithDescription("Set user limit of our voice.")
                .AddOption("limit", ApplicationCommandOptionType.Number, "User limit of our voice channel", isRequired: true, minValue: 0, maxValue: 99);

            var guildCommandClaim = new SlashCommandBuilder()
                .WithName("claim")
                .WithDescription("Claim the channel once the owner leaves.");

            try
            {
                await guild.CreateApplicationCommandAsync(guildCommand.Build());
                await guild.CreateApplicationCommandAsync(guildCommandLimit.Build());
                await guild.CreateApplicationCommandAsync(guildCommandClaim.Build());
            }
            catch(HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }

        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            switch(command.Data.Name)
            {
                case "set-voice-name":
                    DBController.setVoiceName(command.User.Id, command.Data.Options.First().Value.ToString());
                    UpdateVoiceChannel(command.User.Id);
                    await command.RespondAsync($"Voice rename. New voice name: {command.Data.Options.First().Value}", ephemeral: true);
                    break;
                case "set-voice-limit":
                    DBController.setVoiceLimit(command.User.Id, int.Parse(command.Data.Options.First().Value.ToString()));
                    UpdateVoiceChannel(command.User.Id);
                    await command.RespondAsync($"Voice user limit update. New voice limit: {command.Data.Options.First().Value}", ephemeral: true);
                    break;
                case "claim":
                    var voiceChannel = (command.User as IGuildUser).VoiceChannel as SocketVoiceChannel;
                    if(voiceChannel == null) 
                    {
                        await command.RespondAsync("You are not in a voice channel", ephemeral: true);
                    }
                    else
                    {
                        ulong ownerId = DBController.getOwnerChannelId(voiceChannel.Id);
                        bool canClaim = true;
                        if (ownerId != 0) { 
                            Array users = voiceChannel.ConnectedUsers.ToArray();
                            foreach( SocketGuildUser user in users)
                            {
                                if (user.Id == ownerId)
                                {
                                    canClaim = false;
                                }
                            }
                        }
                        if(canClaim)
                        {
                            DBController.setActiveChannel(voiceChannel.Id, command.User.Id);
                            UpdateVoiceChannalByChannelId(voiceChannel.Id);
                            await command.RespondAsync("Voice claimed.", ephemeral: true);
                        }
                        else if(ownerId == command.User.Id)
                        {
                            await command.RespondAsync("This is your channel", ephemeral: true);
                        }
                        else
                        {
                            await command.RespondAsync("Сhannel owner is in it.", ephemeral: true);
                        }
                    }
                    break;
            }    
        }

        private async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldVoiceState, SocketVoiceState newVoiceState)
        {
            if (newVoiceState.VoiceChannel != null && newVoiceState.VoiceChannel.Id == ID_VOICE_CREATE_CHANNEL)
            {
                String voiceName = DBController.getVoiceName(user.Id);
                if (voiceName == "") 
                {
                    voiceName = user.Username;
                }

                int? voiceLimit = DBController.getVoiceLimit(user.Id);
                if(voiceLimit <= 0)
                {
                    voiceLimit = null;
                }
                var channel = await newVoiceState.VoiceChannel.Guild.CreateVoiceChannelAsync(voiceName, x => { x.CategoryId = newVoiceState.VoiceChannel.CategoryId; x.UserLimit = voiceLimit; });

                DBController.setActiveChannel(channel.Id, user.Id);

                MoveUser(newVoiceState.VoiceChannel.Guild.GetUser(user.Id), (ulong)channel.Id);

            }
            if (oldVoiceState.VoiceChannel != null && oldVoiceState.VoiceChannel.CategoryId == oldVoiceState.VoiceChannel.Guild.GetVoiceChannel(ID_VOICE_CREATE_CHANNEL).CategoryId && oldVoiceState.VoiceChannel.Id != ID_VOICE_CREATE_CHANNEL && oldVoiceState.VoiceChannel.ConnectedUsers.ToArray().Length == 0)
            {
                DBController.deleteActiveChannel(oldVoiceState.VoiceChannel.Id);
                await oldVoiceState.VoiceChannel.DeleteAsync();
            }
        }

        private async void MoveUser(SocketGuildUser user, ulong new_channel)
        {
            await user.ModifyAsync(x => { x.ChannelId = new_channel; });
        }

        private async void UpdateVoiceChannel(ulong userId)
        {
            List<ulong> listChannel = DBController.getChannelIdByOwner(userId);
            foreach (var channelId in listChannel)
            {
                if (channelId != 0)
                {
                    String voiceName = DBController.getVoiceName(userId);
                    if (voiceName == "")
                    {
                        voiceName = _client.GetUser(userId).Username;
                    }

                    int? voiceLimit = DBController.getVoiceLimit(userId);
                    if (voiceLimit <= 0)
                    {
                        voiceLimit = null;
                    }

                    var voiceChannel = _client.GetChannel(channelId) as IVoiceChannel;
                    if (voiceChannel != null)
                    {
                        await voiceChannel.ModifyAsync(x => { x.Name = voiceName; x.UserLimit = voiceLimit; });
                    }
                }
            }
        }

        private async void UpdateVoiceChannalByChannelId(ulong channelId)
        {
            ulong userId = DBController.getOwnerChannelId(channelId);
            String voiceName = DBController.getVoiceName(userId);
            if (voiceName == "")
            {
                voiceName = _client.GetUser(userId).Username;
            }

            int? voiceLimit = DBController.getVoiceLimit(userId);
            if (voiceLimit <= 0)
            {
                voiceLimit = null;
            }

            var voiceChannel = _client.GetChannel(channelId) as IVoiceChannel;
            if (voiceChannel != null)
            {
                await voiceChannel.ModifyAsync(x => { x.Name = voiceName; x.UserLimit = voiceLimit; });
            }
        }
    }
}

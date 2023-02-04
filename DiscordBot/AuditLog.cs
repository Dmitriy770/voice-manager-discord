using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    internal class AuditLog
    {
        private DiscordSocketClient _client;
        private ulong ID_AUDIT_LOG__CHANNEL;
        public AuditLog(DiscordSocketClient client)
        {
            ID_AUDIT_LOG__CHANNEL = ulong.Parse(Environment.GetEnvironmentVariable("auditLogChannelId"));
            _client = client;
            _client.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
        }

        private async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldVoiceState, SocketVoiceState newVoiceState)
        {
            var auditLogChannel = (ITextChannel)_client.GetChannel(ID_AUDIT_LOG__CHANNEL);
            if (oldVoiceState.VoiceChannel == null && newVoiceState.VoiceChannel != null)
            {
                var embed = new EmbedBuilder().WithAuthor(user).WithDescription($"Подключился к голосовому каналу: **{newVoiceState.VoiceChannel.Name}**.").WithCurrentTimestamp().WithColor(Color.Green);
                await auditLogChannel.SendMessageAsync(embed: embed.Build());
            }
            else if(oldVoiceState.VoiceChannel != null && newVoiceState.VoiceChannel != null && oldVoiceState.VoiceChannel != newVoiceState.VoiceChannel)
            {
                var embed = new EmbedBuilder().WithAuthor(user).WithDescription($"Перешел из голосового канала: **{oldVoiceState.VoiceChannel.Name}** в голосовой канал: **{newVoiceState.VoiceChannel.Name}**.").WithCurrentTimestamp().WithColor(Color.Orange);
                await auditLogChannel.SendMessageAsync(embed: embed.Build());
            }
            else if(oldVoiceState.VoiceChannel != null && newVoiceState.VoiceChannel == null)
            {
                var embed = new EmbedBuilder().WithAuthor(user).WithDescription($"Отключился от голосового канала: **{oldVoiceState.VoiceChannel.Name}**.").WithCurrentTimestamp().WithColor(Color.Red);
                await auditLogChannel.SendMessageAsync(embed: embed.Build());
            }
        }
    }
}

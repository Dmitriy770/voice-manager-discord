using System;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using Discord;
using Discord.WebSocket;

namespace DiscordBot
{
    public class Program
    {
        private DiscordSocketClient _client;
        public static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100
            };
            _client = new DiscordSocketClient(config);

            _client.Log += Log;

            var token = Environment.GetEnvironmentVariable("token"); //токен

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync(); //запуск бота

            DBController.initialization();

            new VoiceManager(_client);
            new AuditLog(_client);

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
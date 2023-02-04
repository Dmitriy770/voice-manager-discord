# C# voice management discord bot
This bot is designed to create audit logs and provide an opportunity for each user to create their own voice channel.

Technologies:
- C# .NET 6.0
- Discord.Net
- Microsoft.Data.Sqlite

## Quick demo
![creating voice](https://github.com/Dmitriy770/voice-manager-discord/blob/master/description_assets/CreateVoice.gif)
![audit logs](https://github.com/Dmitriy770/voice-manager-discord/blob/main/description_assets/auditlog.png)
## Description
bot token and channel ID are specified in environment variables

Commands:
- /set-voice-name - sets the name of the voice channel
- /set-voice-limit - sets the limit of voice channel users
- /claim - applies your voice channel settings to someone else's if you left it

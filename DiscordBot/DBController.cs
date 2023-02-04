using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    internal class DBController
    {
        public static void initialization()
        {
            using (var connnection = new SqliteConnection("Data Source=guilddata.db"))
            {
                connnection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connnection;
                //command.CommandText = "CREATE TABLE SystemChannels(id INTEGER NOT NULL PRIMARY KEY, type TEXT NOT NULL)";
                //command.ExecuteNonQueryAsync();

                command.CommandText = "CREATE TABLE ActiveChannels(id INTEGER NOT NULL PRIMARY KEY, owner_id INTEGER NOT NULL)";
                command.ExecuteNonQueryAsync();

                command.CommandText = "CREATE TABLE ChannelsSetting(user_id INTEGER NOT NULL PRIMARY KEY, user_limit INTEGER, channel_name TEXT)";
                command.ExecuteNonQueryAsync();
            }
        }

        public static void setVoiceLimit(ulong userId, int limit)
        {
            using (var connnection = new SqliteConnection("Data Source=guilddata.db"))
            {
                connnection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connnection;
                command.CommandText = $"SELECT * FROM ChannelsSetting WHERE user_id={userId}";
                bool hasValue = false;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    hasValue = reader.HasRows;
                }
                if (hasValue)
                {
                    command.CommandText = $"UPDATE ChannelsSetting SET user_limit={limit} WHERE user_id={userId}";
                    command.ExecuteNonQueryAsync();
                }
                else
                {
                    command.CommandText = $"INSERT INTO ChannelsSetting (user_id, user_limit) VALUES ({userId}, {limit})";
                    command.ExecuteNonQueryAsync();
                }
            }

        }

        public static int getVoiceLimit(ulong userId)
        {
            int limit = 0;
            using (var connnection = new SqliteConnection("Data Source=guilddata.db"))
            {
                connnection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connnection;
                command.CommandText = $"SELECT user_limit FROM ChannelsSetting WHERE user_id={userId}";
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        if (!reader.IsDBNull(0))
                        {
                            limit = reader.GetInt16(0);
                        }
                    }
                }
            }
            return limit;
        }

        public static void setVoiceName(ulong userId, string name)
        {
            using (var connnection = new SqliteConnection("Data Source=guilddata.db"))
            {
                connnection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connnection;
                command.CommandText = $"SELECT * FROM ChannelsSetting WHERE user_id={userId}";
                bool hasValue = false;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    hasValue = reader.HasRows;
                }
                if (hasValue)
                {
                    command.CommandText = $"UPDATE ChannelsSetting SET channel_name='{name}' WHERE user_id={userId}";
                    command.ExecuteNonQuery();
                }
                else
                {
                    command.CommandText = $"INSERT INTO ChannelsSetting (user_id, channel_name) VALUES ({userId}, '{name}')";
                    command.ExecuteNonQueryAsync();
                }
            }

        }

        public static String getVoiceName(ulong userId)
        {
            String name = "";
            using (var connnection = new SqliteConnection("Data Source=guilddata.db"))
            {
                connnection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connnection;
                command.CommandText = $"SELECT channel_name FROM ChannelsSetting WHERE user_id={userId}";
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        if (!reader.IsDBNull(0))
                        {
                            name = reader.GetString(0);
                        }
                    }
                }
            }
            return name;
        }

        /*public static void setSystemChannel(ulong channelId, string type)
        {
            using (var connnection = new SqliteConnection("Data Source=guilddata.db"))
            {
                connnection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connnection;
                command.CommandText = $"SELECT * FROM SystemChannels WHERE id={channelId}";
                bool hasValue = false;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    hasValue = reader.HasRows;
                }
                if (hasValue)
                {
                    command.CommandText = $"UPDATE SystemChannels SET type='{type}' WHERE id={channelId}";
                    command.ExecuteNonQuery();
                }
                else
                {
                    command.CommandText = $"INSERT INTO SystemChannels (id, type) VALUES ({channelId}, '{type}')";
                    command.ExecuteNonQueryAsync();
                }
            }

        }*/

       /* public static ulong getSystemChannel(string type)
        {
            ulong id = 0;
            using (var connnection = new SqliteConnection("Data Source=guilddata.db"))
            {
                connnection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connnection;
                command.CommandText = $"SELECT id FROM SystemChannels WHERE type='{type}'";
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        id = (ulong) reader.GetInt64(0);
                    }
                }
            }
            return id;
        }*/

        public static void setActiveChannel(ulong channelId, ulong ownerId)
        {
            using (var connnection = new SqliteConnection("Data Source=guilddata.db"))
            {
                connnection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connnection;
                command.CommandText = $"SELECT * FROM ActiveChannels WHERE id={channelId}";
                bool hasValue = false;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    hasValue = reader.HasRows;
                }
                if (hasValue)
                {
                    command.CommandText = $"UPDATE ActiveChannels SET owner_id={ownerId} WHERE id={channelId}";
                    command.ExecuteNonQuery();
                }
                else
                {
                    command.CommandText = $"INSERT INTO ActiveChannels (id, owner_id) VALUES ({channelId}, {ownerId})";
                    command.ExecuteNonQueryAsync();
                }
            }

        }

        public static ulong getOwnerChannelId(ulong channelId)
        {
            ulong id = 0;
            using (var connnection = new SqliteConnection("Data Source=guilddata.db"))
            {
                connnection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connnection;
                command.CommandText = $"SELECT owner_id FROM ActiveChannels WHERE id={channelId}";
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        id = (ulong) reader.GetInt64(0);
                    }
                }
            }
            return id;
        }

        public static List<ulong> getChannelIdByOwner(ulong channelId)
        {
            List<ulong> idList = new List<ulong>();
            using (var connnection = new SqliteConnection("Data Source=guilddata.db"))
            {
                connnection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connnection;
                command.CommandText = $"SELECT id FROM ActiveChannels WHERE owner_id={channelId}";
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            idList.Add((ulong)reader.GetInt64(0));
                        }
                    }
                }
            }
            return idList;
        }

        public static void deleteActiveChannel(ulong channelId)
        {
            using (var connnection = new SqliteConnection("Data Source=guilddata.db"))
            {
                connnection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connnection;
                command.CommandText = $"DELETE FROM ActiveChannels WHERE id={channelId}";
            }
        }
    }
}

﻿using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;
using Server.Server;
using Server.Context;
using Server.Log;
using Server.Settings.Structures;
using LmpCommon.Message.Data.Chat;
using LmpCommon.Message.Server;

namespace Server.DiscordBot
{
    public class DiscordClient
    {
        public static Task StartAsync() => new DiscordClient().MainAsync();
        public static DiscordSocketClient Client = new();
        public async Task MainAsync()
        {
            Client.Log += Log;
            Client.MessageReceived += MessageReceived;
            var token = File.ReadAllText("token.txt");
            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();
            await Task.Delay(-1);
        }
        private Task Log(LogMessage msg)
        {
            Console.WriteLine($"[Discord] {msg.ToString()}");
            return Task.CompletedTask;
        }
        public static async Task<SocketTextChannel> GetKSPChatChannelAsync()
        {
            return await Client.GetChannelAsync(1010603662780940299) as SocketTextChannel;
        }
        public static async Task<RestUserMessage> SendMessageToDiscordAsync(string text)
        {
            // this took me 5 minutes
            var KspChatChannel = await GetKSPChatChannelAsync();
            return await KspChatChannel.SendMessageAsync(text);
        }
        public static void SendMessageToKSP(string text)
        {
            // this took me 4 hours
            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<ChatMsgData>();
            msgData.From = "[Discord]";
            msgData.Text = text;
            MessageQueuer.SendToAllClients<ChatSrvMsg>(msgData);
        }
        public static async Task MessageReceived(SocketMessage message)
        {
            if (message.Channel.Id == 1010603662780940299)
            {
                if (message.Author.Id == 1009878741285081149) return;
                LunaLog.ChatMessage($"[Discord] {message.Author.Username}: {message}");
                SendMessageToKSP($"{message.Author.Username}: {message}");
            }
        }
    }
}

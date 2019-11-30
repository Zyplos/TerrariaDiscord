using System;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using System.Net;
using System.IO;

namespace TerrariaDiscord
{

    [ApiVersion(2, 1)]
    public class TerrariaDiscord : TerrariaPlugin
    {
     
        public override string Name => "TerrariaDiscord";

        public override Version Version => new Version(1, 0, 0);

        public override string Author => "Zyplos";

        public override string Description => "A simple plugin that hooks to player chat and posts to a discord webhook.";

        public TerrariaDiscord(Main game) : base(game)
        {
           
        }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerChat.Register(this, OnChat);
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGameInitialize);
            ServerApi.Hooks.ServerJoin.Register(this, OnPlayerJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnPlayerLeave);
        }

        /// <summary>
        /// Used to dispose of and deregister hooks and resources.
        /// This indicates that the plugin is being destroyed and the server is shutting down.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGameInitialize);
                ServerApi.Hooks.ServerJoin.Deregister(this, OnPlayerJoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnPlayerLeave);
            }
            base.Dispose(disposing);
        }

        private void sendWebhookMessage(String text)
        {
            /// https://stackoverflow.com/questions/9145667/how-to-post-json-to-a-server-using-c
       
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://canary.discordapp.com/api/webhooks/650246617139642369/rRwC6IoWH219R0o0FZKpyuddIZxd4MBfSQBs936QeTLYbTZW2xPoRmDsnk_ttB1t2mr9");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{" +
                                $"\"content\": \"{text}\""
                                + "}";
            
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }
        }

        // [NOTE TO FUTURE SELF] Go to Definition (F12) on the args types to see what they contain
        private void OnChat(ServerChatEventArgs args)
        {
            sendWebhookMessage($"<{TShock.Players[args.Who].Name}> {args.Text}");
        }

        private void OnGameInitialize(EventArgs args)
        {
            sendWebhookMessage("✅ Terraria server started!");
        }

        private void OnPlayerJoin(JoinEventArgs args)
        {
            sendWebhookMessage($"```diff\\n+ {TShock.Players[args.Who].Name} joined the server.\\n```");
        }

        private void OnPlayerLeave(LeaveEventArgs args)
        {
            sendWebhookMessage($"```diff\\n- {TShock.Players[args.Who].Name} left the server.\\n```");
        }

        private void OnNpcSpawn(NpcSpawnEventArgs args)
        {
            sendWebhookMessage($"🏠 {Terraria.NPC.HeadIndexToType(args.NpcId)} moved in.");
        }

        private void OnNpcKilled(NpcKilledEventArgs args)
        {
            sendWebhookMessage($"💀 {args.npc.FullName} was slain.");
        }
    }
}
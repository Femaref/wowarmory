using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wowarmory;
using wowarmory.Network;
using wowarmory.Chat;

namespace ChatConsole
{
    class Chat
    {
        ChatModule chat;
        Session session;

        public Chat (string accountName, string password, string characterName, string realmName)
        {
            session = new Session();

            if (chat == null)
            {
                chat = new ChatModule(session, characterName, realmName);

                var msgDele = new ChatModule.OnMessageDelegate(OnMessage);
                chat.OnMessageGuildChat += msgDele;
                chat.OnMessageOfficerChat += msgDele;
                chat.OnMessageWhisper += msgDele;
                chat.OnMessageMOTD += msgDele;
                chat.OnPresenceChange += new ChatModule.OnPresenceDelegate(OnPresenceChange);
                chat.OnChatLoggedOut += new ChatModule.OnChatLoggedInOutDelegate(OnChatLoggedOut);
                chat.OnChatLoggedIn += new ChatModule.OnChatLoggedInOutDelegate(OnChatLoggedIn);

                chat.OnLoginFailed += new ChatModule.OnLoginFailedDelegate(OnLoginFailed);
            }

            session.Start(accountName, password);
            session.OnSessionClosed += new Session.OnSessionClosedDelegate(OnSessionClosed);

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version.ToString();
            Title = String.Format("Guild Chat ({0}/{1}) {2}", characterName, realmName, version);

            AppendLine("Logging into chat..");
        }

        public void Close()
        {
            chat.Close();
        }

        private void AppendLine(string text)
        {
            Console.WriteLine(text);
        }

        private string Title
        {
            get
            {
                return Console.Title;
            }
            set {
                Console.Title = value;
            }
        }

        void OnChatLoggedIn()
        {
            AppendLine(".. logged in.");
        }

        void OnLoginFailed(string reason)
        {
            OnSessionClosed(reason);
            session.Close();
        }

        void OnChatLoggedOut()
        {
            session.Close();
        }

        void OnSessionClosed(string reason)
        {
            if (!String.IsNullOrEmpty(reason))
                AppendLine("Connection closed: " + reason);
        }

        void OnPresenceChange(ChatModule module, Presence presence)
        {
            var arrow = "->";
            if (presence.Offline)
                arrow = "<-";

            AppendLine(String.Format("{0} {1} ({2})", arrow, presence.Name, presence.Type));
        }

        void OnMessage(ChatModule module, Message m)
        {
            if (m.Type == Message.CHAT_MSG_TYPE_GUILD_CHAT || m.Type == Message.CHAT_MSG_TYPE_OFFICER_CHAT)
            {
                AppendLine(String.Format("<{0}> {1}", m.CharacterName, m.Body));
            }
            else if (m.Type == Message.CHAT_MSG_TYPE_GUILD_MOTD)
            {
                AppendLine("MOTD: " + m.Body);
            }
            else if (m.Type == Message.CHAT_MSG_TYPE_WHISPER)
            {
                AppendLine(String.Format("whisper <{0}> {1}", m.CharacterName, m.Body));
            }
        }

        public void Process(String input)
        {
            var msg = input;
            if (msg.StartsWith("/"))
            {
                var tokens = msg.Split(' ');
                if (tokens[0] == "/whisper" || tokens[0] == "/w")
                {
                    if (tokens.Length < 3)
                    {
                        AppendLine("Usage: /whisper <target> <message>");
                    }
                    else
                    {
                        chat.SendWhisper(tokens[1], String.Join(" ", tokens, 2, tokens.Length - 2));
                    }
                }
                else if (tokens[0] == "/officer" || tokens[0] == "/o")
                {
                    if (tokens.Length < 2)
                    {
                        AppendLine("Usage: /officer <message>");
                    }
                    else
                    {
                        chat.SendMessage(String.Join(" ", tokens, 1, tokens.Length - 1), wowarmory.Chat.Message.CHAT_MSG_TYPE_OFFICER_CHAT);
                    }
                }
                else
                {
                    AppendLine("Unknown command: " + tokens[0]);
                    AppendLine("Available commands:");
                    AppendLine("/w, /whisper");
                }
            }
            else
            {
                chat.SendMessage(msg);
            }
        }
    }
}

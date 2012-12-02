using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Net;

namespace Steamirc
{
    class Irc
    {
        public const int NICKLEN = 9;
        public const int CHANLEN = 49;

        
        TcpListener tcpLsn;
        Socket newClient;
        String nick = "wheytest";

        byte[] buffer = new byte[32768]; // 32k

        





        public Irc()
        {}


        public void start()
        {
            tcpLsn = new TcpListener(new IPAddress(new byte[] { 0, 0, 0, 0 }), 8888); // lol fuck off deprecation
            tcpLsn.Start();
            newClient = tcpLsn.AcceptSocket();
        }


        public void SendString(String tosend)
        {
            newClient.Send(Encoding.ASCII.GetBytes(tosend + "\r\n"));
        }

        public void SendPM(String from, String message)
        {
            String a =(":" + from + " PRIVMSG " + nick + " :" + message);
            Console.WriteLine(a);
            SendString(a);
        }

        public void SendChannelMsg(String from, String channel, String message)
        {
            SendString(":" + from + " PRIVMSG " + channel + " :" + message);
        }

        public void SendChannelMsg(String message)
        {
            SendChannelMsg("steamirc", "#steamirc", message);
        }

        public void NoSuchNick(String to)
        {
            SendString("401 " + nick + " " + to + " :No such nick/channel");
        }

        public void sendNames(String channel, String[] users)
        {
            String tosend = "353 " + nick + " = " + channel + " :";
            foreach (String user in users)
            {
                tosend += user + " ";
            }
            Console.WriteLine(tosend);
            SendString(tosend);
            SendString("366 " + nick + " " + channel + " :End of /NAMES list.");
        }

        public void sendNames(String channel){
            sendNames(channel, new String[]{"@steamirc", nick});
        }

        public void join(String channel)
        {
            SendString("JOIN :" + channel);
        }





        public Message Receive()
        {
            int length = newClient.Receive(buffer, SocketFlags.None);
            String msg = Encoding.ASCII.GetString(buffer, 0, length).TrimEnd('\r', '\n');
            String to = "";
            MSGTYPE type = MSGTYPE.UNUSED;

            if (msg.IndexOf("PRIVMSG") == 0)
            {
                Console.WriteLine(msg);
                //type = MSGTYPE.PM;
                int i;
                for (i = 8; msg[i] != ' ' && i < msg.Length; i++) ; // 8 is len("PRIVMSG ")
                
                to = msg.Substring(8, i - 8);
                if(to[0] == '#'){
                    type = MSGTYPE.CHANMSG;
                }
                else{
                    type = MSGTYPE.PM;
                }


                msg = msg.Substring(i + 2, msg.Length - (i + 2));
                //Console.WriteLine(msg + msg.Length);
                //msg = msg.Substring(8, msg.Length - 8); // 8 is len("PRIVMSG ")

            }

            /*if (msg.IndexOf("NICK") == 0)
            {
                type = MSGTYPE.UNUSED;
                //Console.WriteLine(msg);
                //nick = msg.Substring(5, msg.Length-5);
                //Console.WriteLine(nick + nick.Length);
                
            }*/

            // Skip nick setting, nick is forced
            else if (msg.IndexOf("USER") == 0)
            {
                type = MSGTYPE.UNUSED;
                Console.WriteLine("IRC user connected");
                SendString("001 " + nick);
                SendString("002 " + nick);
                SendString("003 " + nick);
                SendString("004 " + nick);
                String tosend = "MODE " + nick + " :+i";
                Console.WriteLine(tosend);
                SendString(tosend);

                join("#steamirc");
                sendNames("#steamirc");
            }

            else if (msg.IndexOf("JOIN") == 0)
            {
                to = Util.pysubstr(msg, 5, 0);
                type = MSGTYPE.JOIN;
            }

            else if (msg.IndexOf("PING") == 0)
            {
                type = MSGTYPE.UNUSED;
                Console.WriteLine("PONG");
                SendString("PONG");
            }

            else if (msg.IndexOf("QUIT") == 0)
            {
                type = MSGTYPE.QUIT;
            }

            return new Message(to, msg, type);
        }

        public void Stop(){
            newClient.Close();
            tcpLsn.Stop();
        }
    }

    public enum MSGTYPE
    {
        UNUSED,
        PM,
        CHANMSG,
        JOIN,
        QUIT
    }

    class Message
    {
        public String to;
        public String msg;
        public MSGTYPE type;

        public Message(String to, String msg, MSGTYPE type)
        {
            this.to = to;
            this.msg = msg;
            this.type = type;
        }
    }
        
}

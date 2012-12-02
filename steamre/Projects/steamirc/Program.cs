using System;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using SteamKit2;
using System.Collections;


namespace Steamirc
{

    class Program
    {
        static Steam steam;
        static Irc irc;
        //static TwoWayDict<String, SteamID> nicks;
        //static TwoWayDict<String, SteamID> clans;


        // Gets steam users list, and initialises nicklist
        /*private static void getNicks()
        {
            Dictionary<SteamID, idcache> namecache = steam.getSteamIDs();
            nicks = new TwoWayDict<string, SteamID>();

            String nick;
            foreach (KeyValuePair<SteamID, idcache> pair in namecache)
            {
                if (pair.Value.name.Length != 0)
                {
                    nick = Util.fixName(pair.Value.name, nicks, Irc.NICKLEN);
                    //nicks.Add(fixNick(pair.Value), pair.Key);
                    nicks[nick] = pair.Key;
                    Console.WriteLine("ircnick: " + nick);
                }
            }
        }*/

        /* private static void getClans()
         {
             clans = new TwoWayDict<string, SteamID>();
             List<SteamID> tempclans = steam.getClans();

             foreach (SteamID clan in tempclans)
             {
                 clans.Add(steam.getClanName(clan), clan);
             }
         }*/

        /*static int lol = 25;
        public static void test(Action<int> testfunc){
            testfunc(lol);
        }*/

        //static int lol = 25;

        // Exactly what it says on the tin.
        // Passed to Steam object, where it is used when a PM comes in
        private static void relayPmToIrc(SteamID id, String msg)
        {
            irc.SendPM(steam.nicks[id], msg);
        }

        public static void Main()
        {
            irc = new Irc();
            steam = new Steam(irc);
            irc.start();
            

            //irc.join("#steamirc");
            //irc.sendNames("#steamirc");
            //getNicks();


            //getClans();

            //List<SteamID> friends = steam.getFriends();




            Message msg;
            do
            {
                msg = irc.Receive();
                switch (msg.type)
                {
                    case MSGTYPE.PM:
                        {
                            if (steam.nicks.Contains(msg.to))
                            {
                                steam.sendpm(steam.nicks[msg.to], msg.msg);
                            }
                            else
                            {
                                irc.NoSuchNick(msg.to);
                            }
                            break;
                        }

                    case MSGTYPE.CHANMSG:
                        {
                            //Console.WriteLine("chanmsg");
                            //irc.SendChannelMsg("steamirc", msg.to, msg.msg);
                            
                            /* Send message to steam chat, if clan exists, and we are joined to it's chat */
                            if (steam.clans.Contains(msg.to) && steam.chanstates[steam.clans[msg.to]] == chanstate.joined)
                            {
                                steam.sendChanmsg(steam.clans[msg.to], msg.msg);
                            }

                            if (msg.to == "#steamirc")
                            {
                                msg.msg = msg.msg.ToLower();

                                if (msg.msg.StartsWith("list"))
                                {
                                    String listwhat;
                                    try { listwhat = msg.msg.Split(' ')[1]; }
                                    catch { break; }

                                    if (listwhat == "friends")
                                    {
                                        foreach (SteamID friend in steam.friends)
                                        {
                                            irc.SendChannelMsg(steam.nicks[friend] + " " + steam.getFriendState(friend));
                                        }
                                    }
                                    else if (listwhat == "clans")
                                    {
                                        foreach (var pair in steam.clans)
                                        {
                                            irc.SendChannelMsg(pair.Key);
                                        }

                                    }
                                }
                            }


                            break;
                        }

                    case MSGTYPE.JOIN:
                        {
                            Console.WriteLine("joining " + msg.to);
                            if (steam.clans.Contains(msg.to))
                            {
                                 
                                steam.joinchat(steam.clans[msg.to]);
                                while (steam.chanstates[steam.clans[msg.to]] != chanstate.joined) ;

                                List<String> names = new List<string>();

                                //SteamID a = steam.clans[msg.to];
                                List<SteamID> ids = steam.clanchatters[steam.clans[msg.to]];
                                foreach(SteamID id in ids){
                                    names.Add(steam.nicks[id]);
                                    Console.WriteLine(steam.nicks[id]);
                                }

                                irc.sendNames(msg.to, names.ToArray());
                            }
                            //irc.sendNames(msg.to);
                            break;
                        }

                    default:
                        {
                            break;
                        }
                }

            } while (msg.type != MSGTYPE.QUIT);

            irc.Stop();
            steam.disconnect();


            Console.WriteLine("fin");
            Console.ReadLine();


        }
    }

}
    
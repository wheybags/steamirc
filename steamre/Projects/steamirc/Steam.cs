using System;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using SteamKit2;


namespace Steamirc
{
    public enum chanstate
    {
        waiting,
        joined,
        denied
    }

    class Steam
    {
        static SteamClient steamClient = new SteamClient(); // initialize our client
        static SteamUser steamUser = steamClient.GetHandler<SteamUser>(); // we'll use this later to logon
        static SteamFriends steamFriends = steamClient.GetHandler<SteamFriends>(); // get the friends handler


        static Dictionary<SteamID, idcache> namecache = new Dictionary<SteamID, idcache>();
        public TwoWayDict<String, SteamID> nicks = new TwoWayDict<string, SteamID>();
        public List<SteamID> friends = new List<SteamID>();

        public TwoWayDict<String, SteamID> clans = new TwoWayDict<string, SteamID>();
        public Dictionary<SteamID, List<SteamID>> clanchatters = new Dictionary<SteamID, List<SteamID>>();
        public Dictionary<SteamID, chanstate> chanstates = new Dictionary<SteamID, chanstate>();

        
        bool ready = false; // Indicates if steam is ready to use yet

        //public TwoWayDict<String, SteamID> getNicks

        public Dictionary<SteamID, idcache> getSteamIDs()
        {
            while (!ready) ;
            return namecache;
        }

        public void controlFunc()
        {
            //steamFriends.SetPersonaState(SteamKit2.EPersonaState.Online);
            //sprint("ONLINE");


            //Console.WriteLine(steamFriends.GetClanCount());
            /*SteamID chatid = steamFriends.GetClanByIndex(0);
            steamFriends.JoinChat(chatid);

            steamFriends.SendChatRoomMessage(chatid, SteamKit2.EChatEntryType.ChatMsg, "abcdefg");
            Thread.Sleep(1000);
            */

            
            //String a = Regex.Replace("abc de\f", " ", "\\ ");



            /*String input;
            while (true)
            {
                input = Console.ReadLine();

                // Format:
                // PM STEAMID message
                if (input.IndexOf("PM") == 0)
                {
                    sendpm(input.Substring(3));
                }

                // Print list of friends in format:
                // FRIEND STEAMID NAME
                else if (input.IndexOf("FRIENDLIST") == 0)
                {
                    listFriends();
                }

                else if(input.IndexOf("INCHANNEL") == 0){
                    SteamID person;
                    for (int i = 0; i != steamFriends.(); i++)
                    {
                        person = steamFriends.GetFriendByIndex(i);
                        Console.WriteLine("FRIEND " + person + " " + escape(steamFriends.GetFriendPersonaName(person)));
                    }
                }


            }*/

            //namecache.

            /*SteamID friend;
            for (int i = 0; i != steamFriends.GetFriendCount(); i++)
            {
                friend = steamFriends.GetFriendByIndex(i);

                namecache[friend] = steamFriends.GetFriendPersonaName(friend);
                Console.WriteLine("FRIEND " + friend + " " + namecache[friend]);
            }*/

            //Console.WriteLine("asdsa " + namecache[new SteamID("STEAM_0:1:17023099")]);
        }

        // Disconnect from steam
        public void disconnect()
        {
            steamClient.Disconnect();
        }

        public void listFriends()
        {
            SteamID friend;
            for (int i = 0; i != steamFriends.GetFriendCount(); i++)
            {
                friend = steamFriends.GetFriendByIndex(i);
                Console.WriteLine("FRIEND " + friend + " " + steamFriends.GetFriendPersonaName(friend));
            }
        }

        public List<SteamID> getFriends()
        {
            List<SteamID> friends = new List<SteamID>();
            for (int i = 0; i < steamFriends.GetFriendCount(); i++)
            {
                SteamID friend = steamFriends.GetFriendByIndex(i);
                friends.Add(friend);
                /*if (!nicks.Contains(friend))
                {
                    nicks[friend] = Util.fixName(steamFriends.GetFriendPersonaName(friend), nicks, Irc.NICKLEN);
                }*/

            }
            return friends;
        }

        public TwoWayDict<String, SteamID> getClans()
        {
            TwoWayDict<String, SteamID> clans = new TwoWayDict<String, SteamID>();
            for (int i = 0; i < steamFriends.GetClanCount(); i++)
            {
                clans.Add("#" + Util.fixName(steamFriends.GetClanName(steamFriends.GetClanByIndex(i)), clans, Irc.CHANLEN), steamFriends.GetClanByIndex(i));
            }
            return clans;
        }

        public String getClanName(SteamID clan)
        {
            return steamFriends.GetClanName(clan);
        }

        public EPersonaState getFriendState(SteamID id)
        {
            return namecache[id].state;
        }


        public void sendpm(SteamID recipient, String msg)
        {
            while (!ready) ; // Wait until finsihed logging in
            steamFriends.SendChatMessage(recipient, EChatEntryType.ChatMsg, msg);
        }


        public void sprint(String msg)
        {
            Console.WriteLine("STATUS " + msg);
        }

        public void pmprint(SteamID id, String msg)
        {
            Console.WriteLine("PM_IN " + id + " " + steamFriends.GetFriendPersonaName(id) + " " + msg);
        }

        public void grpprint(SteamID room, SteamID from, String msg)
        {
            Console.WriteLine("GRP_IN " + room + " " + from + " " + steamFriends.GetFriendPersonaName(from) + " " + msg);
        }

        public void joinchat(SteamID channel)
        {
            chanstates[channel] = chanstate.waiting;
            steamFriends.JoinChat(channel);
        }

        public void sendChanmsg(SteamID clan, String message)
        {
            steamFriends.SendChatRoomMessage(clan, EChatEntryType.ChatMsg, message);
        }

        public Steam(Irc irc)
        {
            Thread steamloop = new Thread(() => SteamLoop(irc));
            steamloop.Start();
            while (!ready) ; // wait until ready
        }

        private void SteamLoop(Irc irc){
             

            string userName = "test";
            string passWord = "test";


            steamClient.Connect(); // connect to the steam network

            bool finished = false;
            while (!finished)
            {
                CallbackMsg msg = steamClient.WaitForCallback(true); // block and wait until a callback is posted
                msg.Handle<SteamClient.ConnectedCallback>(callback =>
                {
                    // the Handle function will call this lambda method for this callback

                    if (callback.Result != EResult.OK)
                    {
                        // break; // the connect result wasn't OK, so something failed
                        sprint("STEAM_CONNECT_FAILED");
                        return;
                    }

                    sprint("STEAM_CONNECT_SUCCESS");

                    // we've successfully connected to steam3, so lets logon with our details
                    steamUser.LogOn(new SteamUser.LogOnDetails
                    {
                        Username = userName,
                        Password = passWord,
                    });
                });

                // Handle logging in
                msg.Handle<SteamUser.LoggedOnCallback>(callback =>
                {
                    if (callback.Result != EResult.OK)
                    {
                        // logon failed! 
                        sprint("LOGIN_FAILED_REASON " + callback.Result);
                        if (callback.Result == EResult.AccountLogonDenied)
                        {
                            sprint("steamguard is shite: Disable it");
                        }
                        return;
                    }

                    sprint("LOGIN_SUCCEEDED");
                    // we've now logged onto Steam3
                });


                // Print incoming pms, messages with newlines will be treated as multiple messages
                msg.Handle<SteamFriends.FriendMsgCallback>(callback =>
                {
                    if (callback.EntryType == EChatEntryType.ChatMsg)
                    {
                        String[] lines = callback.Message.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string s in lines)
                        {
                            pmprint(callback.Sender, s);
                            //relayPmToIrc(callback.Sender, s);
                            irc.SendPM(nicks[callback.Sender], s);
                        }
                    }
                });

                // Print incoming group chat messages, messages with newlines will be treated as multiple messages
                msg.Handle<SteamFriends.ChatMsgCallback>(callback =>
                {
                    if (callback.ChatMsgType == EChatEntryType.ChatMsg)
                    {
                        String[] lines = callback.Message.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string s in lines)
                        {
                            grpprint(callback.ChatRoomID, callback.ChatterID, s);

                            /* Turn the chat id into the clan id */
                            callback.ChatRoomID.AccountType = EAccountType.Clan;
                            callback.ChatRoomID.AccountInstance = 0;

                            irc.SendChannelMsg(nicks[callback.ChatterID], clans[callback.ChatRoomID], s);
                        }
                    }
                    else
                    {
                        Console.WriteLine("AJHGJHGDJ");
                    }
                });

                // Handle disconnection
                // Only happens when you've been disconnected for a while
                msg.Handle<SteamClient.DisconnectedCallback>(callback =>
                {
                    Console.WriteLine("DISCONNECTED");
                    finished = true;
                });


                msg.Handle<SteamFriends.ChatEnterCallback>(callback =>
                {
                    Console.WriteLine(callback.EnterResponse);
                    //Console.WriteLine(clanchatters[callback.ClanID]);
                    chanstates[callback.ClanID] = chanstate.joined;
                });


                // Alerts name changes
                msg.Handle<SteamFriends.PersonaStateCallback>(callback =>
                {
                    SteamID id = callback.FriendID;
                    String name = steamFriends.GetFriendPersonaName(id);

                   
                    /* Add users to channel list */
                    if (callback.SourceSteamID.AccountType == EAccountType.Chat)
                    {
                        SteamID asclan = callback.SourceSteamID;
                        asclan.AccountType = EAccountType.Clan;
                        asclan.AccountInstance = 0;
                        //Console.WriteLine(steamFriends.GetClanName(asclan) + " " + name);

                        if (!clanchatters.ContainsKey(asclan))
                        {
                            clanchatters.Add(asclan, new List<SteamID>());
                        }
                        clanchatters[asclan].Add(id);
                        Console.WriteLine(name + " in " + clans[asclan]);
                    }

                    
                    
                    EPersonaState state = steamFriends.GetFriendPersonaState(id);

                    //Console.WriteLine(name + " " + state);

               
                    if (id.AccountType == EAccountType.Individual && name.Length > 0)
                    {
                        if (!namecache.ContainsKey(id))
                        {
                            namecache[id] = new idcache(name, state);
                            nicks.Add(Util.fixName(name, nicks, Irc.NICKLEN), id);
                        }
                        else
                        {
                            if (namecache[id].name != name)
                            {
                                //namecache[id] = new idcache(name, state);
                                namecache[id].name = name;
                                //Console.WriteLine("name change " + id + " " + name + " " + id.AccountType);
                            }

                            if (namecache[id].state != state)
                            {
                                namecache[id].state = state;
                                //Console.WriteLine("state change " + name + " " + state);
                            }
                        }
                    }
                });

                msg.Handle<SteamFriends.ChatMemberInfoCallback>(callback =>
                {
                    Console.WriteLine("asdhg " + callback.StateChangeInfo.ChatterActedOn);
                });

                // Start doing stuff once ready
                msg.Handle<SteamUser.LoginKeyCallback>(callback =>
                {
                    sprint("READY");
                    ready = true; // Allows other parts to start doing stuff
 
                    steamFriends.SetPersonaState(SteamKit2.EPersonaState.Online);
                    sprint("ONLINE");

                    // Initialise steamid cache
                    /*SteamID friend;
                    String name;
                    EPersonaState state;
                    for (int i = 0; i != steamFriends.GetFriendCount(); i++)
                    {
                        friend = steamFriends.GetFriendByIndex(i);
                        name = steamFriends.GetFriendPersonaName(friend);
                        state = steamFriends.GetFriendPersonaState(friend);

                        namecache[friend] = new idcache(name, state);
                        Console.WriteLine("FRIEND " + friend + " " + namecache[friend].name);

                        Thread.Sleep(1000);
                        Console.WriteLine("a----------------" + steamFriends.GetClanName(steamFriends.GetClanByIndex(0)));
                    }*/
                    friends = getFriends();
                    clans = getClans();

                    //Thread.Sleep(1000);
                    //steamFriends.JoinChat(steamFriends.GetClanByIndex(0));
                });

            }
        }
    }

    // Cache object for one steamID
    class idcache{
        public String name;
        public EPersonaState state;

        public idcache(String name, EPersonaState state){
            this.name = name;
            this.state = state;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using System.Collections;

namespace Steamirc
{
    // What it says on the tin
    // Warning: do not add two of the same key, it'll break shit
    public class TwoWayDict<K, V> : IEnumerable<KeyValuePair<K,V>>
    {
        private Dictionary<K, V> dict;
        private Dictionary<V, K> reverse;

        public TwoWayDict()
        {
            Console.WriteLine();

            dict = new Dictionary<K,V>();
            reverse = new Dictionary<V,K>();
        }

        public IEnumerator<KeyValuePair<K,V>> GetEnumerator()
        {
            foreach (var pair in dict)
            {
                yield return pair;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(K a, V b){
            dict.Add(a, b);
            reverse.Add(b, a);
        }

        public void Add(V a, K b)
        {
            reverse.Add(a, b);
            dict.Add(b, a);
        }

        public void Remove(K key)
        {
            reverse.Remove(dict[key]);
            dict.Remove(key);
        }

        public void Remove(V key)
        {
            dict.Remove(reverse[key]);
            reverse.Remove(key);
        }

        public bool Contains(K key)
        {
            return dict.ContainsKey(key);
        }

        public bool Contains(V key)
        {
            return reverse.ContainsKey(key);
        }

        public V this[K key]
        {
            get { return dict[key]; }
            set
            {
                dict[key] = value;
                reverse[value] = key;
            }
        }

        public K this[V key]
        {
            get { return reverse[key]; }
            set
            {
                reverse[key] = value;
                dict[value] = key;
            }
        }

    }


    
    class Util
    {
       
        public static String pysubstr(String orig, int start, int end) // python style substring 
        {
            int len = orig.Length;
            if (end > len || end == 0) // if end is 0, or too big, make it just be the end of the string
            {
                end = len;
            }

            if (end < 0)
            { // if end is negative, minus that many chars off the orig strings end
                end = len + end;
            }

            try
            {
                return orig.Substring(start, end - start);
            }
            catch
            {
                return "";
            }
        }

        // Makes a steam name into a valid nick/chan name, and guarantees no collision
        public static String fixName(String sname, TwoWayDict<String, SteamID> existing, int len)
        {
            //Console.WriteLine(sname + " " + (sname == "").ToString());

            sname = sname.Replace(' ', '_');

            int i;
            // Find first valid character for nick start
            // NOTE: nubers and the '-' character are valid in nicks, but not at the beginning 
            for (i = 0; !(Char.IsLetter(sname[i]) || sname[i] == '_' || sname[i] == '\\' || sname[i] == '[' ||
                    sname[i] == ']' || sname[i] == '{' || sname[i] == '}' || sname[i] == '^' || sname[i] == '`' || sname[i] == '|') && i != sname.Length - 1; i++) ;

            String nick = "";

            //Console.WriteLine(i);
            for (; nick.Length < len && i < sname.Length; i++)
            {
                // Filter out invalid characters, and truncate
                if (Char.IsLetterOrDigit(sname[i]) || sname[i] == '_' || sname[i] == '-' || sname[i] == '\\' || sname[i] == '[' ||
                    sname[i] == ']' || sname[i] == '{' || sname[i] == '}' || sname[i] == '^' || sname[i] == '`' || sname[i] == '|')
                {
                    nick += sname[i];
                }
            }

            // Force uniqueness
            while (existing.Contains(nick))
            {
                nick = increment(nick, len);
            }

            //Console.WriteLine(nick);
            return nick;
        }

        // Will add numbers onto the end of a string, and increment them if they're already there
        private static String increment(String nick, int len)
        {
            int i;
            for (i = nick.Length - 1; Char.IsDigit(nick[i]) && i >= 0; i--) ;

            i++;

            String num = "0";
            if (i != nick.Length)
            {
                num = "" + (Int32.Parse(nick.Substring(i, nick.Length - i)) + 1);
            }

            nick = nick.Substring(0, i);

            if (num.Length >= len)
            {
                return "lol"; // 2 people's names were "999999999", or 1000000000 people used the same name AKA never gonna fucking happen
            }
            while (nick.Length + num.Length > len)
            {
                nick = nick.Remove(nick.Length - 1);
            }

            return nick + num;
        }
    }
}

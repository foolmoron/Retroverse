using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Retroverse
{
    public static class Names
    {
        public static readonly char[] VOWELS = new char[]{ 'a', 'e', 'i', 'o', 'u', 'y' };
        private static Random rand = new Random();
        public static List<string> prefixes = new List<string>();
        public static List<string> suffixes = new List<string>();
        public const int CHAR_LIMIT = 9;

        static Names()
        {
            if (File.Exists("Content\\nameprefixes.txt") && File.Exists("Content\\namesuffixes.txt"))
            {
                using (StreamReader sr1 = File.OpenText("Content\\nameprefixes.txt"), sr2 = File.OpenText("Content\\namesuffixes.txt"))
                {
                    string s = "";
                    while ((s = sr1.ReadLine()) != null)
                    {
                        prefixes.Add(s.Trim());
                    }
                    while ((s = sr2.ReadLine()) != null)
                    {
                        suffixes.Add(s.Trim());
                    }
                }
            }
            else
            {
                prefixes.Add("Def");
                suffixes.Add("ault");
            }
        }

        public static string getRandomName(int repeatLimit = 5)
        {
            String pre = prefixes[rand.Next(prefixes.Count)];
            String suf = suffixes[rand.Next(suffixes.Count)];
            if (repeatLimit > 0 && ((pre+suf).Length >= CHAR_LIMIT || (VOWELS.Contains(pre[pre.Length - 1]) && VOWELS.Contains(suf[0])) || (!VOWELS.Contains(pre[pre.Length - 1]) && !VOWELS.Contains(suf[0]))))
                return getRandomName(repeatLimit - 1); //just give whatever you have after 5 tries
            else return pre + suf;
        }

        public static string getRandomPrefix()
        {
            return prefixes[rand.Next(prefixes.Count)];
        }

        public static string getRandomSuffix()
        {
            return suffixes[rand.Next(suffixes.Count)];
        }

        public static string getPrefix(int index)
        {
            if (index < 0 || index >= prefixes.Count)
                throw new ArgumentOutOfRangeException("index", "Must be between 0 and max number of prefixes (" + prefixes.Count + ")");
            return prefixes[index];
        }

        public static string getSuffix(int index)
        {
            if (index < 0 || index >= suffixes.Count)
                throw new ArgumentOutOfRangeException("index", "Must be between 0 and max number of suffixes (" + suffixes.Count + ")");
            return suffixes[index];
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCfixer
{
    public class Pitchmap
    {
        public List<string> Pitches;
        public string Dir;
        public Dictionary<string, string> Suffixes;
        public Dictionary<string, string> Prefixes;
        public string[] PrefixesArray;
        public string[] SuffixesArray;

        public Pitchmap(string dir)
        {
            Pitches = new List<string>();
            Suffixes = new Dictionary<string, string>();
            Prefixes = new Dictionary<string, string>();
            var prefixesArray = new List<string>();
            var suffixesArray = new List<string>();
            string filename = dir;
            if (!File.Exists(filename)) filename = Path.Combine(dir, @"prefix.map");
            if (!File.Exists(filename)) return;
            var lines = File.ReadAllLines(filename);
            Dir = filename;
            foreach (string line in lines)
            {
                var pr = line.Trim('\r', '\n').Split('\t');
                var prefix = pr[1];
                if (prefix.Length > 0 && !prefixesArray.Contains(prefix))
                {
                    Pitches.Add(prefix);
                    prefixesArray.Add(prefix);
                }
                var suffix = pr[2];
                if (suffix.Length > 0 && !suffixesArray.Contains(suffix))
                {
                    Pitches.Add(suffix);
                    suffixesArray.Add(suffix);
                }
                Prefixes[pr[0]] = prefix;
                Suffixes[pr[0]] = suffix;
            }
            suffixesArray.Add("");
            prefixesArray.Add("");
            SuffixesArray = suffixesArray.ToArray();
            PrefixesArray = prefixesArray.ToArray();
        }

    }

}

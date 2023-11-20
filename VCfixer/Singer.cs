using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace VCfixer
{
    public enum Order
    {
        AppendPitch,
        PitchAppend
    }

    public class Singer
    {
        public static List<Singer> Singers = new List<Singer>();
        public static Dictionary<string, Singer> SingerNames = new Dictionary<string, Singer>();

        public static Singer Current;

        public string Name { get; private set; }
        public string Author { get; private set; }
        public string Image { get; private set; }
        public string Sample { get; private set; }
        public string Dir { get; private set; }
        public string Type { get; private set; }
        public string Readme { get; private set; }
        public List<string> Subs { get; private set; }
        public List<Oto> Otos { get; private set; }
        public bool IsEnabled { get; private set; }
        public Pitchmap Pitchmap { get; private set; }

        public string[] Prefixes { get; set; }
        public string[] Suffixes { get; set; }

        public Order PrefixOrder = Order.PitchAppend;
        public Order SuffixOrder = Order.AppendPitch;

        public string GetOrderedPrefix(string append, string pitch)
        {
            if (PrefixOrder == Order.AppendPitch)
                return append + pitch;
            else
                return pitch + append;

        }

        public string GetOrderedSuffix(string append, string pitch)
        {
            if (SuffixOrder == Order.AppendPitch)
                return append + pitch;
            else
                return pitch + append;
        }

        private Dictionary<string, Oto> OtoDictionary { get; set; }

        public Singer(string dir)
        {
            Current = this;
            Dir = dir;
            CheckVoicebank();
            if (!IsEnabled) return;
            CharLoad();
            Pitchmap = new Pitchmap(Dir);
            Load();
        }

        void Add()
        {
            Singers.Add(this);
            SingerNames[Name] = this;
        }

        void CheckVoicebank()
        {
            if (!Directory.Exists(Dir))
            {
                IsEnabled = false;
                return;
            }
            Subs = Directory.EnumerateDirectories(Dir).Select(n => Path.GetFileName(n)).ToList();
            Subs.Add("");
            IsEnabled = false;
            foreach (string sub in Subs)
            {
                string subdir = Path.Combine(Dir, sub, "oto.ini");
                if (File.Exists(subdir))
                {
                    IsEnabled = true;
                    return;
                }
            }
            IsEnabled = false;
            return;
        }

        private void CharLoad()
        {
            string charfile = Path.Combine(Dir, "character.txt");
            if (IsEnabled && File.Exists(charfile))
            {
                string[] charlines = File.ReadAllLines(charfile);
                foreach (string line in charlines)
                {
                    if (line.StartsWith("author=")) Author = line.Substring("author=".Length);
                    if (line.StartsWith("image=")) Image = line.Substring("image=".Length);
                    if (line.StartsWith("name=")) Name = line.Substring("name=".Length);
                    if (line.StartsWith("sample=")) Sample = line.Substring("sample=".Length);
                    if (line.StartsWith("VoicebankType=")) Type = line.Substring("VoicebankType=".Length);
                    if (line.StartsWith("type=")) Type = line.Substring("type=".Length);
                }
            }
            if (Name == null) Name = Path.GetFileName(Dir);
        }

        public void Load()
        {
            Otos = new List<Oto> { };
            foreach (string sub in Subs)
            {
                string filename = Path.Combine(Dir, sub, "oto.ini");
                if (File.Exists(filename))
                {
                    string[] lines = File.ReadAllLines(filename);
                    foreach (string line in lines)
                    {
                        string pattern = "(.*)=(.*),(.*),(.*),(.*),(.*),(.*)";
                        var arr = Regex.Split(line, pattern);
                        double temp;
                        if (arr.Length == 1) continue;
                        Oto Oto = new Oto()
                        {
                            File = Path.Combine(sub, arr[1]),
                            Alias = arr[2],
                            Offset = double.TryParse(arr[3], out temp) ? temp : 0,
                            Consonant = double.TryParse(arr[4], out temp) ? temp : 0,
                            Cutoff = double.TryParse(arr[5], out temp) ? temp : 0,
                            Preutterance = double.TryParse(arr[6], out temp) ? temp : 0,
                            Overlap = double.TryParse(arr[7], out temp) ? temp : 0,
                        };
                        Otos.Add(Oto);
                    }
                }
                else File.Create(filename);
            }

            OtoDictionary = new Dictionary<string, Oto>();
            foreach (var oto in Otos)
                OtoDictionary[oto.Alias + oto.Suffix] = oto;
        }

        public Oto FindOto(UNote note)
        {
            string notenum = MusicMath.NoteNum2String(note.NoteNum);
            string suffix = Pitchmap.Suffixes.ContainsKey(notenum) ? Pitchmap.Suffixes[notenum] : "";
            string prefix = Pitchmap.Prefixes.ContainsKey(notenum) ? Pitchmap.Prefixes[notenum] : "";
            
            return FindOto(note.CleanLyric, note.Prefix, note.Suffix, prefix, suffix);
        }

        public Oto FindOto(string lyric, string append_prefix = "", string append_suffix = "",
                            string pitch_prefix = "", string pitch_suffix = "")
        {
            if (IsEnabled)
            {
                //foreach (Oto Oto in Otos)
                //    if (prefix + Oto.Alias + suffix == lyric)
                //        return Oto;
                //foreach (Oto Oto in Otos)
                //    if (Oto.Alias + suffix == lyric)
                //        return Oto;
                //foreach (Oto Oto in Otos)
                //    if (prefix + Oto.Alias == lyric)
                //        return Oto;
                //foreach (Oto Oto in Otos)
                //    if (Oto.Alias == lyric)
                //        return Oto;
                //return null;
                var prefix = GetOrderedPrefix(append_prefix, pitch_prefix);
                var suffix = GetOrderedSuffix(append_suffix, pitch_suffix);

                foreach (string alias in new[]
                {
                    prefix + lyric + suffix,
                    prefix + lyric + append_suffix,
                    append_prefix + lyric + suffix,
                    lyric + suffix,
                    prefix + lyric,
                    lyric + append_suffix,
                    append_prefix + lyric,
                    pitch_prefix + lyric + pitch_suffix,
                    lyric + pitch_suffix,
                    pitch_prefix + lyric,
                    lyric
                })
                {
                    if (OtoDictionary.ContainsKey(alias))
                        return OtoDictionary[alias];
                }
                return null;
            }
            return null;
        }

        /// <summary>
        /// extract pitch&append prefixes and suffixes
        /// </summary>
        public void SeparateOto()
        {
            foreach (var oto in Otos)
            {
                _findFixes(oto);
            }
        }
        
        private void _findFixes(Oto oto)
        {
            foreach (var pitch_suffix in Pitchmap.SuffixesArray)
            {
                foreach (var pitch_prefix in Pitchmap.PrefixesArray)
                {
                    foreach (var append_suffix in Suffixes)
                    {
                        foreach (var append_prefix in Prefixes)
                        {
                            var prefix = pitch_prefix + append_prefix;
                            var suffix = append_suffix + pitch_suffix;
                            if (PrefixOrder == Order.AppendPitch)
                                prefix = append_prefix + pitch_prefix;
                            if (SuffixOrder == Order.PitchAppend)
                                suffix = pitch_suffix + append_suffix;

                            if (oto.Alias == "ka_N")
                                Console.WriteLine();

                            if (oto.Alias.StartsWith(prefix) && oto.Alias.EndsWith(suffix))
                            {
                                oto.Suffix = suffix;
                                oto.Prefix = prefix;
                                oto.Alias = oto.Alias.Substring(prefix.Length, oto.Alias.Length - prefix.Length - suffix.Length);
                                return;
                            }
                        }
                    }
                }
            }

        }
    }
}

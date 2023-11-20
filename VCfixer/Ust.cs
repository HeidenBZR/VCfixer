using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
using System.Text;

namespace VCfixer
{
    public enum Insert : int
    {
        Before = -1,
        After = 0,
        Append = 1
    }

    public class Ust
    {
        public string VoiceDir;
        public double Tempo;
        public double Version;
        public List<UNote> MainNotes;
        public UNote[] Notes { get; set; }

        public bool IsLoaded = false;
        public string Dir;

        public void TakeOut(string line, string name, out string value) { value = line.Substring(name.Length + 1); }
        public void TakeOut(string line, string name, out int value) { value = int.Parse(line.Substring(name.Length + 1), new CultureInfo("ja-JP")); }
        public void TakeOut(string line, string name, out double value) { value = double.Parse(line.Substring(name.Length + 1), new CultureInfo("ja-JP")); }
        public string TakeIn(string name, dynamic value) { return $"{name}={value}"; }

        public static Ust Current;

        public Ust(string dir)
        {
            Dir = dir;
            if (!File.Exists(dir))
                throw new Exception("Error: temp ust doesn't exist");

            string[] lines = File.ReadAllLines(Dir, System.Text.Encoding.GetEncoding(932));
            Read(lines);
            Current = this;
        }

        public Ust()
        {

        }

        public Ust Clone()
        {
            Ust ust = new Ust()
            {
                VoiceDir = VoiceDir,
                Tempo = Tempo,
                Version = Version,
                Dir = Dir
            };
            List<UNote> notes = new List<UNote>();
            foreach (UNote note in Notes)
            {
                notes.Add(note.Clone());
            }
            ust.Notes = notes.ToArray();
            return ust;
        }

        public void Reload()
        {
            string[] lines = File.ReadAllLines(Dir);
            Read(lines);
        }

        public string[] Save()
        {
            string[] text = GetText();
            File.WriteAllLines(Dir, text, Encoding.GetEncoding(932));
            Console.WriteLine("Successfully saved UST.");
            return text;
        }

        public void Save(string dir)
        {
            string[] text = GetText();
            File.WriteAllLines(dir, text);
            Console.WriteLine("Successfully saved debug UST.");
        }


        private void Read(string[] lines)
        {
            int i = 0;
            // Reading version
            if (lines[0] == Number.Version)
            {
                Version = 1.2;
                i++;
                i++;
            }
            if (lines[i] != Number.Setting) throw new Exception("Error UST reading");
            else i++;

            while (i < lines.Length && !Number.IsNote(lines[i]))
            {
                if (lines[i].StartsWith("UstVersion")) TakeOut(lines[i], "UstVersion", out Version);
                if (lines[i].StartsWith("Tempo")) TakeOut(lines[i], "Tempo", out Tempo);
                if (lines[i].StartsWith("VoiceDir")) TakeOut(lines[i], "VoiceDir", out VoiceDir);
                i++;
            }

            List<UNote> notes = new List<UNote>();
            UNote note = new UNote();
            while (i + 1 < lines.Length)
            {
                note = new UNote();
                note.Number = lines[i];
                Number.LastNumber = note.Number;
                i++;
                while (!Number.IsNote(lines[i]))
                {
                    string line = lines[i];
                    if (lines[i].StartsWith("Length=")) 
                        note.Length = int.Parse(line.Substring("Length=".Length), new CultureInfo("ja-JP"));
                    if (lines[i].StartsWith("NoteNum="))
                        note.NoteNum = int.Parse(line.Substring("NoteNum=".Length), new CultureInfo("ja-JP"));
                    if (lines[i].StartsWith("Lyric="))
                        note.ReadLyric(line.Substring("Lyric=".Length));
                    if (lines[i].StartsWith("Flags="))
                        note.Flags = line.Substring("Flags=".Length);
                    i++;
                    Console.WriteLine(i);
                    if (i == lines.Length) break;
                }
                note.InitLength = note.Length;
                notes.Add(note);
            }
            Notes = notes.ToArray();

            Console.WriteLine("Read UST successfully");
            IsLoaded = true;
            Console.WriteLine(String.Join("\r\n", GetText()));
        }

        public void ValidateLyrics()
        {
            foreach (UNote note in Notes) note.Lyric = note.Lyric;
        }

        public bool IsTempUst(string Dir)
        {
            string filename = Dir.Split('\\').Last();
            return filename.StartsWith("tmp") && filename.EndsWith("tmp");
        }

        public string[] GetText()
        {
            List<string> text = new List<string> { };
            if (Version == 1.2)
            {
                text.Add(Number.Version);
                text.Add("UST Version " + Version.ToString());
                text.Add(Number.Setting);
            }
            else
            {
                text.Add(Number.Setting);
                text.Add(TakeIn("Version", Version));
            }
            text.Add(TakeIn("Tempo", Tempo));
            text.Add(TakeIn("VoiceDir", VoiceDir));
            for (int i = 0; i < Notes.Length; i++)
            {
                UNote note = Notes[i];
                text.AddRange(note.GetText(i == Notes.Length - 1));
            }
            return text.ToArray();
        }

        public UNote GetNextNote(UNote note)
        {
            List<UNote> notes = Notes.ToList();
            int ind = notes.IndexOf(note);
            int newInd = ind + 1;
            if (newInd >= notes.Count) return null;
            return notes[newInd];
        }

        public UNote GetPrevNote(UNote note)
        {
            List<UNote> notes = Notes.ToList();
            int ind = notes.IndexOf(note);
            int newInd = ind - 1;
            if (newInd < 0) return null;
            return notes[newInd];
        }

        public void SeparatePrefixes(string[] prefixes, string[] suffixes)
        {
            foreach (var note in Notes)
            {
                note.CleanLyric = note.Lyric;
                foreach (var prefix in prefixes)
                {
                    if (note.CleanLyric.StartsWith(prefix))
                    {
                        note.Prefix = prefix;
                        note.CleanLyric = note.CleanLyric.Substring(prefix.Length);
                        break;
                    }
                }
                foreach (var suffix in suffixes)
                {
                    if (note.CleanLyric.EndsWith(suffix))
                    {
                        note.Suffix = suffix;
                        note.CleanLyric = note.CleanLyric.Substring(0, note.CleanLyric.Length - suffix.Length);
                        break;
                    }
                }
            }
        }
    }
}

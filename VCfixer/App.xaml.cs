using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace VCfixer
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static App Current;

        public string Dir;
        public Ust Ust;
        public Singer Singer;

        public App()
        {
            Current = this;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                Dir = e.Args[0];
                Ust = new Ust(Dir);
                Singer = new Singer(Ust.VoiceDir);
            }
        }

        public void FixVC(string[] vowels, double velocity)
        {
            for (int i = 0; i + 2 < Ust.Notes.Length; i++)
            {
                UNote prev = Ust.Notes[i];
                UNote note = Ust.Notes[i + 1];
                UNote next = Ust.Notes[i + 2];
                if (note.Lyric.Contains("et"))
                    Console.WriteLine();
                if (note.MustSkip) continue;
                if (next.CleanLyric == "R") continue;
                if (!vowels.Any(n => prev.CleanLyric.EndsWith(n))) continue;
                if (vowels.Any(n => note.CleanLyric == n)) continue;
                if (vowels.Any(n => n + "-" == note.CleanLyric)) continue;
                if (vowels.Any(n => note.CleanLyric.EndsWith(n))) continue;
                if (next.CleanLyric.Trim() == "") continue;
                int length = prev.Length + note.Length;
                Oto oto = Singer.FindOto(note);
                Oto nextoto = Singer.FindOto(next);
                Console.WriteLine($"{note.Lyric}");
                Console.WriteLine($"{note.CleanLyric}");
                if (oto is null || nextoto is null) continue;
                double value = oto.InnerLength;
                if (!vowels.Any(n => note.CleanLyric.StartsWith(n)) && note.CleanLyric.StartsWith("-"))
                {
                    /// -C
                    value += nextoto.Preutterance < nextoto.Overlap ? nextoto.Overlap : nextoto.Preutterance;
                }
                else if (note.CleanLyric.Contains("-") && !note.CleanLyric.StartsWith("-"))
                    /// VC-
                    //value = value + nextoto.Preutterance < nextoto.Overlap ? nextoto.Overlap : nextoto.StraightPreutterance;
                    value += nextoto.Preutterance < nextoto.Overlap ? nextoto.Overlap : nextoto.Preutterance;
                else
                    /// VC
                    value += nextoto.Preutterance < nextoto.Overlap ? nextoto.Overlap : nextoto.Preutterance;
                //note.Length = MusicMath.MillisecondToTick(oto.FixedVowel + nextoto.StraightPreutterance, Ust.Tempo);
                value = (value / velocity);
                note.Length = MusicMath.MillisecondToTick(value, Ust.Tempo) + 1;
                Console.WriteLine($"VC {note.Length}");
                if (note.Length + 10 < length)
                    prev.Length = length - note.Length;
                else
                {
                    prev.Length = 10;
                    note.Length = length - 10;
                }
                Console.WriteLine($"VC {note.Length}");
                Console.WriteLine($"prev {prev.Length}");
            }
            Ust.Save();
        }
    }

}

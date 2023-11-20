using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace VCfixer
{

    class Number
    {

        public const string Next = "[#NEXT]";
        public const string Prev = "[#PREV]";
        public const string Insert = "[#INSERT]";
        public const string Delete = "[#DELETE]";
        public const string Version = "[#VERSION]";
        public const string Setting = "[#SETTING]";
        public static string LastNumber;

        public static bool IsNote(string number)
        {
            if (number.Length < 6) return false;
            if (number == Next) return true;
            if (number == Prev) return true;
            return int.TryParse(number.Substring(2, 4), out int i);
        }

        public static string GetNumber(int i)
        {
            return $"[#{i.ToString().PadLeft(4, '0')}]";
        }

        public static int GetInt(string number)
        {
            //if (number == "[#NEXT]") return Ust.Notes.Count();
            return int.Parse(number.Substring(2, 4));
        }
    }

    public class UNote
    {
        public Singer Singer { get { return Singer.Current; } }
        public object UI { get; set; }
        public int InitLength { get; set; }
        public int Preutterance { get; set; }
        public int NoteNum { get; set; }
        //public List<Subnote> Subnotes;
        public UNote Parent { get; set; }
        public List<UNote> Children { get; set; }

        private string _number;
        private string _lyric;
        private int _length;
        private string _flags = "";

        public string CleanLyric = "";
        public string Prefix = "";
        public string Suffix = "";

        public string Lyric { get => _lyric; set => _lyric = value; }
        public string Number { get => _number; set => _number = value; }
        public string Flags { get => _flags; set => _flags = value; }
        public bool IsRest { get; set; }
        public int Length
        {
            get { return _length; }
            set
            {
                if (value <= 0)
                {
                    throw new Exception($"Got negative note length on note {Lyric} ");
                    Length = 10;
                }
                else _length = value;
            }
        }
        public bool MustSkip
        {
            get
            {
                return Flags.Contains("x");
            }
        }

        public UNote()
        {
            Children = new List<UNote>();
            
        }


        public UNote Clone()
        {
            UNote note = new UNote()
            {
                Number = Number,
                Length = Length,
                Preutterance = Preutterance,
                Lyric = Lyric,
                NoteNum = NoteNum,
                InitLength = InitLength,
                IsRest = IsRest
            };
            return note;
        }

        public string[] GetText(bool last = false)
        {
            string lyric = Lyric;
            if (lyric == "r") lyric = "rr";
            string number = Number == VCfixer.Number.LastNumber ? VCfixer.Number.Insert : Number;
            if (last) number = VCfixer.Number.LastNumber;
            List<string> text = new List<string>
            {
                number,
                $"Length={Length}",
                $"Lyric={lyric}",
                $"NoteNum={NoteNum}",
                $"Flags={Flags}"
            };
            if (Number == VCfixer.Number.Insert) text.Add("Modulation=0");
            return text.ToArray();
        }

        public void ReadLyric(string lyric, bool keepRest = false)
        {
            Lyric = lyric;
        }

    }

}

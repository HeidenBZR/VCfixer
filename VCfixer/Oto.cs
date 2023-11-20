using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VCfixer
{

    public class Oto
    {
        public string File;
        public string Alias;
        public string Prefix = "";
        public string Suffix = "";
        public double Offset;
        public double Consonant;
        public double Cutoff;
        public double Preutterance;
        public double Overlap;
        public double FullLength
        {
            get
            {
                if (Cutoff < 0) return Math.Abs(Cutoff);
                Wav wav = new Wav(File);
                return wav.Length - Cutoff - Offset;
            }
        }
        public double StraightPreutterance
        {
            get
            {
                return Math.Abs(Preutterance - Overlap);
            }
        }
        public double Length
        {
            get
            {
                if (Preutterance > Overlap)
                    return FullLength - Preutterance;
                else
                    return FullLength + Preutterance - Overlap;
            }
        }
        public double InnerLength
        {
            get
            {
                if (Preutterance > Overlap)
                    return 0;
                else
                    return Overlap - Preutterance;
            }
        }
        public double FixedVowel
        {
            get
            {
                return Consonant - Preutterance;
            }
        }
    }
}

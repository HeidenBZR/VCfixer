using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VCfixer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class VCFixerWindow : Window
    {
        public static VCFixerWindow Current;


        public VCFixerWindow()
        {
            InitializeComponent();
            Current = this;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonFix_Click(object sender, RoutedEventArgs e)
        {
            var vowels = TextboxVowels.Text.Split(',');
            var velocity = 1.0;
            if (double.TryParse(TextboxVelocity.Text, out double result))
                velocity = result;
            var prefixes = (TextboxPrefixes.Text + ",").Split(',');
            var suffixes = (TextboxSuffixes.Text + ",").Split(',');

            if (radiobuttonPrefixOrderAP.IsChecked.Value)
                Singer.Current.PrefixOrder = Order.AppendPitch;
            if (radiobuttonSuffixOrderPA.IsChecked.Value)
                Singer.Current.SuffixOrder = Order.PitchAppend;
            Singer.Current.Suffixes = suffixes;
            Singer.Current.Prefixes = prefixes;
            Singer.Current.SeparateOto();

            Ust.Current.SeparatePrefixes(prefixes, suffixes);
            App.Current.FixVC(vowels, velocity);
            Close();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using System.Windows.Threading;

namespace VideoTool
{
    public partial class MainWindow : Window
    {
        private static string filePath = App.FilePath;

        public MainWindow()
        {
            InitializeComponent();
#if !DEBUG
            this.MainGrid.Children.Remove(this.TextDevBuild);
#endif
            
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => Main()));
        }

        static bool videoLoaded = false;
        private async void Main()
        {
            this.TextThreads.Text = Environment.ProcessorCount.ToString();

            foreach (MenuItem item in this.MenuListPreset.Items)
            {
                item.Click += (object sender, RoutedEventArgs e) =>
                {
                    this.MenuListPreset.Header = $"Preset: {item.Header}";
                };
            }
            foreach (MenuItem item in this.MenuListPreset2.Items)
            {
                item.Click += (object sender, RoutedEventArgs e) =>
                {
                    this.MenuListPreset2.Header = $"Audio channels: {item.Header}";
                };
            }

            if (!String.IsNullOrEmpty(filePath))
            {
                this.TextPath.Text = filePath;
                this.MediaPlayer.Source = new Uri(filePath);
                while (!videoLoaded) await Task.Delay(100);
                this.SliderVideo.Minimum = 0;
                this.SliderVideo.Maximum = this.MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                this.TextEndVideo.Text = DateTime.Parse(this.MediaPlayer.NaturalDuration.TimeSpan.ToString()).ToString("mm:ss");
                this.MediaPlayer.LoadedBehavior = MediaState.Manual;
                this.MediaPlayer.UnloadedBehavior = MediaState.Manual;
                this.MediaPlayer.SpeedRatio = 0.00000000000001;
                this.MediaPlayer.Play();
                SetBuffering(false);
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(async () =>
                {
                    string lastValue = "";
                    while (true)
                    {
                        if (this.TextSliderValue.Content.ToString() == this.MediaPlayer.Position.TotalSeconds.ToString() 
                        && lastValue == this.TextSliderValue.Content.ToString()) SetBuffering(true);
                        else SetBuffering(false);
                        lastValue = this.TextSliderValue.Content.ToString();
                        this.TextSliderValue.Content = this.MediaPlayer.Position.TotalSeconds.ToString();
                        await Task.Delay(100);
                    }
                }));
            }
        }

        private void SetBuffering(bool @switch)
        {
            if (@switch && (bool)this.BoxShowBuffering.IsChecked)
            {
                this.LabelBuffering.Visibility = Visibility.Visible;
                this.SquareBuffering.Visibility = Visibility.Visible;
            }
            else
            {
                this.LabelBuffering.Visibility = Visibility.Hidden;
                this.SquareBuffering.Visibility = Visibility.Hidden;
            }
        }

        private void ButtonBrowsePath_Click(object sender, RoutedEventArgs e)
        {

        }

        double nowSeconds = 0;
        double nowMiliseconds = 0;
        private async void SliderVideo_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.MediaPlayer.Position = new TimeSpan(0, 0, (int)this.SliderVideo.Value);
            nowSeconds = this.MediaPlayer.Position.Seconds;
            nowMiliseconds = this.MediaPlayer.Position.TotalMilliseconds;
            /*this.MediaPlayer.Play();
            await Task.Delay(10);
            this.MediaPlayer.Stop();*/
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            videoLoaded = true;
        }

        private void TextCRF_KeyUp(object sender, KeyEventArgs e)
        {
            if (!int.TryParse(this.TextCRF.Text, out int crf) || crf < 0 || crf > 51 || this.TextCRF.Text.Length > 2) this.TextCRF.Text = "23";
        }

        private void ButtonRender_Click(object sender, RoutedEventArgs e)
        {
            string cut = "";
            string audioChannels = "";
            string compress = "";
            string threads = "";
            string audioValue = this.MenuListPreset2.Header.ToString().Trim().Split(':')[1];
            //if (int.Parse(this.TextStartVideo.Text) >= int.Parse(this.TextEndVideo.Text)) return;
            if (this.TextStartVideo.Text != "00:00") cut = $"-ss {this.TextStartVideo.Text} -to {this.TextEndVideo.Text}";
            if (audioValue == "first") audioChannels = "-map 0:a:0";
            else if (audioValue == "second") audioChannels = "-map 0:a:1";
            else if (audioValue == "both") audioChannels = "-map 0:a:0 -map 0:a:1";
            if ((bool)this.BoxCompression.IsChecked) compress = $"-preset {this.MenuListPreset.Header.ToString().Trim().Split(':')[1]} -crf {this.TextCRF.Text}";

            Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-i \"{filePath}\" {threads} {cut} -map 0:v:0 {audioChannels} {compress} \"{filePath}.output.mp4\"", //-progress progressinfo.txt
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });

            Environment.Exit(0);
        }

        private void TextThreads_KeyUp(object sender, KeyEventArgs e)
        {
            if (!int.TryParse(this.TextThreads.Text, out int threads) || threads > Environment.ProcessorCount || threads < 0) 
                this.TextThreads.Text = Environment.ProcessorCount.ToString();
        }
    }
}

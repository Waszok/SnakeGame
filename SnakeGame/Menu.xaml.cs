using System; //EventHandler, Uri, TimeSpan
using System.Windows; //RoutedEventArgs
using System.Windows.Controls; //UserControl, Image
using System.Windows.Media; //MediaPlayer
using System.Windows.Media.Animation; //Storyboard - snake animation 
using System.Windows.Media.Imaging; //BitmapImage - sound button image (on/off) 

namespace SnakeGame
{
    public sealed partial class Menu : UserControl
    {
        private static Menu _menu = null;
        private static readonly object m_oPadLock = new object();
        public static MediaPlayer MenuMusic { get; set; } = new MediaPlayer();
        public static MediaPlayer ClickSound { get; set; } = new MediaPlayer();
        public static bool OnOff { get; set; } = true; //Czy soundBtn wlaczony, czy wylaczony
        public static double Volume { get; set; } //Przechowujemy obecna glosnosc dzwieku w menu
        private Menu()
        {
            InitializeComponent();
            Loaded += AnimatingControl_Loaded;
            PlayMusic();
            Volume = 0;
        }
        public static Menu Instance
        {
            get
            {
                lock (m_oPadLock)
                {
                    if (_menu == null)
                    {
                        _menu = new Menu();
                    }
                    return _menu;
                }
            }
        }
        /// <summary>
        /// Dzwiek klikniecia
        /// </summary>
        public static void PlayClickSound()
        {
            ClickSound.Open(new Uri(System.IO.Path.GetFullPath("../../" + "Sounds/clickSound.mp3"), UriKind.RelativeOrAbsolute));
            ClickSound.Play();
        }
        /// <summary>
        /// Odtwarzanie muzyki
        /// </summary>
        private void PlayMusic()
        {
            MenuMusic.Open(new Uri(System.IO.Path.GetFullPath("../../" + "Sounds/Homelander.mp3"), UriKind.RelativeOrAbsolute));
            MenuMusic.MediaEnded += new EventHandler(Media_Repeat);
            MenuMusic.Play();
        }
        private void Media_Repeat(object sender, EventArgs e)
        {
            MenuMusic.Position = TimeSpan.Zero;
            MenuMusic.Play();
        }
        /// <summary>
        /// Przycisk do dzwieku
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSound_Click(object sender, RoutedEventArgs e)
        {
            PlayClickSound();
            Image image = new Image();
            if (OnOff == true)
            {
                image.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../" + "Images/soundOffImage.png"), UriKind.RelativeOrAbsolute));
                OnOff = false;
                Volume = MenuMusic.Volume;
                MenuMusic.Volume = 0;
                Options.Instance.switchBgMusic.IsChecked = false;
                Options.Instance.volumeBarMenu.IsEnabled = false;
            }
            else
            {
                image.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../" + "Images/soundOnImage.png"), UriKind.RelativeOrAbsolute));
                OnOff = true;
                Options.Instance.switchBgMusic.IsChecked = true;
                Options.Instance.volumeBarMenu.IsEnabled = true;
                Console.WriteLine(Volume);
                Console.WriteLine(Options.VolumeMenuTmp);
                if (Volume == 0) MenuMusic.Volume = Options.VolumeMenuTmp / 100;
                else MenuMusic.Volume = Volume;
            }
            btnSound.Content = image;
        }
        private void AnimatingControl_Loaded(object sender, RoutedEventArgs e)
        {
            Storyboard sb = FindResource("AnimateImage") as Storyboard;
            sb.Begin();
        }
        private void Singleplayer_Click(object sender, RoutedEventArgs e)
        {
            PlayClickSound();
            Window.GetWindow(this).Content = Single.Instance;
        }
        private void Multiplayer_Click(object sender, RoutedEventArgs e)
        {
            PlayClickSound();
            Window.GetWindow(this).Content = Multi.Instance;
        }
        private void Options_Click(object sender, RoutedEventArgs e)
        {
            PlayClickSound();
            Window.GetWindow(this).Content = Options.Instance;
            Options.Instance.VolumeMenu = MenuMusic.Volume * 100;
        }
        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            PlayClickSound();
            Application.Current.Shutdown();
        }
    }
}

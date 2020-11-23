using System; //Uri
using System.Collections.Generic; //List<string>
using System.ComponentModel; //INotifyPropertyChanged (volume bar)
using System.Threading; //Thread.Sleep() (interface sound button)
using System.Windows; //RoutedEventArgs (przy button click)
using System.Windows.Controls; //UserControl
using System.Windows.Controls.Primitives; //ToggleButton
using System.Windows.Input; //Obsluga myszy (volume bar)
using System.Windows.Media.Imaging; //BitmapImage - snake skin

namespace SnakeGame
{
    public sealed partial class Options : UserControl, INotifyPropertyChanged
    {
        private static Options _options = null;
        private static readonly object o_oPadLock = new object();
        private int _speedMode = 1;
        private int _speedModeValue = 40;
        public static List<string> Textures { get; set; } = new List<string> {
            "Images/defaultTex.bmp",
            "Images/abstractSkinTex.bmp",
            "Images/yellowSkinTex.bmp",
            "Images/waterColorTex.bmp",
            "Images/skinRedTex.bmp",
            "Images/redTex.bmp",
            "Images/leopardTex.bmp",
            "Images/leafTex.bmp",
            "Images/blackCircleTex.bmp",
            "Images/barkTex.bmp",
            "Images/abstractColorTex.bmp",
            "Images/colorfulTex.bmp"
        };
        private int _indexSkin;
        public static bool GameMusic { get; set; } = true;
        private double _volumeMenu = Menu.MenuMusic.Volume * 100;
        public static double VolumeMenuTmp { get; set; } = Menu.MenuMusic.Volume * 100;
        private double _volumeForStart = Menu.Volume * 100;
        private double _volumeGame = 10;
        private double _volumeGameTmp = 10;
        private bool mouseCaptured = false;

        public double VolumeMenu
        {
            get { return _volumeMenu; }
            set
            {
                _volumeMenu = value;
                OnPropertyChanged("VolumeMenu");
            }
        }
        public double VolumeGame
        {
            get { return _volumeGame; }
            set
            {
                _volumeGame = value;
                OnPropertyChanged("VolumeGame");
            }
        }
        private Options()
        {
            InitializeComponent();
            DataContext = this;
        }
        public static Options Instance
        {
            get
            {
                lock (o_oPadLock)
                {
                    if (_options == null)
                    {
                        _options = new Options();
                    }
                    return _options;
                }
            }
        }
        /// <summary>
        /// Zmiana predkosci weza
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSpeedClick(object sender, RoutedEventArgs e)
        {
            Menu.PlayClickSound();
            switch (_speedMode)
            {
                case 1:
                    snakeSpeedBtnType.Content = "FAST";
                    _speedMode++;
                    _speedModeValue = 25;
                    break;
                case 2:
                    snakeSpeedBtnType.Content = "SLOW";
                    _speedMode++;
                    _speedModeValue = 75;
                    break;
                case 3:
                    snakeSpeedBtnType.Content = "NORMAL";
                    _speedMode = 1;
                    _speedModeValue = 40;
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Zmiana tekstury
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSkinClick(object sender, RoutedEventArgs e)
        {
            Menu.PlayClickSound();
            _indexSkin++;
            if (_indexSkin == Textures.Count)
            {
                _indexSkin = 0;
            }
            texSnakeImg.Source = new BitmapImage(new Uri(Textures[_indexSkin], UriKind.RelativeOrAbsolute));
            snakeTexture.Content = texSnakeImg;
        }
        // Wlaczanie/wylaczanie dziewkow, multilife
        private void SwitchButtonMultiChecked(object sender, RoutedEventArgs e)
        {
            Menu.PlayClickSound();
            var button = sender as ToggleButton;
            button.Content = "ON";
            PlayerData.NumberOfLifes = 3;
            PlayerData.MultiOnOff = true;
        }
        private void SwitchButtonMultiUnchecked(object sender, RoutedEventArgs e)
        {
            Menu.PlayClickSound();
            var button = sender as ToggleButton;
            button.Content = "OFF";
            PlayerData.NumberOfLifes = 1;
            PlayerData.MultiOnOff = false;
        }
        private void SwitchButtonBgChecked(object sender, RoutedEventArgs e)
        {
            Menu.PlayClickSound();
            var button = sender as ToggleButton;
            button.Content = "ON";
            if (volumeBarMenu != null) volumeBarMenu.IsEnabled = true;
            if (VolumeMenuTmp == 0) VolumeMenu = _volumeForStart;
            else VolumeMenu = VolumeMenuTmp;
            Menu.MenuMusic.Volume = VolumeMenu / 100;
            Image image = new Image
            {
                Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../" + "Images/soundOnImage.png"), UriKind.RelativeOrAbsolute))
            };
            Menu.Instance.btnSound.Content = image;
            Menu.OnOff = true;
        }
        private void SwitchButtonBgUnchecked(object sender, RoutedEventArgs e)
        {
            Menu.PlayClickSound();
            var button = sender as ToggleButton;
            button.Content = "OFF";
            if (volumeBarMenu != null) volumeBarMenu.IsEnabled = false;
            VolumeMenuTmp = VolumeMenu;
            VolumeMenu = 0;
            Menu.MenuMusic.Volume = 0;
            Image image = new Image
            {
                Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../" + "Images/soundOffImage.png"), UriKind.RelativeOrAbsolute))
            };
            Menu.Instance.btnSound.Content = image;
            Menu.OnOff = false;
        }
        private void SwitchButtonInterChecked(object sender, RoutedEventArgs e)
        {
            Menu.ClickSound.Volume = 1;
            Menu.PlayClickSound();
            var button = sender as ToggleButton;
            button.Content = "ON";
        }
        private void SwitchButtonInterUnchecked(object sender, RoutedEventArgs e)
        {
            Menu.PlayClickSound();
            var button = sender as ToggleButton;
            button.Content = "OFF";
            Thread.Sleep(100);
            Menu.ClickSound.Volume = 0;
        }
        private void SwitchButtonGMsChecked(object sender, RoutedEventArgs e)
        {
            Menu.PlayClickSound();
            var button = sender as ToggleButton;
            button.Content = "ON";
            if (volumeBarGame != null) volumeBarGame.IsEnabled = true;
            GameMusic = true;
            VolumeGame = _volumeGameTmp;
        }
        private void SwitchButtonGMsUnchecked(object sender, RoutedEventArgs e)
        {
            Menu.PlayClickSound();
            var button = sender as ToggleButton;
            button.Content = "OFF";
            if (volumeBarGame != null) volumeBarGame.IsEnabled = false;
            GameMusic = false;
            _volumeGameTmp = VolumeGame;
            VolumeGame = 0;

        }
        // Przesuwanie (glosnosc)
        private new void MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed && mouseCaptured)
            {
                var x = e.GetPosition((ProgressBar)sender).X;
                var ratio = x / ((ProgressBar)sender).ActualWidth;
                if (sender.Equals(volumeBarMenu)) {
                    VolumeMenu = ratio * ((ProgressBar)sender).Maximum;
                    Menu.MenuMusic.Volume = _volumeMenu / 100;
                    Menu.Volume = _volumeMenu / 100; 
                }
                else VolumeGame = ratio * ((ProgressBar)sender).Maximum;
                Menu.MenuMusic.Volume = _volumeMenu / 100;
                Console.WriteLine("przesuwam Menu" + VolumeMenu + ", " + "przesuwam Game " + VolumeGame);
            }
        }
        // Klikanie (glosnosc)
        private new void MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseCaptured = true;
            var x = e.GetPosition((ProgressBar)sender).X;
            var ratio = x / ((ProgressBar)sender).ActualWidth;
            if (sender.Equals(volumeBarMenu)) {
                VolumeMenu = ratio * ((ProgressBar)sender).Maximum;
                Menu.MenuMusic.Volume = _volumeMenu / 100;
                Menu.Volume = _volumeMenu / 100;
            } 
            else VolumeGame = ratio * ((ProgressBar)sender).Maximum;
            Console.WriteLine("klikam Menu" + VolumeMenu + ", " + "klikam Game " + VolumeGame);
        }
        private new void MouseUp(object sender, MouseButtonEventArgs e)
        {
            mouseCaptured = false;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); // jesli jest rozny od null, to ustawiamy
                                                                                       // PropertyChanged
        }
        /// <summary>
        /// Powrot do menu glownego
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackOptionMenu(object sender, RoutedEventArgs e)
        {
            Menu.PlayClickSound();
            PlayerData.SnakeSkin = Textures[_indexSkin];
            PlayerData.SnakeSkinIndex = _indexSkin;
            PlayerData.SnakeSpeed = _speedModeValue;
            PlayerData.VolumeGame = VolumeGame;
            Window.GetWindow(this).Content = Menu.Instance;
        }
    }
}

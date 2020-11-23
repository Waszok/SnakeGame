using System;
using System.Media;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SnakeGame
{
    public partial class WpfMessageBox : Window
    {
        public enum MessageBoxType
        {
            //ConfirmationWithYesNo = 0,
            //ConfirmationWithYesNoCancel,
            Information = 0,
            Error,
            GameOver,
            GameOverMulti,
            Pause,
            Warning
        }

        public enum MessageBoxImage
        {
            Warning = 0,
            Information,
            Error,
            GameOver,
            GameOverMulti,
            Pause,
            None
        }
        public WpfMessageBox()
        {
            InitializeComponent();
        }
        private static MediaPlayer _gameOverSound = new MediaPlayer();
        static WpfMessageBox _messageBox;
        static MessageBoxResult _result = MessageBoxResult.No;
        public static MessageBoxResult Show
        (string caption, string msg, MessageBoxType type)
        {
            switch (type)
            {
                case MessageBoxType.Pause:
                    return Show(caption, msg, MessageBoxButton.YesNo,
                    MessageBoxImage.Pause);
                case MessageBoxType.GameOver:
                    return Show(caption, msg, MessageBoxButton.YesNoCancel,
                    MessageBoxImage.GameOver);
                case MessageBoxType.Information:
                    return Show(caption, msg, MessageBoxButton.OK,
                    MessageBoxImage.Information);
                case MessageBoxType.Error:
                    return Show(caption, msg, MessageBoxButton.OK,
                    MessageBoxImage.Error);
                case MessageBoxType.Warning:
                    return Show(caption, msg, MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                default:
                    return MessageBoxResult.No;
            }
        }
        public static MessageBoxResult Show(string msg, MessageBoxType type)
        {
            return Show(string.Empty, msg, type);
        }
        public static MessageBoxResult Show(string msg)
        {
            return Show(string.Empty, msg,
            MessageBoxButton.OK, MessageBoxImage.None);
        }
        public static MessageBoxResult Show
        (string caption, string text)
        {
            return Show(caption, text,
            MessageBoxButton.OK, MessageBoxImage.None);
        }
        public static MessageBoxResult Show
        (string caption, string text, MessageBoxButton button)
        {
            return Show(caption, text, button,
            MessageBoxImage.None);
        }
        public static MessageBoxResult Show
        (string caption, string text,
        MessageBoxButton button, MessageBoxImage image)
        {
            _messageBox = new WpfMessageBox
            { txtMsg = { Text = text }, MessageTitle = { Text = caption } };
            SetVisibilityOfButtons(button);
            SetImageOfMessageBox(image);
            _messageBox.ShowDialog();
            return _result;
        }
        private static void SetVisibilityOfButtons(MessageBoxButton button)
        {
            switch (button)
            {
                case MessageBoxButton.OK: // Invalid symbol (IP) (and connection problem)
                    _messageBox.btnCancel.Visibility = Visibility.Collapsed;
                    _messageBox.btnBack.Visibility = Visibility.Collapsed;
                    _messageBox.btnTryAgain.Visibility = Visibility.Collapsed;
                    _messageBox.btnResume.Visibility = Visibility.Collapsed;
                    _messageBox.btnOk.Focus();
                    break;
                case MessageBoxButton.OKCancel: // Game Paused
                    _messageBox.btnOk.Visibility = Visibility.Collapsed;
                    _messageBox.btnBack.Visibility = Visibility.Collapsed;
                    _messageBox.btnTryAgain.Visibility = Visibility.Collapsed;
                    _messageBox.btnCancel.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNo: // Game Over (Single)
                    _messageBox.btnOk.Visibility = Visibility.Collapsed;
                    _messageBox.btnCancel.Visibility = Visibility.Collapsed;
                    _messageBox.btnResume.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNoCancel: // GameOverMulti
                    _messageBox.btnCancel.Visibility = Visibility.Collapsed;
                    _messageBox.btnResume.Visibility = Visibility.Collapsed;
                    _messageBox.btnTryAgain.Visibility = Visibility.Collapsed;
                    _messageBox.btnOk.Visibility = Visibility.Collapsed;
                    _messageBox.btnBack.Focus();
                    break;
                default:
                    break;
            }
        }
        private static void SetImageOfMessageBox(MessageBoxImage image)
        {
            switch (image)
            {
                case MessageBoxImage.Warning: // Invalid symbol (Ip)
                    SystemSounds.Asterisk.Play();
                    _messageBox.SetImage("warningImg.png");
                    break;
                case MessageBoxImage.Error: // Connection problem ...
                    SystemSounds.Asterisk.Play();
                    _messageBox.SetImage("errorImg.png");
                    break;
                case MessageBoxImage.Information: //?
                    SystemSounds.Exclamation.Play();
                    _messageBox.SetImage("gameOverImg.png");
                    break;
                case MessageBoxImage.Pause: //Game Paused
                    _messageBox.SetImage("gamePauseImg.png");
                    break;
                case MessageBoxImage.GameOver: //Game Over
                    _gameOverSound.Open(new Uri(System.IO.Path.GetFullPath("../../" + "Sounds/gameOverSound.mp3"), UriKind.RelativeOrAbsolute));
                    _gameOverSound.Play();
                    _messageBox.SetImage("gameOverImg.png");
                    break;
                default:
                    SystemSounds.Exclamation.Play();
                    _messageBox.img.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender == btnOk)
                _result = MessageBoxResult.OK;
            else if (sender == btnTryAgain)
                _result = MessageBoxResult.Yes;
            else if (sender == btnBack)
                _result = MessageBoxResult.No;
            else if (sender == btnCancel)
                _result = MessageBoxResult.Cancel;
            else
                _result = MessageBoxResult.None;
            if (_messageBox == null)
            {
                _messageBox = this;
            }
            _messageBox.Close();
            _messageBox = null;
        }
        // Wstawienie obrazu do MessageBoxa:
        private void SetImage(string imageName)
        {
            string uri = string.Format("Images/{0}", imageName);
            var uriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
            img.Source = new BitmapImage(uriSource);
        }
        // Obsluga przyciskow w przypadku Game Over (Single):
        private void TryAgainClick(object sender, RoutedEventArgs e)
        {
            _result = MessageBoxResult.Yes;
            Menu.PlayClickSound();
            _messageBox.Close();
            _messageBox = null;
            Application.Current.MainWindow.Content = new GamePlay();
        }
        private void BackClick(object sender, RoutedEventArgs e)
        {
            _result = MessageBoxResult.No;
            Menu.PlayClickSound();
            _messageBox.Close();
            _messageBox = null;
            Application.Current.MainWindow.Content = Menu.Instance;
            if (Menu.OnOff == true)
            {
                Menu.MenuMusic.Play();
            }
            else
            {
                Menu.MenuMusic.Play();
                Menu.MenuMusic.Volume = 0;
            }
        }
        // Obsluga przycisku w przypadku pauzy:
        private void ResumeClick(object sender, RoutedEventArgs e)
        {
            _result = MessageBoxResult.OK;
            Menu.PlayClickSound();
            _messageBox.Close();
            _messageBox = null;
            GamePlay.GameMusic.Play();
            GamePlay.Timer.Start();
        }
    }
}
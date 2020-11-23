using System.Windows; //RoutedEventArgs
using System.Windows.Controls; //UserControl

namespace SnakeGame
{
    public sealed partial class Single : UserControl
    {
        private static Single _single = null;
        private static readonly object sg_oPadLock = new object();
        public Single()
        {
            InitializeComponent();
        }
        public static Single Instance
        {
            get
            {
                lock (sg_oPadLock)
                {
                    if (_single == null)
                    {
                        _single = new Single();
                    }
                    return _single;
                }
            }
        }
        private void PlayClickS(object sender, RoutedEventArgs e)
        {
            Menu.PlayClickSound();
            PlayerData.Nick = textBoxNickS.Text;
            PlayerData.GameMode = 0;
            Menu.MenuMusic.Stop();
            Window.GetWindow(this).Content = new GamePlay();
        }
        private void BackSingleMenu(object sender, RoutedEventArgs e)
        {
            Menu.PlayClickSound();
            Window.GetWindow(this).Content = Menu.Instance;
        }
    }
}

using System; //Console + Exception
using System.Net.Sockets;
using System.Windows; // Event + MessageBox
using System.Windows.Controls; // Dziedziczenie po UserControl
using System.Windows.Input; // Potrzebne do CheckSpell (e.Key)

namespace SnakeGame
{
    public sealed partial class Multi : UserControl
    {
        private static Multi _multi = null;
        private static readonly object mul_oPadLock = new object();
        public static TcpClient clientSocket;
        private bool _tmp = true;
        public Multi()
        {
            InitializeComponent();
        }
        public static Multi Instance
        {
            get
            {
                lock (mul_oPadLock)
                {
                    if (_multi == null)
                    {
                        _multi = new Multi();
                    }
                    return _multi;
                }
            }
        }
        /// <summary>
        /// Sprawdzenie poprawnosci wpisywanych znakow adresu ip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckSpell(object sender, KeyEventArgs e)
        {
            if((e.Key>=Key.D0 && e.Key<=Key.D9) || e.Key==Key.Back || (e.Key>=Key.NumPad0
                && e.Key<=Key.NumPad9) || e.Key == Key.OemPeriod || e.Key == Key.Enter)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
                var messageBoxResult = WpfMessageBox.Show ("Warning", "Invalid symbol", MessageBoxButton.OK, WpfMessageBox.MessageBoxImage.Warning);
            }
        }
        /// <summary>
        /// Rozpoczecie gry
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayClickM(object sender, RoutedEventArgs e)
        {
            Menu.PlayClickSound();
            PlayerData.Nick = textBoxNickM.Text;
            PlayerData.Ip = textBoxIp.Text;
            PlayerData.GameMode = 1;
            DoConnection(PlayerData.Ip, 8888);
        }
        /// <summary>
        /// Nawiazywanie polaczenia z serwerem
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        private void DoConnection(string ip, int port)
        {
            try
            {
                _tmp = true;
                clientSocket = new TcpClient();
                clientSocket.Connect(ip, port);
                //clientSocket.Connect("127.0.0.1", port);
            }
            catch (SocketException e)
            {
                Console.WriteLine("s");
                _tmp = false;
                var messageBoxResult = WpfMessageBox.Show("Error", "Connection problem ... Check the IP address.", MessageBoxButton.OK, WpfMessageBox.MessageBoxImage.Error);
                Console.WriteLine("SocketException: {0}", e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
            if(_tmp == true) {
                Console.WriteLine("b");
                Menu.MenuMusic.Stop();
                Window.GetWindow(this).Content = new GamePlay();
            }
        }
        /// <summary>
        /// Powrot do menu glownego
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackMultiMenu(object sender, RoutedEventArgs e)
        {
            Menu.PlayClickSound();
            Window.GetWindow(this).Content = Menu.Instance;
        }
    }
}

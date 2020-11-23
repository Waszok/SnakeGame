using System.Windows; //Window

namespace SnakeGame
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Content = Menu.Instance;
            //Content = new GamePlay();
        }
    }
}


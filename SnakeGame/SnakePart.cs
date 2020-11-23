using System; //Uri
using System.Windows; //Application access
using System.Windows.Media; //Brushes
using System.Windows.Shapes; //Rectangle

namespace SnakeGame
{
    class SnakePart
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Rectangle Rect { get; private set; }

        public SnakePart(int x, int y)
        {
            X = x;
            Y = y;
            Rect = new Rectangle();
            Rect.Width = Rect.Height = 10;
            UpdateSkin();
        }

        private void UpdateSkin()
        {
            if(PlayerData.SnakeSkin == null) Rect.Fill = (Brush)Application.Current.FindResource("defaultSkinColor");
            else
            {
                ImageBrush _imgSkin = new ImageBrush
                {
                    ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(System.IO.Path.GetFullPath("../../" + PlayerData.SnakeSkin), UriKind.RelativeOrAbsolute))
                };
                Rect.Fill = _imgSkin;
            }
        }
    }
}

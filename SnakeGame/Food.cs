using System.Windows.Shapes; //Elipsa
using System.Windows.Media; //Brushes
using System.Windows.Controls; //Canvas
using System.Collections.Generic; //Lista
using System; //Random

namespace SnakeGame
{
    class Food
    {
        private Random rnd = new Random();
        public int X { get; set; }
        public int Y { get; set; }
        public Ellipse Ellipse { get; private set; }
        private List<string> _fruits;
        public int DrawnFruit { get; set; }
        public Food(int x, int y)
        {
            _fruits = new List<string> { "Images/bananaTex.png",
                                         "Images/appleTex.png",
                                         "Images/cherryTex.png",
                                         "Images/grapeTex.png",
                                         "Images/pearTex.png",
                                         "Images/pineappleTex.png",
                                         "Images/raspberryTex.png",
                                         "Images/watermelonTex.png"};
            Ellipse = new Ellipse();
            X = x;
            Y = y;
            var _fruitsCpy = new List<string>();
            foreach (var f in _fruits)
            {
                _fruitsCpy.Add(System.IO.Path.GetFullPath("../../" + f));
            }
            _fruits = _fruitsCpy;
        }
        /// <summary>
        /// Losowanie owocow (single)
        /// </summary>
        public void RandFruit()
        {
            DrawnFruit = rnd.Next(0, 8);
            ImageBrush _imgFruit = new ImageBrush
            {
                ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(_fruits[DrawnFruit], UriKind.RelativeOrAbsolute))
            };
            Ellipse.Fill = _imgFruit;
        }
        /// <summary>
        /// Rysowanie owocow
        /// </summary>
        public void RedrawFood()
        {
            Ellipse.Width = Ellipse.Height = 18;
            RandFruit();
            Canvas.SetLeft(Ellipse, X);
            Canvas.SetTop(Ellipse, Y);
        }
        /// <summary>
        /// Wersja dla multi 
        /// </summary>
        /// <param name="typeOfFood"></param>
        public void RedrawFoodForMulti(int typeOfFood)
        {
            Ellipse.Width = Ellipse.Height = 18;
            ImageBrush _imgFruit = new ImageBrush
            {
                ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(_fruits[typeOfFood], UriKind.RelativeOrAbsolute))
            };
            Ellipse.Fill = _imgFruit;
            Canvas.SetLeft(Ellipse, X);
            Canvas.SetTop(Ellipse, Y);
        }
    }
}

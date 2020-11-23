using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SnakeGame
{
    class Snake
    {
        public SnakePart Head { get; private set; }
        public int Length { get; set; } = 10;
        public List<SnakePart> Parts { get; private set; }

        public Snake(int x, int y)
        {
            Head = new SnakePart(x, y);
            Head.Rect.Width = Head.Rect.Height = Length;
            Head.Rect.Fill = Brushes.Black;
            Parts = new List<SnakePart> {
                new SnakePart(x - Length, y)
            };
        }
        //public double x, y;
        //public double Head_x { get; private set; }
        //public double Head_y { get; private set; }

        //public Rectangle rec = new Rectangle();
        //public Snake(double x,double y)
        //{
        //    Head_x = x;
        //    Head_y = y;
        //    rec.Width = rec.Height = 10;
        //    rec.Fill = Brushes.Red;
        //    Canvas.SetLeft(rec, x);
        //    Canvas.SetTop(rec, y);
        //    Canvas.SetLeft(rec, x - 10);
        //    Canvas.SetTop(rec, y - 10);
        //    //this.x = x;
        //    //this.y = y;
        //}
        public void RedrawSnake()
        {
            Canvas.SetLeft(Head.Rect, Head.X);
            Canvas.SetTop(Head.Rect, Head.Y);
            foreach (SnakePart snakePart in Parts)
            {
                Canvas.SetLeft(snakePart.Rect, snakePart.X);
                Canvas.SetTop(snakePart.Rect, snakePart.Y);
            }
        }
    }
}

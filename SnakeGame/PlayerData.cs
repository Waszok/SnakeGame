using System; // Uri (imageBrush - HP)
using System.Windows; //FontWeightas
using System.Windows.Controls; //ItemControls
using System.Windows.Media; //Brush
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SnakeGame
{
    public class PlayerData
    {
        public static ItemsControl Item { get; set; }
        public static ItemsControl ItemEnemy { get; set; }
        public static string Ip { get; set; } = null;
        public static string Nick { get; set; } = null;
        public static TextBlock ScoreText { get; set; }
        public static TextBlock ScoreTextEnemy { get; set; }
        public static string SnakeSkin { get; set; } = null;
        public static int SnakeSkinIndex { get; set; } = 0;
        public static int SnakeSpeed { get; set; } = 40;
        public static double VolumeGame { get; set; } = 10;
        public static int NumberOfLifes { get; set; } = 1;
        public static int GameMode { get; set; } = 0;
        public static bool MultiOnOff { get; set; } = false;

        PlayerData() { }
        /// <summary>
        /// Ustawienie danych o graczu w tablicy (oknie)
        /// </summary>
        /// <returns></returns>
        public static ItemsControl SetItem()
        {
            Item = new ItemsControl();
            Label _nick = new Label
            {
                FontSize = 20,
                FontWeight = FontWeights.Heavy,
                Foreground = (Brush)Application.Current.FindResource("nickColor1"),
                Content = Nick,
                Margin = new Thickness(-10, 0, 0, 0)
            };

            Label _score = new Label
            {
                FontSize = 16,
                FontWeight = FontWeights.Heavy,
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(Colors.Black),
                Content = "Score:",
                Margin = new Thickness(-10, -15, 0, 0)
            };

            ScoreText = new TextBlock
            {
                Text = "0",//Scores.ToString(),
                FontSize = 16,
                FontWeight = FontWeights.Heavy,
                Margin = new Thickness(45, -25, 0, 0)
            };
            Item.Items.Add(_nick);
            Item.Items.Add(_score);
            Item.Items.Add(ScoreText);

            if (GameMode == 0 && NumberOfLifes > 1)
            {
                ImageBrush img = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../" + "Images/gameHpImg.png"), UriKind.RelativeOrAbsolute))
                };
                Label _hp = new Label
                {
                    FontSize = 16,
                    FontWeight = FontWeights.Heavy,
                    Foreground = new SolidColorBrush(Colors.Red),
                    Content = "Hp:",
                    Margin = new Thickness(-10, -15, 0, 0)
                };
                Rectangle _rec1 = new Rectangle
                {
                    Width = 25,
                    Height = 20,
                    Margin = new Thickness(-10, -25, 0, 0),
                    Fill = img
                };
                Rectangle _rec2 = new Rectangle
                {
                    Width = 25,
                    Height = 20,
                    Margin = new Thickness(25, -25, 0, 0),
                    Fill = img
                };
                Rectangle _rec3 = new Rectangle
                {
                    Width = 25,
                    Height = 20,
                    Margin = new Thickness(60, -25, 0, 0),
                    Fill = img
                };
                Item.Items.Add(_hp);
                Item.Items.Add(_rec1);
                Item.Items.Add(_rec2);
                Item.Items.Add(_rec3);
            }
            return Item;
        }
        /// <summary>
        /// Aktualizacja punktow (single)
        /// </summary>
        /// <param name="score"></param>
        public static void UpdateScore(int score)
        {
            ScoreText.Text = score.ToString();
        }
        /// <summary>
        /// Ustawienie danych o przeciwniku (multi)
        /// </summary>
        /// <param name="nick"></param>
        /// <returns></returns>
        public static ItemsControl SetItemEnemy(string nick)
        {
            ItemEnemy = new ItemsControl();
            Label _nick = new Label
            {
                FontSize = 20,
                FontWeight = FontWeights.Heavy,
                Foreground = (Brush)Application.Current.FindResource("nickColor1"),
                Content = nick,
                Margin = new Thickness(-10, 0, 0, 0)
            };

            Label _score = new Label
            {
                FontSize = 16,
                FontWeight = FontWeights.Heavy,
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(Colors.Black),
                Content = "Score:",
                Margin = new Thickness(-10, -15, 0, 0)
            };

            ScoreTextEnemy = new TextBlock
            {
                Text = "0",//Scores.ToString(),
                FontSize = 16,
                FontWeight = FontWeights.Heavy,
                Margin = new Thickness(45, -25, 0, 0)
            };
            Item.Items.Add(_nick);
            Item.Items.Add(_score);
            Item.Items.Add(ScoreTextEnemy);
            return ItemEnemy;
        }
        /// <summary>
        /// Aktualizacja punktow przeciwnika (wyswietlanie)
        /// </summary>
        /// <param name="score"></param>
        public static void UpdateScoreEnemy(int score)
        {
            ScoreTextEnemy.Text = score.ToString();
        }
    }
}

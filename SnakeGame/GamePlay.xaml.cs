using System; // Random, UriKind, EventArgs
using System.Collections.Generic; // List<>
using System.Windows; // MessageBox, RoutedEventArgs, ustawienie ItemsControl
using System.Windows.Controls; // UserControl, Canvas
using System.Windows.Input; // Key
using System.Net.Sockets; // NetworkStream
using System.Windows.Media; // MediaPlayer
using System.Windows.Shapes; // Rectangle
using System.Windows.Threading; // DispatcherTimer
using System.IO; // BinaryReader, BinaryWriter
using System.Threading; // Thread.Sleep() - uzyte przy multilifes
using System.Text; // Encoding

namespace SnakeGame
{
    /// <summary>
    /// Struktura przechowujaca dane o polaczonym przeciniku
    /// </summary>
    public struct DataEnemies
    {
        public DataEnemies(int Id, string Nick, int Skin, int Score)
        {
            this.Id = Id;
            this.Nick = Nick;
            this.Skin = Skin;
            this.Score = Score;
        }
        public int Id { get; set; }
        public string Nick { get; set; }
        public int Skin { get; set; }
        public int Score { get; set; }
    }
    /// <summary>
    /// Glowna klasa aplikacji odpowiadajaca za cala logike
    /// oraz polaczenie w grze
    /// </summary>
    public partial class GamePlay : UserControl
    {
        // DZWIEKI - muzyka w grze, odglos jedzenia, dzwiek w momencie zderzenia: 
        private MediaPlayer _eatingSound = new MediaPlayer();
        private MediaPlayer _killSound = new MediaPlayer();
        public static MediaPlayer GameMusic { get; private set; } = new MediaPlayer();
        
        NetworkStream serverStream; // strumien (polaczenie z serwerem)
        public static DispatcherTimer Timer { get; private set; } // timer - w trybie normal ustawiony jest na 40ms
        private List<DataEnemies> _dataAboutEnemies = new List<DataEnemies>(); // przechowuje dane o przeciwniku
                                                                               // nick, id, score, skin
        Random rd = new Random();
        Snake snake;
        Food food;
        // wspolrzedne snake'a na starcie:
        int _x;
        int _y;

        // kierunki (odpowiadaja za kierunek poruszania sie weza na planszy)
        private int _directionX = 10;
        private int _directionY = 0;

        private int _score = 0; // liczba punktow
        private int _lifes = 1; // liczba zyc (1/3)
        private int _gameMode; // tryb gry (single/multi)
        private int _indexOfHearts = 6; // prawidlowe usuwanie serduszek z tablicy gracza

        // wspolrzedne i rodzaj (typ) owocow w trybie multiplayer:
        int _xFood = 150, _yFood = 150, _typeOfFood = 1;
        
        // dane o snake'u, tj. nick gracza, rodzaj tekstury (skin) oraz wspolrzedne, ktore wysylamy do serwera:  
        private string _coordinatesToSend;

        //Zmienna pomocnicza do kill:
        private bool _isKilled = false; //zeby nie mozna bylo wyslac czegos po tym jak zginie
        // Zmienna pomocnicza do food:
        private bool _isFirstFood = true; // zeby dodawal punkty i aktualizowal owoce dokladnie raz po kazdym
                                          // zjedzeniu (zabezpieczenie)
        // zmienna pomocnicza do testow:
        private bool _tmpForTest = false; //zeby nie startowal od razu

        public GamePlay()
        {
            InitializeComponent();
            if (Options.GameMusic == true) PlayGameMusic();
            _x = rd.Next(5, 44) * 10;
            _y = rd.Next(5, 44) * 10;
            snake = new Snake(_x, _y);
           
            _gameMode = PlayerData.GameMode;
            if(_gameMode == 0) _lifes = PlayerData.NumberOfLifes;

            infoAboutPlayers.Items.Add(PlayerData.SetItem());
            infoAboutPlayers.HorizontalAlignment = HorizontalAlignment.Center;

            if (_gameMode == 0)
            {
                InitFood();
                InitSnake();
                InitTimer();
            }
            else
            {
                serverStream = Multi.clientSocket.GetStream();
                InitFoodInMulti(_xFood, _yFood, _typeOfFood);
                InitSnake();
                InitTimer();
            }            
        }
        /// <summary>
        /// Interakcja - ladowanie okna + zdarzenia (sygnaly od wciskanych przyciskow) 
        /// </summary>
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && _directionY != 10)
            {
                _directionX = 0;
                _directionY = -10;
            }
            if (e.Key == Key.Down && _directionY != -10)
            {
                _directionX = 0;
                _directionY = 10;
            }
            if (e.Key == Key.Left && _directionX != 10)
            {
                _directionX = -10;
                _directionY = 0;
            }
            if (e.Key == Key.Right && _directionX != -10)
            {
                _directionX = 10;
                _directionY = 0;
            }
            if(e.Key == Key.P)
            {
                Timer.Stop();
                GameMusic.Pause();
                var messageBoxResult = WpfMessageBox.Show("Pause!", "Game paused", MessageBoxButton.OKCancel, WpfMessageBox.MessageBoxImage.Pause);
            }
            // ten if jest dla testow:
            if(e.Key == Key.G)
            {
                _tmpForTest = true;
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            KeyDown += UserControl_KeyDown;
            Focusable = true;
            Focus();
        }
        /// <summary>
        /// Ruch weza 
        /// </summary>
        private void MoveSnake()
        {
            for (int i = snake.Parts.Count - 1; i >= 1; i--)
            {
                snake.Parts[i].X = snake.Parts[i - 1].X;
                snake.Parts[i].Y = snake.Parts[i - 1].Y;
            }
            snake.Parts[0].X = snake.Head.X;
            snake.Parts[0].Y = snake.Head.Y;
            snake.Head.X += _directionX;
            snake.Head.Y += _directionY;
            snake.RedrawSnake();
        }
        /// <summary>
        /// Inicjalizacja timera 
        /// </summary>
        private void InitTimer()
        {
            Timer = new DispatcherTimer();
            Timer.Tick += new EventHandler(TimerTick);
            Timer.Interval = new TimeSpan(0, 0, 0, 0, PlayerData.SnakeSpeed);
            Timer.Start();
        }
        /// <summary>
        /// Odtwarzanie muzyki i odglosow jedzenia 
        /// </summary>
        private void PlayGameMusic()
        {
            GameMusic.Open(new Uri(System.IO.Path.GetFullPath("../../" + "Sounds/gameMusic.wav"), UriKind.RelativeOrAbsolute));
            GameMusic.Volume = PlayerData.VolumeGame / 100;
            GameMusic.MediaEnded += new EventHandler(Media_Repeat);
            GameMusic.Play();
        }
        private void Media_Repeat(object sender, EventArgs e)
        {
            GameMusic.Position = TimeSpan.Zero;
            GameMusic.Play();
        }
        private void PlayEatingSound()
        {
            _eatingSound.Open(new Uri(System.IO.Path.GetFullPath("../../" + "Sounds/eatingSound.mp3"), UriKind.RelativeOrAbsolute));
            _eatingSound.Play();
        }
        private void PlayKillSound()
        {
            _killSound.Open(new Uri(System.IO.Path.GetFullPath("../../" + "Sounds/lostLifeSound.mp3"), UriKind.RelativeOrAbsolute));
            _killSound.Volume = 0.08;
            _killSound.Play();
        }
        /// <summary>
        /// Inicjalizacja i przerysowywanie jedzenia na planszy 
        /// </summary>
        void InitFood()
        {
            food = new Food(rd.Next(0, 49) * 10, rd.Next(0, 49) * 10);
            food.RedrawFood();
            BoardCanvas.Children.Insert(0, food.Ellipse);
        }
        void UpdateFood()
        {
            BoardCanvas.Children.RemoveAt(0);
            food = new Food(rd.Next(0, 49) * 10, rd.Next(0, 49) * 10);
            food.RedrawFood();
            BoardCanvas.Children.Insert(0, food.Ellipse);
        }
        /// <summary>
        /// Inicjalizacja i rysowanie jedzenia na planszy w trybie Multiplayer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="typeOfFood"></param>
        void InitFoodInMulti(int x, int y, int typeOfFood)
        {
            food = new Food(x, y);
            food.RedrawFoodForMulti(typeOfFood);
            BoardCanvas.Children.Insert(0, food.Ellipse);
        }
        /// <summary>
        /// Inicjalizacja i rysowanie jedzenia na planszy w trybie Multiplayer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="typeOfFood"></param>
        void UpdateFoodInMulti(int x, int y, int typeOfFood)
        {
            BoardCanvas.Children.RemoveAt(0);
            food = new Food(x, y);
            food.RedrawFoodForMulti(typeOfFood);
            BoardCanvas.Children.Insert(0, food.Ellipse);
        }
        /// <summary>
        /// Inicjalizacja i przerysowywanie weza na planszy 
        /// </summary>
        void InitSnake()
        {
            BoardCanvas.Children.Add(snake.Head.Rect);
            for (int i = 0; i < snake.Parts.Count; i++)
            {
                BoardCanvas.Children.Add(snake.Parts[i].Rect);
            }
            snake.RedrawSnake();
        }
        /// <summary>
        /// Aktualizacja snake'a
        /// </summary>
        void UpdateSnake()
        {
            int count = 0;
            for (int i = 0; i < BoardCanvas.Children.Count; i++)
            {
                if (BoardCanvas.Children[i] is Rectangle)
                    count++;
            }
            BoardCanvas.Children.RemoveRange(1, count);

            BoardCanvas.Children.Add(snake.Head.Rect);
            for (int i = 0; i < snake.Parts.Count; i++)
            {
                BoardCanvas.Children.Add(snake.Parts[i].Rect);
            }
            snake.RedrawSnake();
        }
        // KOLIZJE:
        // W przypadku kolizji sprawdzamy tryb gry (single/multi) i odpowiednio zarzadzamy HP oraz konczymy gre
        // w przypadku utraty wszystkich zyc:
        /// <summary>
        /// Wykrywanie kolizji
        /// </summary>
        void LifeVsCollision()
        {
            _lifes--;
            if (PlayerData.MultiOnOff == true && _gameMode == 0)
            {
                Rectangle a = (Rectangle)PlayerData.Item.Items[_indexOfHearts];
                a.Opacity = 0;
                _indexOfHearts--;
            }
            if (_lifes > 0)
            {
                snake = new Snake(100, 100);
                _directionX = 10;
                _directionY = 0;
                InitSnake();
                Thread.Sleep(1000);
            }
            if (_lifes == 0)
            {
                Timer.Stop();
                GameMusic.Stop();
                var messageBoxResult = WpfMessageBox.Show("Game Over", "Your result is: " + _score.ToString(),
                                                           MessageBoxButton.YesNo, WpfMessageBox.MessageBoxImage.GameOver);
            }
        }
        /// <summary>
        /// Wykrywanie kolozji w trybie multi
        /// </summary>
        void LifeVsCollisionMulti()
        {
            _lifes--;
            if (_lifes == 0)
            {
                Timer.Stop();
                GameMusic.Stop();
                SendDataToServer("!"); // Wysylamy info o kolizji ze sciana lub samym soba
                serverStream.Close();
                Multi.clientSocket.Close();
                var messageBoxResult = WpfMessageBox.Show("Game Over", "Your result is: " + _score.ToString(),
                                                           MessageBoxButton.YesNoCancel, WpfMessageBox.MessageBoxImage.GameOver);
                _isKilled = true;
            }
        }
        /// <summary>
        /// Kolizja miedzy wezami
        /// </summary>
        void KilledBySnake()
        {
            Timer.Stop();
            GameMusic.Stop();
            SendDataToServer("?");
            serverStream.Close();
            Multi.clientSocket.Close();
            var messageBoxResult = WpfMessageBox.Show("Game Over", "Your result is: " + _score.ToString(),
                                                       MessageBoxButton.YesNoCancel, WpfMessageBox.MessageBoxImage.GameOver);
            _isKilled = true;
        }
        /// <summary>
        /// Kolizje snake'a ze scianami oraz samym soba 
        /// </summary>
        void CollisionSnakeBoard()
        {
            if (snake.Head.X > 490 || snake.Head.Y > 490 || snake.Head.X < 0 || snake.Head.Y < 0)
            {
                PlayKillSound();
                if (_gameMode == 0) LifeVsCollision();
                else LifeVsCollisionMulti();
            }
        }
        void CollisionSnakeYourself()
        {
            for (int i = 0; i < snake.Parts.Count; i++)
            {
                if (snake.Head.X == snake.Parts[i].X && snake.Head.Y == snake.Parts[i].Y)
                {
                    PlayKillSound();
                    if (_gameMode == 0) LifeVsCollision();
                    else LifeVsCollisionMulti();
                    break;
                }                 
            }
        }
        /// <summary>
        /// Kolizja miedzy wezami  
        /// </summary>
        void CollisionBetweenSnakes()
        {
            PlayKillSound();
            KilledBySnake();
        }
        /// <summary>
        /// Kolizje snake'a z food
        /// </summary>
        void CollisionSnakeFood()
        {
            Rect myRect = new Rect(food.X + 3, food.Y + 3, 12, 12);
            Rect myRect2 = new Rect(snake.Head.X + 5, snake.Head.Y + 5, 1, 1);
            if (myRect2.IntersectsWith(myRect) == true)
            {
                PlayEatingSound();
                snake.Parts.Add(new SnakePart(snake.Parts[snake.Parts.Count - 1].X,
                                              snake.Parts[snake.Parts.Count - 1].Y));


                UpdateFood();
                snake.RedrawSnake();
                _score++;
                PlayerData.UpdateScore(_score);
            }
        }
        /// <summary>
        /// Kolizje snake'a z food w trybie Multiplayer
        /// </summary>
        void CollisionSnakeFoodInMulti()
        {
            Rect myRect = new Rect(food.X + 3, food.Y + 3, 12, 12);
            Rect myRect2 = new Rect(snake.Head.X + 5, snake.Head.Y + 5, 1, 1);
            if (myRect2.IntersectsWith(myRect) == true)
            {
                
                PlayEatingSound();
                snake.Parts.Add(new SnakePart(snake.Parts[snake.Parts.Count - 1].X,
                                              snake.Parts[snake.Parts.Count - 1].Y));
               
                if (_isFirstFood == true)
                {
                    snake.RedrawSnake();
                    _score++;
                    PlayerData.UpdateScore(_score);
                    SendDataToServer("F"); // Wysylamy info ze zjedlismy owoc
                    SendDataToServer("P" + _score.ToString()); // Wysylamy aktualna liczbe punktow
                    _isFirstFood = false;
                }
            }
        }
        /// <summary>
        /// Funkcja przygotowujaca dane o snake'u do wyslania 
        /// </summary>
        void PrepareDataAboutSnake()
        {
            _coordinatesToSend = "S#";
            _coordinatesToSend += PlayerData.Nick + "!";
            _coordinatesToSend += PlayerData.SnakeSkinIndex.ToString() + "$";
            _coordinatesToSend += snake.Head.X.ToString() + "," + snake.Head.Y.ToString() + ";";
            foreach (SnakePart part in snake.Parts)
            {
                _coordinatesToSend += part.X.ToString();
                _coordinatesToSend += ",";
                _coordinatesToSend += part.Y.ToString();
                _coordinatesToSend += ";";
            }
            _coordinatesToSend += "&";
        }
        /// <summary>
        /// Funkcja wysylajaca dane do serwera 
        /// </summary>
        /// <param name="dataToSend"></param>
        void SendDataToServer(string dataToSend)
        {
            int numberOfBytes = Encoding.ASCII.GetByteCount(dataToSend);
            BinaryWriter tmp = new BinaryWriter(serverStream);
            tmp.Write(numberOfBytes);

            Byte[] sendBuffer = Encoding.ASCII.GetBytes(dataToSend);
            serverStream.Write(sendBuffer, 0, sendBuffer.Length);
            sendBuffer = null;
        }
        /// <summary>
        ///  Glowna funkcja 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TimerTick(object sender, EventArgs e)
        {
            if(_tmpForTest == true) MoveSnake();
            UpdateSnake();

            if (_gameMode == 0) CollisionSnakeFood();
            else CollisionSnakeFoodInMulti();

            CollisionSnakeBoard();
            CollisionSnakeYourself();
                      

            // Wysylanie danych o snake'u do serwera:
            if (_gameMode != 0 && _isKilled == false)
            {
                // Przygotowujemy dane (nick, wspolrzedne, rodzaj tekstury) do wyslania:
                PrepareDataAboutSnake();

                // Wysylamy dane o snake'u do serwera:
                SendDataToServer(_coordinatesToSend);

                // Odbieramy dane od serwera:
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                BinaryReader readerSize = new BinaryReader(serverStream);
                int sizeReceived = readerSize.ReadInt32();
                Byte[] receiveBuffer = new Byte[sizeReceived];
                Int32 sData;
                sData = serverStream.Read(receiveBuffer, 0, receiveBuffer.Length);

                string fromServerAtBeginning = string.Empty;
                fromServerAtBeginning = Encoding.ASCII.GetString(receiveBuffer, 0, sData);
                fromServerAtBeginning = fromServerAtBeginning.Split('&')[0];


                char typeOfMessage = fromServerAtBeginning[0]; // Sprawdzamy pierwszy znak odczytanej wiadomosci i 
                                                               // na jego podstawie rozpoznajemy rodzaj wiadomosci
                                                               // co pozwala na wykonanie odpowiedniej akcji

                if (typeOfMessage == 'S') // Aktualizacja pozycji snake'a (S - Snake)
                {
                    string[] split1 = fromServerAtBeginning.Remove(0, 2).Split('*');
                    int id = Int32.Parse(split1[0].ToString()); //id gracza od ktorego dostalismy dane
                    string[] split2 = split1[1].Split('!');
                    string nick = split2[0];
                    string[] split3 = split2[1].Split('$');
                    int skin = Int32.Parse(split3[0].ToString());
                    string[] positions = split3[1].Split(';');
                    ImageBrush imgSkinEnemy = new ImageBrush
                    {
                        ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(System.IO.Path.GetFullPath("../../" + Options.Textures[skin]), UriKind.RelativeOrAbsolute))
                    };
                    Rectangle addNewHead = new Rectangle();
                    addNewHead.Width = addNewHead.Height = 10;
                    addNewHead.Fill = Brushes.Black;
                    string[] positionsXYHead = positions[0].Split(',');
                    if (positionsXYHead.Length > 1)
                    {
                        Canvas.SetLeft(addNewHead, Int32.Parse(positionsXYHead[0]));
                        Canvas.SetTop(addNewHead, Int32.Parse(positionsXYHead[1]));
                        BoardCanvas.Children.Add(addNewHead);
                    }
                    for (int i = 1; i < positions.Length; i++)
                    {
                        Rectangle addNew = new Rectangle();
                        addNew.Width = addNew.Height = 10;
                        addNew.Fill = imgSkinEnemy;
                        string[] positionsXY = positions[i].Split(',');
                        if (positionsXY.Length > 1)
                        {
                            Canvas.SetLeft(addNew, Int32.Parse(positionsXY[0]));
                            Canvas.SetTop(addNew, Int32.Parse(positionsXY[1]));
                            BoardCanvas.Children.Add(addNew);
                        }
                    }
                    if (_dataAboutEnemies.Count == 0)
                    {
                        DataEnemies newDataEnemy = new DataEnemies(id, nick, skin, 0);
                        _dataAboutEnemies.Add(newDataEnemy);
                    }
                    if (infoAboutPlayers.Items.Count == 1)
                    {
                        infoAboutPlayers.Items.Add(PlayerData.SetItemEnemy(_dataAboutEnemies[0].Nick));
                        infoAboutPlayers.HorizontalAlignment = HorizontalAlignment.Center;
                    }
                }
                if (typeOfMessage == 'F') // Nowe pozycje i rodzaj owoca (F - Food)
                {
                    Console.WriteLine(fromServerAtBeginning);
                    string[] splitf1 = fromServerAtBeginning.Remove(0, 2).Split('@');
                    _typeOfFood = Int32.Parse(splitf1[0].ToString());
                    string[] splitf2 = splitf1[1].Split(',');
                    _xFood = Int32.Parse(splitf2[0].ToString());
                    _yFood = Int32.Parse(splitf2[1].ToString());

                    UpdateFoodInMulti(_xFood, _yFood, _typeOfFood);
                    _isFirstFood = true;
                }
                if (typeOfMessage == 'P') // Aktualizacja punktow na tablicy przeciwnika (P - Points)
                {
                    string splitp = fromServerAtBeginning.Remove(0, 1);
                    int scoreEnemy = Int32.Parse(splitp.ToString());
                    PlayerData.UpdateScoreEnemy(scoreEnemy);
                }
                watch.Stop();
                Console.WriteLine("czas wykonania w ms: " + watch.ElapsedMilliseconds);
                //Console.WriteLine("czas w taktach procka: " + watch.ElapsedTicks);
                if (typeOfMessage == 'K') // Wymazanie informacji o przeciwniku (nick, points) po jego odejsciu z gry (K - Kill) 
                {
                    if (infoAboutPlayers.Items.Count > 1)
                    {
                        infoAboutPlayers.Items.RemoveAt(0);
                        infoAboutPlayers.Items.Add(PlayerData.SetItem());
                        PlayerData.UpdateScore(_score);
                    }
                    _dataAboutEnemies.RemoveAt(0);
                }
                if(typeOfMessage == 'C')
                {
                    CollisionBetweenSnakes();
                }
            }
        }
        /// <summary>
        /// Wyjscie z gry (powrot do menu) - obsluga przycisku "Back"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackGameToMenu(object sender, RoutedEventArgs e)
        {
            if (_gameMode != 0)
            {
                SendDataToServer("!"); // Wysylamy do serwera info o wyjsciu z gry
                serverStream.Close();
                Multi.clientSocket.Close();
            }
            Timer.Stop();
            GameMusic.Stop();
            if(Menu.OnOff == true)
            {
                Menu.MenuMusic.Play();
            }
            else
            {
                Menu.MenuMusic.Play();
                Menu.MenuMusic.Volume = 0;
            }
            Window.GetWindow(this).Content = Menu.Instance;
        }

    }
}

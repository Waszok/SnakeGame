using System;
using System.Collections;
using System.Collections.Generic; //uzycie List<>
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SnakeServer
{
    /// <summary>
    /// Struktura przechowujaca dane o polaczonym kliencie (id oraz socket)
    /// </summary>
    public struct DataClients
    {
        public DataClients(int Id, TcpClient Socket)
        {
            this.Id = Id;
            this.Socket = Socket;
        }
        public int Id { get; set; }
        public TcpClient Socket { get; set; }
    }
    /// <summary>
    /// Glowna klasa serwera
    /// </summary>
    public class Server
    {
        private static TcpListener _serverListener = null;
        private static Int32 _port;
        private static IPAddress _localAddr;
        public static List<DataClients> _clientSockets = new List<DataClients>();
        private static int _currentId = 1;

        private static Random rnd = new Random();
        public static int _typeOfFood = 1;
        public static int _foodX = 150;
        public static int _foodY = 150;

        public static int[,] tabA = new int[50, 50];
        public static int[,] tabB = new int[50, 50];

        /// <summary>
        /// Funkcja zwracajaca adres IP hosta lub 127.0.0.1
        /// </summary>
        /// <returns></returns>
        private static string GetIpAddress()
        {
            IPAddress[] ip = Dns.GetHostAddresses(Dns.GetHostName());

            foreach(IPAddress i in ip)
            {
                if (i.AddressFamily == AddressFamily.InterNetwork)
                    return i.ToString();
            }
            return "127.0.0.1";
        }
        /// <summary>
        /// Funckja przydzielajaca najmniejsze dostepne ID
        /// </summary>
        private static void SearchAvailableId()
        {
            if (_clientSockets.Count > 0)
            {
                while (FindElement(_clientSockets, _currentId) == true)
                {
                    _currentId++;
                }
            }
        }
        /// <summary>
        /// Znajdowanie elementu w podanej w argumencie liscie
        /// </summary>
        /// <param name="list"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        private static bool FindElement(List<DataClients> list, int element)
        {
            foreach (DataClients ct in list)
            {
                if (ct.Id == element) return true;
            }
            return false;
        }
        /// <summary>
        /// Zwrocenie indeksu elementu podanego w argumencie,
        /// na ktorym wystepuje on w podanej w argumencie liscie
        /// </summary>
        /// <param name="list"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public static int GetIndex(List<DataClients> list, int element)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (element == list[i].Id) return i;
            }
            return -1;
        }
        /// <summary>
        /// Losowanie nowych wspolrzednych oraz rodzaju owocu
        /// </summary>
        public static void RandInfoAboutFood()
        {
            _typeOfFood = rnd.Next(0, 8);
            _foodX = rnd.Next(0, 49) * 10;
            _foodY = rnd.Next(0, 49) * 10;
        } 
        /// <summary>
        /// Glowna funkcja.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                _localAddr = IPAddress.Parse(GetIpAddress());
                //_localAddr = IPAddress.Parse("127.0.0.1");
                _port = 8888;
                _serverListener = new TcpListener(_localAddr, _port);
                _serverListener.Start();
                Console.WriteLine(" >> " + "Server Started");
                Console.WriteLine("The local End point is: " + _serverListener.LocalEndpoint);
                while(true)
                {
                    if (_clientSockets.Count < 2)
                    {
                        TcpClient _clientSocketNew = _serverListener.AcceptTcpClient(); //zwarca obiekt klasy TcpClient
                                                                                        //odpowiada za gniazdo klienckie
                        SearchAvailableId();
                        DataClients data = new DataClients(_currentId, _clientSocketNew);
                        _clientSockets.Add(data);
                        int lastId = _currentId;
                        _currentId = 1;
                        HandleClient client = new HandleClient();
                        client.StartClient(_clientSocketNew, lastId);
                    }
                    else
                    { 
                        Console.WriteLine("Server is full!!!");
                        throw new SocketException();
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                _serverListener.Stop();
            }
        }
        /// <summary>
        /// Podlaczanie kazdego z klientow oddzielnie.
        /// Kazdy w dwoch klientow dostaje swoj obiekt tej klasy.
        /// Zawiera ona metode tworzaca nowy watek dla danego klienta
        /// oraz metode obslugujaca przesylanie danych pomiedzy danym
        /// klientem a serwerem.
        /// </summary>
        public class HandleClient
        {
            private TcpClient _clientSocket; //gniazdo
            private int lastId; //id
            ArrayList tabPos = new ArrayList(); //lista aktualnych pozycji (wspolrzednych) weza 

            /// <summary>
            /// Utworzenie i wystartowanie nowego watku
            /// </summary>
            /// <param name="clientSocket"></param>
            /// <param name="lastId"></param>
            public void StartClient(TcpClient clientSocket, int lastId)
            {
                _clientSocket = clientSocket;
                this.lastId = lastId;
                Thread _clientThread = new Thread(UpdateData);
                _clientThread.Start();
            }
            /// <summary>
            /// Wysylanie danych do serwera
            /// </summary>
            /// <param name="dataToSendCli"></param>
            /// <param name="clientSocket"></param>
            public void SendDataToClient(string dataToSendCli, TcpClient clientSocket)
            {
                int numberOfBytesCli = Encoding.ASCII.GetByteCount(dataToSendCli);
                BinaryWriter tmp = new BinaryWriter(clientSocket.GetStream());
                tmp.Write(numberOfBytesCli);
                Byte[] sendBufferCli = Encoding.ASCII.GetBytes(dataToSendCli);
                clientSocket.GetStream().Write(sendBufferCli, 0, sendBufferCli.Length);
                sendBufferCli = null;
            }
            /// <summary>
            /// Aktualizacja tablicy ze wspolrzednymi weza
            /// </summary>
            /// <param name="tab"></param>
            /// <param name="tabPos"></param>
            public void UpdateArray(int[,] tab, ArrayList tabPos)
            {
                for (int j = 0; j < tabPos.Count; j += 2)
                {
                    tab[(int)tabPos[j], (int)tabPos[j + 1]] = 1;
                }
            }
            /// <summary>
            /// Wyzerowanie tablicy
            /// </summary>
            /// <param name="tab"></param>
            /// <param name="tabPos"></param>
            public void ZeroArray(int[,] tab, ArrayList tabPos)
            {
                for (int j = 0; j < tabPos.Count; j += 2)
                {
                    tab[(int)tabPos[j], (int)tabPos[j + 1]] = 0;
                }
            }
            /// <summary>
            /// Sprawdzenie kolizcji pomiedzy wezami na podstawie porownania 
            /// pozycji w tablicach tabA i tabB.
            /// </summary>
            /// <param name="tabA"></param>
            /// <param name="tabB"></param>
            /// <returns></returns>
            public bool CheckCollsion(int[,] tabA, int[,] tabB)
            {
                for(int i = 0; i < 50; i++)
                {
                    for(int j = 0; j < 50; j++)
                    {
                        if (tabA[i, j] == 1 & tabA[i, j] == tabB[i, j]) return true;
                    }
                }
                return false;
            }
            /// <summary>
            /// Wymiana danych (klient - serwer). Podstawowa funkcja
            /// w ktorej obslugujemy danego klienta
            /// </summary>
            private void UpdateData()
            {                
                while (true)
                {
                    try
                    {
                        BinaryReader odbr = new BinaryReader(_clientSocket.GetStream());
                        int howMuchreceive = odbr.ReadInt32();
                        String ReceivedData;
                        Byte[] data = new Byte[howMuchreceive];
                        Int32 sData;
                        sData = _clientSocket.GetStream().Read(data, 0, data.Length);
                        ReceivedData = Encoding.ASCII.GetString(data, 0, sData);

                        if( ReceivedData[0] == '?') // Obaj przegrali
                        {
                            Console.WriteLine("Client {0} disconnected", lastId.ToString());
                            if ((lastId == _clientSockets[0].Id && _clientSockets[0].Id < _clientSockets[1].Id)
                                    || (lastId == _clientSockets[1].Id && _clientSockets[0].Id > _clientSockets[1].Id))
                            {
                                ZeroArray(tabA, tabPos);
                                
                            }
                            if ((lastId == _clientSockets[1].Id && _clientSockets[0].Id < _clientSockets[1].Id)
                                || (lastId == _clientSockets[0].Id && _clientSockets[0].Id > _clientSockets[1].Id))
                            {
                                ZeroArray(tabB, tabPos);
                            }
                            tabPos.Clear();
                            _clientSocket.GetStream().Close();
                            _clientSocket.Close();
                            _clientSockets.RemoveAt(GetIndex(_clientSockets, lastId));
                            _clientSocket.Dispose();                        
                            break;
                        }

                        if (ReceivedData[0] == '!') // Ktorys przegral badz sie rozlaczyl
                        {
                            if (_clientSockets.Count > 1)
                            {
                                for (int i = 0; i < _clientSockets.Count; i++)
                                {
                                    if (_clientSockets[i].Socket != _clientSocket)
                                    {
                                        SendDataToClient("K" + "&", _clientSockets[i].Socket);
                                    }
                                }
                            }
                            if (_clientSockets.Count > 1)
                            {
                                if ((lastId == _clientSockets[0].Id && _clientSockets[0].Id < _clientSockets[1].Id)
                                        || (lastId == _clientSockets[1].Id && _clientSockets[0].Id > _clientSockets[1].Id))
                                {
                                    ZeroArray(tabA, tabPos);

                                }
                                if ((lastId == _clientSockets[1].Id && _clientSockets[0].Id < _clientSockets[1].Id)
                                    || (lastId == _clientSockets[0].Id && _clientSockets[0].Id > _clientSockets[1].Id))
                                {
                                    ZeroArray(tabB, tabPos);
                                }
                            }
                            else
                            {
                                if (lastId == 1) ZeroArray(tabA, tabPos);
                                else ZeroArray(tabB, tabPos);
                            }
                            tabPos.Clear();
                            Console.WriteLine("Client {0} disconnected", lastId.ToString());
                            _clientSocket.GetStream().Close();
                            _clientSocket.Close();
                            _clientSockets.RemoveAt(GetIndex(_clientSockets, lastId));
                            //_clientSocket.Dispose();
                            break;
                        }
                        if (ReceivedData[0] == 'F') // Pozycje jedzenia
                        {
                            RandInfoAboutFood();
                            for (int i = 0; i < _clientSockets.Count; i++)
                            {
                                string dataAboutFood = "F#" + _typeOfFood.ToString() + "@"
                                        + _foodX.ToString() + "," + _foodY.ToString() + "&";

                                SendDataToClient(dataAboutFood, _clientSockets[i].Socket);
                            }
                        }
                        if (ReceivedData[0] == 'S') // Wspolrzedne weza
                        {
                            if (_clientSockets.Count > 1)
                            {
                                for (int i = 0; i < _clientSockets.Count; i++)
                                {
                                    if (_clientSockets[i].Socket != _clientSocket)
                                    {
                                        string toSend = "S#" + lastId.ToString() + "*" + ReceivedData.Remove(0, 2);
                                        SendDataToClient(toSend, _clientSockets[i].Socket);
                                    }
                                }
                                if ((lastId == _clientSockets[0].Id && _clientSockets[0].Id < _clientSockets[1].Id)
                                    || (lastId == _clientSockets[1].Id && _clientSockets[0].Id > _clientSockets[1].Id))
                                {
                                    ZeroArray(tabA, tabPos);
                                }
                                if ((lastId == _clientSockets[1].Id && _clientSockets[0].Id < _clientSockets[1].Id)
                                    || (lastId == _clientSockets[0].Id && _clientSockets[0].Id > _clientSockets[1].Id))
                                {
                                    ZeroArray(tabB, tabPos);
                                }
                                tabPos.Clear();
                                string[] split1 = ReceivedData.Split('&');
                                string[] split2 = split1[0].Split('$');
                                string[] positions = split2[1].Split(';');

                                foreach (var el in positions)
                                {

                                    string[] tmp = el.Split(',');
                                    if (tmp.Length > 1)
                                    {
                                        tabPos.Add(Int32.Parse(tmp[0]) / 10);
                                        tabPos.Add(Int32.Parse(tmp[1]) / 10);
                                    }

                                }
                                if ((lastId == _clientSockets[0].Id && _clientSockets[0].Id < _clientSockets[1].Id)
                                    || (lastId == _clientSockets[1].Id && _clientSockets[0].Id > _clientSockets[1].Id))
                                {
                                    UpdateArray(tabA, tabPos);
                                }
                                if ((lastId == _clientSockets[1].Id && _clientSockets[0].Id < _clientSockets[1].Id)
                                    || (lastId == _clientSockets[0].Id && _clientSockets[0].Id > _clientSockets[1].Id))
                                {
                                    UpdateArray(tabB, tabPos);
                                }
                                //if (lastId == _clientSockets[0].Id)
                                //{
                                //    UpdateArray(tabA, tabPos);

                                //}
                                //else UpdateArray(tabB, tabPos);
                                if (CheckCollsion(tabA, tabB) == true)
                                {
                                    for (int k = 0; k < _clientSockets.Count; k++)
                                    {
                                        if (_clientSockets[k].Socket != _clientSocket)
                                        {
                                            SendDataToClient("C&", _clientSockets[k].Socket);
                                        }
                                    }
                                }
                            }
                        }
                        if (ReceivedData[0] == 'P') //Wyslanie punktow
                        {
                            if (_clientSockets.Count > 1)
                            {
                                for (int i = 0; i < _clientSockets.Count; i++)
                                {
                                    if (_clientSockets[i].Socket != _clientSocket)
                                    {
                                        string toSend2 = ReceivedData + "&";
                                        SendDataToClient(toSend2, _clientSockets[i].Socket);
                                    }
                                }
                            }
                        }
                        if (_clientSockets.Count == 1)
                        {
                            string toSendIfOne = lastId.ToString() + "*" + "0";
                            SendDataToClient(toSendIfOne, _clientSocket);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(" >> " + ex.ToString());
                    }
                    
                }   
            }
        }
    }
}
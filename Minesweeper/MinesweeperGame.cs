using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace Minesweeper
{
    internal class MinesweeperGame : INotifyPropertyChanged
    {
        private int width;
        private int height;
        private int mines;
        private int flags;
        private int time;
        private int coveredFields;
        private bool sound;

        private GameState gameState;
        public MinesweeperField[,] Board;
        private Random random = new Random();
        private DispatcherTimer timer;
        private MediaPlayer tickPlayer = new MediaPlayer(); 
        private MediaPlayer minePlayer = new MediaPlayer();

        public MinesweeperGame(int width, int height, int mines,bool sound)
        {
            Width = width;
            Height = height;
            Mines = mines;

            Board = new MinesweeperField[height, width];
            Flags = 0;
            coveredFields = width * height;
            State = GameState.ReadyToStart;

            Sound = sound;
            tickPlayer.MediaEnded += TickPlayer_MediaEnded;
            minePlayer.Volume = 0.1;
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;

            createBoard();
        }

        public int Width
        {
            get { return width; }

            private set
            {
                width = value;
                OnPropertyChanged("Width");
            }
        }
        public int Height
        {
            get { return height; }

            private set
            {
                height = value;
                OnPropertyChanged("Height");
            }
        }
        public int Mines
        {
            get { return mines; }

            private set
            {
                mines = value;
                OnPropertyChanged("Mines");
            }
        }

        public int Flags
        {
            get { return flags; }

            private set { 
                flags = value;
                OnPropertyChanged("Flags");
            }
        }

        public int Time
        {
            get { return time; }

            private set
            {
                time = value;
                OnPropertyChanged("Time");
            }
        }

        public GameState State
        {
            get { return gameState; }

            private set { 
                gameState = value;
                OnPropertyChanged("State");
            }
        }

        public bool Sound
        {
            get { return sound; }

            set 
            { 
                sound = value;
                if(State == GameState.GameInProgress)
                {
                    if(Sound) 
                    {
                        if(!tickPlayer.HasAudio)
                        {
                            tickPlayer.Open(new Uri(Path.Combine(Directory.GetCurrentDirectory(), @"Sounds\ticking-clock_1-27477.mp3")));
                        }
                        tickPlayer.Play(); 
                    }
                    else 
                    { 
                        tickPlayer.Stop(); 
                    }
                }
                OnPropertyChanged("Sound");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void createBoard()
        {
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    Board[h, w] = new MinesweeperField(false, false, true);
                }
            }

            int counter = 0;

            do
            {
                int h = random.Next(0, height);
                int w = random.Next(0, width);
                if (!Board[h, w].Mine)
                {
                    Board[h, w].Mine = true;
                    counter++;
                }
            } while (counter != mines);
        }

        public void flagField(int h, int w)
        {
            if (Board[h,w].Covered && (gameState == GameState.GameInProgress || gameState == GameState.ReadyToStart))
            {
                if (Board[h, w].Flag)
                {
                    Board[h, w].Flag = false;
                    Flags--;
                }
                else
                {
                    Board[h, w].Flag = true;
                    Flags++;
                }
            }
        }

        public void CheckField(int h, int w)
        {
            switch(State)
            {
                case GameState.ReadyToStart:
                    timer.Start();
                    State = GameState.GameInProgress;
                    if (Sound)
                    {
                        tickPlayer.Open(new Uri(Path.Combine(Directory.GetCurrentDirectory(), @"Sounds\ticking-clock_1-27477.mp3")));                       
                        tickPlayer.Play();
                    }
                    if (!Board[h, w].Flag)
                    {
                        DiscoverField(h, w);
                    }
                    break;

                case GameState.GameInProgress:
                    if (!Board[h, w].Flag)
                    {
                        DiscoverField(h, w);
                    }
                    break;
            }
            if (coveredFields == mines)
            {
                timer.Stop();
                ClosePlayers();
                State = GameState.Won;
            }
        }

        public void DiscoverField(int h, int w)
        {
            if (Board[h, w].Mine) 
            {
                timer.Stop();
                if(Sound)
                {
                    minePlayer.Open(new Uri(Path.Combine(Directory.GetCurrentDirectory(), @"Sounds\Bomb.wav")));
                    minePlayer.Play();
                }
                ClosePlayers();
                State = GameState.Lost;
                return;          
            }

            if (Board[h, w].Covered)
            {
                Board[h, w].Covered = false;
                coveredFields--;

                if (Board[h, w].Flag == true)
                {
                    Board[h, w].Flag = false;
                    Flags--;
                }

                int mineCounter = 0;

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        if ((h + i >= 0) && (h + i < height) && (w + j >= 0) && (w + j < width))
                        {
                            if (Board[h + i, w + j].Mine)
                            {
                                mineCounter++;
                            }
                        }
                    }
                }

                if (mineCounter == 0)
                {
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            if ((h + i >= 0) && (h + i < height) && (w + j >= 0) && (w + j < width))
                            {
                                if (!Board[h + i, w + j].Mine) DiscoverField(h + i, w + j);
                            }
                        }
                    }
                }
                Board[h, w].MinesAround = mineCounter;
            }
        }
        public void ClosePlayers()
        {
            tickPlayer.Close();
            //minePlayer.Close();          
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Time++;
        }

        private void TickPlayer_MediaEnded(object? sender, EventArgs e)
        {
            tickPlayer.Open(new Uri(Path.Combine(Directory.GetCurrentDirectory(), @"Sounds\ticking-clock_1-27477.mp3")));
            tickPlayer.Play();
        }

        public enum GameState
        {
            ReadyToStart,
            GameInProgress,
            Won,
            Lost,                      
        }
    }
}

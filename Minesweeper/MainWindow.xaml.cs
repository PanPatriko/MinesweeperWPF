using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        int bombs = 10;
        int height = 9;
        int width = 9;
        const int buttonSize = 40;

        Difficult difficult = Difficult.Easy;
        MinesweeperButton[,]? buttonBoard;
        MinesweeperGame game;

        public MainWindow()
        {
            InitializeComponent();
            SoundMenuItem.IsChecked = true;
            StartGame();
        }

        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void Button_MouseRightDown(object sender, MouseButtonEventArgs e)
        {
            MinesweeperButton button = (MinesweeperButton)sender;
            int w = button.W;
            int h = button.H;

            game.flagField(h, w);
            RefreshBoard();

            e.Handled = true;
        }        

        private void Button_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            MinesweeperButton button = (MinesweeperButton)sender;
            int w = button.W;
            int h = button.H;

            game.CheckField(h, w);
            RefreshBoard();

            e.Handled = true;
        }

        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            if(GameGrid != null)
            {
                RadioButton item = (RadioButton)sender;

                MessageBoxResult result = MessageBoxResult.OK;

                if (game.State == MinesweeperGame.GameState.GameInProgress)
                {
                    result = MessageBox.Show("Do you want to cancel the current game?", "", MessageBoxButton.OKCancel);
                }

                if (result == MessageBoxResult.OK)
                {
                    switch (item.Content)
                    {
                        case "Easy":

                            difficult = Difficult.Easy;
                            bombs = 10;
                            height = 9;
                            width = 9;
                            StartGame();

                            break;

                        case "Medium":

                            difficult = Difficult.Medium;
                            bombs = 40;
                            height = 16;
                            width = 16;
                            StartGame();

                            break;

                        case "Hard":

                            difficult = Difficult.Hard;
                            bombs = 99;
                            height = 16;
                            width = 30;
                            StartGame();
                            break;
                    }
                }
                else 
                {
                    switch (difficult)
                    {
                        case Difficult.Easy:
                            RadioBttnEasy.IsChecked = true;
                            break;

                        case Difficult.Medium:
                            RadioBttnMedium.IsChecked = true;
                            break;

                        case Difficult.Hard:
                            RadioBttnHard.IsChecked = true;
                            break;
                    }
                }
            }         
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GameStateMessage(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "State")
            {
                RefreshBoard();
                switch (game.State)
                {
                    case MinesweeperGame.GameState.Lost:
                        ShowMinesOnBoard();
                        MessageBox.Show("Game Over !!!");                        
                        break;
                    case MinesweeperGame.GameState.Won:
                        MessageBox.Show("Congratulations, You won !!!");
                        break;
                }
            }
        }

        private void StartGame()
        {

            GameGrid.Children.Clear();
            GameGrid.ColumnDefinitions.Clear();
            GameGrid.RowDefinitions.Clear();

            buttonBoard = new MinesweeperButton[height, width];

            for (int i = 0; i < width; i++)
            {
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int j = 0; j < height; j++)
            {
                GameGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    MinesweeperButton bttn = new MinesweeperButton();

                    bttn.H = i;
                    bttn.W = j;
                    bttn.Background = Brushes.DimGray;
                    bttn.Height = buttonSize;
                    bttn.Width = buttonSize;
                    bttn.FontSize = 24;
                    bttn.MouseRightButtonDown += Button_MouseRightDown;
                    bttn.PreviewMouseLeftButtonDown += Button_MouseLeftDown;

                    Grid.SetColumn(bttn, j);
                    Grid.SetRow(bttn, i);

                    GameGrid.Children.Add(bttn);
                    buttonBoard[i, j] = bttn;
                }
                GameGrid.SetValue(Grid.ColumnProperty, i);
            }

            if (game != null)
            {
                game.ClosePlayers();
                //GC.Collect();
            }

            game = new MinesweeperGame(width, height, bombs, SoundMenuItem.IsChecked);
            DataContext = game;
            game.PropertyChanged += GameStateMessage;
        }

        private void RefreshBoard()
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (game.Board[i, j].Flag)
                    {
                        buttonBoard[i, j].Background = Brushes.Red;
                    }
                    else
                    {
                        buttonBoard[i, j].Background = Brushes.DimGray;
                    }

                    if (!game.Board[i, j].Covered)
                    {
                        buttonBoard[i, j].Content = "";
                        buttonBoard[i, j].Background = Brushes.LightGray;
                        //buttonBoard[i, j].IsEnabled = false;
                        if (game.Board[i, j].MinesAround > 0)
                        {
                            buttonBoard[i, j].Content = game.Board[i, j].MinesAround.ToString();
                            switch (game.Board[i, j].MinesAround)
                            {
                                case 1:
                                    buttonBoard[i, j].Foreground = Brushes.Blue;
                                    break;
                                case 2:
                                    buttonBoard[i, j].Foreground = Brushes.Green;
                                    break;
                                case 3:
                                    buttonBoard[i, j].Foreground = Brushes.Red;
                                    break;
                                case >= 4:
                                    buttonBoard[i, j].Foreground = Brushes.DarkRed;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void ShowMinesOnBoard()
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {                  
                    if (game.Board[i, j].Mine)
                    {
                        //buttonBoard[i, j].Background = Brushes.Red;
                        Image image = new Image();
                        image.Source = new BitmapImage(new Uri(System.IO.Path.Combine(Directory.GetCurrentDirectory(), @"Icons\mine.ico")));
                        image.Stretch = Stretch.Uniform;
                        buttonBoard[i, j].Content = image;
                    }
                }
            }
        }
    }

    public enum Difficult
    {
        Easy,
        Medium,
        Hard,
        Custom,
    }
}

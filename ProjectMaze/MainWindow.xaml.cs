using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectMaze
{
    public partial class MainWindow : Window
    {
        ObservableCollection<ObservableCollection<Cell>> mapCells;

        public event PropertyChangedEventHandler PropertyChanged;
        Player player { get; set; }

        int _rowsCount = 30;
        public int RowsCount
        {
            get => _rowsCount;
            set
            {
                if (value > 100)
                    _rowsCount = 100;
                else
                    _rowsCount = value;
            }
        }

        int _columnsCount = 30;
        public int ColumnsCount
        {
            get => _columnsCount;
            set
            {
                if (value > 100)
                    _columnsCount = 100;
                else
                    _columnsCount = value;
            }
        }

        Key currentKey = Key.None;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        private void GeneratePlayerPosition(int[,] mas)
        {
            Random rnd = new Random();
            int rowPlayer, colPlayer;


            do //Размещение игрока
            {
                rowPlayer = rnd.Next(1, ColumnsCount - 1);
                colPlayer = rnd.Next(1, RowsCount - 1);
            } while (mas[rowPlayer, colPlayer] == 1 || mas[rowPlayer, colPlayer] == 5 || mas[rowPlayer, colPlayer] == 8); // Изменить проверку

            mas[rowPlayer, colPlayer] = 5;
            player = new Player { Row = rowPlayer, Col = colPlayer };
            Console.WriteLine($"player generated pos: {rowPlayer},{colPlayer}");

        }
        private void MapGenerateButton(object sender, RoutedEventArgs e)
        {
            int rows = RowsCount, columns = ColumnsCount;
            Console.WriteLine($"rows = {rows}, columns = {columns}");
            int[,] mas = new int[ColumnsCount, RowsCount];
            GeneratePlayerPosition(mas);
            mapCells = new ObservableCollection<ObservableCollection<Cell>>();
            for (int i = 0; i < ColumnsCount; i++)
            {
                mapCells.Add(new ObservableCollection<Cell>());
                for (int j = 0; j < RowsCount; j++)
                {
                    switch (mas[i, j])
                    {
                        case 0:
                            mapCells[i].Add(new Space { Row = i, Col = j });
                            break;
                        case 1:
                            mapCells[i].Add(new Wall { Row = i, Col = j });
                            break;
                        case 5:
                            mapCells[i].Add(new Player { Row = i, Col = j });
                            break;
                        case 8:
                            mapCells[i].Add(new Point { Row = i, Col = j });
                            break;
                        case 9:
                            mapCells[i].Add(new Exit { Row = i, Col = j });
                            break;
                    }
                }
            }

            Map.ItemsSource = mapCells;
            GenerateWindow.Visibility = Visibility.Collapsed;
        }
        private void CheckOnlyDigitsKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                string number = e.Key.ToString();
                if (number.Length > 1)
                    number = number.Substring(1);
                if (!Char.IsDigit(char.Parse(number)))
                    e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                e.Handled = true;
            }
        }
        private void Win()
        {

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat || e.Key == Key.None)
                return;

            currentKey = e.Key;

            switch (e.Key)
            {
                case Key.Up:
                case Key.Right:
                case Key.Down:
                case Key.Left:
                    Console.WriteLine($"pressed {currentKey}");
                    Move(currentKey);
                    break;
                case Key.Escape:
                    if (MessageBox.Show("Начать заново?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        GenerateWindow.Visibility = Visibility.Visible;
                        Keyboard.ClearFocus();
                    }
                    break;
            }

        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            currentKey = Key.None;
        }
        private void Move(Key pressed)
        {
            int dx = 0, dy = 0;
            switch (pressed)
            {
                case Key.Up: dy = -1; break;
                case Key.Right: dx = 1; break;
                case Key.Down: dy = 1; break;
                case Key.Left: dx = -1; break;
                default: return;
            }

            int X = player.Col, Y = player.Row;

            int nextY = Y + dy, nextX = X + dx;

            if (nextX < 0 || nextX > RowsCount - 1) return;
            if (nextY < 0 || nextY > ColumnsCount - 1) return;

            Cell target = mapCells[nextY][nextX];

            //if (target is Point)
            //    player.Score++;
            if (target is Exit)
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Win();
                }), null);

            if (target.IsTransient)
            {
                mapCells[Y][X] = new Space() { Row = Y, Col = X };
                mapCells[nextY][nextX] = player;
                player.Row = nextY;
                player.Col = nextX;
                player.Step++;
                Console.WriteLine($"Ход совершен.");
            }
            else
                Console.WriteLine($"Ход невозможен.");

            Console.WriteLine($"X = {nextX},Y = {nextY}");
        }
    }
}

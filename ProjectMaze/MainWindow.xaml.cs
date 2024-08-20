using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
            player = new Player { y = rowPlayer, x = colPlayer };
            Console.WriteLine($"player generated pos: {rowPlayer},{colPlayer}");

        }

        List<Cell> GetNeighbours(Cell cell, int width, int height, Cell[,] mapArray)
        {
            int dist = 2;
            int x = cell.x;
            int y = cell.y;
            Cell left = new Space { x = x - dist, y = y };
            Cell up = new Space { x = x, y = y - dist };
            Cell right = new Space { x = x + dist, y = y };
            Cell down = new Space { x = x, y = y + dist };


            List<Cell> nlist = [left, up, right, down];

            List<Cell> newlist = new();


            foreach (Cell n in nlist)
            {
                //Console.WriteLine($"TEST GetNeighbours - x:{n.x}, y:{n.y}");
                if (n.x >= 0 && n.x < width && n.y >= 0 && n.y < height) // and cell isvisited = false 
                {
                    //Console.WriteLine($"[UNV]Cell GetNeighbours - x:{n.x}, y:{n.y}");
                    if (mapArray[n.x, n.y] == null || !mapArray[n.x, n.y].IsVisited)
                    {
                        newlist.Add(n);
                        Console.WriteLine($"[V]Cell GetNeighbours - x:{n.x}, y:{n.y}");
                    }

                }
            }

            return newlist;
        }
        Cell GetWallBetweenCells(Cell first, Cell second)
        {
            int x, y;
            x = second.x - first.x;
            y = second.y - first.y;
            x /= 2; y /= 2;
            Cell cell = new Space();
            cell.x = first.x + x; cell.y = first.y + y;
            Console.WriteLine($"GetWall: x:{cell.x}, y:{cell.y}");
            return cell;
        }
        private void MapGenerateButton(object sender, RoutedEventArgs e)
        {
            int rows = RowsCount, columns = ColumnsCount;

            Console.WriteLine($"rows = {rows}, columns = {columns}");

            //int[,] mas = new int[ColumnsCount, RowsCount];
            Cell[,] mapArray = new Cell[ColumnsCount, RowsCount];

            mapCells = new ObservableCollection<ObservableCollection<Cell>>();

            Cell startCell = new Space
            {
                x = 0,
                y = 0,
                IsVisited = true
            };
            mapArray[startCell.x, startCell.y] = startCell;
            Cell currentCell = startCell;
            Random rnd = new Random();

            List<Cell> neighbours;
            do
            {
                neighbours = GetNeighbours(currentCell, columns, rows, mapArray);
                Console.WriteLine($"CurrentCell: x:{currentCell.x}, y:{currentCell.y}, neighbours count: {neighbours.Count()}");

                if (neighbours.Count() > 0)
                {
                    int rand = rnd.Next(neighbours.Count());

                    Cell neighbourCell = neighbours[rand];
                    Console.WriteLine($"Cell Selected: x:{neighbourCell.x}, y:{neighbourCell.y}");

                    neighbourCell.IsVisited = true;
                    mapArray[neighbourCell.x, neighbourCell.y] = neighbourCell;

                    Cell wall = GetWallBetweenCells(currentCell, neighbourCell);
                    mapArray[wall.x, wall.y] = wall;
                    Console.WriteLine($"Wall removed: x:{wall.x}, y:{wall.y}");

                    currentCell = neighbourCell;
                }
                else
                    Console.WriteLine($"Соседей: {neighbours.Count()}. Завершение работы");
            } while (neighbours.Count() > 0);


            //for (int i = 0; i < ColumnsCount; i++)
            //{
            //    mapCells.Add(new ObservableCollection<Cell>());
            //    for (int j = 0; j < RowsCount; j++)
            //    {
            //        switch (0)
            //        {
            //            case 0:
            //                mapCells[i].Add(new Space { x = j, y = i });
            //                break;
            //            case 1:
            //                mapCells[i].Add(new Wall { y = i, x = j });
            //                break;
            //            case 5:
            //                mapCells[i].Add(new Player { y = i, x = j });
            //                break;
            //            case 8:
            //                mapCells[i].Add(new Point { y = i, x = j });
            //                break;
            //            case 9:
            //                mapCells[i].Add(new Exit { y = i, x = j });
            //                break;
            //        }
            //    }
            //}

            //GeneratePlayerPosition(mas); // i =y ; j = x
            for (int i = 0; i < ColumnsCount; i++)
            {
                mapCells.Add(new ObservableCollection<Cell>());
                for (int j = 0; j < RowsCount; j++)
                {
                    if (mapArray[i, j] != null)
                        mapCells[i].Add(mapArray[i, j]);
                    else
                        mapCells[i].Add(new Wall { x = i, y = j });
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
                    //if (MessageBox.Show("Начать заново?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    //{
                    //    GenerateWindow.Visibility = Visibility.Visible;
                    //    Keyboard.ClearFocus();
                    //}
                    MapGenerateButton(null, null);
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

            int X = player.x, Y = player.y;

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
                mapCells[Y][X] = new Space() { y = Y, x = X };
                mapCells[nextY][nextX] = player;
                player.y = nextY;
                player.x = nextX;
                player.Step++;
                Console.WriteLine($"Ход совершен.");
            }
            else
                Console.WriteLine($"Ход невозможен.");

            Console.WriteLine($"X = {nextX},Y = {nextY}");
        }
    }
}

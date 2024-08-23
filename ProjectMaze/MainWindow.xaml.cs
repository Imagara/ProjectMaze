using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

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
        private void GeneratePlayerPosition(Cell[,] mapArray)
        {
            Random rnd = new Random();
            int rowPlayer, colPlayer;

            rowPlayer = rnd.Next(1, ColumnsCount - 2);
            if (rowPlayer % 2 != 0)
                rowPlayer++;
            colPlayer = rnd.Next(1, RowsCount - 2);
            if (colPlayer % 2 != 0)
                colPlayer++;

            player = new Player { y = colPlayer, x =  rowPlayer};
            mapArray[rowPlayer, colPlayer] = player;
            Console.WriteLine($"Позиция игрока: [{rowPlayer}][{colPlayer}]");
        }

        private List<Cell> GetNeighbours(Cell cell, int width, int height, Cell[,] mapArray, bool isVisitedCheck = true)
        {
            int walkDist = 2;
            int x = cell.x;
            int y = cell.y;

            Cell left = new Space { x = x - walkDist, y = y };
            Cell up = new Space { x = x, y = y - walkDist };
            Cell right = new Space { x = x + walkDist, y = y };
            Cell down = new Space { x = x, y = y + walkDist };

            List<Cell> nlist = [left, up, right, down];
            List<Cell> newlist = new();

            foreach (Cell n in nlist)
            {
                if (n.x >= 0 && n.x < width && n.y >= 0 && n.y < height) // Если в пределах лабиринта
                {
                    if (isVisitedCheck && (mapArray[n.x, n.y] == null || !mapArray[n.x, n.y].IsVisited))
                        newlist.Add(n);
                    else if (!isVisitedCheck)
                        newlist.Add(n);
                }
            }

            return newlist;
        }
        private Cell GetWallBetweenCells(Cell first, Cell second)
        {
            int x, y;
            x = second.x - first.x;
            y = second.y - first.y;
            x /= 2; y /= 2;
            Cell cell = new Space();
            cell.x = first.x + x; cell.y = first.y + y;
            return cell;
        }
        private Cell GetRandomUnVisitedCell(Cell[,] mapArray)
        {
            for (int i = 0; i < ColumnsCount; i += 2)
            {
                for (int j = 0; j < RowsCount; j += 2)
                {
                    if (mapArray[i, j] == null)
                        return new Space { x = i, y = j };
                }
            }
            return null;
        }
        private void AddToTraces(Cell cell, List<Cell> traces)
        {
            traces.Add(cell);
            if (traces.Count > RowsCount * ColumnsCount)
                traces.RemoveAt(0);
        }
        private Cell MoveBack(List<Cell> traces)
        {
            Cell cell = traces.Last();
            traces.RemoveAt(traces.Count() - 1);
            return cell;
        }
        private void MapGenerateButton(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"\n\n\nGenerating...");
            int rows = RowsCount, columns = ColumnsCount;
            Console.WriteLine($"rows = {rows}, columns = {columns}");

            List<Cell> traces = new();
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
            bool isRandomGenerated = false;

            List<Cell> neighbours;
            do
            {
                if (isRandomGenerated)
                {
                    neighbours = GetNeighbours(currentCell, columns, rows, mapArray, !isRandomGenerated);
                    isRandomGenerated = false;
                }
                else
                {
                    neighbours = GetNeighbours(currentCell, columns, rows, mapArray);
                }

                if (neighbours.Count() != 0)
                {
                    int rand = rnd.Next(neighbours.Count());
                    Cell neighbourCell = neighbours[rand];
                    neighbourCell.IsVisited = true;
                    mapArray[neighbourCell.x, neighbourCell.y] = neighbourCell;
                    Console.WriteLine($"Выбрана ячейка для хода: [{neighbourCell.x}][{neighbourCell.y}]");

                    Cell wall = GetWallBetweenCells(currentCell, neighbourCell);
                    mapArray[wall.x, wall.y] = wall;

                    AddToTraces(currentCell, traces);
                    currentCell = neighbourCell;
                }
                else if (neighbours.Count() == 0 && traces.Count > 0 && currentCell != traces.First())
                {
                    //if (GetRandomUnVisitedCell(mapArray) == null)
                    //    break;
                    currentCell = MoveBack(traces);
                    Console.WriteLine($"Возврат на [{currentCell.x}][{currentCell.y}]");
                }
                else if (GetRandomUnVisitedCell(mapArray) != null)
                {
                    currentCell = GetRandomUnVisitedCell(mapArray);
                    Console.WriteLine($"Переход на точку: [{currentCell.x}][{currentCell.y}]");
                    isRandomGenerated = true;
                }
                else
                {
                    Console.WriteLine($"Все клетки посещены. Завершение работы");
                    break;
                }
            } while (true);


            GeneratePlayerPosition(mapArray);

            //
            for (int j = 0; j < rows; j++)
            {
                mapCells.Add(new ObservableCollection<Cell>());
                for (int i = 0; i < columns; i++)
                {
                    if (mapArray[i, j] != null)
                        mapCells[j].Add(mapArray[i, j]);
                    else
                        mapCells[j].Add(new Wall { x = i, y = j });
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

            int nextY = Y + dy * 2, nextX = X + dx * 2;

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
                player.y = nextY;
                player.x = nextX;
                mapCells[nextY][nextX] = player;
                player.Step++;
                Console.WriteLine($"Ход совершен.");
            }
            else
                Console.WriteLine($"Ход невозможен.");

            Console.WriteLine($"X = {nextX},Y = {nextY}");
        }
    }
}

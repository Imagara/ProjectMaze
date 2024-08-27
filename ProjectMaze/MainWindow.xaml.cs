using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ProjectMaze
{
    public partial class MainWindow : Window
    {
        ObservableCollection<ObservableCollection<Cell>> mapCells;
        public event PropertyChangedEventHandler PropertyChanged;
        Player player { get; set; }

        int _difficultySelectedIndex = 0;
        public int DifficultySelectedIndex
        {
            get => _difficultySelectedIndex;
            set
            {
                _difficultySelectedIndex = value;
            }
        }

        int _rowsCount = 15;
        public int RowsCount
        {
            get => (_rowsCount / 2) + 1;
            set
            {
                if (value > 100)
                    _rowsCount = 199;
                else if (value < 3)
                    _rowsCount = 5;
                else
                    _rowsCount = (value * 2) - 1;
            }
        }

        int _columnsCount = 15;
        public int ColumnsCount
        {
            get => (_columnsCount / 2) + 1;
            set
            {
                if (value > 100)
                    _columnsCount = 199;
                else if (value < 3)
                    _columnsCount = 5;
                else
                    _columnsCount = (value * 2) - 1;
            }
        }

        Key currentKey = Key.None;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        private Cell GenerateRandomEmptyPosition(Cell[,] mapArray)
        {
            Random rnd = new Random();
            int x = 0, y = 0;

            do
            {
                x = rnd.Next(0, _columnsCount - 1);
                if (x % 2 != 0)
                    x++;

                y = rnd.Next(0, _rowsCount - 1);
                if (y % 2 != 0)
                    y++;

            } while (mapArray[x, y] is not Space);

            Cell emptyCell = new Space { x = x, y = y };
            return emptyCell;
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
            for (int i = 0; i < _columnsCount; i += 2)
            {
                for (int j = 0; j < _rowsCount; j += 2)
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
            if (traces.Count > _rowsCount / 2 * _columnsCount / 2)
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
            Console.WriteLine($"\n\n\nГенерация...");
            int rows = _rowsCount, columns = _columnsCount;
            Console.WriteLine($"Размер лабиринта = {RowsCount}x{ColumnsCount}");

            List<Cell> traces = new();
            Cell[,] mapArray = new Cell[columns, rows];
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
                    if (traces.Count % 10 == 0 && GetRandomUnVisitedCell(mapArray) == null)
                        break;
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
                    Console.WriteLine($"Все клетки посещены. Лабиринт сгенерирован");
                    break;
                }
            } while (true);


            Cell playerRandomCell = GenerateRandomEmptyPosition(mapArray);
            player = new Player { x = playerRandomCell.x, y = playerRandomCell.y };
            mapArray[playerRandomCell.x, playerRandomCell.y] = player;
            Console.WriteLine($"Позиция игрока: [{playerRandomCell.x}][{playerRandomCell.y}]");
            rightBorder.DataContext = player;

            Cell exitRandomCell = GenerateRandomEmptyPosition(mapArray);
            Exit exit = new Exit { x = exitRandomCell.x, y = exitRandomCell.y };
            mapArray[exitRandomCell.x, exitRandomCell.y] = exit;
            Console.WriteLine($"Exit was created on [{exitRandomCell.x}][{exitRandomCell.y}]");

            int pointsCount = 1 + DifficultySelectedIndex * 2;

            for (int i = 0; i < pointsCount; i++)
            {
                Cell randomCell = GenerateRandomEmptyPosition(mapArray);
                if (randomCell == null)
                    break;
                Point point = new Point { x = randomCell.x, y = randomCell.y };
                mapArray[randomCell.x, randomCell.y] = point;
                Console.WriteLine($"Point was created on [{randomCell.x}][{randomCell.y}]");
            }



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
            //GenerateWindowVisibility = false;
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

            if (nextX < 0 || nextX > _columnsCount - 1)
                return;
            if (nextY < 0 || nextY > _rowsCount - 1)
                return;

            if (mapCells[Y + dy * 2][X + dx * 2] == null)
                return;

            Cell target = mapCells[Y + dy * 2][X + dx * 2];
            Cell targetWall = mapCells[nextY][nextX];

            if (target is Point)
                player.Points++;
            if (target is Exit)
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Win();
                }), null);

            if (target.IsTransient && targetWall.IsTransient)
            {
                mapCells[Y][X] = new Space() { y = Y, x = X };
                player.y = target.y;
                player.x = target.x;
                mapCells[target.y][target.x] = player;
                player.Steps++;
                Console.WriteLine($"Ход совершен.");
            }
            else
                Console.WriteLine($"Ход невозможен.");

            Console.WriteLine($"X = {nextX},Y = {nextY}");
        }
    }
}

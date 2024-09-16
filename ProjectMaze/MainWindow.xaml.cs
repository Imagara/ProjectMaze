using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ProjectMaze
{
    public partial class MainWindow : Window
    {
        //Игровое поле
        ObservableCollection<ObservableCollection<Cell>> mapCells;
        // Игрок
        Player player { get; set; }
        //Таймер для движения игрока без повторного нажатия на клавишу передвижения
        DispatcherTimer pressedKeyTimer { get; set; }
        //Нажатая клавиша
        Key currentKey = Key.None;
        #region Maze settings
        //Выбранная сложность
        int _difficultySelectedIndex = 0;
        public int DifficultySelectedIndex
        {
            get => _difficultySelectedIndex;
            set
            {
                _difficultySelectedIndex = value;
            }
        }
        // Размер лабиринта (строки)
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
        // Размер лабиринта (столбцы)
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
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        public Cell GenerateRandomEmptyCell(Cell[,] mapArray, int columnsCount = 0, int rowsCount = 0)
        {
            Random rnd = new Random();
            int x, y;


            if (columnsCount == 0 || rowsCount == 0)
            {
                columnsCount = _columnsCount;
                rowsCount = _rowsCount;
            }

            do // Нахождение свободной ячейки
            {
                x = rnd.Next(0, columnsCount - 1);
                if (x % 2 != 0)
                    x++;
                y = rnd.Next(0, rowsCount - 1);
                if (y % 2 != 0)
                    y++;
            } while (mapArray[x, y] is not Space);

            Cell emptyCell = new Space { x = x, y = y };
            return emptyCell;
        }
        public List<Cell> GetNeighbours(Cell cell, int width, int height, Cell[,] mapArray, bool isVisitedCheck = true)
        {
            //Дистанция ходьбы
            int walkDist = 2;

            //Расположение клетки
            int x = cell.x;
            int y = cell.y;

            //Соседние клетки
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
                    //Проверка была ли посещена клетка
                    if (isVisitedCheck && (mapArray[n.x, n.y] == null || !mapArray[n.x, n.y].IsVisited))
                        newlist.Add(n);
                    //Без проверки
                    else if (!isVisitedCheck)
                        newlist.Add(n);
                }
            }
            return newlist;
        }
        public Cell GetWallBetweenCells(Cell first, Cell second)
        {
            //Получение клетки между двумя другими
            int x, y;
            x = second.x - first.x;
            y = second.y - first.y;
            x /= 2; y /= 2;
            Cell cell = new Space();
            cell.x = first.x + x; cell.y = first.y + y;
            return cell;
        }
        public Cell GetRandomUnVisitedCell(Cell[,] mapArray, int columnsCount = 0, int rowsCount = 0)
        {
            //Получение случайной не посещенной клетки
            for (int i = 0; i < columnsCount; i += 2)
            {
                for (int j = 0; j < rowsCount; j += 2)
                {
                    if (mapArray[i, j] == null)
                        return new Space { x = i, y = j };
                }
            }
            return null;
        }
        public void AddToTraces(Cell cell, List<Cell> traces)
        {
            //Добавление в список с историей пройденных клеток
            traces.Add(cell);
            //Если список слишком большой - удаляется первая клетка в списке
            if (traces.Count > _rowsCount / 2 * _columnsCount / 2)
                traces.RemoveAt(0);
        }
        public Cell MoveBack(List<Cell> traces)
        {
            //Передвижение на прошлую клетку (из списка с историей пройденных клеток)
            Cell cell = traces.Last();
            traces.RemoveAt(traces.Count() - 1);
            return cell;
        }
        private int GetAllPointsCount()
        {
            //Получение количества семян, которые должны будут сгенерированы(или их общее количество) на карте
            if (TurnSeedCheckBox.IsChecked == false)//Если семена отключены 
                return 0;
            return Convert.ToInt32(1 + ((DifficultySelectedIndex + 1) * 0.75 * ((ColumnsCount + RowsCount) / 5)));
        }
        private void MapGenerateButton(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"\n\n\nГенерация...");
            //Определение размеров лабиринта
            int rows = _rowsCount, columns = _columnsCount;
            Console.WriteLine($"Размер лабиринта = {RowsCount}x{ColumnsCount}");

            List<Cell> traces = new();
            Cell[,] mapArray = new Cell[columns, rows];
            mapCells = new ObservableCollection<ObservableCollection<Cell>>();

            //Стартовая точка генерации
            Cell startCell = new Space
            {
                x = 0,
                y = 0,
                IsVisited = true
            };
            mapArray[startCell.x, startCell.y] = startCell;
            Console.WriteLine($"Стартовая точка генерации: [{startCell.x}][{startCell.y}]");
            Cell currentCell = startCell;
            Random rnd = new Random();
            bool isRandomGenerated = false;

            List<Cell> neighbours;
            do
            {
                if (isRandomGenerated)//Если следующая ячейка генерируется случайно
                {
                    neighbours = GetNeighbours(currentCell, columns, rows, mapArray, !isRandomGenerated);
                    isRandomGenerated = false;
                }
                else
                {
                    neighbours = GetNeighbours(currentCell, columns, rows, mapArray);
                }

                if (neighbours.Count() != 0) //Если есть соседние клетки
                {
                    int rand = rnd.Next(neighbours.Count());
                    Cell neighbourCell = neighbours[rand];
                    neighbourCell.IsVisited = true;
                    mapArray[neighbourCell.x, neighbourCell.y] = neighbourCell;
                    Console.WriteLine($"Переход на точку: [{neighbourCell.x}][{neighbourCell.y}]");

                    //Удаление стены между новой и прошлой клеткой
                    Cell wall = GetWallBetweenCells(currentCell, neighbourCell);
                    mapArray[wall.x, wall.y] = wall;

                    //Добавление клетки в список с историей ходов
                    AddToTraces(currentCell, traces);
                    currentCell = neighbourCell;
                }
                else if (neighbours.Count() == 0 && traces.Count > 0 && currentCell != traces.First()) //Если нет соседей и можно вернутся назад
                {
                    //Ход назад
                    currentCell = MoveBack(traces);
                    Console.WriteLine($"Возврат на [{currentCell.x}][{currentCell.y}]");
                }
                else if (GetRandomUnVisitedCell(mapArray) != null)
                {
                    //Генерация следующей клетки случайно
                    currentCell = GetRandomUnVisitedCell(mapArray);
                    Console.WriteLine($"Переход к следующей случайной клетке: [{currentCell.x}][{currentCell.y}]");
                    isRandomGenerated = true;
                }
                else
                {
                    //Лабиринт сгенерирован, выход из цикла
                    Console.WriteLine($"Все клетки посещены. Лабиринт сгенерирован");
                    break;
                }
            } while (true);

            #region Размещение игрока
            Cell playerRandomCell = GenerateRandomEmptyCell(mapArray);
            player = new Player { x = playerRandomCell.x, y = playerRandomCell.y };
            mapArray[playerRandomCell.x, playerRandomCell.y] = player;
            Console.WriteLine($"Позиция игрока: [{playerRandomCell.x}][{playerRandomCell.y}]");
            RightBorder.DataContext = player;
            #endregion

            #region Создание выхода
            Cell exitRandomCell = GenerateRandomEmptyCell(mapArray);
            Exit exit = new Exit { x = exitRandomCell.x, y = exitRandomCell.y };
            mapArray[exitRandomCell.x, exitRandomCell.y] = exit;
            Console.WriteLine($"Выход был создан [{exitRandomCell.x}][{exitRandomCell.y}]");
            #endregion

            #region Создание Seeds
            for (int i = 0; i < GetAllPointsCount(); i++)
            {
                Cell randomCell = GenerateRandomEmptyCell(mapArray);
                if (randomCell == null)
                    break;
                Point point = new Point { x = randomCell.x, y = randomCell.y };
                mapArray[randomCell.x, randomCell.y] = point;
                Console.WriteLine($"Семечко было создано [{randomCell.x}][{randomCell.y}]");
            }
            Console.WriteLine($"Всего семян было создано: {GetAllPointsCount()}");
            #endregion


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
            RightBorder.Visibility = Visibility.Visible;
            MapBorder.Visibility = Visibility.Visible;
        }
        #region Player movement 
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat || e.Key == Key.None || player == null)
                return;

            switch (e.Key)
            {
                case Key.Up:
                case Key.Right:
                case Key.Down:
                case Key.Left:
                    pressedKeyTimer?.Stop();
                    currentKey = e.Key;
                    Move(null, null);
                    pressedKeyTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(250), DispatcherPriority.Background, Move, Dispatcher);
                    break;
                case Key.Escape:
                    if (MessageBox.Show("Начать заново?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        MapBorder.Visibility = Visibility.Collapsed;
                        ResultGrid.Visibility = Visibility.Collapsed;
                        RightBorder.Visibility = Visibility.Collapsed;
                        GenerateWindow.Visibility = Visibility.Visible;
                        Keyboard.ClearFocus();
                    }
                    break;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            currentKey = Key.None;
            pressedKeyTimer?.Stop();
        }
        private void Move(object sender, EventArgs e)
        {
            int dx = 0, dy = 0;
            switch (currentKey)
            {
                case Key.Up: dy = -1; break;
                case Key.Right: dx = 1; break;
                case Key.Down: dy = 1; break;
                case Key.Left: dx = -1; break;
                default: return;
            }

            int X = player.x, Y = player.y;
            Cell recentPlayerPos = mapCells[Y][X];

            int nextY = Y + dy, nextX = X + dx;

            if (nextX < 0 || nextX > _columnsCount - 1)
                return;
            if (nextY < 0 || nextY > _rowsCount - 1)
                return;

            if (mapCells[Y + dy * 2][X + dx * 2] == null)
                return;

            Cell target = mapCells[Y + dy * 2][X + dx * 2];
            Cell targetWall = mapCells[nextY][nextX];



            if (target.IsTransient && targetWall.IsTransient)
            {
                if (recentPlayerPos is not ExitPlayer && recentPlayerPos is not Exit)
                    mapCells[Y][X] = new Space() { y = Y, x = X };
                else
                    mapCells[Y][X] = new Exit() { y = Y, x = X };

                player.y = target.y;
                player.x = target.x;

                if (target is not Exit)
                    mapCells[target.y][target.x] = player;
                else
                    mapCells[target.y][target.x] = new ExitPlayer { x = target.x, y = target.y };
                player.Steps++;
            }
            else
            {
                Console.WriteLine($"Ход невозможен.");
                return;
            }

            if (target is Point)
            {
                player.Points++;
                target = null;
            }

            if (target is Exit && GetAllPointsCount() <= player.Points)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    //Победа
                    pressedKeyTimer?.Stop();
                    ResultStepsCount.Text = player.Steps.ToString();
                    ResultPointsCount.Text = player.Points.ToString();
                    ResultGrid.Visibility = Visibility.Visible;
                    RightBorder.Visibility = Visibility.Collapsed;
                    MapBorder.Visibility = Visibility.Collapsed;
                    GenerateWindow.Visibility = Visibility.Visible;
                    Keyboard.ClearFocus();
                }), null);

            }
            Console.WriteLine($"Позиция игрока: [{player.x}][{player.y}]");
        }
        #endregion
        private void CheckOnlyDigitsKeyDown(object sender, KeyEventArgs e)
        {
            //Проверка на ввод ТОЛЬКО цифр
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
                Console.WriteLine(ex.Message);
                e.Handled = true;
            }
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //Пункты в меню "О программе"
            MenuItem menuItem = (MenuItem)sender;
            if (menuItem.Header.ToString() == "Справка")
                MessageBox.Show(
                    "\t\t\tИгра лабиринты\n" +
                    "В данной игре необходимо передвигаясь по клеткам найти выход\n" +
                    "Управление происходит персонажем \"Курочка\" на стрелочки:\n" +
                    "\t← - передвижение влево\n" +
                    "\t↑ - передвижение вверх\n" +
                    "\t→ - передвижение вправо\n" +
                    "\t↓ - передвижение вниз\n" +
                    "Данные клавиши можно нажимать для одного хода или зажимать, чтобы персонаж двигался к ближайшей стене в выбранном направлении."
                    );
            if (menuItem.Header.ToString() == "О разработчике")
                MessageBox.Show(
                    "Данная игра была выполнена при выполнении курсового проекта:\n" +
                    "Объектно-ориентированное программирование (КП)\n" +
                    "Индивидуальный вариант: 13 Лабиринты\n" +
                    "Выполнил: Гавриленко Артём Вячеславович\n" +
                    "Группа: з-423П10-4");
        }
    }
}

using System;
using System.Collections.ObjectModel;
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
        //Генератор лабиринтов
        MazeGenerator MazeGenerator { get; set; }
        //Нажатая клавиша
        Key currentKey = Key.None;
        #region Maze settings
        //Выбранная сложность
        private int _difficultySelectedIndex = 0;
        // Размер лабиринта (строки)
        private int _rowsCount = 15;
        // Размер лабиринта (столбцы)
        int _columnsCount = 15;

        public int DifficultySelectedIndex
        {
            get => _difficultySelectedIndex;
            set
            {
                _difficultySelectedIndex = value;
            }
        }
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

        private void GenerateMapButton_Click(object sender, RoutedEventArgs e)
        {
            MazeGenerator = new(_rowsCount,_columnsCount, TurnSeedCheckBox.IsChecked, _difficultySelectedIndex);
            Cell[,] mapArray = MazeGenerator.GetGeneratedMap();

            #region Размещение игрока
            Cell playerRandomCell = MazeGenerator.GenerateRandomEmptyCell(mapArray);
            player = new Player(playerRandomCell.x, playerRandomCell.y);
            mapArray[playerRandomCell.x, playerRandomCell.y] = player;
            Console.WriteLine($"Позиция игрока: [{playerRandomCell.x}][{playerRandomCell.y}]");
            RightBorder.DataContext = player;
            #endregion


            mapCells = new ObservableCollection<ObservableCollection<Cell>>();

            for (int j = 0; j < _rowsCount; j++)
            {
                mapCells.Add(new ObservableCollection<Cell>());
                for (int i = 0; i < _columnsCount; i++)
                {
                    if (mapArray[i, j] != null)
                        mapCells[j].Add(mapArray[i, j]);
                    else
                        mapCells[j].Add(new Wall(i, j));
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

            int x = player.x, y = player.y;
            Cell recentPlayerPos = mapCells[y][x];

            int nextY = y + dy, nextX = x + dx;

            //Если целевая точка в пределах лабиринта
            if (nextX < 0 || nextX > _columnsCount - 1)
                return;
            if (nextY < 0 || nextY > _rowsCount - 1)
                return;
            if (mapCells[y + dy * 2][x + dx * 2] == null)
                return;

            Cell target = mapCells[y + dy * 2][x + dx * 2];
            Cell targetWall = mapCells[nextY][nextX];



            if (target.IsTransient && targetWall.IsTransient)
            {
                if (recentPlayerPos is not ExitPlayer && recentPlayerPos is not Exit)
                    mapCells[y][x] = new Space(x, y);
                else
                    mapCells[y][x] = new Exit(x, y);

                player.y = target.y;
                player.x = target.x;

                if (target is not Exit)
                    mapCells[target.y][target.x] = player;
                else
                    mapCells[target.y][target.x] = new ExitPlayer(target.x, target.y);
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

            if (target is Exit && MazeGenerator.GetAllPointsCount() <= player.Points)
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
                if (e.Key == Key.Tab || e.Key == Key.Escape)
                    e.Handled = false;
                // Если не цифра - запретить ввод
                else if (!Char.IsDigit(char.Parse(number)))
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
                    "В данной игре необходимо передвигаясь по клеткам найти выход.\n" +
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

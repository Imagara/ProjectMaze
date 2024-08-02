using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
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
using System.Windows.Threading;

namespace ProjectMaze
{
    public partial class MainWindow : Window
    {
        ObservableCollection<ObservableCollection<Cell>> mapCells;
        Player player { get; set; }

        static int RowsCount = 15;
        static int ColumnsCount = 15;

        bool isGenerated = false;

        Key currentKey = Key.None;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var hash = System.Security.Cryptography.SHA1.Create();

            output.Text = string.Concat(hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(inputTB.Text)).Select(x => x.ToString("X2")));
        }
        private void GeneratePlayerPosition(int[,] mas)
        {
            Random rnd = new Random();
            int rowPlayer, colPlayer;
            

            do //Размещение игрока
            {
                rowPlayer = rnd.Next(1, RowsCount);
                colPlayer = rnd.Next(1, ColumnsCount);
            } while (mas[rowPlayer, colPlayer] == 1 || mas[rowPlayer, colPlayer] == 5 || mas[rowPlayer, colPlayer] == 8); // Изменить проверку

            player = new Player { Row = rowPlayer, Col = colPlayer};

            mas[rowPlayer, colPlayer] = 5;
        }
        private void MapGenerateButton(object sender, RoutedEventArgs e)
        {
            int rows = RowsCount, columns = ColumnsCount;
            int[,] mas = new int[15, 15] //Изначальный массив лабиринта
            {
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1 },
                { 1, 0, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 1, 0, 1 },
                { 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1 },
                { 1, 0, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1 },
                { 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 1, 1 },
                { 1, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 1, 8, 9 },
                { 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 8, 1 },
                { 1, 0, 1, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 8, 1 },
                { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1 },
                { 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1, 0, 1, 0, 1 },
                { 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1 },
                { 1, 0, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1 },
                { 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            };

            GeneratePlayerPosition(mas);
            mapCells = new ObservableCollection<ObservableCollection<Cell>>();
            for (int i = 0; i < RowsCount; i++)
            {
                mapCells.Add(new ObservableCollection<Cell>());
                for (int j = 0; j < RowsCount; j++)
                {
                    switch (mas[i, j])
                    {
                        case 1:
                            mapCells[i].Add(new Wall { Row = i, Col = j });
                            break;
                        case 0:
                            mapCells[i].Add(new Space { Row = i, Col = j });
                            break;
                        case 5:
                            mapCells[i].Add(new Player { Row = i, Col = j });
                            break;
                        case 8:
                            mapCells[i].Add(new Klyuch { Row = i, Col = j });
                            break;
                        case 9:
                            mapCells[i].Add(new Exit { Row = i, Col = j });
                            break;
                    }
                }
            }

            Map.ItemsSource = mapCells;
            isGenerated = true;
            Map.Focus();
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
            
            

            Console.WriteLine($"pressed {currentKey}");

            if (e.IsRepeat)
                return;

            currentKey = e.Key;

            switch (e.Key)
            {
                case Key.Up:
                case Key.Right:
                case Key.Down:
                case Key.Left:
                    Move(currentKey);
                    Console.WriteLine($"Ход совершен.");
                    break;
                case Key.Escape:
                    if (MessageBox.Show("Начать заново?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
            if (!isGenerated)
                return;
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
            Console.WriteLine($"Y = {Y}, X = {X}");
            int nextY = Y + dy, nextX = X + dx;

            if (nextY < 0 || nextY > 14) return; // 14 надо заменить на константы размера
            if (nextX < 0 || nextX > 14) return;

            Cell target = mapCells[nextY][nextX];

            if (target is Klyuch)
                player.Score++;
            if (target is Exit && player.Score != 0)
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
            }
        }

        private void BarGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            if(isGenerated)
            {
                Console.WriteLine("Generated and Focus cleared");
                Keyboard.ClearFocus();
                Map.Focus();
            }
                
        }
    }
}

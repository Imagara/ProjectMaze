using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace ProjectMaze
{
    internal abstract class Cell : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //Позиция клетки (Ряд, место)
        public int x, y;

        //Ширина клетки
        private int _cellWidth = 30;
        //Высота клетки
        private int _cellHeight = 30;
        //Вид клетки (Игрок/Стена/Пустая клетка/Выход/Игрок в выходе)
        private string _file = "empty.png";
        // Можно ли пройти через эту клетку
        public virtual bool IsTransient { get; set; }
        // Посещелась ли эта клетка (используется при генерации)
        public bool IsVisited { get; set; }

        public virtual Brush Background
        {
            get => Brushes.Transparent;
            set { }
        }
        public virtual string File
        {
            get => GetImageUri(_file);
            set
            {
                _file = value;
                OnPropertyChanged();
            }
        }
        public virtual int CellWidth
        {
            get
            {
                _cellWidth = x % 2 != 0 ? 3 : 30;
                return _cellWidth;
            }
            set
            {
                _cellWidth = value;
                OnPropertyChanged();
            }
        }
        public virtual int CellHeight
        {
            get
            {
                _cellHeight = y % 2 != 0 ? 3 : 30;
                return _cellHeight;
            }
            set
            {
                _cellHeight = value;
                OnPropertyChanged();
            }
        }

        //Конструктор Cell
        public Cell(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        internal string GetImageUri(string shortname)
        {
            string src = new AssemblyName(GetType().Assembly.FullName).Name;
            return "/" + src + ";component/src/" + shortname;
        }
        internal void OnPropertyChanged([CallerMemberName] string prop = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
    internal class Space : Cell
    {
        //Конструктор пустой клетки (Проходимой)
        public Space(int x, int y, bool isVisited = false) : base(x, y)
        {

        }

        public override bool IsTransient => true;
    }

    internal class Player : Cell
    {
        //Вид ячейки - курочка
        string _file = "player.png";
        //Количество шагов
        private int _steps;
        //Количество собранных семян (очков)
        int _points = 0;

        //Конструктор Player
        public Player(int x, int y) : base(x, y)
        {

        }

        public override string File
        {
            get => GetImageUri(_file);
            set
            {
                _file = value;
                OnPropertyChanged();
            }
        }
        public int Steps
        {
            get => _steps;
            set
            {
                _steps = value;
                OnPropertyChanged();
            }
        }

        public int Points
        {
            get => _points;
            set
            {
                _points = value;
                OnPropertyChanged();
            }
        }
    }
    internal class Wall : Cell
    {
        //Вид ячейки - стена (непроходимая)
        string _file = "wall.png";
        Brush _background = Brushes.Black;
        //Конструктор стены
        public Wall(int x, int y) : base(x, y)
        {
        }

        public override string File
        {
            get => GetImageUri(_file);
            set
            {
                _file = value;
                OnPropertyChanged();
            }
        }

        public override Brush Background
        {
            get => _background;
            set
            {
                _background = value;
                OnPropertyChanged();
            }
        }
    }
    internal class Point : Cell
    {
        public override bool IsTransient => true;
        //Вид ячейки - семечко
        string _file = "seed.png";
        Brush _background = Brushes.Transparent;
        //Конструктор
        public Point(int x, int y) : base(x, y)
        {
        }

        public override string File
        {
            get => GetImageUri(_file);
            set
            {
                _file = value;
                OnPropertyChanged();
            }
        }

        public override Brush Background
        {
            get => _background;
            set
            {
                _background = value;
                OnPropertyChanged();
            }
        }
    }
    internal class Exit : Cell
    {
        public override bool IsTransient => true;
        private string _file = "exit.png";
        Brush _background = Brushes.Transparent;

        //Конструктор
        public Exit(int x, int y) : base(x, y)
        {
        }

        public override string File
        {
            get => GetImageUri(_file);
            set
            {
                _file = value;
                OnPropertyChanged();
            }
        }

        public override Brush Background
        {
            get => _background;
            set
            {
                _background = value;
                OnPropertyChanged();
            }
        }
    }
    class ExitPlayer : Exit
    {
        //Вид ячейки - игрок в выходе
        private string _file = "player_exit.png";
        //Конструктор
        public ExitPlayer(int x, int y) : base(x, y)
        {
        }

        public override string File
        {
            get => GetImageUri(_file);
            set
            {
                _file = value;
                OnPropertyChanged();
            }
        }
    }
}

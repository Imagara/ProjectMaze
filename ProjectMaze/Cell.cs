using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace ProjectMaze
{
    public abstract class Cell : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int x, y;
        private int _cellWidth = 30;
        public virtual bool IsTransient { get; set; }
        public virtual bool IsVisited { get; set; }
        string _file = "empty.png";
        public virtual Brush Background { get => Brushes.Transparent; set { } }
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
                if (x % 2 != 0)
                    _cellWidth = 3;
                return _cellWidth;
            }
            set
            {
                _cellWidth = value;
                OnPropertyChanged();
            }
        }

        private int _cellHeight = 30;
        public virtual int CellHeight
        {
            get
            {
                if (y % 2 != 0)
                    _cellHeight = 3;
                return _cellHeight;
            }
            set
            {
                _cellHeight = value;
                OnPropertyChanged();
            }
        }
        protected string GetImageUri(string shortname)
        {
            string src = new AssemblyName(GetType().Assembly.FullName).Name;
            return "/" + src + ";component/src/" + shortname;
        }
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
    public class Space : Cell
    {
        public override bool IsTransient => true;
    }

    public class Player : Cell
    {
        string _file = "player.png";
        override public string File
        {
            get => GetImageUri(_file);
            set
            {
                _file = value;
                OnPropertyChanged();
            }
        }
        private int _steps;
        public int Steps
        {
            get => _steps;
            set
            {
                _steps = value;
                OnPropertyChanged();
            }
        }

        int _points = 0;
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
    public class Wall : Cell
    {
        string _file = "wall.png";
        public override string File
        {
            get => GetImageUri(_file);
            set
            {
                _file = value;
                OnPropertyChanged();
            }
        }
        Brush background = Brushes.Black;
        public override Brush Background
        {
            get => background;
            set
            {
                background = value;
                OnPropertyChanged();
            }
        }
    }
    public class Point : Cell
    {
        public override bool IsTransient => true;

        string _file = "seed.png";
        public override string File
        {
            get => GetImageUri(_file);
            set
            {
                _file = value;
                OnPropertyChanged();
            }
        }

        Brush background = Brushes.Transparent;
        public override Brush Background
        {
            get => background;
            set
            {
                background = value;
                OnPropertyChanged();
            }
        }
    }
    public class Exit : Cell
    {
        public override bool IsTransient => true;

        string _file = "exit.png";
        new public string File
        {
            get => GetImageUri(_file);
            set
            {
                _file = value;
                OnPropertyChanged();
            }
        }

        Brush background = Brushes.Transparent;
        public override Brush Background
        {
            get => background;
            set
            {
                background = value;
                OnPropertyChanged();
            }
        }
    }
    class ExitPlayer : Exit
    {
        string _file = "player_exit.png";
        new public string File
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

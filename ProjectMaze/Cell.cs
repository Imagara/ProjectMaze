﻿using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace ProjectMaze
{
    public abstract class Cell : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int x, y;
        private int _cellWidth = 30;
        public virtual int CellWidth
        {
            get
            {
                if (y % 2 != 0) // если нечетн
                    _cellWidth = 3;//3
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
                if (x % 2 != 0)
                    _cellHeight = 3;//3
                return _cellHeight;
            }
            set
            {
                _cellHeight = value;
                OnPropertyChanged();
            }
        }
        public virtual bool IsTransient { get; set; }
        public virtual bool IsVisited { get; set; }
        public virtual string TestText { get => $"[{x}][{y}]"; set { } }

        string _file = "empty.png";
        public virtual Brush Bkg { get => Brushes.Transparent; set { } }
        public virtual string File
        {
            get => GetImageUri(_file);
            set
            {
                _file = value;
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
    class Space : Cell
    {
        public override bool IsTransient => true;
    }

    class Player : Cell
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
        int score = 1;
        public int Score
        {
            get => score;
            set
            {
                score = value;
                OnPropertyChanged("Score");
            }
        }
        private int _step;
        public int Step
        {
            get => _step;
            set
            {
                _step = value;
                OnPropertyChanged();
            }
        }

    }
    class Wall : Cell
    {
        string _file = "brick.png";
        public override string File { get => GetImageUri(_file); set { _file = value; OnPropertyChanged("File"); } }

        Brush bkg = Brushes.Black;
        public override Brush Bkg { get => bkg; set { bkg = value; OnPropertyChanged(); } }

    }
    class Point : Cell
    {
        public override bool IsTransient => true;

        string _file = "key.png";
        public override string File
        {
            get => GetImageUri(_file);
            set
            {
                _file = value;
                OnPropertyChanged("File");
            }
        }

        Brush bkg = Brushes.Transparent;
        public override Brush Bkg
        {
            get => bkg;
            set
            {
                bkg = value;
                OnPropertyChanged();
            }
        }
    }
    class Exit : Cell
    {
        public override bool IsTransient => true;

        string _file = "star.png";
        new public string File
        {
            get => GetImageUri(_file);
            set
            {
                _file = value;
                OnPropertyChanged("File");
            }
        }

        Brush bkg = Brushes.Transparent;
        public override Brush Bkg
        {
            get => bkg;
            set
            {
                bkg = value;
                OnPropertyChanged();
            }
        }
    }
}

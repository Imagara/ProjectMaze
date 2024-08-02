using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace ProjectMaze
{
    public abstract class Cell : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Row, Col;
        public virtual bool IsTransient { get; set; }// означает - можно ли проходить игроку сквозь эту клетку

        string file = "empty.png";
        public virtual Brush Bkg { get => Brushes.Transparent; set { } }
        public virtual string File { get => GetImageUri(file); set { file = value; OnPropertyChanged(); } }
        protected string GetImageUri(string shortname)
        {
            string ass = new AssemblyName(GetType().Assembly.FullName).Name;
            return "/" + ass + ";component/src/" + shortname;
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
        string file = "player.png";
        override public string File { get => GetImageUri(file); set { file = value; OnPropertyChanged(); } }
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
        string file = "brick.png";
        public override string File { get => GetImageUri(file); set { file = value; OnPropertyChanged("File"); } }

        Brush bkg = Brushes.Black;
        public override Brush Bkg { get => bkg; set { bkg = value; OnPropertyChanged(); } }

    }
    class Klyuch : Cell
    {
        public override bool IsTransient => true;

        string file = "key.png";
        public override string File { get => GetImageUri(file); set { file = value; OnPropertyChanged("File"); } }

        Brush bkg = Brushes.Transparent;
        public override Brush Bkg { get => bkg; set { bkg = value; OnPropertyChanged(); } }
    }
    class Exit : Cell
    {
        public override bool IsTransient => true;

        string file = "star.png";
        new public string File { get => GetImageUri(file); set { file = value; OnPropertyChanged("File"); } }

        Brush bkg = Brushes.Transparent;
        public override Brush Bkg { get => bkg; set { bkg = value; OnPropertyChanged(); } }
    }
}

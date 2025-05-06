using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TetrisWPF
{
    public partial class MainWindow : Window
    {
        private TetrisGame game = new TetrisGame();
        private DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            game.GridChanged += DrawGrid;
            game.GameOver += OnGameOver;

            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += GameTick;

            this.KeyDown += OnKeyDown;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            game.StartGame();
            timer.Start();
            Focus(); // Capture key input
        }

        private void GameTick(object? sender, EventArgs e)
        {
            game.Move(0, 1); // Try to move piece down
            timer.Interval = TimeSpan.FromMilliseconds(Math.Max(100, 500 - (game.Level - 1) * 50));
        }

        private void OnGameOver()
        {
            timer.Stop();
            MessageBox.Show("Game Over!");
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
                game.Move(-1, 0);
            else if (e.Key == Key.Right)
                game.Move(1, 0);
            else if (e.Key == Key.Down)
                game.Move(0, 1);
            else if (e.Key == Key.Up)
                game.Rotate();
        }

        private void DrawGrid()
        {
            GameCanvas.Children.Clear();

            for (int x = 0; x < TetrisGame.Columns; x++)
            {
                for (int y = 0; y < TetrisGame.Rows; y++)
                {
                    string shape = game.Grid[x, y];
                    if (shape != null)
                    {
                        Rectangle rect = new Rectangle
                        {
                            Width = TetrisGame.BlockSize - 1,
                            Height = TetrisGame.BlockSize - 1,
                            Fill = TetrisGame.TetrominoColors[shape]
                        };

                        Canvas.SetLeft(rect, x * TetrisGame.BlockSize);
                        Canvas.SetTop(rect, y * TetrisGame.BlockSize);
                        GameCanvas.Children.Add(rect);
                    }
                }
            }

            ScoreText.Text = game.Score.ToString();
            LevelText.Text = game.Level.ToString();
        }
    }
}

using System.Windows;
using System.Windows.Media;

namespace TetrisWPF
{
    public class TetrisGame
    {
        public const int Rows = 20;
        public const int Columns = 10;
        public const int BlockSize = 20;

        public static readonly Dictionary<string, Point[][]> Tetrominoes = new()
        {
            // Each tetromino has 4 rotation states
            ["I"] = new[]
            {
                new[] { new Point(0,1), new Point(1,1), new Point(2,1), new Point(3,1) },
                new[] { new Point(2,0), new Point(2,1), new Point(2,2), new Point(2,3) },
                new[] { new Point(0,2), new Point(1,2), new Point(2,2), new Point(3,2) },
                new[] { new Point(1,0), new Point(1,1), new Point(1,2), new Point(1,3) },
            },
            ["O"] = new[]
            {
                new[] { new Point(1,0), new Point(2,0), new Point(1,1), new Point(2,1) },
                new[] { new Point(1,0), new Point(2,0), new Point(1,1), new Point(2,1) },
                new[] { new Point(1,0), new Point(2,0), new Point(1,1), new Point(2,1) },
                new[] { new Point(1,0), new Point(2,0), new Point(1,1), new Point(2,1) },
            },
            ["T"] = new[]
            {
                new[] { new Point(1,0), new Point(0,1), new Point(1,1), new Point(2,1) },
                new[] { new Point(1,0), new Point(1,1), new Point(2,1), new Point(1,2) },
                new[] { new Point(0,1), new Point(1,1), new Point(2,1), new Point(1,2) },
                new[] { new Point(1,0), new Point(0,1), new Point(1,1), new Point(1,2) },
            },
            ["S"] = new[]
            {
                new[] { new Point(1,0), new Point(2,0), new Point(0,1), new Point(1,1) },
                new[] { new Point(1,0), new Point(1,1), new Point(2,1), new Point(2,2) },
                new[] { new Point(1,1), new Point(2,1), new Point(0,2), new Point(1,2) },
                new[] { new Point(0,0), new Point(0,1), new Point(1,1), new Point(1,2) },
            },
            ["Z"] = new[]
            {
                new[] { new Point(0,0), new Point(1,0), new Point(1,1), new Point(2,1) },
                new[] { new Point(2,0), new Point(1,1), new Point(2,1), new Point(1,2) },
                new[] { new Point(0,1), new Point(1,1), new Point(1,2), new Point(2,2) },
                new[] { new Point(1,0), new Point(0,1), new Point(1,1), new Point(0,2) },
            },
            ["J"] = new[]
            {
                new[] { new Point(0,0), new Point(0,1), new Point(1,1), new Point(2,1) },
                new[] { new Point(1,0), new Point(2,0), new Point(1,1), new Point(1,2) },
                new[] { new Point(0,1), new Point(1,1), new Point(2,1), new Point(2,2) },
                new[] { new Point(1,0), new Point(1,1), new Point(0,2), new Point(1,2) },
            },
            ["L"] = new[]
            {
                new[] { new Point(2,0), new Point(0,1), new Point(1,1), new Point(2,1) },
                new[] { new Point(1,0), new Point(1,1), new Point(1,2), new Point(2,2) },
                new[] { new Point(0,1), new Point(1,1), new Point(2,1), new Point(0,2) },
                new[] { new Point(0,0), new Point(1,0), new Point(1,1), new Point(1,2) },
            }
        };

        public static readonly Dictionary<string, Brush> TetrominoColors = new()
        {
            ["I"] = Brushes.Cyan,
            ["O"] = Brushes.Yellow,
            ["T"] = Brushes.Purple,
            ["S"] = Brushes.Green,
            ["Z"] = Brushes.Red,
            ["J"] = Brushes.Blue,
            ["L"] = Brushes.Orange,
        };

        // Game state
        public string[,] Grid = new string[Columns, Rows];
        public int Score { get; private set; }
        public int Level { get; private set; } = 1;

        private Tetromino currentPiece;
        private Random rng = new Random();

        public event Action? GridChanged;
        public event Action? GameOver;

        public void StartGame()
        {
            Array.Clear(Grid, 0, Grid.Length);
            Score = 0;
            Level = 1;
            SpawnNewPiece();
        }

        private void SpawnNewPiece()
        {
            var keys = Tetrominoes.Keys.ToArray();
            string shape = keys[rng.Next(keys.Length)];
            currentPiece = new Tetromino(shape);
            if (!IsValidPosition(currentPiece))
            {
                GameOver?.Invoke();
            }
            else
            {
                DrawPiece(currentPiece, true);
                GridChanged?.Invoke();
            }
        }

        // More logic (Move, Rotate, Drop, Collision Detection, Clear Lines, etc.) will go here...

        // Get the absolute block positions of the current piece
        private IEnumerable<Point> GetAbsolutePositions(Tetromino piece)
        {
            foreach (var block in piece.GetBlocks())
            {
                yield return new Point(piece.X + (int)block.X, piece.Y + (int)block.Y);
            }
        }

        // Check if piece is in a valid position (not colliding)
        private bool IsValidPosition(Tetromino piece)
        {
            foreach (var pos in GetAbsolutePositions(piece))
            {
                int x = (int)pos.X;
                int y = (int)pos.Y;
                if (x < 0 || x >= Columns || y < 0 || y >= Rows)
                    return false;
                if (Grid[x, y] != null)
                    return false;
            }
            return true;
        }

        // Draw or erase piece on grid
        private void DrawPiece(Tetromino piece, bool draw)
        {
            foreach (var pos in GetAbsolutePositions(piece))
            {
                int x = (int)pos.X;
                int y = (int)pos.Y;
                if (x >= 0 && x < Columns && y >= 0 && y < Rows)
                    Grid[x, y] = draw ? piece.Shape : null;
            }
        }

        // Attempt to move the piece
        public void Move(int dx, int dy)
        {
            DrawPiece(currentPiece, false);
            currentPiece.X += dx;
            currentPiece.Y += dy;

            if (!IsValidPosition(currentPiece))
            {
                currentPiece.X -= dx;
                currentPiece.Y -= dy;
                DrawPiece(currentPiece, true);
                if (dy > 0) // If downward move failed, lock piece and spawn new one
                {
                    LockPiece();
                    ClearLines();
                    SpawnNewPiece();
                }
                return;
            }

            DrawPiece(currentPiece, true);
            GridChanged?.Invoke();
        }

        // Attempt to rotate the piece
        public void Rotate()
        {
            DrawPiece(currentPiece, false);
            int originalRotation = currentPiece.Rotation;
            currentPiece.Rotation = (currentPiece.Rotation + 1) % 4;

            if (!IsValidPosition(currentPiece))
            {
                currentPiece.Rotation = originalRotation;
            }

            DrawPiece(currentPiece, true);
            GridChanged?.Invoke();
        }

        // Lock current piece into grid
        private void LockPiece()
        {
            foreach (var pos in GetAbsolutePositions(currentPiece))
            {
                int x = (int)pos.X;
                int y = (int)pos.Y;
                if (y >= 0 && y < Rows)
                {
                    Grid[x, y] = currentPiece.Shape;
                }
            }
        }

        private void ClearLines()
        {
            int linesCleared = 0;
            for (int y = Rows - 1; y >= 0; y--)
            {
                bool fullLine = true;
                for (int x = 0; x < Columns; x++)
                {
                    if (Grid[x, y] == null)
                    {
                        fullLine = false;
                        break;
                    }
                }

                if (fullLine)
                {
                    linesCleared++;
                    // Move all rows above down
                    for (int row = y; row > 0; row--)
                    {
                        for (int x = 0; x < Columns; x++)
                        {
                            Grid[x, row] = Grid[x, row - 1];
                        }
                    }

                    for (int x = 0; x < Columns; x++)
                    {
                        Grid[x, 0] = null;
                    }

                    y++; // Check same row again after shifting
                }
            }

            if (linesCleared > 0)
            {
                Score += linesCleared * 100;
                Level = 1 + Score / 500;
                GridChanged?.Invoke();
            }
        }
    }

    public class Tetromino
    {
        public string Shape { get; }
        public int Rotation { get; set; } = 0;
        public int X { get; set; } = 3;
        public int Y { get; set; } = 0;

        public Tetromino(string shape)
        {
            Shape = shape;
        }

        public Point[] GetBlocks()
        {
            return TetrisGame.Tetrominoes[Shape][Rotation];
        }
    }
}

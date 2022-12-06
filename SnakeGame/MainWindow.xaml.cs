using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SnakeGame
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            gridImages = SetupGrid();
            gameState = new GameState(rows, columns);
        }

        private bool gameRunning = false;
        private readonly int rows = 15, columns = 15;
        private readonly Image[,] gridImages;
        private GameState gameState; //initialized in the constructor

        private readonly Dictionary<GridValue, ImageSource> gridValueToImage = new()
        {
            {GridValue.Empty, LoadImages.Empty }, //if the grid is empty display empty image
            {GridValue.Snake, LoadImages.Body },
            {GridValue.Food, LoadImages.Food }
        };

        private readonly Dictionary<Direction, int> dirToRotateHead = new()
        {
            {Direction.Up, 0}, //for up direction no rotation needed for head png
            {Direction.Right, 90},
            {Direction.Down, 180},
            {Direction.Left, 270}
        };

        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[rows, columns]; //create a 2d array
            GameGrid.Rows = rows; //set the number of rows and columns on the grid
            GameGrid.Columns = columns;

            for (int r = 0; r < rows; r++) 
            {
                for (int c = 0; c < columns; c++)
                {
                    Image image = new Image
                    {
                        Source = LoadImages.Empty, //want the initial image to be the empty image
                        RenderTransformOrigin = new Point(.5, .5) //make the images rotate around the center point
                    };

                    images[r, c] = image; //store this image inside of the array
                    GameGrid.Children.Add(image); //add it as a child of game grid
                }
            }
            return images;
        }

        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            ScoreText.Text = $"SCORE: {gameState.Score}";
        }
        
        private void DrawGrid()
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    GridValue gridValue = gameState.Grid[r, c];
                    gridImages[r, c].Source = gridValueToImage[gridValue];
                    gridImages[r, c].RenderTransform = Transform.Identity; //reset images so just the head image rotate
                }
            }
        }

        private async Task RunGame()
        {
            Draw();
            await ShowStartCountDown();
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            await ShowGameOver();
            gameState = new GameState(rows, columns); //create a fresh game
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver)
                return;

            switch (e.Key)
            {
                case Key.Left:
                    gameState.ChangeSnakeDirection(Direction.Left);
                    break;
                case Key.Right:
                    gameState.ChangeSnakeDirection(Direction.Right);
                    break;
                case Key.Up:
                    gameState.ChangeSnakeDirection(Direction.Up);
                    break;
                case Key.Down:
                    gameState.ChangeSnakeDirection(Direction.Down);
                    break;
            }
        }

        private async Task GameLoop() 
        {
            while (!gameState.GameOver) //make moves at regular intervals and will run until game over
            {
                await Task.Delay(400);
                gameState.MoveSnake();
                Draw();
            }
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Overlay.Visibility == Visibility.Visible)
                e.Handled = true; //will prevent calling Windows_KeyDown so the snake will not move at first key press

            if (!gameRunning)
            {
                gameRunning= true;
                await RunGame();
                gameRunning= false;
            }
        }

        private async Task ShowStartCountDown()
        {
            for (int i = 3; i >= 1; i--)
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(500);
            }
        }

        private async Task ShowGameOver()
        {
            await DrawDeadSnake();
            await Task.Delay(500);
            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = "You died! Press any key to start again";
        }

        private void DrawSnakeHead()
        {
            Position headPos = gameState.HeadPosition();
            Image img = gridImages[headPos.Row, headPos.Column];
            img.Source = LoadImages.Head;

            int rotation = dirToRotateHead[gameState.Dir];
            img.RenderTransform = new RotateTransform(rotation);
        }

        private async Task DrawDeadSnake()
        {
            List<Position> positions = new List<Position>(gameState.SnakePositions());

            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];
                ImageSource source = (i == 0) ? LoadImages.DeadHead : LoadImages.DeadBody;
                gridImages[pos.Row, pos.Column].Source = source;
                await Task.Delay(1);
            }
        }
    }
}
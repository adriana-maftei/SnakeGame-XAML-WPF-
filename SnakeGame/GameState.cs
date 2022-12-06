using System;
using System.Collections.Generic;


namespace SnakeGame
{
    public class GameState
    {
        public int Rows { get; }
        public int Columns { get; }
        public GridValue[,] Grid { get; } //2 dimensional rectangular array of grid values
        public Direction Dir { get; private set; }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }

        private readonly LinkedList<Position> snakePosition = new LinkedList<Position>();
        private readonly LinkedList<Direction> directionChanges = new LinkedList<Direction>(); //buffer for key press delay
        //the first element is the head, last the tail of the snake
        private readonly Random random = new Random(); //spam of food object

        public GameState(int rows, int cols)
        {
            this.Rows = rows;
            this.Columns = cols;
            Grid = new GridValue[rows, cols];
            Dir = Direction.Right; //when game starts the snake goes right
            AddSnake();
            AddFood();
        }

        private void AddSnake() //make snake appear on the middle row & left position when game starts
        {
            int middleRow = Rows / 2;

            for (int col = 1; col <= 3; col++)
            {
                Grid[middleRow, col] = GridValue.Snake;
                snakePosition.AddFirst(new Position(middleRow, col)); //add snake postion to the list
            }
        }

        private IEnumerable<Position> EmptyPositions()
        {
            for (int r = 0; r < Rows; r++) //we loop through the rows and columns
            {
                for (int c = 0; c < Columns; c++)
                {
                    if (Grid[r, c] == GridValue.Empty) //if the grid is empty
                        yield return new Position(r, c); //create a new position on the empty r and c
                }
            }
        }

        private void AddFood()
        {
            List<Position> emptyPos = new List<Position>(EmptyPositions());

            if (emptyPos.Count == 00) //if there are no more empties or snake is dead return
                return;

            Position pos = emptyPos[random.Next(emptyPos.Count)]; //if not, food spam in a random empty position in the grid
            Grid[pos.Row, pos.Column] = GridValue.Food;
        }

        public Position HeadPosition()
        {
            return snakePosition.First.Value;
        }

        public Position TailPosition()
        {
            return snakePosition.Last.Value;
        }

        public IEnumerable<Position> SnakePositions()
        {
            return snakePosition;
        }

        private void AddHead(Position pos)
        {
            snakePosition.AddFirst(pos);
            Grid[pos.Row, pos.Column] = GridValue.Snake;
        }

        private void RemoveTail()
        {
            Position tail = snakePosition.Last.Value; //find the snake tail
            Grid[tail.Row, tail.Column] = GridValue.Empty; //clear the corresponding grid
            snakePosition.RemoveLast(); //remove it from the linked list
        }

        private Direction GetLastDirection()
        {
            if (directionChanges.Count == 0)
                return Dir;

            return directionChanges.Last.Value;
        }

        private bool CanChangeDirection(Direction newDir)
        {
            if (directionChanges.Count == 2) //if the buffer stores 2 key presses already can't change directions
                return false;

            Direction lastDir = GetLastDirection();
            return newDir != lastDir && newDir != lastDir.Opposite();
        }

        public void ChangeSnakeDirection(Direction dir)
        {
            /* i created a short delay between key presses to avoid the bug when user press up then down immediately 
             * there is the risk for snake to hit himself, so the game is over
            */
            if (CanChangeDirection(dir)) //if can change direction, add it to the buffer
                directionChanges.AddLast(dir); 
        }

        private bool OutsideGrid(Position pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Column < 0 || pos.Column >= Columns;
        }

        private GridValue DoesHitSomething(Position newHeadPosition)
        {
            if (OutsideGrid(newHeadPosition)) //if the new head position will be outside the grid
                return GridValue.Outside;

            if (newHeadPosition == TailPosition()) //if the new head position will hit the tail
                return GridValue.Empty; //remove the tail so the grid will be empty

            return Grid[newHeadPosition.Row, newHeadPosition.Column];
        }

        public void MoveSnake()
        {
            //we need just to move the head and remove the tail gradually, but when he eats the tail stays and get longer

            if (directionChanges.Count > 0) //check if there is a direction change in the buffer
            {
                Dir = directionChanges.Last.Value; //if so, change snake direction accordingly
                directionChanges.RemoveFirst(); //and remove it from the buffer
            }

            Position newHeadPos = HeadPosition().Translate(Dir); //determine the direction
            GridValue hit = DoesHitSomething(newHeadPos); //check if hits something

            if (hit == GridValue.Outside || hit == GridValue.Snake) //if snake hits the margins of the grid or himself
                GameOver = true;
            else if (hit == GridValue.Empty) //if snake hits nothing
            {
                RemoveTail();
                AddHead(newHeadPos);
            }
            else if (hit == GridValue.Food) // if snake hits food
            {
                AddHead(newHeadPos);
                Score++;
                AddFood(); //spam new food somewhere else
            }
        }
    }
}
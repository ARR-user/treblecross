using System;
using System.Collections.Generic;
using System.IO;

namespace TrebleCross
{
    public class GameBoard
    {
        private char[] board;
        private MoveHistory moveHistory;

        public GameBoard(int size)
        {
            board = new char[size];
            InitializeBoard();
            moveHistory = new MoveHistory();
        }

        public char[] Board { get { return board; } }
        public MoveHistory MoveHistory { get { return moveHistory; } }

        public bool IsValidMove(int position)
        {
            return position >= 0 && position < board.Length && board[position] == ' ';
        }

        public bool WinCondition(char symbol)
        {
            for (int i = 0; i < board.Length; i++)
            {
                if (board[i] == symbol && board[(i + 1) % board.Length] == symbol && board[(i + 2) % board.Length] == symbol)
                {
                    return true;
                }
            }

            return false;
        }

        public void PlaceMove(int position, char symbol)
        {
            board[position] = symbol;
        }

        public bool IsBoardFull()
        {
            foreach (char cell in board)
            {
                if (cell == ' ')
                {
                    return false;
                }
            }
            return true;
        }

        public void DisplayBoard()
        {
            Console.WriteLine("Board:");
            for (int i = 0; i < board.Length; i++)
            {
                Console.Write(board[i] + " ");
            }
            Console.WriteLine();
        }

        public void InitializeBoard()
        {
            for (int i = 0; i < board.Length; i++)
            {
                board[i] = ' ';
            }
        }
    }

    public abstract class Player
    {
        protected GameBoard gameBoard;
        public char Symbol { get; set; }
        public int PlayerNumber { get; set; }

        protected Player(char symbol, int playerNumber, GameBoard board)
        {
            Symbol = symbol;
            PlayerNumber = playerNumber;
            gameBoard = board;
        }

        public abstract int GetMove();
    }

    public class HumanPlayer : Player
    {
        public HumanPlayer(char symbol, int playerNumber, GameBoard board)
            : base(symbol, playerNumber, board)
        {
        }

        public override int GetMove()
        {
            while (true)
            {
                Console.WriteLine($"Player {PlayerNumber}, enter the cell number (0 - {gameBoard.Board.Length - 1}) or '#' for menu:");
                string input = Console.ReadLine();

                if (input == "#")
                {
                    DisplayMenu();
                    continue;
                }

                if (int.TryParse(input, out int cellNumber))
                {
                    if (gameBoard.IsValidMove(cellNumber))
                    {
                        gameBoard.MoveHistory.AddMove(cellNumber);
                        return cellNumber;
                    }
                }

                Console.WriteLine("Invalid move! Try again.");
            }
        }

        private void DisplayMenu()
        {
            Console.WriteLine("Menu:");
            Console.WriteLine("1. Save game");
            Console.WriteLine("2. Load game");
            Console.WriteLine("3. Get hints");
            Console.WriteLine("4. Show Move History");
            Console.WriteLine("5. Undo Move");
            Console.WriteLine("6. Redo Move");
            string choice = Console.ReadLine();
            HandleMenuChoice(choice);

            // Print the board after accessing the menu
            gameBoard.DisplayBoard();
        }

        private void HandleMenuChoice(string choice)
        {
            SaveManager saveManager = new SaveManager();
            switch (choice)
            {
                case "1":
                    Console.WriteLine("Enter the file name to save:");
                    string fileName = Console.ReadLine();
                    saveManager.Save(gameBoard, gameBoard.MoveHistory, fileName);
                    break;
                case "2":
                    Console.WriteLine("Enter the file name to load:");
                    fileName = Console.ReadLine();
                    saveManager.Load(gameBoard, gameBoard.MoveHistory, fileName);
                    break;
                case "3":
                    DisplayHints();
                    break;
                case "4":
                    gameBoard.MoveHistory.DisplayHistory();
                    break;
                case "5":
                    gameBoard.MoveHistory.UndoMove(gameBoard);
                    break;
                case "6":
                    gameBoard.MoveHistory.RedoMove(gameBoard, Symbol);
                    break;
                default:
                    Console.WriteLine("Invalid choice!");
                    break;
            }
        }

        private void DisplayHints()
        {
            Console.WriteLine("Number theory.");
        }
    }

    public class ComputerPlayer : Player
    {
        private Random random;

        public ComputerPlayer(char symbol, int playerNumber, GameBoard board) : base(symbol, playerNumber, board)
        {
            random = new Random();
        }

        public override int GetMove()
        {
            List<int> availableMoves = new List<int>();

            for (int i = 0; i < gameBoard.Board.Length; i++)
            {
                if (gameBoard.Board[i] == ' ')
                {
                    availableMoves.Add(i);
                }
            }

            if (availableMoves.Count > 0)
            {
                int randomIndex = random.Next(availableMoves.Count);
                return availableMoves[randomIndex];
            }

            return -1;
        }
    }

    public class MoveHistory
    {
        private List<int> moves;
        private Stack<int> undoStack;
        private Stack<int> redoStack;

        public MoveHistory()
        {
            moves = new List<int>();
            undoStack = new Stack<int>();
            redoStack = new Stack<int>();
        }

        public List<int> Moves { get { return moves; } }

        public void AddMove(int cellNumber)
        {
            moves.Add(cellNumber);
            undoStack.Push(cellNumber);
            redoStack.Clear(); // Clear redo stack after new move
        }

        public void UndoMove(GameBoard board)
        {
            if (undoStack.Count > 0)
            {
                int lastMove = undoStack.Pop();
                moves.Remove(lastMove);
                redoStack.Push(lastMove);
                board.Board[lastMove] = ' '; // Reset the cell after undo
                DisplayHistory(); // Display move history after undo
            }
        }

        public void RedoMove(GameBoard board, char symbol)
        {
            if (redoStack.Count > 0)
            {
                int redoMove = redoStack.Pop();
                moves.Add(redoMove);
                undoStack.Push(redoMove);
                board.Board[redoMove] = symbol; // Set the symbol back after redo
                DisplayHistory(); // Display move history after redo
            }
        }

        public void ClearHistory()
        {
            moves.Clear();
            undoStack.Clear();
            redoStack.Clear();
        }

        public void DisplayHistory()
        {
            Console.WriteLine("Move History:");
            foreach (int move in moves)
            {
                Console.Write(move + " ");
            }
            Console.WriteLine();
        }
    }

    public class SaveManager
    {
        public void Save(GameBoard board, MoveHistory moveHistory, string fileName)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(fileName))
                {
                    foreach (char cell in board.Board)
                    {
                        writer.Write(cell);
                    }
                    writer.WriteLine();

                    foreach (int move in moveHistory.Moves)
                    {
                        writer.Write(move);
                        writer.Write(' ');
                    }
                }

                Console.WriteLine("Game saved successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving game: {ex.Message}");
            }
        }

        public void Load(GameBoard board, MoveHistory moveHistory, string fileName)
        {
            try
            {
                using (StreamReader reader = new StreamReader(fileName))
                {
                    string boardState = reader.ReadLine();
                    if (boardState != null)
                    {
                        for (int i = 0; i < boardState.Length; i++)
                        {
                            board.Board[i] = boardState[i];
                        }

                        moveHistory.ClearHistory();

                        string[] moves = reader.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        foreach (string move in moves)
                        {
                            if (int.TryParse(move, out int cellNumber))
                            {
                                moveHistory.AddMove(cellNumber);
                            }
                        }

                        Console.WriteLine("Game loaded successfully!");
                    }
                    else
                    {
                        Console.WriteLine("Invalid save file format.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading game: {ex.Message}");
            }
        }
    }

    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Treblecross (Tic-Tac-Toe)!");
            Console.WriteLine("Enter the number of cells in a row:");
            int size = int.Parse(Console.ReadLine());
            GameBoard board = new GameBoard(size);

            Console.WriteLine("Choose mode:");
            Console.WriteLine("1. Player vs Player");
            Console.WriteLine("2. Player vs Computer");

            string mode = Console.ReadLine();
            bool vsComputer = (mode == "2");

            PlayGame(board, vsComputer);
        }

        public static void PlayGame(GameBoard board, bool vsComputer)
        {
            Player player1 = new HumanPlayer('X', 1, board);
            Player player2 = vsComputer ? new ComputerPlayer('O', 2, board) : new HumanPlayer('O', 2, board);

            int currentPlayerNumber = 1;

            while (true)
            {
                Console.Clear();
                board.DisplayBoard();
                board.MoveHistory.DisplayHistory();
                int move = (currentPlayerNumber == 1) ? player1.GetMove() : player2.GetMove();

                if (move == -1)
                {
                    Console.WriteLine("Invalid move! Try again.");
                    continue;
                }

                char currentPlayerSymbol = (currentPlayerNumber == 1) ? player1.Symbol : player2.Symbol;
                board.PlaceMove(move, currentPlayerSymbol);

                if (board.WinCondition(currentPlayerSymbol))
                {
                    Console.Clear();
                    board.DisplayBoard();
                    Console.WriteLine($"Player {currentPlayerNumber} wins!");
                    board.MoveHistory.DisplayHistory();
                    break;
                }
                else if (board.IsBoardFull())
                {
                    Console.Clear();
                    board.DisplayBoard();
                    Console.WriteLine("It's a tie!");
                    board.MoveHistory.DisplayHistory();
                    break;
                }

                currentPlayerNumber = (currentPlayerNumber == 1) ? 2 : 1;
            }
        }
    }
}
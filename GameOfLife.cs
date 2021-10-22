using System;
using System.Threading;
using System.Diagnostics;

namespace ConwaysGameOfLife
{
    class Program
    {
        // create the initial board;
        static int N = 10;
        static bool[,] board = new bool[N, N];
        static bool[,] temp = new bool[N, N];
        static void Main(string[] args)
        {
            // set initial values - don't fill border values
            //initLine(2, 1); // oscillates
            //initBox(6, 3); // static
            initRand();


            // get number of threads
            int nThreads = 8;

            // create array containing threads
            Thread[] threadArr = new Thread[nThreads];

            // create array containing obj
            //GameOfLife[] workerArr = new GameOfLife[nThreads];

            // instantiate stopwatch
            Stopwatch stopWatch = new Stopwatch();

            // start stopwatch
            stopWatch.Start();

            print_board();
            Console.WriteLine("");

            int final_time = 4;
            for (int t = 0; t < final_time; t++)
            {
                // allocate threads and initialize game
                for (int i = 0; i < nThreads; i++)
                {
                    GameOfLife game = new GameOfLife(N, board, temp, i, nThreads);
                    //workerArr[i] = game;
                    Thread thread = new Thread(new ThreadStart(game.life_generation));
                    threadArr[i] = thread;
                }

                // start threads
                for (int i = 0; i < nThreads; i++)
                {
                    Thread thread = threadArr[i];
                    thread.Start();
                }


                // join threads
                for (int i = 0; i < nThreads; i++)
                {
                    Thread thread = threadArr[i];
                    thread.Join();
                }

                print_board();
                Console.WriteLine("");
            }

            // end stopwatch
            stopWatch.Stop();

            // run time
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);

            //// iterate over a number of steps on the whole board - serial
            //int final_time = 4;
            //for (int t = 0; t < final_time; t++)
            //{
            //    life_generation();
            //    Console.WriteLine($"Time: {t}");
            //    print_board();
            //}
        }
        static public void initLine(int r, int c)
        {
            for (int j = c; j < c + 3; j++)
            {
                board[r, j] = true;
            }
        }
        public static void initBox(int r, int c)
        {
            board[r, c] = true;
            board[r, c+1] = true;
            board[r+1, c] = true;
            board[r+1, c+1] = true;
        }
        public static void initRand()
        {
            Random rand = new Random();
            for (int i = 1; i < N-1; i++)
            {
                for (int j = 1; j < N-1; j++)
                {
                    board[i, j] = rand.Next(2) == 1;
                }
            }
        }
        static public void print_board()
        {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (board[i, j])
                    {
                        Console.Write("|");
                    }
                    else
                    {
                        Console.Write("O");
                    }
                    //Console.Write($"{board[i, j]}, ");
                }
                Console.WriteLine();
            }
        }
    }
    class GameOfLife
    {
        // create static variables;
        static int N;
        static bool[,] board;
        static bool[,] temp;
        static int nThreads;

        // create the properties
        int rank;

        // ctor
        public GameOfLife(int n, bool[,] b, bool[,] t, int r, int nTh)
        {
            N = n;
            board = b;
            temp = t;
            rank = r;
            nThreads = nTh;
        }
        
        
        public void life_generation()
        {
            // chunk for thread
            //Console.WriteLine($"My rank: {rank}");
            int chunk = (N-2)/nThreads; // account for border
            int my_first = chunk * rank + 1;
            int my_last = chunk * (rank + 1) + 1;
            if (rank == nThreads - 1)
            {
                my_last = N-1;
            }
            //Console.WriteLine($"Rank: {rank} | Start: {my_first}, End: {my_last}");

            // copy board to tmp
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    temp[i, j] = board[i, j];
                }
            }

            // change board - only the chunk that this thread is responsible for
            for (int i = my_first; i < my_last; i++)
            {
                for (int j = 1; j < N-1; j++)
                {
                    // create a 3x3 matrix around current cell
                    bool[,] inputCells = new bool[3, 3];

                    //inputCells[0, 0] = get_safe_cell(tmp, i - 1, j - 1);
                    //inputCells[0, 1] = get_safe_cell(tmp, i - 1, j);
                    //inputCells[0, 2] = get_safe_cell(tmp, i - 1, j + 1);

                    //inputCells[1, 0] = get_safe_cell(tmp, i, j - 1);
                    //inputCells[1, 1] = get_safe_cell(tmp, i, j);
                    //inputCells[1, 2] = get_safe_cell(tmp, i, j + 1);

                    //inputCells[2, 0] = get_safe_cell(tmp, i + 1, j - 1);
                    //inputCells[2, 1] = get_safe_cell(tmp, i + 1, j);
                    //inputCells[2, 2] = get_safe_cell(tmp, i + 1, j + 1);

                    inputCells[0, 0] = temp[i - 1, j - 1];
                    inputCells[0, 1] = temp[i - 1, j];
                    inputCells[0, 2] = temp[i - 1, j + 1];

                    inputCells[1, 0] = temp[i, j - 1];
                    inputCells[1, 1] = temp[i, j];
                    inputCells[1, 2] = temp[i, j + 1];

                    inputCells[2, 0] = temp[i + 1, j - 1];
                    inputCells[2, 1] = temp[i + 1, j];
                    inputCells[2, 2] = temp[i + 1, j + 1];

                    board[i, j] = life_evaluation(inputCells);
                }
            }
        }
        static public bool get_safe_cell(bool[,] board, int x, int y)
        {
            try
            {
                return board[x, y];
            }
            catch
            {
                return false;
            }
        }
        static public bool life_evaluation(bool[,] cells)
        {
            // cells is a 3x3 array // unroll the for loops here
            int count = 0;
            for (int i = 0; i <= 2; i++)
            {
                for (int j = 0; j <= 2; j++)
                {
                    if ((i != 1) || (j != 1)) // count the cells around the middle
                    {
                        if (cells[i, j])
                        {
                            count ++ ;
                        }
                    }
                }
            }
            return life_count_evaluation(cells[1, 1], count);
        }
        static public bool life_count_evaluation(bool cell, int count)
        {// 0 = death, 1 = life
            //cell is the value of the current cell (1 or 0)
            // count is the number of neighbors to the current cell
            if (count < 2)
            {
                return false; // lonliness
            }
            else if (count > 3)
            {
                return false; // overcrowding
            }
            else if (cell && (count == 2 || count == 3))
            {
                return true; // live on
            }
            else if (!cell && count == 3)
            {
                return true; // spontaneous generation
            }
            else
            {
                return false; // no change in dead cells
            }
        }

    }
}

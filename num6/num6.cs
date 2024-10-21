using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;


class Program
{
    static async Task Main(string[] args)
    {
        int size = 1000; 
        var matrixA = GenerateRandomMatrix(size);
        var matrixB = GenerateRandomMatrix(size);

        string filePathA = "matrixA.txt";
        string filePathB = "matrixB.txt";
        string filePathResult = "resultMatrix.txt";

        await WriteMatrixToFile(filePathA, matrixA);
        await WriteMatrixToFile(filePathB, matrixB);

        var stopwatch = Stopwatch.StartNew();
        var resultMatrix = await MultiplyMatricesAsync(matrixA, matrixB);
        stopwatch.Stop();

        await WriteMatrixToFile(filePathResult, resultMatrix);

        Console.WriteLine($"Multiplication completed in {stopwatch.ElapsedMilliseconds} ms");
    }

    static int[,] GenerateRandomMatrix(int size)
    {
        Random rand = new Random();
        int[,] matrix = new int[size, size];
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                matrix[i, j] = rand.Next(1, 10);
        return matrix;
    }

    static async Task WriteMatrixToFile(string filePath, int[,] matrix)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    await writer.WriteAsync(matrix[i, j].ToString() + " ");
                }
                await writer.WriteLineAsync();
            }
        }
    }

    static async Task<int[,]> MultiplyMatricesAsync(int[,] matrixA, int[,] matrixB)
    {
        int size = matrixA.GetLength(0);
        int[,] resultMatrix = new int[size, size];

        Task[] tasks = new Task[size];

        for (int i = 0; i < size; i++)
        {
            int rowIndex = i; 
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < size; j++)
                {
                    resultMatrix[rowIndex, j] = 0;
                    for (int k = 0; k < size; k++)
                    {
                        resultMatrix[rowIndex, j] += matrixA[rowIndex, k] * matrixB[k, j];
                    }
                }
            });
        }

        await Task.WhenAll(tasks);
        return resultMatrix;
    }
}

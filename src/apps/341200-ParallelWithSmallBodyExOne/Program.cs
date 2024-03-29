﻿using System;
using System.Diagnostics;
using System.Dynamic;

class Program
{
    delegate void ProcessValue(int value);
    static double[] resultData = new double[100];
    static void Main(string[] args)
    {
        // perform the parallel loop
        Parallel.For(0, resultData.Length, (int index) => {
            // compuute the result for the current index
            resultData[index] = Math.Pow(index, 2);
        });

        //for (int i = 0; i < resultData.Length; i++)
        //    Console.WriteLine($"The index is {i} and value is {resultData[i]}");
        
        // perform the loop again, but make the delegate explicit
        Parallel.For(0, resultData.Length, delegate (int index) {
            resultData[index] = Math.Pow((double)index, 2);
        });

        // perform the loop once more, but this time using
        // a declared delegate and action
        ProcessValue pdel = new ProcessValue(computeResultValue);
        Action<int> paction = new Action<int>(pdel);
        Parallel.For(0, resultData.Length, paction);


        // wait for input before exiting
        Console.WriteLine("Press enter to finish");
        Console.ReadLine();

    }

    static void computeResultValue(int indexValue)
    {
        resultData[indexValue] = Math.Pow(indexValue, 2);
    }
}

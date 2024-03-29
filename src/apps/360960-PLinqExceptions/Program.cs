﻿
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Dynamic;

class Program
{
    static void Main(string[] args)
    {
        // create some source data
        int[] sourceData = new int[100];
        for (int i = 0; i < sourceData.Length; i++)
        {
            sourceData[i] = i;
        }

        // define the query and force parallelism
        var results =
            sourceData.AsParallel()
            .Select(item => {
                if (item == 45)
                {
                    throw new Exception();
                }
                return Math.Pow(item, 2);
            });

        // enumerate the results
        try
        {
            foreach (double d in results)
            {
                Console.WriteLine("Result {0}", d);
            }
        }
        catch (AggregateException aggException)
        {
            aggException.Handle(exception => {
                Console.WriteLine("Handled exception of type: {0}",
                    exception.GetType());
                return true;
            });
        }

        // wait for input before exiting
        Console.WriteLine("Press enter to finish");
        Console.ReadLine();
    }
}

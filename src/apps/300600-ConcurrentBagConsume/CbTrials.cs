﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentBagConsume
{
    public class CbTrials
    {
        private ConcurrentBag<int> concurrentBag = new ConcurrentBag<int>();

        private List<Task> removeNumberToConcurrentBagTaskList = new List<Task>();

        private List<Task> addNumberToConcurrentBagTaskList = new List<Task>();

        private int itemsInBagCount = 0;

        public void AddTasksToAddNumbers()
        {
            Console.WriteLine("Start of AddTasks method");

            for (int i = 0; i < 500; i++)
            {
                var numberToAdd = i;
                
                addNumberToConcurrentBagTaskList.Add(Task.Run(() => {

                    concurrentBag.Add(numberToAdd);

                    Console.WriteLine($"A new task is added to list, which will add {numberToAdd} to concurrent bag ");

                }));
            }

            Console.WriteLine("End of AddTasks method");
        }

        public void EnsureAllTasksAreCompleted()
        {
            Console.WriteLine("Start of EnsureAllTasksAreCompleted");

            Task.WaitAll(addNumberToConcurrentBagTaskList.ToArray());

            Console.WriteLine("All tasks done ");
        }

        public void AddTasksToRemoveNumbers()
        {
            
            while (!concurrentBag.IsEmpty)
            {
                removeNumberToConcurrentBagTaskList.Add(Task.Run(() =>
                {
                    int item;
                    if (concurrentBag.TryTake(out item))
                    {
                        Console.WriteLine(item);
                        Interlocked.Increment(ref itemsInBagCount);
                    }
                }));
            }
        }

        public void CheckForItemsInBag()
        {
            Console.WriteLine("Start of Remove RunAllTasks");

            Task.WaitAll(removeNumberToConcurrentBagTaskList.ToArray());

            Console.WriteLine($"There were {itemsInBagCount} items in the bag");

            int unexpectedItem;
            if (concurrentBag.TryPeek(out unexpectedItem))
                Console.WriteLine("Found an item in the bag when it should be empty");

            Console.WriteLine("Remove All tasks done ");
        }
    }
}

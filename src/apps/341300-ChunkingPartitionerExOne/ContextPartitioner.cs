﻿using System.Collections;
using System.Collections.Concurrent;

namespace ChunkingPartitionerExOne
{

    class ContextPartitioner : Partitioner<WorkItem>
    {
        // the set of data items to partition
        protected WorkItem[] dataItems;
        // the target sum of values per chunk
        protected int targetSum;
        // the first unchunked item
        private long sharedStartIndex = 0;
        // lock object to avoid index data races
        private object lockObj = new object();
        // the object used to create enumerators
        private EnumerableSource enumSource;

        public ContextPartitioner(WorkItem[] data, int target)
        {
            // set the instance variables from the parameters
            dataItems = data;
            targetSum = target;
            // create the enumerable source
            enumSource = new EnumerableSource(this);
        }

        public override bool SupportsDynamicPartitions
        {
            get
            {
                // dynamic partitions are required for
                // parallel foreach loops
                return true;
            }
        }

        public override IList<IEnumerator<WorkItem>> GetPartitions(int partitionCount)
        {
            // create the list which will be the result
            IList<IEnumerator<WorkItem>> partitionsList = new List<IEnumerator<WorkItem>>();
            // get the IEnumerable that will generate dynamic partitions
            IEnumerable<WorkItem> enumObj = GetDynamicPartitions();
            // create the required number of partitions
            for (int i = 0; i < partitionCount; i++)
            {
                partitionsList.Add(enumObj.GetEnumerator());
            }
            // return the result
            return partitionsList;
        }

        public override IEnumerable<WorkItem> GetDynamicPartitions()
        {
            return enumSource;
        }

        private Tuple<long, long> GetNextChunk()
        {
            // create the result tuple
            Tuple<long, long> result;
            // get an exclusive lock as we perform this operation
            lock (lockObj)
            {
                // check that there is still data available
                if (sharedStartIndex < dataItems.Length)
                {
                    int sum = 0;
                    long endIndex = sharedStartIndex;
                    while (endIndex < dataItems.Length && sum < targetSum)
                    {
                        sum += dataItems[endIndex].WorkDuration;
                        endIndex++;
                    }
                    result = new Tuple<long, long>(sharedStartIndex, endIndex);
                    sharedStartIndex = endIndex;
                }
                else
                {
                    // there is no data available
                    result = new Tuple<long, long>(-1, -1);
                }
            }
            // end of locked region
            // return the result
            return result;
        }

        class EnumerableSource : IEnumerable<WorkItem>
        {
            ContextPartitioner parentPartitioner;

            public EnumerableSource(ContextPartitioner parent)
            {
                parentPartitioner = parent;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<WorkItem>)this).GetEnumerator();
            }

            IEnumerator<WorkItem> IEnumerable<WorkItem>.GetEnumerator()
            {
                return new ChunkEnumerator(parentPartitioner).GetEnumerator();
            }
        }

        class ChunkEnumerator
        {
            private ContextPartitioner parentPartitioner;

            public ChunkEnumerator(ContextPartitioner parent)
            {
                parentPartitioner = parent;
            }

            public IEnumerator<WorkItem> GetEnumerator()
            {
                while (true)
                {
                    // get the indices of the next chunk
                    Tuple<long, long> chunkIndices = parentPartitioner.GetNextChunk();
                    // check that we have data to deliver
                    if (chunkIndices.Item1 == -1 && chunkIndices.Item2 == -1)
                    {
                        // there is no more data
                        break;
                    }
                    else
                    {
                        // enter a loop to yield the data items
                        for (long i = chunkIndices.Item1; i < chunkIndices.Item2; i++)
                        {
                            yield return parentPartitioner.dataItems[i];
                        }
                    }
                }
            }
        }
    }
}

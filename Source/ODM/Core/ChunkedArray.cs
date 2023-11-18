using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODM
{
    public class ChunkedArray<T>
    {
        public int ChunkSize { get; } = 1024;
        public int StepChunks { get; } = 4;

        T[][] values = null;

        public ChunkedArray(int chunkSize = 1024, int step = 4)
        {
            ChunkSize = chunkSize;
            StepChunks = step;

            values = new T[StepChunks][];
            values[0] = new T[ChunkSize];
        }

        public long Size => ChunkSize * values.Length;


        public ref T this[uint index]
        {
            get
            {
                int chunck = (int)index / ChunkSize;
                int chunckIndex = (int)index % ChunkSize;
                if (chunck >= values.Length)
                {
                    Array.Resize(ref values, chunck + 1);
                }

                values[chunck] ??= new T[ChunkSize];

                return ref values[chunck][chunckIndex];
            }


        }

    }
}

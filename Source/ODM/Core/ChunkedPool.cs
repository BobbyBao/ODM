using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ODM
{
    public delegate void RefAction<T>(ref T v);

    public class ChunkedPool<T> : Disposable
    {
        public int ChunkSize { get; } = 1024;
        public int StepChunks { get; } = 4;

        T[][] values = null;

        uint currentID = 1;
        ConcurrentBag<uint> freeList  = new();

        public ChunkedPool(int chunkSize = 1024, int step = 4)
        {
            ChunkSize = chunkSize;
            StepChunks = step;

            values = new T[StepChunks][];
            values[0] = new T[ChunkSize];
        }

        protected override void Destroy(bool disposing)
        {
            //todo:
            base.Destroy(disposing);
        }

        public ref T this[uint index]
        {
            get
            {
                int chunck = (int)index / ChunkSize;
                return ref values[chunck][index];
            }
        }

        public uint Add(in T v)
        {
            if(!freeList.TryTake(out var id))
            {
                id = Interlocked.Increment(ref currentID);
                int chunck = (int)(id - 1) / ChunkSize;
                if(chunck >= values.Length)
                {
                    Array.Resize(ref values, values.Length + StepChunks);
                    values[chunck] = new T[ChunkSize];
                }

            }

            this[id] = v;

            return id;
        }

        public void Free(uint id)
        {
            if(id != 0)
            {
                ref var v = ref this[id];
                if(v is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                else
                {
                    this[id] = default;
                }

                freeList.Add(id);
            }

        }

    }

}

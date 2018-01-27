using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;

namespace YounGenTech.VoxelTech {
    public class WorldThreading : MonoBehaviour {

        [SerializeField]
        World _attachedWorld;

        Queue<Chunk> chunkGenerationQueue;

        #region Properties
        public World AttchedWorld {
            get { return _attachedWorld; }
            set { _attachedWorld = value; }
        }
        #endregion

        void Awake() {
            Initialize();
        }

        public void Initialize() {
            chunkGenerationQueue = new Queue<Chunk>();
        }

        public void QueueChunk(Chunk chunk) {
            if(!chunkGenerationQueue.Contains(chunk))
                chunkGenerationQueue.Enqueue(chunk);

            ThreadPool.QueueUserWorkItem(ThreadCallback, chunk);
        }

        void ThreadCallback(object state) {
            
        }
    }
}
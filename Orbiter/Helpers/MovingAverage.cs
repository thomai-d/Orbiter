using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;

namespace Orbiter.Helpers
{
    public class MovingAverage
    {
        private readonly Queue<Vector3> queue = new Queue<Vector3>();
        private int samples;

        public MovingAverage(int samples)
        {
            this.samples = samples;
        }

        public void AddSample(Vector3 f)
        {
            this.queue.Enqueue(f);
            if (this.queue.Count > this.samples)
                this.queue.Dequeue();
        }

        public Vector3 Average
        {
            get
            {
                return this.queue.Aggregate(Vector3.Zero, (x, y) => x + y) / this.queue.Count;
            }
        }
    }
}

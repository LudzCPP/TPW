using System;
using System.Collections.Concurrent;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Threading;

namespace Data
{
    internal class Logger
    {
        private class BallToSerialize
        {
            public int Id { get; }
            public float X { get; }
            public float Y { get; }
            public float SpeedHorizontal { get; }
            public float SpeedVertical { get; }
            public string EventTime { get; }
            public string LogTime { get; set; }

            public BallToSerialize(Vector2 position, Vector2 speed, int id, string eventTime)
            {
                X = position.X;
                Y = position.Y;
                EventTime = eventTime;
                SpeedHorizontal = speed.X;
                SpeedVertical = speed.Y;
                Id = id;
            }
        }

        private ConcurrentQueue<BallToSerialize> _queue;
        private Timer _timer;
        private readonly object _fileLock = new object();
        private StreamWriter _streamWriter;

        public Logger()
        {
            _queue = new ConcurrentQueue<BallToSerialize>();
            _streamWriter = new StreamWriter("logs.json");
            _timer = new Timer(OnTimedEvent, null, 0, 1000);
        }

        public void AddObjectToQueue(IBall obj, string eventTime)
        {
            _queue.Enqueue(new BallToSerialize(obj.Position, obj.Speed, obj.ID, eventTime));
        }

        private void OnTimedEvent(Object state)
        {
            lock (_fileLock)
            {
                while (_queue.TryDequeue(out BallToSerialize item))
                {
                    item.LogTime = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss.fff");
                    string jsonString = JsonSerializer.Serialize(item);
                    _streamWriter.WriteLine(jsonString);
                }
                _streamWriter.Flush();
            }
        }
    }
}

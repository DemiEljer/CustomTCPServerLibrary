using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CustomTCPServerLibrary.Base
{
    public class DataQueue
    {
        
        private Queue<byte[]> _DataQueue { get; } = new();

        private object _LockObject { get; } = new();
        /// <summary>
        /// Количество элементов в очереди
        /// </summary>
        public int Count => _DataQueue.Count;
        /// <summary>
        /// Извлечь элемент из очереди
        /// </summary>
        /// <returns></returns>
        public byte[]? Pop(bool removeElement = true)
        {
            lock (_LockObject)
            {
                if (Count > 0)
                {
                    if (removeElement)
                    {
                        return _DataQueue.Dequeue();
                    }
                    else
                    {
                        return _DataQueue.First();
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// Добавить элемент в очередь
        /// </summary>
        /// <param name="data"></param>
        public void Push(byte[] data)
        {
            lock (_LockObject)
            {
                _DataQueue.Enqueue(data);
            }
        }
        /// <summary>
        /// Очистить очередь
        /// </summary>
        public void Clear()
        {
            lock (_LockObject)
            {
                _DataQueue.Clear();
            }
        }
    }
}

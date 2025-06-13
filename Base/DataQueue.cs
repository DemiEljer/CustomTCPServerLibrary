using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CustomTCPServerLibrary.Base
{
    public class DataQueue<TDate>
    {
        /// <summary>
        /// Очередь данных
        /// </summary>
        private Queue<TDate> _DataQueue { get; } = new();
        /// <summary>
        /// Объект блокировки
        /// </summary>
        private object _LockObject { get; } = new();
        /// <summary>
        /// Количество элементов в очереди
        /// </summary>
        public int Count => _DataQueue.Count;
        /// <summary>
        /// Флаг, что очередь заблокирована
        /// </summary>
        public bool IsLocked { get; private set; } = true;
        /// <summary>
        /// Разблокировать очередь
        /// </summary>
        public void Unlock()
        {
            IsLocked = false;
        }
        /// <summary>
        /// Извлечь элемент из очереди
        /// </summary>
        /// <returns></returns>
        public TDate? Pop(bool removeElement = true)
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
                        return _DataQueue.Peek();
                    }
                }
                else
                {
                    return default(TDate?);
                }
            }
        }
        /// <summary>
        /// Извлечь все элементы из очереди
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TDate> PopAll(bool removeElement = true)
        {
            lock (_LockObject)
            {
                if (Count > 0)
                {
                    if (removeElement)
                    {
                        while (_DataQueue.Count > 0)
                        {
                            yield return _DataQueue.Dequeue();
                        }
                    }
                    else
                    {
                        foreach (var queueElement in _DataQueue)
                        {
                            yield return queueElement;
                        }
                    }
                }
                else
                {
                    yield break;
                }
            }
        }
        /// <summary>
        /// Извлечь все элементы из очереди
        /// </summary>
        /// <returns></returns>
        public void RemoveElementsFromHead(int count)
        {
            lock (_LockObject)
            {
                for (int i = 0; i < count && _DataQueue.Count > 0; i++)
                {
                    _DataQueue.Dequeue();
                }
            }
        }
        /// <summary>
        /// Добавить элемент в очередь
        /// </summary>
        /// <param name="data"></param>
        public bool Push(TDate data)
        {
            lock (_LockObject)
            {
                if (!IsLocked)
                {
                    _DataQueue.Enqueue(data);

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// Очистить очередь
        /// </summary>
        public void Clear(bool locking)
        {
            lock (_LockObject)
            {
                IsLocked = locking;

                _DataQueue.Clear();
            }
        }
    }
}

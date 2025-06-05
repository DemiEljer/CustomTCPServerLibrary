using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Base
{
    public class MultiTheadObject : IDisposable
    {
        /// <summary>
        /// Список потоков
        /// </summary>
        private List<WrappedThread> _ObjectThreads { get; } = new();
        /// <summary>
        /// Получить доступ к коллекции потоков
        /// </summary>
        public IEnumerable<WrappedThread> GetThreads => _ObjectThreads;
        /// <summary>
        /// Количество потоков
        /// </summary>
        public int Count => _ObjectThreads.Count;
        /// <summary>
        /// Событие возникновения ошибки
        /// </summary>
        public event Action<WrappedThread, Exception>? ExceptionThrowingEvent;
        /// <summary>
        /// Добавить поток
        /// </summary>
        /// <param name="logicHandler"></param>
        public void AddThread(Action<CancellationToken>? logicHandler)
        {
            var thread = new WrappedThread(logicHandler);

            thread.ExceptionThrowingEvent += (_thread, _exception) => ExceptionThrowingEvent?.Invoke(_thread, _exception);

            _ObjectThreads.Add(thread);
        }
        /// <summary>
        /// Начать поток
        /// </summary>
        public void Start()
        {
            foreach (var thread in _ObjectThreads)
            {
                thread.Start();
            }
        }
        /// <summary>
        /// Удалить поток
        /// </summary>
        public async Task DisposeAsync()
        {
            foreach (var thread in _ObjectThreads)
            {
                thread.Dispose();
            }

            // Задача ожидания выключения всех потоков
            Task killingTask = new Task(() =>
            {
                SpinWait waiter = new SpinWait();
                while (_ObjectThreads.Count(thread => thread.IsActive) > 0)
                {
                    waiter.SpinOnce();
                }
            });

            killingTask.Start();

            await killingTask;

            _ObjectThreads.Clear();

            return;
        }

        public void Dispose()
        {
            foreach (var thread in _ObjectThreads)
            {
                thread.Dispose();
            }
            _ObjectThreads.Clear();
        }
    }
}

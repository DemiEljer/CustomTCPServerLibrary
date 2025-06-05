using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Base
{
    public class WrappedThread : IDisposable
    {
        /// <summary>
        /// Объект потока
        /// </summary>
        private Thread _Thread { get; set; }
        /// <summary>
        /// Делегат подключаемой к потоку логики
        /// </summary>
        private Action<CancellationToken>? _LogicHandler { get; set; }
        /// <summary>
        /// Флаг, что поток не был убит
        /// </summary>
        public bool IsDisposed { get; private set; } = false;
        /// <summary>
        /// Токен прекращения работы потока
        /// </summary>
        public CancellationTokenSource CancellationToken { get; private set; } = new CancellationTokenSource();
        /// <summary>
        /// Событие возникновения ошибки
        /// </summary>
        public event Action<WrappedThread, Exception>? ExceptionThrowingEvent;
        /// <summary>
        /// Объект блокировки потоков
        /// </summary>
        private object _LockObject { get; } = new();
        /// <summary>
        /// Флаг, что поток запущен
        /// </summary>
        public bool IsActive { get; private set; } = false;

        public WrappedThread(Action<CancellationToken>? logicHandler)
        {
            _LogicHandler = logicHandler;

            _Thread = new Thread(_ThreadLogicLoop);
        }
        /// <summary>
        /// Запуск потока
        /// </summary>
        public void Start()
        {
            if (!IsActive)
            {
                lock (_LockObject)
                {
                    IsActive = true;

                    _Thread.Start();
                }
            }
        }
        /// <summary>
        /// Логика потока
        /// </summary>
        private void _ThreadLogicLoop()
        {
            lock (_LockObject)
            {
                WrappedThreadsManager.AddThread(this);

                SpinWait waiter = new SpinWait();

                while (!IsDisposed)
                {
                    try
                    {
                        _LogicHandler?.Invoke(CancellationToken.Token);
                    }
                    catch (Exception e)
                    {
                        ExceptionThrowingEvent?.Invoke(this, e);
                    }

                    waiter.SpinOnce();
                }

                WrappedThreadsManager.RemoveThread(this);

                IsActive = false;
            }
        }
        /// <summary>
        /// Убийство потока
        /// </summary>
        public void Dispose()
        {
            IsDisposed = true;
            CancellationToken.Cancel();
        }
    }
}

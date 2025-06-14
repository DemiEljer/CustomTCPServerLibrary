using CustomTCPServerLibrary.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.BaseEntities
{
    public abstract class BaseThreadableObject
    {
        /// <summary>
        /// Менеджер потков
        /// </summary>
        protected MultiTheadObject _ThreadsManager { get; } = new MultiTheadObject();
        /// <summary>
        /// Событие запуска обработчика
        /// </summary>
        public event Action<BaseThreadableObject>? HasStartedEvent;
        /// <summary>
        /// Событие остановки обработчика
        /// </summary>
        public event Action<BaseThreadableObject>? HasStoppedEvent;
        /// <summary>
        /// Ошибка при обработке потоков объекта
        /// </summary>
        public event Action<BaseThreadableObject, Exception>? ThreadExceptionThrowingEvent;
        /// <summary>
        /// Ошибка при запуске объекта
        /// </summary>
        public event Action<BaseThreadableObject, Exception>? StartingExceptionThrowingEvent;
        /// <summary>
        /// Ошибка при запуске объекта
        /// </summary>
        public event Action<BaseThreadableObject, Exception>? LogicLoopExceptionThrowingEvent;
        /// <summary>
        /// Событие циклического вызова логики объекта
        /// </summary>
        public event Action<BaseThreadableObject, CancellationToken>? LogicLoopInvokeEvent;
        /// <summary>
        /// Флаг, что объект в данный момент активен
        /// </summary>
        public bool IsActive { get; private set; } = false;
        /// <summary>
        /// Объект блокировки потоков
        /// </summary>
        private object _LockObject { get; } = new();

        public BaseThreadableObject()
        {
            _ThreadsManager.ExceptionThrowingEvent += (thead, exception) =>
            {
                ThreadExceptionThrowingEvent?.Invoke(this, exception);

                Stop();
            };
        }

        public virtual bool Start()
        {
            bool prevActivationState = IsActive;

            lock (_LockObject)
            {
                if (IsActive)
                {
                    return IsActive;
                }

                _ThreadsInit();

                bool startingStatus = false;

                try
                {
                    startingStatus = _Start();
                }
                catch (Exception e)
                {
                    StartingExceptionThrowingEvent?.Invoke(this, e);
                };

                if (startingStatus)
                {
                    _ThreadsManager.Start();
                }
                else
                {
                    _ThreadsManager.Dispose();
                }

                IsActive = startingStatus;
            }

            if (IsActive
                && prevActivationState != IsActive)
            {
                HasStartedEvent?.Invoke(this);
            }

            return IsActive;
        }
        /// <summary>
        /// Инициализация потоков
        /// </summary>
        protected abstract bool _Start();
        /// <summary>
        /// Остановка работы сетевого объекта
        /// </summary>
        public void Stop()
        {
            lock (_LockObject)
            {
                if (!IsActive)
                {
                    return;
                }

                _Stop();

                _ThreadsManager.Dispose();

                IsActive = false;
            }

            HasStoppedEvent?.Invoke(this);
        }
        /// <summary>
        /// Инициализация потоков
        /// </summary>
        protected abstract void _Stop();
        /// <summary>
        /// Удалить объект
        /// </summary>
        public void Dispose() => Stop();
        /// <summary>
        /// Инициализация потоков
        /// </summary>
        protected virtual void _ThreadsInit()
        {
            // Циклическая логика
            _ThreadsManager.AddThread((token) =>
            {
                try
                {
                    LogicLoopInvokeEvent?.Invoke(this, token);
                }
                catch (Exception e)
                {
                    LogicLoopExceptionThrowingEvent?.Invoke(this, e);
                }
            });
        }
        /// <summary>
        /// Обработать логику в случае, если объект неактивен
        /// </summary>
        protected void _HandleIfNotActive(Action? handler)
        {
            if (handler is null)
            {
                return;
            }

            lock (_LockObject)
            {
                if (!IsActive)
                {
                    handler.Invoke();
                }

            }
        }
        /// <summary>
        /// Обработать логику в случае, если объект активен
        /// </summary>
        protected void _HandleIfActive(Action? handler)
        {
            if (handler is null)
            {
                return;
            }

            lock (_LockObject)
            {
                if (IsActive)
                {
                    handler.Invoke();
                }
            }
        }

        public long GetCurrentTime() => TimeHandler.GetTimemark();
    }
}

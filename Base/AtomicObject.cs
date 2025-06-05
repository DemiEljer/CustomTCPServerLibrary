using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Base
{
    public class AtomicObject<TValue>
    {
        /// <summary>
        /// Собственное значение
        /// </summary>
        private TValue? _Value { get; set; }
        /// <summary>
        /// Свойство доступа к значению
        /// </summary>
        public TValue? Value 
        { 
            get
            {
                lock (_LockObject)
                {
                    return _Value;
                }
            }
            set
            {
                lock (_LockObject)
                {
                    _Value = value;
                }
            }
        }
        /// <summary>
        /// Объект блокировки потоков
        /// </summary>
        private object _LockObject { get; } = new();
        /// <summary>
        /// Безопасная установка значения
        /// </summary>
        /// <param name="setter"></param>
        public void SafetySetInvoke(Func<TValue?>? setter)
        {
            if (setter is null)
            {
                return;
            }

            lock (_LockObject)
            {
                _Value = setter();
            }
        }
        /// <summary>
        /// Установка значения
        /// </summary>
        /// <param name="setter"></param>
        public void SetInvoke(Func<TValue?>? setter)
        {
            if (setter is null)
            {
                return;
            }

            Value = setter();
        }
        /// <summary>
        /// Безопасное чтение значения
        /// </summary>
        /// <param name="getter"></param>
        public void SafetyGetInvoke(Action<TValue>? getter)
        {
            if (getter is null)
            {
                return;
            }

            lock (_LockObject)
            {
                if (_Value is not null)
                {
                    getter(_Value);
                }
            }
        }
        /// <summary>
        /// Безопасное чтение значения
        /// </summary>
        /// <param name="getter"></param>
        public void GetInvoke(Action<TValue>? getter)
        {
            if (getter is null)
            {
                return;
            }

            var value = Value;

            if (value is not null)
            {
                getter.Invoke(value);
            }
        }
    }
}

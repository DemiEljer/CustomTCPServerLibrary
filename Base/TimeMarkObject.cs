using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Base
{
    public class TimeMarkObject
    {
        /// <summary>
        /// Метка времени
        /// </summary>
        public long TimeMark { get; private set; } = TimeHandler.GetTimemark();
        /// <summary>
        /// Обновить метку времени
        /// </summary>
        public void Update() => TimeMark = TimeHandler.GetTimemark();

        public TimeMarkObject()
        {
        }

        public TimeMarkObject(long timeMark)
        {
            TimeMark = timeMark;
        }
        /// <summary>
        /// Проверка, что метка времени была преодолена
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public bool HasIntervalElapsed(long interval) => TimeHandler.HasIntervalElapsed(TimeMark, interval);
    }
}

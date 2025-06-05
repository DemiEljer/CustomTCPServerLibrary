using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Base
{
    public static class TimeHandler
    {
        /// <summary>
        /// Получить текущую метку времени
        /// </summary>
        /// <returns></returns>
        public static long GetTimemark() => (long)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
        /// <summary>
        /// Найти разницу метки времени с текущей
        /// </summary>
        /// <param name="timemark"></param>
        /// <returns></returns>
        public static long GetDelta(long timemark) => GetTimemark() - timemark;
        /// <summary>
        /// Проверить, что интервал времени прошел
        /// </summary>
        /// <param name="timemark"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static bool HasIntervalElapsed(long timemark, long interval) => GetDelta(timemark) >= interval;
    }
}

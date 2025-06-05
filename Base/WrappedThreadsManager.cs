using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Base
{
    public static class WrappedThreadsManager
    {
        private static SafeList<WrappedThread> _Threads { get; } = new SafeList<WrappedThread>();

        public static int Count => _Threads.Count;

        internal static void AddThread(WrappedThread? thread)
        {
            if (thread != null)
            {
                _Threads.Add(thread);
            }
        }

        internal static void RemoveThread(WrappedThread? thread)
        {
            if (thread != null)
            {
                _Threads.Remove(thread);
            }
        }

        public static async Task KillAll()
        {
            _Threads.Clear((thread) => thread.Dispose());
            // Задача ожидания выключения всех потоков
            Task killingTask = new Task(() =>
            {
                SpinWait waiter = new SpinWait();
                while (Count > 0)
                {
                    waiter.SpinOnce();
                }
            });

            killingTask.Start();

            await killingTask;

            return;
        }
    }
}

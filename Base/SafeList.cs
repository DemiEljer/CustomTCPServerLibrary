using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Base
{
    public class SafeList<TElement>
    {
        private List<TElement> _Collection { get; } = new List<TElement>();

        private object _LockObject { get; } = new();

        public int Count => _Collection.Count;

        public IEnumerable<TElement> GetElements()
        {
            lock (_LockObject)
            {
                return _Collection;
            }
        }

        public void Add(TElement element)
        {
            lock (_LockObject)
            {
                _Collection.Add(element);
            }
        }

        public void Remove(TElement element)
        {
            lock (_LockObject)
            {
                _Collection.Remove(element);
            }
        }

        public void Clear(Action<TElement>? disposeActionHandler)
        {
            lock (_LockObject)
            {
                foreach (var element in _Collection)
                {
                    disposeActionHandler?.Invoke(element);
                }
                _Collection.Clear();
            }
        }
    }
}

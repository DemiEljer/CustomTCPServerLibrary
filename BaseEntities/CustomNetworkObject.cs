using CustomTCPServerLibrary.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.BaseEntities
{
    public abstract class CustomNetworkObject : BaseThreadableObject
    {
        /// <summary>
        /// Атомарный объект доступа к конечной точке
        /// </summary>
        protected AtomicObject<NetEndPoint> _EndPoint { get; } = new();
        /// <summary>
        /// Конечная точка
        /// </summary>
        public NetEndPoint? EndPoint { get => _EndPoint.Value; private set => _EndPoint.Value = value; }

        public CustomNetworkObject(NetEndPoint? endPoint = null)
        {
            EndPoint = endPoint;
        }
        /// <summary>
        /// Утсановить конечную точку
        /// </summary>
        /// <param name="endPoint"></param>
        public void SetEndPoint(NetEndPoint? endPoint)
        {
            _HandleIfNotActive(() =>
            {
                EndPoint = endPoint;
            });
        }
    }
}

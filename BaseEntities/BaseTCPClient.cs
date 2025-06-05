using CustomTCPServerLibrary.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using CustomTCPServerLibrary.FinalStateMachines;
using CustomTCPServerLibrary.Configs;
using CustomTCPServerLibrary.Frames;
using System.Net.Mail;

namespace CustomTCPServerLibrary.BaseEntities
{
    public abstract class BaseTCPClient : CustomNetworkObject
    {
        /// <summary>
        /// Атомарный объект доступа к клиенту
        /// </summary>
        protected AtomicObject<TcpClient> _Client { get; } = new();
        /// <summary>
        /// Клиент
        /// </summary>
        public TcpClient? Client { get => _Client.Value; private set => _Client.Value = value; }
        /// <summary>
        /// Тайминги
        /// </summary>
        public TimingConfigs Timings { get; } = new();
        /// <summary>
        /// Очередь отправляемых данных
        /// </summary>
        protected DataQueue _TransmitQueue { get; } = new();
        /// <summary>
        /// Очередь отправляемых данных
        /// </summary>
        protected DataQueue _ReceiveQueue { get; } = new();
        /// <summary>
        /// Конечный автомат состояния клиента
        /// </summary>
        protected ClientFinalStateMachine _FinalStateMachine { get; } = new ClientFinalStateMachine();
        /// <summary>
        /// Событие отправки данных
        /// </summary>
        internal event Action<BaseTCPClient, byte[]>? InternalReceiveDataEvent;
        /// <summary>
        /// Событие приема данных
        /// </summary>
        internal event Action<BaseTCPClient, byte[]>? InternalTransmitDataEvent;
        /// <summary>
        /// Время задержки Ping
        /// </summary>
        public long PingTime { get; protected set; } = 0;
        /// <summary>
        /// Объект блокировки
        /// </summary>
        private object _DataHandlingLockObject { get; } = new();
        /// <summary>
        /// Факт подключения
        /// </summary>
        public bool IsConnected
        {
            get 
            {
                var client = Client;

                return client is null ? false : client.Connected;
            }
        }

        protected BaseTCPClient(NetEndPoint? endPoint = null, TcpClient? client = null) : base(endPoint)
        {
            _SetClient(client);
            // Обработка ситуации, когда вышло время ожидания сообщения Ping
            _FinalStateMachine.CallPingWaitingTimeElapsed += () =>
            {
                Stop();
            };
            // Инициализация обработчика таймингов
            {
                Timings.PingInterval = _FinalStateMachine.PingInterval;
                Timings.PingTimeout = _FinalStateMachine.PingTimeout;

                Timings.UpdateEvent += (timings) =>
                {
                    _Client.GetInvoke((client) =>
                    {
                        if (client.Client is not null)
                        {
                            client.ReceiveTimeout = timings.ReceivingTimeout;
                            client.SendTimeout = timings.SendingTimeout;
                        }
                    });
                    _FinalStateMachine.PingInterval = timings.PingInterval;
                    _FinalStateMachine.PingTimeout = timings.PingTimeout;
                };
            }
        }

        protected void _SetClient(TcpClient? client)
        {
            _HandleIfNotActive(() =>
            {
                _Client.SetInvoke(() =>
                {
                    if (client is not null && client.Client is not null)
                    {
                        // Инициализация обработчика таймингов
                        Timings.ReceivingTimeout = client.ReceiveTimeout;
                        Timings.SendingTimeout = client.SendTimeout;
                    }

                    return client;
                });
            });
        }
        /// <summary>
        /// Отправить данные
        /// </summary>
        /// <param name="data"></param>
        public void TransmitData(byte[]? data)
        {
            if (data != null)
            {
                InternalAddTransmittingData(FramesFabric.CreateDataFrame(data));
            }
        }
        /// <summary>
        /// Добавить отправляемые данные
        /// </summary>
        /// <param name="data"></param>
        internal void InternalAddTransmittingData(byte[] data)
        {
            _TransmitQueue.Push(data);
        }
        /// <summary>
        /// Отправить данные
        /// </summary>
        /// <returns></returns>
        private byte[]? _TransmitData(bool removeElement) => _TransmitQueue.Pop(removeElement);
        /// <summary>
        /// Принять данные
        /// </summary>
        /// <param name="data"></param>
        private void _ReceiveData(byte[] data) => _ReceiveQueue.Push(data);
        /// <summary>
        /// Инициализация потоков
        /// </summary>
        protected override void _ThreadsInit()
        {
            base._ThreadsInit();

            // Поток прекращения работы при разрыве соединения
            _ThreadsManager.AddThread((token) =>
            {
                if (!IsConnected)
                {
                    Stop();
                }
            });
            // Поток отправки сообщений
            _ThreadsManager.AddThread((token) =>
            {
                _Client.GetInvoke((client) =>
                {
                    // Получение массива данных без удаления
                    var data = _TransmitData(false);

                    if (data != null)
                    {
                        bool transmittingFlag = false;

                        lock (_DataHandlingLockObject)
                        {
                            var clientStream = client.GetStream();

                            if (clientStream is not null)
                            {
                                clientStream.Write(data, 0, data.Length);

                                transmittingFlag = true;
                            }
                        }

                        if (transmittingFlag)
                        {
                            // Вызов события отправки данных
                            InternalTransmitDataEvent?.Invoke(this, data);

                            _TransmitData(true);
                        }
                    }
                });
            });
            // Поток чтения сообщений
            _ThreadsManager.AddThread((token) =>
            {
                _Client.GetInvoke((client) =>
                {
                    if (client.Available > 0)
                    {
                        byte[] data;
                        bool receivingFlag = false;

                        lock (_DataHandlingLockObject)
                        {
                            data = new byte[client.Available];

                            var clientStream = client.GetStream();

                            if (clientStream is not null)
                            {
                                clientStream.Read(data, 0, data.Length);

                                receivingFlag = true;
                            }
                        }

                        if (receivingFlag)
                        {
                            _ReceiveData(data);
                        }
                    }
                });
            });
            // Поток приема сообщений
            _ThreadsManager.AddThread((token) =>
            {
                var data = _ReceiveQueue.Pop();
                
                if (data is not null)
                {
                    InternalReceiveDataEvent?.Invoke(this, data);
                }
            });
            // Поток логики
            _ThreadsManager.AddThread((token) =>
            {
                _FinalStateMachine.Invoke();
            });
        }

        protected override bool _Start()
        {
            _FinalStateMachine.Start();

            return true;
        }

        protected override void _Stop()
        {
            _TransmitQueue.Clear();
            _ReceiveQueue.Clear();

            lock (_DataHandlingLockObject)
            {
                Client?.Close();
            }
        }
        /// <summary>
        /// Получить объект клиента
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        protected static TcpClient? _GetClient(NetEndPoint? endPoint)
        {
            TcpClient client;

            if (endPoint is null
                || endPoint.Address is null)
            {
                client = new TcpClient(AddressFamily.InterNetwork);
            }
            else
            {
                client = new TcpClient(endPoint);
            }

            return client;
        }
    }
}

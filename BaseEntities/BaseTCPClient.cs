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
using CustomTCPServerLibrary.Controllers;

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
        /// Параметры данных
        /// </summary>
        public DataConfigs DataConfigs { get; } = new();
        /// <summary>
        /// Очередь отправляемых данных
        /// </summary>
        protected DataQueue<BaseFrame> _TransmitQueue { get; } = new();
        /// <summary>
        /// Очередь отправляемых данных
        /// </summary>
        protected DataQueue<byte[]> _ReceiveQueue { get; } = new();
        /// <summary>
        /// Конечный автомат состояния клиента
        /// </summary>
        protected ClientFinalStateMachine _FinalStateMachine { get; } = new ClientFinalStateMachine();
        /// <summary>
        /// Событие отправки данных
        /// </summary>
        internal event Action<BaseTCPClient, BaseFrame>? InternalReceiveDataEvent;
        /// <summary>
        /// Событие приема данных
        /// </summary>
        internal event Action<BaseTCPClient, BaseFrame>? InternalTransmitDataEvent;
        /// <summary>
        /// Событие возникновение ошибки при приеме сообщения
        /// </summary>
        public event Action<BaseTCPClient, Exception>? MessageReceiveThrowingEvent;
        /// <summary>
        /// Смещение времени между клиентом и сервером 
        /// </summary>
        public long TimeShift { get; protected set; } = 0;
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
        /// <summary>
        /// Обработчик логики подключения
        /// </summary>
        protected TCPClientConnectionController _ConnectionController { get; } = new(); 
        /// <summary>
        /// Контроллер процесса отправки данных
        /// </summary>
        protected TCPStreamWriterController _StreamWriterController { get; } = new();
        /// <summary>
        /// Контроллер процесса чтения данных
        /// </summary>
        protected TCPClientStreamReaderController _StreamReaderController { get; } = new();

        protected BaseTCPClient(NetEndPoint? endPoint = null, TcpClient? client = null) : base(endPoint)
        {
            _SetClient(client);
            // Обработка ситуации, когда вышло время ожидания сообщения Ping
            _FinalStateMachine.CallPingWaitingTimeElapsed += () => Stop();
            _FinalStateMachine.CallingProtocolIsNotValid += () => Stop();
            // Инициализация обработчика таймингов
            {
                Timings.PingInterval = _FinalStateMachine.PingInterval;
                Timings.PingTimeout = _FinalStateMachine.PingTimeout;

                Timings.UpdateEvent += (timings) =>
                {
                    _FinalStateMachine.PingInterval = timings.PingInterval;
                    _FinalStateMachine.PingTimeout = timings.PingTimeout;

                    _Client.GetInvoke((client) =>
                    {
                        if (client.Client is not null)
                        {
                            client.ReceiveTimeout = timings.ReceivingTimeout;
                            client.SendTimeout = timings.SendingTimeout;

                            client.ReceiveBufferSize = 8000;
                            client.SendBufferSize = 8000;
                        }
                    });
                };
            }
            // Инициализация обработчика параметров данных
            {
                DataConfigs.UpdateEvent += (dataConfigs) =>
                {
                    _Client.GetInvoke((client) =>
                    {
                        if (client.Client is not null)
                        {
                            client.ReceiveBufferSize = dataConfigs.ReceiveDataBufferSize;
                            client.SendBufferSize = dataConfigs.TransmitDataBufferSize;
                        }
                    });
                };
            }
            // Событие завершения процесса подключения клиента
            _ConnectionController.ClientConnectionHasFinishedEvent += (_client) =>
            {
                if (_client is not null && _client.Connected)
                {
                    _SetClient(_client);
                }
                else
                {
                    _Stop();
                }
            };
            // Событие завершения процесса отключения клиента
            _ConnectionController.ClientDisconnectionHasFinishedEvent += (_) => _SetClient(null);

            _StreamWriterController.InitNextDataGetter(() =>
            {
                // Получение массива данных без удаления
                var txFrames = _TransmitData();

                if (txFrames.data is not null)
                {
                    // Увеличение размера буфера в случае, если превыщен его размер
                    if (txFrames.data.Length > DataConfigs.TransmitDataBufferSize)
                    {
                        DataConfigs.TransmitDataBufferSize = txFrames.data.Length;
                    }
                }

                return txFrames;
            });
            // Событие отправки данных
            _StreamWriterController.DataHasBeenTransmittedEvent += (data) =>
            {
                // Вызов события отправки данных
                foreach (var frame in data.frames)
                {
                    InternalTransmitDataEvent?.Invoke(this, frame);
                }

                  _AcceptDataTransmitting(data.frames.Length);
            };
            // Инициализация делегата получения размера принятых данных
            _StreamReaderController.InitDataLengthGetter(() =>
            {
                var client = Client;

                if (client is not null)
                {
                    return client.Available;
                }
                else
                {
                    return 0;
                }
            });
            // Событие приема данных
            _StreamReaderController.DataHasBeenReceivedEvent += (data) =>
            {
                _ReceiveData(data);
            };
        }

        protected void _SetClient(TcpClient? client)
        {
            _Client.SetInvoke(() =>
            {
                if (client is not null && client.Client is not null)
                {
                    // Инициализация обработчика таймингов
                    Timings.ReceivingTimeout = client.ReceiveTimeout;
                    Timings.SendingTimeout = client.SendTimeout;
                    // Инициализация обработчика параметров данных
                    DataConfigs.ReceiveDataBufferSize = client.ReceiveBufferSize;
                    DataConfigs.TransmitDataBufferSize = client.SendBufferSize;
                }

                _StreamWriterController.Stream = client?.GetStream();
                _StreamReaderController.Stream = client?.GetStream();

                return client;
            });
        }
        /// <summary>
        /// Отправить данные
        /// </summary>
        /// <param name="data"></param>
        public bool TransmitData(byte[]? data)
        {
            if (data != null && _FinalStateMachine.IsProtocolValid)
            {
                return InternalAddTransmittingData(FramesFabric.CreateDataFrame(data));
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Добавить отправляемые данные
        /// </summary>
        /// <param name="data"></param>
        internal bool InternalAddTransmittingData(BaseFrame frame) => _TransmitQueue.Push(frame);
        /// <summary>
        /// Отправить данные
        /// </summary>
        /// <returns></returns>
        private (byte[]? data, BaseFrame[] frames) _TransmitData()
        {
            var frameSequence = FramesFabric.CreateFrameSequence(_TransmitQueue.PopAll(false).ToArray());

            if (frameSequence.Frames != null
                && frameSequence.Frames.Length > 0)
            {
                return (frameSequence, frameSequence.Frames);
            }
            else
            {
                return (null, Array.Empty<BaseFrame>());
            }
        }
        /// <summary>
        /// Подтвердить отправку сообщений
        /// </summary>
        /// <param name="count"></param>
        private void _AcceptDataTransmitting(int count)
        {
            _TransmitQueue.RemoveElementsFromHead(count);
        }
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
                var client = Client;

                if (client?.Connected == false)
                {
                    Stop();
                }
            });
            // Поток отправки сообщений
            _ThreadsManager.AddThread((token) =>
            {
                _Client.GetInvoke((client) =>
                {
                    if (client.Connected)
                    {
                        _StreamWriterController.TryTransmit();
                    }
                });
            });
            // Поток чтения сообщений
            _ThreadsManager.AddThread((token) =>
            {
                _Client.GetInvoke((client) =>
                {
                    if (client.Connected)
                    {
                        _StreamReaderController.TryReceive();
                    }
                });
            });
            // Поток парсинга принимаемых сообщений
            _ThreadsManager.AddThread((token) =>
            {
                var data = _ReceiveQueue.Pop();

                if (data is not null)
                {
                    bool areFramesValid;

                    var frameSequences = FramesFabric.ParseFrameSequence(data, out areFramesValid);
                    // Установка флага, что все принятые пакеты валидны
                    _FinalStateMachine.SetReceivingFramesValidFlag(areFramesValid);

                    foreach (var frame in FramesFabric.GetBaseFramesFromSequences(frameSequences))
                    {
                        try
                        {
                            InternalReceiveDataEvent?.Invoke(this, frame);
                        }
                        catch (Exception e)
                        {
                            MessageReceiveThrowingEvent?.Invoke(this, e);
                        }
                    }
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
            // Разблокировка очередей для записи
            _ReceiveQueue.Unlock();
            _TransmitQueue.Unlock();
            // Блокировка возможности уменьшения буферов
            DataConfigs.LockDecrease(true);

            return true;
        }

        protected override void _Stop()
        {
            _TransmitQueue.Clear(true);
            _ReceiveQueue.Clear(true);
            // Разблокировка возможности уменьшения буферов
            DataConfigs.LockDecrease(false);

            lock (_DataHandlingLockObject)
            {
                _Client.SafetyGetInvoke((client) =>
                {
                    _ConnectionController.Disconnect(client);
                });
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

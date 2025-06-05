using CustomTCPServerLibrary.Base;
using CustomTCPServerLibrary.BaseEntities;
using CustomTCPServerLibrary.Configs;
using System.Net;
using System.Net.Sockets;

namespace CustomTCPServerLibrary
{
    public class CustomTCPServer : CustomNetworkObject
    {
        /// <summary>
        /// Атомарный объект доступа к клиенту
        /// </summary>
        protected AtomicObject<TcpListener> _Listener { get; } = new();
        /// <summary>
        /// Объект прослушки
        /// </summary>
        public TcpListener? Listener { get => _Listener.Value; protected set => _Listener.Value = value; }
        /// <summary>
        /// Тайминги
        /// </summary>
        public TimingConfigs Timings { get; } = new();
        /// <summary>
        /// Событие подключения клиента
        /// </summary>
        public event Action<CustomTCPServer, CustomTCPServerClient>? ClientHasConnectedEvent;
        /// <summary>
        /// Событие отключения клиента
        /// </summary>
        public event Action<CustomTCPServer, CustomTCPServerClient>? ClientHasDisconnectedEvent;
        /// <summary>
        /// Коллекция клиентов
        /// </summary>
        private SafeList<CustomTCPServerClient> _Clients { get; } = new();
        /// <summary>
        /// Коллекция клиентов
        /// </summary>
        public IEnumerable<CustomTCPServerClient> Clients => _Clients.GetElements();

        public CustomTCPServer(NetEndPoint? endpoint = null) : base(endpoint)
        {
            Timings.UpdateEvent += (timings) =>
            {
                foreach (var client in Clients)
                {
                    client.Timings.UpdateTimings(timings);
                }
            };
        }
        /// <summary>
        /// Разрешение обработки NAT
        /// </summary>
        public void AllowNatTraversal(bool permition)
        {
            _Listener.GetInvoke((listener) =>
            {
                listener.AllowNatTraversal(permition);
            });
        }
        /// <summary>
        /// Инициализация потоков
        /// </summary>
        protected override void _ThreadsInit()
        {
            base._ThreadsInit();

            // Поток подтверждения установки соединения
            _ThreadsManager.AddThread(async (token) =>
            {
                _Listener.GetInvoke(async (listener) =>
                {
                    TcpClient? client = null;

                    try
                    {
                        client = await listener.AcceptTcpClientAsync(token);
                    }
                    catch
                    {
                        return;
                    }

                    // Получается 
                    if (client != null && client.Client != null)
                    {
                        var customClient = new CustomTCPServerClient(client);
                        // Обновить параметры таймингов на стороне клиента
                        customClient.Timings.UpdateTimings(Timings);
                        // Обработка события остановки работы клиента
                        customClient.HasStoppedEvent += (_client) =>
                        {
                            var _customClient = _client as CustomTCPServerClient;

                            if (_customClient is not null)
                            {
                                _Clients.Remove(_customClient);

                                ClientHasDisconnectedEvent?.Invoke(this, _customClient);
                            }
                        };
                        // Добавление клиента в коллекцию клиентов
                        _Clients.Add(customClient);
                        // Вызов события подключения клиента
                        ClientHasConnectedEvent?.Invoke(this, customClient);
                        // Запуск логики обработки клиента
                        customClient.Start();
                    }
                });
            });
        }

        protected override bool _Start()
        {
            // Инициализация объекта прослушки сокета
            Listener = _GetListener(EndPoint);
            // Проверка, что объект прослушки был создан
            if (Listener is null)
            {
                return false;
            }

            Listener.Start();

            return true;
        }

        protected override void _Stop()
        {
            Listener?.Stop();
            // Остановка работы всех подключенных клиентов
            while (_Clients.Count > 0)
            {
                _Clients.GetElements().First().Stop();
            }
        }

        protected static TcpListener? _GetListener(NetEndPoint? endPoint)
        {
            if (endPoint is null)
            {
                return null;
            }

            if (endPoint.Address is null)
            {
                return new TcpListener(IPAddress.Any, endPoint.Port);
            }
            else
            {
                return new TcpListener(endPoint.Address, endPoint.Port);
            }
        }
    }
}

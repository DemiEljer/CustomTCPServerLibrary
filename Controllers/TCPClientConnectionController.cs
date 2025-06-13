using CustomTCPServerLibrary.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Controllers
{
    public class TCPClientConnectionController
    {
        /// <summary>
        /// Внутренний запрос на подключение
        /// </summary>
        private class ClientConnectionRequest : IDisposable
        {
            /// <summary>
            /// Тело запроса (обработчик)
            /// </summary>
            private Action<TcpClient, IPEndPoint>? _RequestHandler { get; set; }
            /// <summary>
            /// Клиент
            /// </summary>
            private TcpClient _Client { get; }
            /// <summary>
            /// Оконечная точка подключения
            /// </summary>
            private IPEndPoint _Endpoint { get; }

            public ClientConnectionRequest(TcpClient client, IPEndPoint endpoint, Action<TcpClient, IPEndPoint> requestHandler)
            {
                _Client = client;
                _Endpoint = endpoint;
                _RequestHandler = requestHandler;
            }
            /// <summary>
            /// Вызвать тело запроса
            /// </summary>
            public void Invoke()
            {
                if (_Client is not null
                    && _Endpoint is not null)
                {
                    _RequestHandler?.Invoke(_Client, _Endpoint);
                }
            }

            public void Dispose()
            {
                _RequestHandler = null;
            }
        }
        /// <summary>
        /// Внутренний запрос на подключение
        /// </summary>
        private class ClientDisconnectionRequest : IDisposable
        {
            /// <summary>
            /// Тело запроса (обработчик)
            /// </summary>
            private Action<TcpClient>? _RequestHandler { get; set; }
            /// <summary>
            /// Клиент
            /// </summary>
            private TcpClient? _Client { get; }

            public ClientDisconnectionRequest(TcpClient? client, Action<TcpClient>? requestHandler)
            {
                _Client = client;
                _RequestHandler = requestHandler;
            }
            /// <summary>
            /// Вызвать тело запроса
            /// </summary>
            public void Invoke()
            {
                if (_Client is not null)
                {
                    _RequestHandler?.Invoke(_Client);
                }
            }

            public void Dispose()
            {
                _RequestHandler = null;
            }
        }
        /// <summary>
        /// Доступен ли запрос на подключение
        /// </summary>
        public bool IsConnectionAvailable => _ConnectionRequest is null;
        /// <summary>
        /// Доступен ли запрос на отключение
        /// </summary>
        public bool IsDisconnectionAvailable => _DisconnectionRequest is null;
        /// <summary>
        /// Факт нахождения в состоянии подключения
        /// </summary>
        public bool IsConnecting => _ConnectionRequest is not null;
        /// <summary>
        /// Факт нахождения в состоянии отключения
        /// </summary>
        public bool IsDisconnecting => _DisconnectionRequest is not null;
        /// <summary>
        /// Запрос на подключение
        /// </summary>
        private ClientConnectionRequest? _ConnectionRequest { get; set; } = null;
        /// <summary>
        /// Запрос на отключение
        /// </summary>
        private ClientDisconnectionRequest? _DisconnectionRequest { get; set; } = null;
        /// <summary>
        /// Событие подключения клиента
        /// </summary>
        public event Action<TcpClient>? ClientConnectionHasFinishedEvent;
        /// <summary>
        /// Событие отключения клиента
        /// </summary>
        public event Action<TcpClient>? ClientDisconnectionHasFinishedEvent;
        /// <summary>
        /// Объект блокировки потоков
        /// </summary>
        private object _LockObject { get; } = new();

        public bool Connect(TcpClient? client, IPEndPoint? endpoint)
        {
            if (client is null || endpoint is null)
            {
                return false;
            }

            lock (_LockObject)
            {
                if (IsConnectionAvailable && client.Connected == false)
                {
                    _ConnectionRequest = new ClientConnectionRequest(client, endpoint, (_client, _endpoint) =>
                    {
                        _client.BeginConnect(_endpoint.Address, _endpoint.Port, (_) =>
                        {
                            ClientConnectionHasFinishedEvent?.Invoke(_client);

                            lock (_LockObject)
                            {
                                _ConnectionRequest?.Dispose();
                                _ConnectionRequest = null;
                            }

                            if (_DisconnectionRequest is not null)
                            {
                                _DisconnectionRequest.Invoke();
                            }
                        }, null);
                    });

                    if (!IsDisconnecting)
                    {
                        _ConnectionRequest.Invoke();
                    }
                }
            }

            return true;
        }

        public void Disconnect(TcpClient? client)
        {
            if (client is null)
            {
                return;
            }

            lock (_LockObject)
            {
                if (IsDisconnectionAvailable && client.Connected == true)
                {
                    _DisconnectionRequest = new ClientDisconnectionRequest(client, (_client) =>
                    {
                        _client.Close();

                        ClientDisconnectionHasFinishedEvent?.Invoke(_client);

                        lock (_LockObject)
                        {
                            _DisconnectionRequest?.Dispose();
                            _DisconnectionRequest = null;
                        }

                        if (_ConnectionRequest is not null)
                        {
                            _ConnectionRequest.Invoke();
                        }
                    });

                    if (!IsConnecting)
                    {
                        _DisconnectionRequest.Invoke();
                    }
                }
            }
        }
    }
}

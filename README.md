## CustomTCPServerLibrary
Библиотека **CustomTCPServerLibrary** оборачивает стандартные объекты **TcpListener** и**TcpClient**, реализует логику проверки соединения (**Ping**) с передачей параметров времени между клиентами и сервером, а также предоставляет более удобный интерфейс для взаимодействия с объектами сетевого общения (**CustomTCPServer**, **CustomTCPClient**, **CustomTCPServerClient**).

### Объекты сетевого взаимодействия

|Имя класса|Описание|
|----------|--------|
|CustomTCPServer|Объект для запуска сервера|
|CustomTCPClient|Объект для запуска удаленного клиента|
|CustomTCPServerClient|Объект подключившегося клиента на стороне сервера|

## Пример использования в коде

### Создание и запуск сервера

```C#

// Создание объекта сервера
CustomTCPServer server = new CustomTCPServer(new NetEndPoint(IPAddress.Parse("192.168.0.5"), 8080));
// Инициализация временных параметров
server.Timings.SendingTimeout = 2000;
server.Timings.ReceivingTimeout = 2000;
server.Timings.PingInterval = 500;
server.Timings.PingTimeout = 5000;
// Обработчик события запуска сервера
server.HasStartedEvent += (_) =>
{
    Console.WriteLine($"Сервер был запущен :: {server.EndPoint.ToString()}");
};
// Обработчик события остановки сервера
server.HasStoppedEvent += (_) =>
{
    Console.WriteLine($"Сервер был выключен :: {server.EndPoint.ToString()}");
};
// Обработчик события подключения клиента к серверу
server.ClientHasConnectedEvent += (_, _client) =>
{
    Console.WriteLine($"Подключился клиент :: {_client.EndPoint.ToString()}");
    // Обработка события получения данных от клиента на стороне сервера
    _client.ReceiveDataEvent += (_, data) =>
    {
        Console.WriteLine($"Сервер получил данные от клиента :: {_client.EndPoint.ToString()} :: {string.Join(" ", data.Select(b => b.ToString("X2")))}");
    };
    // Циклическая отправка данных клиенту
    // (циклический вызов пользовательской логики сервера)
    _client.LogicLoopInvokeEvent += (_, token) =>
    {
        // Отправка данных
        _client.TransmitData(new byte[] { 0x00 });
    };
};
// Обработка события подтверждения протокола общения (на той стороне работает та же библиотека)
client.ProtocolHasBeenVerifiedEvent += (_, data) =>
{
    Console.WriteLine($"Клиент подтвержден :: {client.EndPoint.ToString()}");
};
// Обработка события отключения клиента от сервера
server.ClientHasDisconnectedEvent += (_, client) =>
{
    Console.WriteLine($"Отключился клиент :: {client.EndPoint.ToString()}");
};
// Обработка событие возникновения ошибки при запуске сервера
server.StartingExceptionThrowingEvent += (_, e) =>
{
    Console.WriteLine($"{e.ToString()}");
};
// Обработка событие возникновения ошибки во внутренних потоках сервера
server.ThreadExceptionThrowingEvent += (_, e) =>
{
    Console.WriteLine($"{e.ToString()}");
};
// Обработка события возникновения ошибки при приеме сообщения
client.MessageReceiveExceptionThrowingEvent += (_, data) =>
{
    Console.WriteLine($"{e.ToString()}");
};
// Запуск сервера
server.Start();

```
### Создание и запуск клиента

```C#

// Создание объекта клиента
CustomTCPClient client = new CustomTCPClient(new NetEndPoint(IPAddress.Parse("192.168.0.4"), 8080));
// Установка конечной точки подключения к серверу
client.SetServerEndPoint(new NetEndPoint(IPAddress.Parse("192.168.0.5"), 8080));
// Обработка событие возникновения ошибки при запуске клиента
client.StartingExceptionThrowingEvent += (_, e) =>
{
    Console.WriteLine($"{client.EndPoint.ToString()} -> {client.ServerEndpoint.ToString()}\r\n{e.ToString()}");
};
// Обработка событие возникновения ошибки во внутренних потоках клиента
client.ThreadExceptionThrowingEvent += (_, e) =>
{
    Console.WriteLine($"{e.ToString()}");
};
// Обработка события подтверждения протокола общения (на той стороне работает та же библиотека)
client.ProtocolHasBeenVerifiedEvent += (_, data) =>
{
    Console.WriteLine($"Сервер подтвержден :: {client.EndPoint.ToString()}");
};
// Обработка события возникновения ошибки при приеме сообщения
client.MessageReceiveExceptionThrowingEvent += (_, data) =>
{
    Console.WriteLine($"{e.ToString()}");
};
// Циклическая отправка данных серверу
// (циклический вызов пользовательской логики клиента)
client.LogicLoopInvokeEvent += (_, token) =>
{
    client.TransmitData(new byte[] { 0x55 });
};
// Обработка события получения данных от сервера
client.ReceiveDataEvent += (_, data) =>
{
    Console.WriteLine($"Клиент получил данные от сервера :: {client.EndPoint.ToString()} :: {string.Join(" ", data.Select(b => b.ToString("X2")))}");
};

// Запуск клиента
client.Start();

```

### Примечания

> Используется библиотека для сериализации данных [BinarySerializerLibrary](https://github.com/DemiEljer/BinarySerializerLibrary).

> Используется утилита [CommitContentCreater](https://github.com/DemiEljer/CommitContentCreater) для формирования текстов описания коммитов.

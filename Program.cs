using FitnessCenter.Client;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║   FitnessCenter API Client — Демонстрация делегатов  ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");
Console.ResetColor();

var api = new ApiService("http://localhost/api");
var logFile = "api_log.txt";

// ── Обработчик 1: Вывод в консоль ──
OnRequestCompleted consoleHandler = (endpoint, method, status, ms, body) =>
{
    var color = status >= 200 && status < 300 ? ConsoleColor.Green
              : status >= 400 ? ConsoleColor.Red : ConsoleColor.Yellow;
    Console.ForegroundColor = color;
    Console.WriteLine($"  ✓ {method,-6} /{endpoint,-30} [{status}] {ms}ms");
    Console.ResetColor();
};

// ── Обработчик 2: Запись в лог-файл ──
OnRequestCompleted fileHandler = (endpoint, method, status, ms, body) =>
{
    var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {method} /{endpoint} | {status} | {ms}ms";
    File.AppendAllText(logFile, line + Environment.NewLine);
};

// ── Обработчик ошибок ──
OnRequestError errorHandler = (endpoint, method, error) =>
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"  ✗ ОШИБКА {method} /{endpoint}: {error}");
    Console.ResetColor();
};

// ── Action<T> для логирования ──
api.OnLogMessage = msg => Console.ForegroundColor == ConsoleColor.White
    ? Console.WriteLine(msg)
    : Console.Write(""); // Используется внутри ApiService

// ── Подписка (+=) на оба обработчика ──
api.RequestCompleted += consoleHandler;
api.RequestCompleted += fileHandler;
api.RequestErrored += errorHandler;

Console.WriteLine("\n📌 Шаг 1: Подписаны оба обработчика (консоль + файл)");
Console.WriteLine("─────────────────────────────────────────────────────");

// ── Операция 1: Получить список клиентов (Func<Task<List<ClientDto>?>>) ──
Console.WriteLine("\n[1] Получение списка клиентов через Func<>...");
var clients = await api.GetClientsFunc();
if (clients != null)
{
    Console.WriteLine($"  Найдено клиентов: {clients.Count}");
    foreach (var c in clients.Take(3))
        Console.WriteLine($"    • {c.FirstName} {c.LastName} | {c.Email} | активен: {c.IsActive}");
}

// ── Операция 2: Получить клиента по ID (Func<int, Task<ClientDto?>>) ──
Console.WriteLine("\n[2] Получение клиента по ID=1 через Func<int, ...>...");
var client = await api.GetClientByIdFunc(1);
if (client != null)
    Console.WriteLine($"  Клиент: {client.FirstName} {client.LastName}, членство до {client.MembershipEnd:dd.MM.yyyy}");

// ── Операция 3: Создать нового клиента (Func<CreateClientDto, Task<ClientDto?>>) ──
Console.WriteLine("\n[3] Создание нового клиента через Func<CreateClientDto, ...>...");
var newClient = await api.CreateClientFunc(new CreateClientDto(
    "Тестовый", "Пользователь", $"test_{Guid.NewGuid():N[..6]}@example.com", "+7-999-000-0000",
    new DateTime(1995, 6, 15), DateTime.UtcNow, DateTime.UtcNow.AddMonths(12)));
if (newClient != null)
    Console.WriteLine($"  Создан: ID={newClient.Id}, {newClient.FirstName} {newClient.LastName}");

Console.WriteLine("\n─────────────────────────────────────────────────────");
Console.WriteLine("📌 Шаг 2: Отключаем файловый обработчик (-=)");
// ── Отписка от файлового обработчика (-=) ──
api.RequestCompleted -= fileHandler;
Console.WriteLine("  fileHandler отписан. Следующие операции НЕ пишутся в файл.");
Console.WriteLine("─────────────────────────────────────────────────────\n");

// ── Операция 4: Список тренеров ──
Console.WriteLine("[4] Получение списка тренеров (только консоль, файл отключён)...");
var trainers = await api.GetTrainersAsync();
if (trainers != null)
{
    Console.WriteLine($"  Найдено тренеров: {trainers.Count}");
    foreach (var t in trainers)
        Console.WriteLine($"    • {t.FirstName} {t.LastName} | {t.Specialization} | опыт: {t.ExperienceYears} лет");
}

// ── Операция 5: Список занятий ──
Console.WriteLine("\n[5] Получение списка занятий...");
var classes = await api.GetClassesAsync();
if (classes != null)
{
    Console.WriteLine($"  Найдено занятий: {classes.Count}");
    foreach (var fc in classes)
        Console.WriteLine($"    • {fc.Name} | {fc.DurationMinutes} мин | {fc.Difficulty} | Тренер: {fc.TrainerName}");
}

// ── Операция 6: Создать запись в расписание ──
Console.WriteLine("\n[6] Запись клиента на занятие...");
if (clients?.Count > 0 && classes?.Count > 0)
{
    var schedule = await api.CreateScheduleAsync(new CreateScheduleDto(
        classes[0].Id, clients[0].Id, DateTime.UtcNow.AddDays(5).Date.AddHours(10), "Тестовая запись"));
    if (schedule != null)
        Console.WriteLine($"  Запись создана: ID={schedule.Id}, занятие #{schedule.ClassId}, клиент #{schedule.ClientId}");
}

// ── Операция 7: Удаление созданного тестового клиента ──
if (newClient != null)
{
    Console.WriteLine($"\n[7] Удаление тестового клиента ID={newClient.Id}...");
    var deleted = await api.DeleteClientAsync(newClient.Id);
    Console.WriteLine($"  Результат: {(deleted ? "удалён успешно" : "ошибка удаления")}");
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("\n╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║              Демонстрация завершена                  ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");
Console.ResetColor();
Console.WriteLine($"\nЛог первых 3 операций записан в: {Path.GetFullPath(logFile)}");
Console.WriteLine("Нажмите любую клавишу для выхода...");
Console.ReadKey();

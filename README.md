# 💪 FitnessCenter — Система управления фитнес-центром

> **Лабораторная работа №3 · Вариант 19 · .NET 10**  
> Production-ready система на ASP.NET Core + PostgreSQL + Redis + Docker

---

## 📋 О проекте

Полноценная серверная система для управления фитнес-центром с REST API, кэшированием, мониторингом и двумя пользовательскими интерфейсами.

### Сущности системы

| Сущность | Описание |
|----------|----------|
| 👥 **Client** | Клиенты фитнес-центра с абонементами |
| 🏋️ **Trainer** | Тренеры и их специализации |
| 📋 **FitnessClass** | Виды занятий (йога, HIIT, пилатес и др.) |
| 📅 **Schedule** | Записи клиентов на занятия |

---

## 🏗️ Стек технологий

| Компонент | Технология |
|-----------|-----------|
| Backend API | ASP.NET Core Web API (.NET 10) |
| База данных | PostgreSQL 16 |
| Кэширование | Redis 7 |
| ORM | Entity Framework Core |
| Обратный прокси | Nginx |
| Мониторинг | Prometheus + Grafana |
| Контейнеризация | Docker + Docker Compose |
| CI/CD | GitHub Actions |

---

## 🚀 Быстрый старт

### Требования
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Запуск за 3 шага

```bash
# 1. Клонировать репозиторий
git clone https://github.com/YOUR_USERNAME/FitnessCenter.git
cd FitnessCenter

# 2. Запустить все контейнеры
docker compose up --build -d

# 3. Запустить UI (в двух отдельных терминалах)
cd FitnessCenter.AdminUI && dotnet run   # http://localhost:5001
cd FitnessCenter.UserUI  && dotnet run   # http://localhost:5002
```

---

## 🌐 Адреса сервисов

| Сервис | Адрес | Описание |
|--------|-------|----------|
| 👑 Admin UI | http://localhost:5001 | Панель администратора |
| 🙋 User UI | http://localhost:5002 | Личный кабинет клиента |
| 📖 Swagger | http://localhost/swagger | Документация API |
| 📊 Grafana | http://localhost:3000 | Мониторинг (admin/admin) |
| 🔍 Prometheus | http://localhost:9090 | Метрики |
| 🗄️ PostgreSQL | localhost:5432 | База данных (fitness/fitness123) |

---

## 📁 Структура проекта

```
FitnessCenter/
├── 📂 FitnessCenter.API/          # ASP.NET Core Web API
│   ├── Controllers/               # CRUD контроллеры (4 сущности)
│   ├── Models/                    # Модели + DTO
│   ├── Data/                      # AppDbContext + Seeding
│   ├── Services/                  # Redis кэш-сервис
│   └── Dockerfile
│
├── 📂 FitnessCenter.AdminUI/      # Веб-панель администратора
├── 📂 FitnessCenter.UserUI/       # Личный кабинет клиента
├── 📂 FitnessCenter.Client/       # Консольный клиент (делегаты)
├── 📂 FitnessCenter.Tests/        # xUnit тесты
│
├── 📂 nginx/                      # Конфигурация обратного прокси
├── 📂 prometheus/                 # Конфигурация сбора метрик
├── 📂 grafana/                    # Дашборды и provisioning
├── 📂 .github/workflows/          # CI/CD пайплайн
│
├── docker-compose.yml
└── start.html                     # Страница выбора роли
```

---

## 🔌 API Endpoints

<details>
<summary><b>👥 Клиенты /api/clients</b></summary>

| Метод | URL | Описание | Кэш |
|-------|-----|----------|-----|
| GET | `/api/clients` | Список всех клиентов | ✅ Redis 5 мин |
| GET | `/api/clients/{id}` | Клиент по ID | ✅ Redis 10 мин |
| POST | `/api/clients` | Создать клиента | — |
| PUT | `/api/clients/{id}` | Обновить клиента | — |
| DELETE | `/api/clients/{id}` | Удалить клиента | — |

</details>

<details>
<summary><b>🏋️ Тренеры /api/trainers</b></summary>

| Метод | URL | Описание | Кэш |
|-------|-----|----------|-----|
| GET | `/api/trainers` | Список всех тренеров | ✅ Redis 10 мин |
| GET | `/api/trainers/{id}` | Тренер по ID | ✅ Redis 10 мин |
| POST | `/api/trainers` | Создать тренера | — |
| PUT | `/api/trainers/{id}` | Обновить тренера | — |
| DELETE | `/api/trainers/{id}` | Удалить тренера | — |

</details>

<details>
<summary><b>📋 Занятия /api/fitnessclasses</b></summary>

| Метод | URL | Описание | Кэш |
|-------|-----|----------|-----|
| GET | `/api/fitnessclasses` | Список занятий | ✅ Redis 10 мин |
| GET | `/api/fitnessclasses/{id}` | Занятие по ID | — |
| POST | `/api/fitnessclasses` | Создать занятие | — |
| PUT | `/api/fitnessclasses/{id}` | Обновить занятие | — |
| DELETE | `/api/fitnessclasses/{id}` | Удалить занятие | — |

</details>

<details>
<summary><b>📅 Расписание /api/schedules</b></summary>

| Метод | URL | Описание | Кэш |
|-------|-----|----------|-----|
| GET | `/api/schedules` | Всё расписание | — |
| GET | `/api/schedules/{id}` | Запись по ID | — |
| GET | `/api/schedules/client/{id}` | Расписание клиента | ✅ Redis 3 мин |
| POST | `/api/schedules` | Создать запись | — |
| PUT | `/api/schedules/{id}` | Обновить / сменить статус | — |
| DELETE | `/api/schedules/{id}` | Удалить запись | — |

</details>

---

## 🖥️ Пользовательские интерфейсы

### 👑 Панель администратора (порт 5001)

- **Дашборд** — статистика по всем сущностям и ближайшие записи
- **Клиенты** — полный CRUD с поиском по таблице
- **Тренеры** — управление тренерским составом
- **Занятия** — виды тренировок с уровнями сложности
- **Расписание** — управление записями, смена статусов (📋 Записан / ✅ Завершено / ❌ Отменено)

### 🙋 Личный кабинет клиента (порт 5002)

- **Вход** — выбор профиля из списка клиентов
- **Профиль** — карточка с информацией о членстве и оставшихся днях
- **Мои записи** — расписание с возможностью отмены
- **Каталог занятий** — все доступные тренировки
- **Запись** — форма записи на занятие с выбором времени

---

## 🗃️ База данных

Подключение через **DBeaver** или любой PostgreSQL-клиент:

```
Host:     localhost
Port:     5432
Database: fitnessdb
Username: fitness
Password: fitness123
```

При первом запуске автоматически создаются таблицы и добавляются тестовые данные:
- 3 тренера
- 4 вида занятий
- 4 клиента
- 5 записей расписания

---

## 📊 Мониторинг

Grafana доступна на `http://localhost:3000` (логин: `admin`, пароль: `admin`).

Дашборд **FitnessCenter API Dashboard** включает:
- 📈 HTTP Requests/sec
- ⏱️ Avg Response Time (ms)
- ❌ 5xx Errors
- 📉 HTTP Requests Over Time

---

## 🧪 Тесты

```bash
cd FitnessCenter.Tests
dotnet test --verbosity normal
```

5 тестов покрывают основные операции с базой данных:

| Тест | Проверяет |
|------|-----------|
| `CanAddAndRetrieveClient` | Создание и получение клиента |
| `CanAddAndRetrieveTrainer` | Создание тренера |
| `CanCreateFitnessClassLinkedToTrainer` | Занятие со связью на тренера |
| `CanCreateScheduleForClientAndClass` | Создание записи расписания |
| `CanDeleteClient` | Удаление клиента |

---

## 🤖 Консольный клиент (делегаты)

```bash
cd FitnessCenter.Client
dotnet run
```

Демонстрирует применение делегатов в C#:

```csharp
// Собственный тип делегата
public delegate void OnRequestCompleted(string endpoint, string method,
    int statusCode, long elapsedMs, string? responseBody);

// Многоадресный делегат — два обработчика
api.RequestCompleted += consoleHandler;  // вывод в консоль
api.RequestCompleted += fileHandler;     // запись в файл

// После 3 операций — динамическая отписка
api.RequestCompleted -= fileHandler;

// Func<T> для CRUD операций
var clients = await api.GetClientsFunc();
var client  = await api.GetClientByIdFunc(1);
var created = await api.CreateClientFunc(dto);
```

---

## ⚙️ CI/CD

GitHub Actions автоматически запускает пайплайн при каждом `push`:

```
✅ Checkout  →  ✅ Setup .NET 10  →  ✅ Restore  →  ✅ Build  →  ✅ Test
```

Файл конфигурации: `.github/workflows/ci.yml`

---

## 🛑 Остановка

```bash
docker compose down        # остановить контейнеры
docker compose down -v     # остановить + удалить данные БД
```

---

<div align="center">

**ASP.NET Core · PostgreSQL · Redis · Docker · Nginx · Prometheus · Grafana · .NET 10**

</div>

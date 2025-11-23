## Требования к структуре и оформлению

### Структура папки Scripts
Всегда поддерживайте актуальность этой структуры при изменениях папок (глубина до второго уровня).
- Assets/Scripts — корневой каталог игровых скриптов.
- Assets/Scripts/Core — базовая функциональность и общие компоненты ядра.

### Общие требования к коду
- Всегда указывайте namespace для всех создаваемых классов.
- В начале каждого файла размещайте комментарий с кратким назначением (1–2 предложения) и обновляйте его при изменении роли файла.

# Architectural Roles

## Model

Зона ответственности

Представляет данные и доменную логику, не зная о UI, сценах и конкретном движке (по возможности).

Хранит состояние: HP, уровень, список способностей, прогресс квеста и т.п.

Содержит правила изменения этих данных (инварианты, валидация, расчёты).

Ограничения и требования

Не тянет зависимости на MonoBehaviour, GameObject, UI и т.п.

Не занимается загрузкой сцен, ресурсами, аудио и т.д.

Может использовать простые события/колбеки, но не должен напрямую дергать View.

Минимальный пример

public class HealthModel
{
    public int Max { get; }
    public int Current { get; private set; }

    public bool IsDead => Current <= 0;

    public HealthModel(int max)
    {
        if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
        Max = max;
        Current = max;
    }

    public void TakeDamage(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Current = Math.Max(0, Current - amount);
    }

    public void Heal(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Current = Math.Min(Max, Current + amount);
    }
}

## View

Зона ответственности

Визуальное представление данных для игрока: спрайты, текст, полоски HP, анимации.

Реагирует на изменения модели или контроллера и обновляет UI/графику.

Ограничения и требования

Не содержит бизнес-логики.

Не принимает решения “что делать дальше” — только показывает “что сейчас”.

Может знать про MonoBehaviour, RectTransform, анимации и т.п., но не должна хранить “истину” (истина — в Model).

Минимальный пример

public class HealthView : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Slider _slider;

    public void SetValue(int current, int max)
    {
        _slider.maxValue = max;
        _slider.value = current;
    }
}

## Controller

Зона ответственности

Связывает Model и View, обрабатывает ввод пользователя.

Принимает решения на уровне одной сущности или маленькой подсистемы:

“Игрок нажал кнопку атаки → обновить модель → обновить view”.

Инкапсулирует сценарии: последовательности операций для конкретного объекта/экрана.

Ограничения и требования

Не должен знать про глобальные синглтоны, менеджеры сцен и т.п. (по возможности).

Работает в рамках своей области: одного юнита, одного экрана, одного боя.

Если контроллер начинает управлять “пол-игры” — это сигнал, что нужен *Manager/*System.

Минимальный пример

public class HealthController
{
    private readonly HealthModel _model;
    private readonly HealthView _view;

    public HealthController(HealthModel model, HealthView view)
    {
        _model = model;
        _view = view;
        RefreshView();
    }

    public void ApplyDamage(int amount)
    {
        _model.TakeDamage(amount);
        RefreshView();
    }

    private void RefreshView()
    {
        _view.SetValue(_model.Current, _model.Max);
    }
}

## Manager

Зона ответственности

Управляет множеством однотипных сущностей или ресурса.

Отвечает за lifecycle: создание/удаление/поиск/регистрацию.

Часто выступает как точка доступа: “дай мне юнит по ID”, “дай текущую сцену”, “зарегистрируй врага”.

Ограничения и требования

Не превращаться в “god-object” (всё обо всём).

Иметь чётко очерченную область:

AudioManager — только аудио;

UnitManager — только коллекция юнитов (а не сцены, UI и т.д.).

Не хранить бизнес-логику, которая должна быть в моделях/системах.

Желательно иметь понятный жизненный цикл (инициализация/очистка).

Минимальный пример

public class UnitManager
{
    private readonly Dictionary<int, HealthModel> _units = new();

    public void RegisterUnit(int id, HealthModel model)
    {
        _units[id] = model;
    }

    public void UnregisterUnit(int id)
    {
        _units.Remove(id);
    }

    public HealthModel GetUnit(int id)
    {
        return _units.TryGetValue(id, out var model) ? model : null;
    }
}

## System

Зона ответственности

Инкапсулирует правила и механику определённой доменной области, обычно работающую поверх множества моделей:

система боя,

система эффектов,

система квестов,

система инвентаря.

Работает с моделями как с данными: “тикнуть все эффекты”, “пересчитать ход боя”, “обработать события начала раунда”.

Ограничения и требования

Не должна привязываться к конкретному UI/сценам.

Может иметь зависимость на EventBus, репозитории моделей, менеджеры.

Логика внутри *System должна быть чётко доменной: правила боёв, эффекты, прогрессия — не загрузка ресурсов и не отрисовка.

Минимальный пример

public class DamageSystem
{
    public void ApplyDamage(HealthModel target, int baseDamage)
    {
        // Здесь могла бы быть логика брони, сопротивлений, критов и т.п.
        target.TakeDamage(baseDamage);
    }

    public void ApplyDamageToAll(IEnumerable<HealthModel> targets, int baseDamage)
    {
        foreach (var t in targets)
            ApplyDamage(t, baseDamage);
    }
}

## Service

Зона ответственности

Предоставляет повторно используемую функциональность, не обязательно доменную, но:

работу с инфраструктурой (файлы, сеть, сохранения),

утилитарную логику (рандом, локализация, время),

интеграции (SDK, API и т.п.).

Часто stateful, но не доменная сущность: умеет делать “что-то полезное” по запросу других модулей.

Ограничения и требования

Не должен знать про конкретные View.

Не должен держать в себе бизнес-сущности, как модели игры (только оперировать ими).

У сервиса должен быть узкий, чёткий контракт (“что он делает для мира”).

Минимальный пример

public interface ISaveService
{
    void Save(string slotId, string json);
    string Load(string slotId);
}

public class FileSaveService : ISaveService
{
    private readonly string _basePath;

    public FileSaveService(string basePath)
    {
        _basePath = basePath;
    }

    public void Save(string slotId, string json)
    {
        var path = Path.Combine(_basePath, slotId + ".json");
        File.WriteAllText(path, json);
    }

    public string Load(string slotId)
    {
        var path = Path.Combine(_basePath, slotId + ".json");
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }
}

## Сводка различий

Кратко в виде “если ты пишешь класс и думаешь, как его назвать”:

Model — “истина про данные и правила их изменения”.

View — “как это видит игрок”.

Controller — “связывает Model и View, реагирует на ввод, orchestration внутри одной сущности/экрана”.

Manager — “управляет множеством однотипных вещей, lifecycle, регистрацией”.

System — “реализует механику/правила домена поверх множества моделей”.

Service — “технический или инфраструктурный помощник с чётким контрактом”.

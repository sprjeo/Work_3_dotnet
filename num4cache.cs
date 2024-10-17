using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class CacheItem<T>
{
    public T Value { get; }
    public DateTime Time {  get; }
    public CacheItem(T value, TimeSpan lifetime) //TimeSpan - time interval
    {
        Value = value;
        Time = DateTime.UtcNow.Add(lifetime); //время когда удалится объект
    }

    public bool IsExpired()
    {
        return DateTime.UtcNow >= Time;
    }
}
public class Cache<T>
{
    private readonly TimeSpan _lifetime;
    private readonly UInt32 _maxSize;
    private readonly ConcurrentDictionary<string, CacheItem<T>> _cache; //коллекция пар ключ-значение(доступ может осуществляться несколькими потоками)
    private readonly ReaderWriterLockSlim _lock;


    public Cache(TimeSpan lifetime, uint maxSize)
    {
        _lifetime = lifetime;
        _maxSize = maxSize;
        _cache = new ConcurrentDictionary<string, CacheItem<T>>();
        _lock = new ReaderWriterLockSlim();
    }

    public void Save(string key, T value)
    {
        if (_cache.ContainsKey(key))
        {
            throw new ArgumentException($"The key '{key}' already exist in the cache");

        }
        if (_cache.Count >= _maxSize)
        {
            RemoveOldestItem();
        }
        var item = new CacheItem<T>(value, _lifetime);
        _cache[key] = item;
    }
    public T Get(string key)
    {
        if (!_cache.TryGetValue(key, out var item) || item.IsExpired())
        {
            throw new KeyNotFoundException($"The key '{key}' was not found in the cache or has expired.");

        }
        return item.Value;
    }

    private void RemoveOldestItem()
    {
        _lock.EnterWriteLock(); //вход в блокировку в режиме записи
        try
        {
            var expiredKeys = _cache.Where(keyAndValue => keyAndValue.Value.IsExpired()).Select(keyAndValue => keyAndValue.Key).ToList();
            // фильтруем кэш по условию: лямбда выражение, принимает параметр keyAndValue и проверяет истек ли срок жизни. Если да, добавляем в коллекцию.
            // Потом получаем только ключи. ToList() - преобразует результат в список. Итог строки - список ключей элементов, срок действия которых истёк

            foreach (var key in expiredKeys)
            {
                _cache.TryRemove(key, out _); // _ - плейсхолдер - мы не заинтересованы в значении, оно игнорируется
            }

            if (_cache.Count >= _maxSize)
            {
                var oldestKey = _cache.OrderBy(keyAndValue => keyAndValue.Value.Time).First().Key; //сортировка в порядке возрастания(у нас по времени)
                _cache.TryRemove(oldestKey, out _);

            }

        }
        finally { _lock.ExitWriteLock(); }
    }
}
class Program{
    public static void Main()
    {
        var cache = new Cache<String>(TimeSpan.FromSeconds(5), 3); // life time - 5s, max - 3 elements

        try // создаем значения
        {
            cache.Save("1", "hello");
            cache.Save("2", "how");
            cache.Save("3", "are");
            cache.Save("1", "you"); //ошибка

        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
        }

        try
        {
            Console.WriteLine(cache.Get("1")); //выводим
            Console.WriteLine(cache.Get("2"));
            Console.WriteLine(cache.Get("3"));

            Task.Delay(6000).Wait(); // ждем 6s когда ключи истекут

            Console.WriteLine(cache.Get("1")); //ошибка
        }
        catch (KeyNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
        }
        try
        {
            cache.Save("4", "ododkkspa");
            cache.Save("5", "fffffffff");
            cache.Save("6", "aaaaaaaaaa");

            Console.WriteLine(cache.Get("4"));
            Console.WriteLine(cache.Get("5"));

            Console.WriteLine(cache.Get("2")); //исключение
        }
        catch (KeyNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}


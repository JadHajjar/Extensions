using Extensions;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

using Timer = System.Timers.Timer;

namespace Extensions;

public abstract class PeriodicProcessor<TEntity, TResult> where TResult : ITimestamped
{
	private readonly Timer _timer;
	private readonly HashSet<TEntity> _entities;
	private readonly ConcurrentDictionary<TEntity, TResult> _results;
	private int failedAttempts;
	private DateTime lastFailedAttempt;

	public TimeSpan MaxCacheTime { get; set; } = TimeSpan.FromMinutes(15);
	public int ProcessingPower { get; }

	public event Action ItemsLoaded;

	public PeriodicProcessor(int processingPower, int interval, ConcurrentDictionary<TEntity, TResult> cache)
	{
		ProcessingPower = processingPower;

		_results = cache ?? [];
		_entities = [];
		_timer = new Timer(interval) { AutoReset = false };
		_timer.Elapsed += _timer_Elapsed;
		_timer.Start();
	}

	private async void _timer_Elapsed(object sender, ElapsedEventArgs e)
	{
		await Process();
	}

	public async Task Process()
	{
		if (!CanProcess())
		{
			_timer.Start();
			return;
		}

		var entities = new List<TEntity>();

		try
		{
			lock (this)
			{
				foreach (var entity in _entities)
				{
					if (!_results.TryGetValue(entity, out var result) || DateTime.Now - result.Timestamp > MaxCacheTime)
					{
						entities.Add(entity);
					}
				}

				_entities.Clear();
			}

			if (entities.Count > 0)
			{
				await ProcessInChunks(entities);
			}
		}
		catch { }

		_timer.Start();
	}

	protected virtual bool CanProcess()
	{
		return true;
	}

	protected abstract void CacheItems(ConcurrentDictionary<TEntity, TResult> results);

	protected abstract Task<(ConcurrentDictionary<TEntity, TResult> results, bool failed)> ProcessItems(List<TEntity> entities);

	public void CacheItems()
	{
		CacheItems(_results);
	}

	public void Add(TEntity entity)
	{
		lock (this)
		{
			_entities.Add(entity);
		}
	}

	public void AddRange(IEnumerable<TEntity> entities)
	{
		foreach (var item in entities)
		{
			lock (this)
			{
				_entities.Add(item);
			}
		}
	}

	public async Task<TResult> Get(TEntity entity, bool wait = false)
	{
		try
		{
			if (TryGetEntityFromCache(entity, out var result))
			{
				if (!_entities.Contains(entity) && DateTime.Now - result.Timestamp > MaxCacheTime)
				{
					_entities.Add(entity);
				}

				return result;
			}

			if (!wait)
			{
				_entities.Add(entity);
			}
		}
		catch { } // catch useless potential IndexOutOfRangeException errors

		if (!wait)
		{
			return default;
		}

		var results = await ProcessItems([entity]);

		if (!results.failed)
		{
			foreach (var item in results.results)
			{
				lock (this)
				{
					return _results[item.Key] = item.Value;
				}
			}
		}

		return default;
	}

	protected virtual bool TryGetEntityFromCache(TEntity entity, out TResult result)
	{
		return _results.TryGetValue(entity, out result);
	}

	private async Task ProcessInChunks(IEnumerable<TEntity> mainList)
	{
		var chunks = mainList.Chunk(ProcessingPower);
		var tasks = new List<Task<ConcurrentDictionary<TEntity, TResult>>>();

		foreach (var chunk in chunks)
		{
			tasks.Add(ProcessItemsWrapper(chunk.ToList()));
		}

		await Task.WhenAll(tasks);
	}

	protected async Task<ConcurrentDictionary<TEntity, TResult>> ProcessItemsWrapper(List<TEntity> entities)
	{
		ConcurrentDictionary<TEntity, TResult> results;

		try
		{
			if (failedAttempts > 3 && DateTime.Now - lastFailedAttempt < TimeSpan.FromMinutes(5))
			{
				results = [];
			}
			else
			{
				if (failedAttempts > 3)
				{
					failedAttempts = 0;
				}

				var result = await ProcessItems(entities);

				results = result.results;

				if (result.failed)
				{
					failedAttempts++;
					lastFailedAttempt = DateTime.Now;
				}
			}
		}
		catch
		{
			failedAttempts++;
			lastFailedAttempt = DateTime.Now;
			throw;
		}

		if (results.Count == 0)
		{
			return results;
		}

		foreach (var entity in results)
		{
			lock (this)
			{
				_results[entity.Key] = entity.Value;
			}
		}

		lock (this)
		{
			CacheItems(_results);
		}

		ItemsLoaded?.Invoke();

		return results;
	}

	public void AddToCache(Dictionary<TEntity, TResult> results)
	{
		foreach (var entity in results)
		{
			lock (this)
			{
				_results[entity.Key] = entity.Value;
			}
		}

		lock (this)
		{
			CacheItems(_results);
		}
	}

	public void Clear()
	{
		_results.Clear();

		CacheItems(_results);
	}

	public List<TResult> GetCache()
	{
		lock (this)
		{
			return _results.Values.ToList();
		}
	}
}

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
	private readonly int[] _usagePerMinute = new int[6];
	private readonly Timer _timer;
	private readonly HashSet<TEntity> _entities;
	private readonly int _ratePerMinute;
	private readonly ConcurrentDictionary<TEntity, TResult> _results;
	private int failedAttempts;
	private DateTime lastFailedAttempt;
	private int lastMinute;

	public TimeSpan MaxCacheTime { get; set; } = TimeSpan.FromMinutes(15);
	public int ProcessingPower { get; }

	public event Action ItemsLoaded;

	public PeriodicProcessor(int processingPower, int interval, int ratePerMinute, ConcurrentDictionary<TEntity, TResult> cache)
	{
		ProcessingPower = processingPower;
		_ratePerMinute = ratePerMinute;
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

			AddUsage(0);

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

				AddUsage(entities.Count);
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
		return _ratePerMinute == 0 || _usagePerMinute.Sum() < _ratePerMinute;
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
			lock (this)
			{
				foreach (var item in results.results)
				{
					return _results[item.Key] = item.Value;
				}

				AddUsage(results.results.Count);
			}
		}

		return default;
	}

	public async Task Refresh(TEntity entity)
	{
		var results = await ProcessItems([entity]);

		if (!results.failed)
		{
			foreach (var item in results.results)
			{
				lock (this)
				{
					_results[item.Key] = item.Value;
				}
			}
		}
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

			AddUsage(_results.Count);
		}

		ItemsLoaded?.Invoke();

		return results;
	}

	private void AddUsage(int count)
	{
		if (_ratePerMinute == 0)
		{
			return;
		}

		var second = DateTime.Now.Second / 10;

		if (lastMinute != second)
		{
			lastMinute = second;
			_usagePerMinute[second] = count;
		}
		else
		{
			_usagePerMinute[second] += count;
		}
	}

	public bool IsQueued(TEntity entity)
	{
		lock (this)
		{
			return _entities.Contains(entity);
		}
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

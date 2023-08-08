using Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Extensions
{
	public abstract class PeriodicProcessor<TEntity, TResult> where TResult : ITimestamped
	{
		private readonly Timer _timer;
		private readonly HashSet<TEntity> _entities;
		private readonly Dictionary<TEntity, TResult> _results;
		private bool processing;

		public TimeSpan MaxCacheTime { get; set; } = TimeSpan.FromMinutes(15);
		public int ProcessingPower { get; }

		public event Action ItemsLoaded;

		public PeriodicProcessor(int processingPower, int interval, Dictionary<TEntity, TResult> cache)
		{
			ProcessingPower = processingPower;

			_results = cache ?? new Dictionary<TEntity, TResult>();
			_entities = new HashSet<TEntity>(new List<TEntity>());
			_timer = new Timer(interval) { AutoReset = false };
			_timer.Elapsed += _timer_Elapsed;
			_timer.Start();
		}

		private void _timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Process();
		}

		private async void Process()
		{
			if (!CanProcess())
			{
				_timer.Start();
				return;
			}

			var entities = new List<TEntity>();

			try
			{
				processing = true;

				//lock (this)
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

				processing = false;
			}
			catch { }

			_timer.Start();
		}

		protected virtual bool CanProcess()
		{
			return true;
		}

		protected abstract void CacheItems(Dictionary<TEntity, TResult> results);

		protected abstract Task<Dictionary<TEntity, TResult>> ProcessItems(List<TEntity> entities);

		public void Add(TEntity entity)
		{
			//lock (this)
			{
				_entities.Add(entity);
			}
		}

		public void AddRange(IEnumerable<TEntity> entities)
		{
			//lock (this)
			{
				foreach (var item in entities)
				{
					_entities.Add(item);
				}
			}
		}

		public void Run()
		{
			if (processing)
			{
				return;
			}

			_timer.Stop();

			new BackgroundAction(Process).Run();
		}

		public async Task<TResult> Get(TEntity entity, bool wait = false)
		{
			//lock (this)
			{
				try
				{
					if (_results.TryGetValue(entity, out var result))
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
			}

			var results = await ProcessItems(new List<TEntity> { entity });

			//lock (this)
			{
				foreach (var item in results)
				{
					return _results[item.Key] = item.Value;
				}
			}

			return default;
		}

		private async Task ProcessInChunks(IEnumerable<TEntity> mainList)
		{
			var chunks = mainList.Chunk(ProcessingPower);
			var tasks = new List<Task<Dictionary<TEntity, TResult>>>();

			foreach (var chunk in chunks)
			{
				tasks.Add(ProcessItemsWrapper(chunk.ToList()));
			}

			await Task.WhenAll(tasks);
		}

		protected async Task<Dictionary<TEntity, TResult>> ProcessItemsWrapper(List<TEntity> entities)
		{
			var results = await ProcessItems(entities);

			//lock (this)
			{
				foreach (var entity in results)
				{
					_results[entity.Key] = entity.Value;
				}

				CacheItems(_results);
			}

			ItemsLoaded?.Invoke();

			return results;
		}

		public void AddToCache(Dictionary<TEntity, TResult> results)
		{
			//lock (this)
			{
				foreach (var entity in results)
				{
					_results[entity.Key] = entity.Value;
				}

				CacheItems(_results);
			}
		}

		public void Clear()
		{
			_results.Clear();
		}

		public Dictionary<TEntity, TResult> GetCache()
		{
			return _results;
		}
	}
}

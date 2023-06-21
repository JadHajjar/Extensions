using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Extensions
{
	public class ServiceCollection
	{
		public ServiceCollection()
		{
			_singletons = new ConcurrentDictionary<Type, object>();
			_singletonInitializers = new ConcurrentDictionary<Type, Func<ServiceCollection, object>>();
			_transient = new ConcurrentDictionary<Type, Func<ServiceCollection, object>>();
		}

		private readonly ConcurrentDictionary<Type, object> _singletons;
		private readonly ConcurrentDictionary<Type, Func<ServiceCollection, object>> _singletonInitializers;
		private readonly ConcurrentDictionary<Type, Func<ServiceCollection, object>> _transient;

		public void AddSingleton<T>(Func<ServiceCollection, T> initializer = null) where T : class
		{
			if (initializer == null)
			{
				_singletonInitializers.TryAdd(typeof(T), s => CreateInstance(typeof(T)));
			}

			_singletonInitializers.TryAdd(typeof(T), s => initializer(s));
		}

		public void AddSingleton<T, T2>(Func<ServiceCollection, T2> initializer = null) where T2 : class
		{
			if (initializer == null)
			{
				_singletonInitializers.TryAdd(typeof(T), s => CreateInstance(typeof(T2)));
			}

			_singletonInitializers.TryAdd(typeof(T), s => initializer(s));
		}

		public void AddTransient<T>(Func<ServiceCollection, T> initializer = null) where T : class
		{
			if (initializer == null)
			{
				_transient.TryAdd(typeof(T), s => CreateInstance(typeof(T)));
			}

			_transient.TryAdd(typeof(T), s => initializer(s));
		}

		public void AddTransient<T, T2>(Func<ServiceCollection, T2> initializer = null) where T2 : class
		{
			if (initializer == null)
			{
				_transient.TryAdd(typeof(T), s => CreateInstance(typeof(T2)));
			}

			_transient.TryAdd(typeof(T), s => initializer(s));
		}

		public void RecycleSingleton<T>() where T : class
		{
			_singletons.TryRemove(typeof(T), out _);

			_singletons.TryAdd(typeof(T), _singletonInitializers[typeof(T)](this));
		}

		private object CreateInstance(Type serviceType)
		{
			var constructor = serviceType.GetConstructors().FirstOrDefault();

			if (constructor != null)
			{
				return constructor.Invoke(constructor.GetParameters().Select(x => GetService(x.ParameterType)).ToArray());
			}

			return Activator.CreateInstance(serviceType);
		}

		public T GetService<T>()
		{
			return (T)GetService(typeof(T));
		}

		public object GetService(Type type)
		{
			if (_singletons.TryGetValue(type, out var singleton))
			{
				return singleton;
			}

			if (_singletonInitializers.TryGetValue(type, out var singletonInitializer))
			{
				_singletons.TryAdd(type, _singletonInitializers[type](this));

				return _singletons[type];
			}

			if (_transient.TryGetValue(type, out var transient))
			{
				return transient(this);
			}

			return null;
		}
	}

	public interface IService
	{
		void Dispose();
	}
}

using System;
using System.Collections.Generic;
namespace Internal
{
	public static class Locator
	{
		private static Dictionary<Type, object> map = new Dictionary<Type, object>();

		public static void Register(Type   type, object @object) => map[type] = @object;
		public static void Unregister(Type type) => map[type] = null;

		public static T GetObject<T>() where T : class
		{
			if (!map.ContainsKey(typeof(T))) return null;
			return (T)map[typeof(T)];
		}
	}
}

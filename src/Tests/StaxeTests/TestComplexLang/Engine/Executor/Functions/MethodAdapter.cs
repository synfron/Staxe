using System;
using System.Linq;
using System.Reflection;

namespace StaxeTests.TestComplexLang.Engine.Executor.Functions
{
	public class MethodAdapter
	{
		private readonly Type _hostType;
		private readonly object _host;
		private readonly string _methodName;

		public MethodAdapter(Type hostType, object host, string methodName)
		{
			_hostType = hostType;
			_host = host;
			_methodName = methodName;
		}

		public object DynamicInvoke(object[] args)
		{
			Type[] types = args.Select(arg => arg.GetType()).ToArray();
			MethodInfo methdodInfo = _hostType.GetMethod(_methodName, types);
			return methdodInfo.Invoke(_host, args);
		}
	}
}

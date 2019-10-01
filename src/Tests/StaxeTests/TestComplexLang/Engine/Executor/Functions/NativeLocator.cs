using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Executor.Values;
using Synfron.Staxe.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StaxeTests.TestComplexLang.Engine.Executor.Functions
{
	public static class NativeLocator
	{
		private static readonly string[] _typeWhiteList = new[] {
			"System.Math"
		};

		public static IReadOnlyList<string> TypeWhiteList => _typeWhiteList.ToList();

		public static DeclaredValuePointer<G> GetNativePointer<G>(ExecutionState<G> executionState, G groupState, string location, string name, PointerOperation operation, IValueProvider<G> valueProvider) where G : IGroupState<G>, new()
		{
			return new DeclaredValuePointer<G>(location, valueProvider.GetAsValue(GetNative<G>(location, name)))
			{
				Identifier = name
			};
		}

		public static object GetNative<G>(string locator, string name) where G : IGroupState<G>, new()
		{
			if (!_typeWhiteList.Contains(locator)) throw new EngineRuntimeException($"'{locator}' not found");

			Type type = Type.GetType(locator);

			MemberInfo member = type.GetMember(name).First();

			object value;
			switch (member)
			{
				case FieldInfo fieldInfo:
					value = fieldInfo.GetValue(null);
					break;
				case PropertyInfo propertyInfo:
					value = propertyInfo.GetValue(null);
					break;
				default:
					value = new MethodAdapter(type, null, name);
					break;
			}

			return value;
		}
	}
}

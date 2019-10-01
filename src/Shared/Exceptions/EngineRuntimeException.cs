using System;

namespace Synfron.Staxe.Shared.Exceptions
{
	public class EngineRuntimeException : EngineException
	{

		public EngineRuntimeException(string message, int? position = null, Exception innerException = null) : base(message, position, innerException)
		{
		}
	}
}

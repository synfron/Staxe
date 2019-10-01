using System;

namespace Synfron.Staxe.Shared.Exceptions
{

	public class LanaguageSyntaxException : EngineException
	{
		public LanaguageSyntaxException(string message, int? position, Exception innerException = null) : base(message, position, innerException)
		{
		}
	}
}

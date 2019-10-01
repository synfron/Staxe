using System;

namespace Synfron.Staxe.Shared.Exceptions
{
	public abstract class EngineException : Exception
	{

		public int? Position
		{
			get;
			private set;
		}

		public EngineException(string message, int? position, Exception innerException) : base(message, innerException)
		{
			Position = position;
		}
	}
}

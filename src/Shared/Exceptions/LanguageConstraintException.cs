using System;

namespace Synfron.Staxe.Shared.Exceptions
{
	public class LanguageConstraintException : EngineException
	{

		public LanguageConstraintException(string message, int? position, Exception innerException = null) : base(message, position, innerException)
		{
		}
	}
}

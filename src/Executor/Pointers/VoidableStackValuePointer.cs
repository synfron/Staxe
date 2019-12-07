using Synfron.Staxe.Executor.Values;
using Synfron.Staxe.Shared.Exceptions;

namespace Synfron.Staxe.Executor.Pointers
{
	public class VoidableStackValuePointer<G> : StackValuePointer<G> where G : IGroupState<G>, new()
	{

		public VoidableStackValuePointer()
		{
		}

		public override IValuable<G> Value
		{
			get
			{
				if (IsVoid) ThrowUndeclared();
				return base.Value;
			}
			set
			{
				if (value != null) IsVoid = false;
				base.Value = value;
			}
		}


		public bool IsVoid
		{
			get;
			set;
		}

		protected virtual void ThrowUndeclared()
		{
			string errorMessage = "Undeclared variable";
			if (Identifier != null)
			{
				string variableName = "'{Identifier}'";
				errorMessage += $" {variableName}";
			}
			throw new EngineRuntimeException(errorMessage);
		}
	}
}

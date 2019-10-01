using Synfron.Staxe.Executor.Instructions.Flags;
using Synfron.Staxe.Executor.Values;
using Synfron.Staxe.Shared.Exceptions;

namespace Synfron.Staxe.Executor.Pointers
{
	public class DeclaredValuePointer<G> : ValuePointer<G> where G : IGroupState<G>, new()
	{
		private Modifiers _modifiers;
		private bool _readRestricted;
		private bool _writeRestricted;


		public DeclaredValuePointer(string location)
		{
			Location = location;
			IsDeclared = true;
		}

		public DeclaredValuePointer(string location, IValuable<G> value)
		{
			Location = location;
			base.Value = value;
			IsDeclared = true;
		}

		public Modifiers Modifiers
		{
			get
			{
				return _modifiers;
			}
			set
			{
				_modifiers = value;
				_readRestricted = Modifiers.Contains(Modifiers.ReadRestricted);
				_writeRestricted = Modifiers.Contains(Modifiers.WriteRestricted);
			}
		}

		public override IValuable<G> Value
		{
			get
			{
				if (!IsDeclared) ThrowUndeclared();
				if (_readRestricted) ThrowReadRestricted();

				return base.Value;
			}
			set
			{
				if (!IsDeclared) ThrowUndeclared();
				if (_writeRestricted) ThrowWriteRestricted();
				base.Value = value;
			}
		}


		public bool IsDeclared
		{
			get;
			set;
		}

		public bool IsDynamic
		{
			get;
			set;
		}

		public string Location
		{
			get;
			private set;
		}

		protected virtual void ThrowUndeclared()
		{
			string errorMessage = "Undeclared variable";
			if (Identifier != null)
			{
				string variableName = "'";
				if (Location != null && Location != "context" && Location != "local")
				{
					variableName += $"{Location}.";
				}
				variableName += $"{Identifier}'";
				errorMessage += $" {variableName}";
			}
			throw new EngineRuntimeException(errorMessage);
		}

		private void ThrowReadRestricted()
		{
			string errorMessage = "Read restricted variable";
			if (Identifier != null)
			{
				string identifierName = "'";
				if (Location != null && Location != "context" && Location != "local")
				{
					identifierName += $"{Location}.";
				}
				identifierName += $"{Identifier}'";
				errorMessage += $" {identifierName}";
			}
			throw new EngineRuntimeException(errorMessage);
		}

		private void ThrowWriteRestricted()
		{
			string errorMessage = "Write restricted variable";
			if (Identifier != null)
			{
				string identifierName = "'";
				if (Location != null && Location != "context" && Location != "local")
				{
					identifierName += $"{Location}.";
				}
				identifierName += $"{Identifier}'";
				errorMessage += $" {identifierName}";
			}
			throw new EngineRuntimeException(errorMessage);
		}

		public DeclaredValuePointer<G> Clone(bool includeModifiers)
		{
			return new DeclaredValuePointer<G>(Location)
			{
				Value = Modifiers.Contains(Modifiers.Component) ? base.Value : null,
				IsDeclared = IsDeclared,
				IsDynamic = IsDynamic,
				Identifier = Identifier,
				Modifiers = includeModifiers ? Modifiers : Modifiers.None
			};
		}

		public IValuable<G> ForceGetValue()
		{
			return base.Value;
		}

		public void ForceSetValue(IValuable<G> value)
		{
			base.Value = value;
		}
	}
}

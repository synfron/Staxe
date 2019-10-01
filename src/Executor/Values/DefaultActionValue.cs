using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Shared.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace Synfron.Staxe.Executor.Values
{
	public sealed class DefaultActionValue<G> : IActionValue<G> where G : IGroupState<G>, new()
	{

		public DefaultActionValue(G groupState, int location)
		{
			GroupState = groupState;
			Location = location;
		}

		public List<StackValuePointer<G>> InitStackPointers
		{
			get;
			set;
		} = new List<StackValuePointer<G>>();

		public ValueType ValueType => ValueType.Action;

		public IValuable<G> Add(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case ICollectionValue<G> collectionValue:
					return Add(collectionValue, valueProvider);
				default:
					throw new EngineRuntimeException("Invalid values for addition");
			}
		}

		private IValuable<G> Add(ICollectionValue<G> collectionValue, IValueProvider<G> valueProvider)
		{
			IValuable<G> newValuable = null;
			switch (collectionValue)
			{
				case DefaultCollectionValue<G> listValue when !listValue.IsMap:
					newValuable = valueProvider.GetCollection(new[] { new ValuePointer<G> { Value = this } }.Concat(collectionValue.GetEntries().Select(entry => entry.Value)), null);
					break;
				default:
					throw new EngineRuntimeException("Invalid values for addition");
			}
			return newValuable;
		}

		public IValuable<G> And(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for and");
		}

		public IValuable<G> DivideBy(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for division");
		}

		public IValuable<G> IsEqualTo(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Equals(value));
		}

		public IValuable<G> IsGreaterThan(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for greater than");
		}

		public IValuable<G> IsGreaterThanOrEqualTo(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for greater than or equals");
		}

		public IValuable<G> IsLessThan(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for less than");
		}

		public IValuable<G> IsLessThanOrEqualTo(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for less than or equals");
		}

		public IValuable<G> IsNotEqualTo(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(!Equals(value));
		}

		public IValuable<G> LeftShift(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for left shift");
		}

		public IValuable<G> RightShift(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for right shift");
		}

		public IValuable<G> Minus(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for subtraction");
		}

		public IValuable<G> MultiplyBy(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for multiplication");
		}

		public IValuable<G> Not(IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid value for negation");
		}

		public IValuable<G> Or(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for or");
		}

		public IValuable<G> Remainder(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for remainder");
		}

		public bool IsTrue()
		{
			throw new EngineRuntimeException("Value is not a boolean");
		}

		public ValuePointer<G> Get(IValuable<G> value, bool createNonExistent, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Value is not keyed");
		}

		public ValuePointer<G> GetAt(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Value is not indexed");
		}

		public G GroupState
		{
			get;
			private set;
		}

		public int Location
		{
			get;
			private set;
		}

		public int Size
		{
			get
			{
				throw new EngineRuntimeException("Value does not provide a size");
			}
		}

		public string Identifier
		{
			get;
			set;
		}

		public void Execute(ExecutionState<G> executionState, IValueProvider<G> valueProvider)
		{
			if ((GroupState.ActionOverrides?.Count ?? 0) > 0 && GroupState.ActionOverrides.TryGetValue(Location, out IValuable<G> overrideActionValue) && overrideActionValue != this)
			{
				overrideActionValue.Execute(executionState, valueProvider);
			}
			else
			{
				Frame<G> frame = new Frame<G>()
				{
					PreviousInstructionIndex = Location - 1,
					GroupState = GroupState
				};
				int count = InitStackPointers.Count;
				int startIndex = frame.StackPointers.Reserve(count);
				for (int i = 0; i < count; i++)
				{
					frame.StackPointers.UnsafeSet(startIndex + i, InitStackPointers[i]);
				}
				executionState.Frames.Add(frame);
				executionState.Sync(frame);
			}
		}

		public IActionValue<G> Clone(G newGroupState = default, int? location = null)
		{
			return new DefaultActionValue<G>(!EqualityComparer<G>.Default.Equals(newGroupState, default) ? newGroupState : GroupState, location ?? Location)
			{
				InitStackPointers = InitStackPointers.ToList()
			};
		}
	}
}

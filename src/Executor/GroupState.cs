using Synfron.Staxe.Executor.Instructions.Flags;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Executor.Values;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synfron.Staxe.Executor
{
	public class GroupState : GroupState<GroupState>
	{
	}

	public class GroupState<G> : IGroupState<G> where G : GroupState<G>, new()
	{

		public List<G> Dependencies
		{
			get;
			private set;
		} = new List<G>();


		public List<DeclaredValuePointer<G>> GroupPointers
		{
			get;
			private set;
		} = new List<DeclaredValuePointer<G>>();

		public Group<G> Group
		{
			get;
			set;
		} = new Group<G>();

		public Dictionary<string, int> PointerMap
		{
			get;
			private set;
		} = new Dictionary<string, int>(StringComparer.Ordinal);

		public Dictionary<int, IValuable<G>> ActionOverrides
		{
			get;
			private set;
		} = new Dictionary<int, IValuable<G>>();

		public Modifiers Modifiers
		{
			get;
			set;
		}

		public virtual G Clone(Copy copyOptions)
		{
			G newGroupState = (G)Activator.CreateInstance(typeof(G));
			newGroupState.Group = Group;
			bool includeModifiers = copyOptions.Contains(Copy.Modifiers);
			newGroupState.Modifiers = includeModifiers ? Modifiers : Modifiers.None;

			Dictionary<G, G> oldToNewState = new Dictionary<G, G>()
			{
				{ (G)this, newGroupState }
			};

			Dictionary<DeclaredValuePointer<G>, DeclaredValuePointer<G>> dependencyPointerMap = new Dictionary<DeclaredValuePointer<G>, DeclaredValuePointer<G>>();

			foreach (G dependency in Dependencies)
			{
				G newDependency = dependency;
				if (dependency.Modifiers.Contains(Modifiers.Component) && !dependency.Modifiers.Contains(Modifiers.Static))
				{
					newDependency = dependency.Clone(copyOptions);
					oldToNewState[dependency] = newDependency;

					for (int pointerIndex = 0; pointerIndex < dependency.GroupPointers.Count; pointerIndex++)
					{
						dependencyPointerMap[dependency.GroupPointers[pointerIndex]] = newDependency.GroupPointers[pointerIndex];
					}
				}
				newGroupState.Dependencies.Add(newDependency);
			}

			newGroupState.GroupPointers = GroupPointers.Select(pointer =>
			{
				if (!dependencyPointerMap.TryGetValue(pointer, out DeclaredValuePointer<G> newPointer))
				{
					newPointer = pointer.Clone(includeModifiers);
					switch (pointer.ForceGetValue())
					{
						case IActionValue<G> actionValue when pointer.Modifiers.Contains(Modifiers.Component):
							G actionGroupState;
							if (oldToNewState.TryGetValue(actionValue.GroupState, out actionGroupState))
							{
								newPointer.ForceSetValue(actionValue.Clone(actionGroupState));
							}
							break;
					}
				}
				return newPointer;
			}).ToList();

			newGroupState.PointerMap = PointerMap.ToDictionary(entry => entry.Key, entry => entry.Value);

			foreach (KeyValuePair<int, IValuable<G>> pair in ActionOverrides)
			{
				switch (pair.Value)
				{
					case IActionValue<G> actionValue:
						G actionGroupState;
						if (oldToNewState.TryGetValue(actionValue.GroupState, out actionGroupState))
						{
							newGroupState.ActionOverrides.Add(pair.Key, actionValue.Clone(actionGroupState));
						}
						else
						{
							newGroupState.ActionOverrides.Add(pair.Key, pair.Value);
						}
						break;
					default:
						newGroupState.ActionOverrides.Add(pair.Key, pair.Value);
						break;
				}
			}
			return newGroupState;
		}

		public void Merge(G otherGroupState, IValueProvider<G> valueProvider, GroupMerge options)
		{
			int pointersOffset = GroupPointers.Count;
			bool maoPointers = options.Contains(GroupMerge.MapPointers);
			bool reverseMapInstructions = options.Contains(GroupMerge.ReverseMapInstructions);
			bool overridePointers = options.Contains(GroupMerge.OverridePointers);
			bool asDependency = options.Contains(GroupMerge.AsDependency);
			if (maoPointers)
			{
				GroupPointers.AddRange(otherGroupState.GroupPointers);
				foreach (KeyValuePair<string, int> pair in otherGroupState.PointerMap)
				{
					if (overridePointers)
					{
						PointerMap[pair.Key] = pair.Value + pointersOffset;
					}
					else
					{
						if (!PointerMap.ContainsKey(pair.Key))
						{
							PointerMap[pair.Key] = pair.Value + pointersOffset;
						}
					}
				}
			}
			if (reverseMapInstructions)
			{
				foreach (KeyValuePair<string, int> pair in otherGroupState.Group.InstructionMap)
				{
					if (Group.InstructionMap.TryGetValue(pair.Key, out int location))
					{
						otherGroupState.ActionOverrides[pair.Value] = valueProvider.GetAction((G)this, location);
					}
				}
			}
			if (asDependency)
			{
				Dependencies.Add(otherGroupState);
			}
		}
	}

}

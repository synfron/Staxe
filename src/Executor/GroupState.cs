using Synfron.Staxe.Executor.Instructions.Flags;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Executor.Values;
using Synfron.Staxe.Shared.Collections;
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

		public virtual G Clone(Copy copyOptions, Dictionary<object, object> entityMap = null)
		{
			G newGroupState = (G)Activator.CreateInstance(typeof(G));
			newGroupState.Group = Group;
			bool includeModifiers = copyOptions.Contains(Copy.Modifiers);
			newGroupState.Modifiers = includeModifiers ? Modifiers : Modifiers.None;

			entityMap = entityMap ?? new Dictionary<object, object>();
			entityMap[this] = newGroupState;

			Dictionary<DeclaredValuePointer<G>, DeclaredValuePointer<G>> dependencyPointerMap = new Dictionary<DeclaredValuePointer<G>, DeclaredValuePointer<G>>();

			foreach (G dependency in Dependencies)
			{
				G newDependency = dependency;
				if (dependency.Modifiers.Contains(Modifiers.Component) && !dependency.Modifiers.Contains(Modifiers.Static))
				{
					newDependency = entityMap.GetValueOrDefault(dependency) as G ?? dependency.Clone(copyOptions, entityMap);

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
					if (entityMap.GetValueOrDefault(pointer) is DeclaredValuePointer<G> storedPointer)
					{
						newPointer = storedPointer;
					}
					else
					{
						newPointer = pointer.Clone(includeModifiers);
						entityMap[pointer] = newPointer;
					}
					switch (pointer.ForceGetValue())
					{
						case IActionValue<G> actionValue when pointer.Modifiers.Contains(Modifiers.Component):
							if (entityMap.GetValueOrDefault(actionValue.GroupState) is G actionGroupState)
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
						if (entityMap.GetValueOrDefault(actionValue.GroupState) is G actionGroupState)
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
			bool asDependency = options.Contains(GroupMerge.AsComponentDependency);
			bool mapPointers = options.Contains(GroupMerge.MapPointers);
			bool reverseMapInstructions = options.Contains(GroupMerge.ReverseMapInstructions);
			bool overridePointers = options.Contains(GroupMerge.OverrideDependencyPointers);
			bool componentDepdendencies = options.Contains(GroupMerge.Dependencies);
			bool cloneDepdendencies = options.Contains(GroupMerge.CloneNewDependencies);

			Dictionary<object, object> entityMap = new Dictionary<object, object>()
			{
				{ this, this }
			};
			if (asDependency)
			{
				otherGroupState = cloneDepdendencies ? entityMap.GetValueOrDefault(otherGroupState) as G ?? otherGroupState.Clone(Copy.Modifiers, entityMap) : otherGroupState;
				otherGroupState.Modifiers |= Modifiers.Component;
				Dependencies.Add(otherGroupState);
			}
			if (mapPointers)
			{
				foreach (KeyValuePair<string, int> pair in otherGroupState.PointerMap)
				{
					if (!PointerMap.ContainsKey(pair.Key))
					{
						PointerMap[pair.Key] = GroupPointers.Count;
						GroupPointers.Add(otherGroupState.GroupPointers[pair.Value]);
					}
				}
			}
			if (reverseMapInstructions) ReverseMapInstructions(otherGroupState, valueProvider);
			if (overridePointers) OverrideDepdencyPointers(otherGroupState);
			if (componentDepdendencies)
			{
				foreach (G otherDependency in otherGroupState.Dependencies)
				{
					if (otherDependency.Modifiers.Contains(Modifiers.Component))
					{
						Dependencies.Add(otherDependency);
						if (reverseMapInstructions) ReverseMapInstructions(otherDependency, valueProvider);
						if (overridePointers) OverrideDepdencyPointers(otherDependency);
					}
				}
			}
		}

		private void OverrideDepdencyPointers(G dependency)
		{
			foreach (KeyValuePair<string, int> pair in PointerMap)
			{
				if (dependency.PointerMap.TryGetValue(pair.Key, out int location))
				{
					dependency.GroupPointers[location] = GroupPointers[pair.Value];
				}
			}
		}

		private void ReverseMapInstructions(G dependency, IValueProvider<G> valueProvider)
		{
			foreach (KeyValuePair<string, int> pair in dependency.Group.InstructionMap)
			{
				if (Group.InstructionMap.TryGetValue(pair.Key, out int location))
				{
					dependency.ActionOverrides[pair.Value] = valueProvider.GetAction((G)this, location);
				}
			}
		}
	}

}

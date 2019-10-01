# Executor

Extensible stack based virtual machine

## Basic Usage

The code below executes instructions that compute the result of 20 multiplied by 20

```csharp
GroupState groupState = new GroupState();
groupState.Group.Instructions = new[] {
  InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { 20 }),
  InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR),
  InstructionProvider<GroupState>.GetInstruction(InstructionCode.RMultiply)
};
InstructionExecutor<GroupState> executor = new InstructionExecutor<GroupState>();
executor.Execute(new ExecutionState<GroupState>(groupState)); 
```
Here's a breakdown of some of the classes used above:<br />
**[GroupState](https://github.com/synfron/Staxe/blob/master/src/Executor/IGroupState.cs)** - Represents a unit of instructions and value pointers. One of the main ways OOP can be achieves in the engine.<br />
**[InstructionCode](https://github.com/synfron/Staxe/blob/master/src/Executor/Instructions/InstructionCode.cs)** - Enum representing the opcode of the instruction. [See all Instruction Codes and usage.](https://synfron.github.io/Staxe/Executor/Instruction-Codes.html)<br />
**[InstructionProvider](https://github.com/synfron/Staxe/blob/master/src/Executor/Instructions/InstructionProvider.cs)** - Factory for creating Instructions for a specified InstructionCode and payload that the engine can execute.<br />
**[ExecutionState](https://github.com/synfron/Staxe/blob/master/src/Executor/ExecutionState.cs)** - The execution context, providing access to the stack, register, and context specific interrupts.<br />
**[InstructionExecutor](https://github.com/synfron/Staxe/blob/master/src/Executor/InstructionExecutor.cs)** - Executes instructions and provides access to hooks in order to extend or interop with the engine.

## Modifying Value and Operating Handling

Although the engine is primarily built for dynamic typing, all values are converted to a typed representation so that operations can be performed on them. The classes located in [Synfron.Staxe.Executor.Values](https://github.com/synfron/Staxe/tree/master/src/Executor/Values) provide the engine's default value handling functionality. Implementations of [IValueProvider](https://github.com/synfron/Staxe/blob/master/src/Executor/Values/IValueProvider.cs) can be registered with the [InstructionExecutor](https://github.com/synfron/Staxe/blob/master/src/Executor/InstructionExecutor.cs). This is used by the engine to create [IValuables](https://github.com/synfron/Staxe/blob/master/src/Executor/Values/IValuable.cs) depending on the type of the value. All operations between values (addition, multiplication, etc.) are implemented by the value type specific IValuable implementation.

## Debugger Hooks

Debuggers can interop with the engine using [Interrupts](https://github.com/synfron/Staxe/tree/master/src/Executor/Interrupts). Based on the condition used by the built-in or custom Interrupts, if an [Instruction](https://github.com/synfron/Staxe/blob/master/src/Executor/Instructions/Instruction.cs) is marked as interruptable, [InterruptHandlers](https://github.com/synfron/Staxe/blob/master/src/Executor/Interrupts/InterruptedEventArgs.cs) set on the [InstructionExecutor](https://github.com/synfron/Staxe/blob/master/src/Executor/InstructionExecutor.cs) will be called before the Instruction is executed.
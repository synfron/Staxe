using Synfron.Staxe.Executor.Instructions;
using Synfron.Staxe.Generator;
using System.Collections.Generic;
using System.IO;
using Xunit;
using G = Synfron.Staxe.Executor.GroupState;

namespace GeneratorTests
{
	public class BinaryConverterTests
	{
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void BinaryConverter_ToBinary_ToInstructions(bool withDebugInto)
		{
			InstructionProvider<G> instructionProvider = new InstructionProvider<G>();
			instructionProvider.SpecialInstructionMap.Add("SPECIAL", (executor, executionState, payload, stackRegister, stackPointers) => { });
			List<Instruction<G>> input = new List<Instruction<G>>
			{
				instructionProvider.GetSpecialInstruction(new object[] { "SPECIAL", "other" }, 20, true),
				InstructionProvider<G>.GetInstruction(InstructionCode.J, new object[] { 8 }, 20, true),
				InstructionProvider<G>.GetInstruction(InstructionCode.RAR, new object[] { "name" }, 20, false),
				InstructionProvider<G>.GetInstruction(InstructionCode.RGAR, new object[] {null }, 20, true),
				InstructionProvider<G>.GetInstruction(InstructionCode.RGDPR, new object[] { "loc", null }, default, true),
				InstructionProvider<G>.GetInstruction(InstructionCode.DPR, new object[] { "loc", "name" }, 20, true),
				InstructionProvider<G>.GetInstruction(InstructionCode.RVK, new object[] { true }, 20, true),
				InstructionProvider<G>.GetInstruction(InstructionCode.DPDP, new object[] { "loc", "name", "loc", "name" }, 20, true),
				InstructionProvider<G>.GetInstruction(InstructionCode.VDP, new object[] { true, "loc", "name" }, 20, true),
				InstructionProvider<G>.GetInstruction(InstructionCode.VGP, new object[] { 20L, 30 }, 20, true),
				InstructionProvider<G>.GetInstruction(InstructionCode.SPSP, new object[] { 20, 30 }, 20, true),
				InstructionProvider<G>.GetInstruction(InstructionCode.SPDP, new object[] { 5, "loc", "name" }, 20, true),
				InstructionProvider<G>.GetInstruction(InstructionCode.DPGP, new object[] { "loc", "name", 5 }, 20, true),
				InstructionProvider<G>.GetInstruction(InstructionCode.SPCSP, new object[] { 20, "name" }, 20, true),
				InstructionProvider<G>.GetInstruction(InstructionCode.CPR, new object[]
				{
					-1, 4, InstructionCode.DPR, "loc", "name", InstructionCode.GPR, 5, InstructionCode.SPR, 10, InstructionCode.VR, 10D
				}, 20, true),
				InstructionProvider<G>.GetInstruction(InstructionCode.RCP, new object[]
				{
					6, InstructionCode.DPR, "loc", "name", InstructionCode.GPR, 5, InstructionCode.SPR, 10,
					InstructionCode.CDP, "loc", "name", InstructionCode.CGP, "name", InstructionCode.CSP, "name"
				}, 20, true)
			};

			IList<Instruction<G>> results = null;
			byte[] buffer = null;
			BinaryConverter<G> sut = new BinaryConverter<G>(instructionProvider) { WriteDebugInfo = withDebugInto };
			using (MemoryStream stream = new MemoryStream())
			{
				sut.ToBinary(stream, input);
				buffer = stream.ToArray();
			}
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				results = sut.ToInstructions(stream);
			}

			Assert.Equal(input, results, new InstructionComparer<G>() { WithDebugInfo = withDebugInto });
		}
	}
}

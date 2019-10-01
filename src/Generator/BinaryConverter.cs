using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Instructions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Synfron.Staxe.Generator
{
	public class BinaryConverter<G> where G : IGroupState<G>, new()
	{

		private const int BinaryVersion = 1;
		private readonly InstructionProvider<G> _instructionProvider;

		public BinaryConverter(InstructionProvider<G> instructionProvider)
		{
			_instructionProvider = instructionProvider;
		}

		public bool WriteDebugInfo
		{
			get;
			set;
		}

		public IList<Instruction<G>> ToInstructions(Stream stream)
		{
			using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8, true))
			{
				if (binaryReader.ReadInt32() != BinaryVersion)
				{
					throw new InvalidDataException("Unsupported binary version");
				}
				bool hasDebugInfo = binaryReader.ReadBoolean();
				IList<Instruction<G>> instructions = new Instruction<G>[binaryReader.ReadInt32()];
				for (int instructionIndex = 0; instructionIndex < instructions.Count; instructionIndex++)
				{
					instructions[instructionIndex] = ReadInstruction(binaryReader, hasDebugInfo);
				}
				return instructions;
			}
		}

		public void ToBinary(Stream stream, IList<Instruction<G>> instructions)
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8, true))
			{
				binaryWriter.Write(1);
				binaryWriter.Write(WriteDebugInfo);
				binaryWriter.Write(instructions.Count);
				foreach (Instruction<G> instruction in instructions)
				{
					WriteInstruction(binaryWriter, instruction);
				}
			}
		}

		private void WriteInstruction(BinaryWriter binaryWriter, Instruction<G> instruction)
		{
			binaryWriter.Write((byte)instruction.Code);
			switch (instruction.Code)
			{
				case InstructionCode.LE:
				case InstructionCode.CSE:
				case InstructionCode.SPR:
				case InstructionCode.GPR:
				case InstructionCode.CG:
				case InstructionCode.MG:
				case InstructionCode.RM:
				case InstructionCode.GDR:
				case InstructionCode.OA:
				case InstructionCode.RGGPR:
				case InstructionCode.LRAS:
				case InstructionCode.US:
				case InstructionCode.C:
				case InstructionCode.NC:
				case InstructionCode.J:
				case InstructionCode.RLR:
					binaryWriter.Write((int)instruction.Payload[0]);
					break;
				case InstructionCode.G:
				case InstructionCode.CSP:
				case InstructionCode.CGP:
				case InstructionCode.RGAR:
				case InstructionCode.RAR:
				case InstructionCode.RGH:
				case InstructionCode.HGR:
				case InstructionCode.VR:
					WriteObject(binaryWriter, instruction.Payload?.ElementAtOrDefault(0));
					break;
				case InstructionCode.CDP:
				case InstructionCode.DPR:
				case InstructionCode.RGDPR:
				case InstructionCode.VCSP:
				case InstructionCode.VCGP:
					WriteObject(binaryWriter, instruction.Payload[0]);
					WriteObject(binaryWriter, instruction.Payload.ElementAtOrDefault(1));
					break;
				case InstructionCode.RVK:
				case InstructionCode.A:
					binaryWriter.Write((bool)instruction.Payload[0]);
					break;
				case InstructionCode.LRR:
					binaryWriter.Write((int)instruction.Payload[0]);
					binaryWriter.Write((bool)instruction.Payload[1]);
					break;
				case InstructionCode.DPDP:
				case InstructionCode.DPCDP:
					WriteObject(binaryWriter, instruction.Payload[0]);
					WriteObject(binaryWriter, instruction.Payload[1]);
					WriteObject(binaryWriter, instruction.Payload[2]);
					WriteObject(binaryWriter, instruction.Payload[3]);
					break;
				case InstructionCode.VDP:
				case InstructionCode.VCDP:
				case InstructionCode.DPCSP:
				case InstructionCode.DPCGP:
					WriteObject(binaryWriter, instruction.Payload[0]);
					WriteObject(binaryWriter, instruction.Payload[1]);
					WriteObject(binaryWriter, instruction.Payload[2]);
					break;
				case InstructionCode.MI:
				case InstructionCode.MP:
				case InstructionCode.VSP:
				case InstructionCode.VGP:
					WriteObject(binaryWriter, instruction.Payload[0]);
					binaryWriter.Write((int)instruction.Payload[1]);
					break;
				case InstructionCode.CVR:
					binaryWriter.Write((bool)instruction.Payload[0]);
					WriteObject(binaryWriter, instruction.Payload.ElementAtOrDefault(1));
					break;
				case InstructionCode.SPSP:
				case InstructionCode.SPGP:
				case InstructionCode.GPSP:
				case InstructionCode.GPGP:
				case InstructionCode.SPCDP:
				case InstructionCode.MF:
					binaryWriter.Write((int)instruction.Payload[0]);
					binaryWriter.Write((int)instruction.Payload[1]);
					break;
				case InstructionCode.SPDP:
				case InstructionCode.GPDP:
				case InstructionCode.GPCDP:
					binaryWriter.Write((int)instruction.Payload[0]);
					WriteObject(binaryWriter, instruction.Payload[1]);
					WriteObject(binaryWriter, instruction.Payload[2]);
					break;
				case InstructionCode.DPSP:
				case InstructionCode.DPGP:
					WriteObject(binaryWriter, instruction.Payload[0]);
					WriteObject(binaryWriter, instruction.Payload[1]);
					binaryWriter.Write((int)instruction.Payload[2]);
					break;
				case InstructionCode.SPCSP:
				case InstructionCode.SPCGP:
				case InstructionCode.GPCSP:
				case InstructionCode.GPCGP:
				case InstructionCode.AR:
					binaryWriter.Write((int)instruction.Payload[0]);
					WriteObject(binaryWriter, instruction.Payload.ElementAtOrDefault(1));
					break;
				case InstructionCode.CPR:
					WriteCPRPayload(binaryWriter, instruction.Payload);
					break;
				case InstructionCode.RCP:
					WriteRCPPayload(binaryWriter, instruction.Payload);
					break;
				case InstructionCode.SPL:
					WriteSPLPayload(binaryWriter, instruction.Payload);
					break;
				default:
					break;
			}
			if (WriteDebugInfo)
			{
				WriteObject(binaryWriter, instruction.SourcePosition);
				binaryWriter.Write(instruction.Interruptable);
			}
		}

		private Instruction<G> ReadInstruction(BinaryReader binaryReader, bool hasDebugInfo)
		{
			InstructionCode code = (InstructionCode)binaryReader.ReadByte();
			object[] payload;
			switch (code)
			{
				case InstructionCode.LE:
				case InstructionCode.CSE:
				case InstructionCode.SPR:
				case InstructionCode.GPR:
				case InstructionCode.CG:
				case InstructionCode.MG:
				case InstructionCode.RM:
				case InstructionCode.GDR:
				case InstructionCode.OA:
				case InstructionCode.RGGPR:
				case InstructionCode.LRAS:
				case InstructionCode.US:
				case InstructionCode.C:
				case InstructionCode.NC:
				case InstructionCode.J:
				case InstructionCode.RLR:
					payload = new object[] { binaryReader.ReadInt32() };
					break;
				case InstructionCode.G:
				case InstructionCode.CSP:
				case InstructionCode.CGP:
				case InstructionCode.RGAR:
				case InstructionCode.RAR:
				case InstructionCode.RGH:
				case InstructionCode.HGR:
				case InstructionCode.VR:
					payload = new object[] { ReadObject(binaryReader) };
					break;
				case InstructionCode.CDP:
				case InstructionCode.DPR:
				case InstructionCode.RGDPR:
				case InstructionCode.VCSP:
				case InstructionCode.VCGP:
					payload = new object[] { ReadObject(binaryReader), ReadObject(binaryReader) };
					break;
				case InstructionCode.RVK:
				case InstructionCode.A:
					payload = new object[] { binaryReader.ReadBoolean() };
					break;
				case InstructionCode.LRR:
					payload = new object[] { binaryReader.ReadInt32(), binaryReader.ReadBoolean() };
					break;
				case InstructionCode.DPDP:
				case InstructionCode.DPCDP:
					payload = new object[] { ReadObject(binaryReader), ReadObject(binaryReader), ReadObject(binaryReader), ReadObject(binaryReader) };
					break;
				case InstructionCode.VDP:
				case InstructionCode.VCDP:
				case InstructionCode.DPCSP:
				case InstructionCode.DPCGP:
					payload = new object[] { ReadObject(binaryReader), ReadObject(binaryReader), ReadObject(binaryReader) };
					break;
				case InstructionCode.MI:
				case InstructionCode.MP:
				case InstructionCode.VSP:
				case InstructionCode.VGP:
					payload = new object[] { ReadObject(binaryReader), binaryReader.ReadInt32() };
					break;
				case InstructionCode.CVR:
					payload = new object[] { binaryReader.ReadBoolean(), ReadObject(binaryReader) };
					break;
				case InstructionCode.SPSP:
				case InstructionCode.SPGP:
				case InstructionCode.GPSP:
				case InstructionCode.GPGP:
				case InstructionCode.SPCDP:
				case InstructionCode.MF:
					payload = new object[] { binaryReader.ReadInt32(), binaryReader.ReadInt32() };
					break;
				case InstructionCode.SPDP:
				case InstructionCode.GPDP:
				case InstructionCode.GPCDP:
					payload = new object[] { binaryReader.ReadInt32(), ReadObject(binaryReader), ReadObject(binaryReader) };
					break;
				case InstructionCode.DPSP:
				case InstructionCode.DPGP:
					payload = new object[] { ReadObject(binaryReader), ReadObject(binaryReader), binaryReader.ReadInt32() };
					break;
				case InstructionCode.SPCSP:
				case InstructionCode.SPCGP:
				case InstructionCode.GPCSP:
				case InstructionCode.GPCGP:
				case InstructionCode.AR:
					payload = new object[] { binaryReader.ReadInt32(), ReadObject(binaryReader) };
					break;
				case InstructionCode.CPR:
					payload = ReadCPRPayload(binaryReader);
					break;
				case InstructionCode.RCP:
					payload = ReadRCPPayload(binaryReader);
					break;
				case InstructionCode.SPL:
					payload = ReadSPLPayload(binaryReader);
					break;
				default:
					payload = null;
					break;
			}
			int? sourcePosition = null;
			bool interruptable = false;
			if (hasDebugInfo)
			{
				sourcePosition = ReadObject(binaryReader) as int?;
				interruptable = binaryReader.ReadBoolean();
			}

			return code != InstructionCode.SPL ? InstructionProvider<G>.GetInstruction(code, payload, sourcePosition, interruptable) : _instructionProvider.GetSpecialInstruction(payload, sourcePosition, interruptable);
		}

		private void WriteSPLPayload(BinaryWriter binaryWriter, object[] payload)
		{
			WriteObject(binaryWriter, payload[0]);
			binaryWriter.Write(payload.Length - 1);
			foreach (object item in payload.Skip(1))
			{
				WriteObject(binaryWriter, item);
			}
		}

		private object[] ReadSPLPayload(BinaryReader binaryReader)
		{
			List<object> payload = new List<object>() { ReadObject(binaryReader) };
			int count = binaryReader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				payload.Add(ReadObject(binaryReader));
			}
			return payload.ToArray();
		}

		private void WriteCPRPayload(BinaryWriter binaryWriter, object[] payload)
		{
			binaryWriter.Write((int)payload[0]);
			int count = (int)payload[1];
			binaryWriter.Write(count);
			int payloadIndex = 2;
			for (int i = 0; i < count; i++)
			{
				InstructionCode code = (InstructionCode)payload[payloadIndex++];
				binaryWriter.Write((byte)code);
				switch (code)
				{
					case InstructionCode.SPR:
					case InstructionCode.GPR:
						binaryWriter.Write((int)payload[payloadIndex++]);
						break;
					case InstructionCode.VR:
						WriteObject(binaryWriter, payload[payloadIndex++]);
						break;
					case InstructionCode.DPR:
						WriteObject(binaryWriter, payload[payloadIndex++]);
						WriteObject(binaryWriter, payload[payloadIndex++]);
						break;
				}
			}
		}

		private void WriteRCPPayload(BinaryWriter binaryWriter, object[] payload)
		{
			int count = (int)payload[0];
			binaryWriter.Write(count);
			int payloadIndex = 1;
			for (int i = 0; i < count; i++)
			{
				InstructionCode code = (InstructionCode)payload[payloadIndex++];
				binaryWriter.Write((byte)code);
				switch (code)
				{
					case InstructionCode.SPR:
					case InstructionCode.GPR:
						binaryWriter.Write((int)payload[payloadIndex++]);
						break;
					case InstructionCode.CSP:
					case InstructionCode.CGP:
						WriteObject(binaryWriter, payload[payloadIndex++]);
						break;
					case InstructionCode.DPR:
					case InstructionCode.CDP:
						WriteObject(binaryWriter, payload[payloadIndex++]);
						WriteObject(binaryWriter, payload[payloadIndex++]);
						break;
				}
			}
		}

		private object[] ReadCPRPayload(BinaryReader binaryReader)
		{
			List<object> payload = new List<object>() { binaryReader.ReadInt32() };
			int count = binaryReader.ReadInt32();
			payload.Add(count);
			for (int i = 0; i < count; i++)
			{
				InstructionCode code = (InstructionCode)binaryReader.ReadByte();
				payload.Add(code);
				switch (code)
				{
					case InstructionCode.SPR:
					case InstructionCode.GPR:
						payload.Add(binaryReader.ReadInt32());
						break;
					case InstructionCode.VR:
						payload.Add(ReadObject(binaryReader));
						break;
					case InstructionCode.DPR:
						payload.Add(ReadObject(binaryReader));
						payload.Add(ReadObject(binaryReader));
						break;
				}
			}
			return payload.ToArray();
		}

		private object[] ReadRCPPayload(BinaryReader binaryReader)
		{
			List<object> payload = new List<object>();
			int count = binaryReader.ReadInt32();
			payload.Add(count);
			for (int i = 0; i < count; i++)
			{
				InstructionCode code = (InstructionCode)binaryReader.ReadByte();
				payload.Add(code);
				switch (code)
				{
					case InstructionCode.SPR:
					case InstructionCode.GPR:
						payload.Add(binaryReader.ReadInt32());
						break;
					case InstructionCode.CSP:
					case InstructionCode.CGP:
						payload.Add(ReadObject(binaryReader));
						break;
					case InstructionCode.DPR:
					case InstructionCode.CDP:
						payload.Add(ReadObject(binaryReader));
						payload.Add(ReadObject(binaryReader));
						break;
				}
			}
			return payload.ToArray();
		}

		private void WriteObject(BinaryWriter binaryWriter, object value)
		{
			switch (value)
			{
				case string strVal:
					byte[] bytes = Encoding.UTF8.GetBytes(strVal);
					binaryWriter.Write(bytes.Length);
					binaryWriter.Write(bytes);
					break;
				case null:
					binaryWriter.Write(-1);
					break;
				case int intVal:
					binaryWriter.Write(-2);
					binaryWriter.Write(intVal);
					break;
				case long longVal:
					binaryWriter.Write(-3);
					binaryWriter.Write(longVal);
					break;
				case double doubleVal:
					binaryWriter.Write(-4);
					binaryWriter.Write(doubleVal);
					break;
				case bool boolVal:
					binaryWriter.Write(-5);
					binaryWriter.Write(boolVal);
					break;
				default:
					WriteExternal(binaryWriter, value);
					break;
			}
		}

		private object ReadObject(BinaryReader binaryReader)
		{
			object value;
			int id = binaryReader.ReadInt32();
			switch (id)
			{
				case -1:
					value = null;
					break;
				case -2:
					value = binaryReader.ReadInt32();
					break;
				case -3:
					value = binaryReader.ReadInt64();
					break;
				case -4:
					value = binaryReader.ReadDouble();
					break;
				case -5:
					value = binaryReader.ReadBoolean();
					break;
				default:
					value = id >= 0 ? Encoding.UTF8.GetString(binaryReader.ReadBytes(id)) : ReadExternal(binaryReader, id);
					break;
			}
			return value;
		}

		protected virtual void WriteExternal(BinaryWriter binaryWriter, object value)
		{
			throw new NotImplementedException("No implementation for externals");
		}

		protected virtual object ReadExternal(BinaryReader binaryReader, int id)
		{
			throw new NotImplementedException("No implementation for externals");
		}
	}
}

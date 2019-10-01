using System;

namespace Synfron.Staxe.Executor.Instructions
{
	public class InstructionAttribute : Attribute
	{
		public InstructionAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }
	}

	public enum InstructionCode
	{
		/// <summary>
		/// NONE - No op
		///</summary>
		[Instruction(nameof(NON))]
		NON,
		/// <summary>
		/// Special - Custom instruction<br />
		/// Register: *<br />
		/// Payload: &lt;instruction code&gt; [&lt;instruction playload&gt;...]
		///</summary>
		[Instruction(nameof(SPL))]
		SPL,
		/// <summary>
		/// Group start - Creates a new Group/GroupState and execute its instructions. All instructions after this until the GE instruction are considered a part of the new Group.<br/><br/>
		/// Register:<br />
		/// Payload: &lt;group name&gt;
		/// </summary>
		[Instruction(nameof(G))]
		G,
		/// <summary>
		/// Group End - Denotes the end of instructions that belong to the new Group.
		///</summary>
		[Instruction(nameof(GE))]
		GE,
		/// <summary>
		/// Instruction Feed End - Denotes the end of group instructions execution
		///</summary>
		[Instruction(nameof(IFE))]
		IFE,
		/// <summary>
		/// Action start - Denotes the start of a subroutine<br /><br />
		/// Payload: &lt;create frame&gt;
		///</summary>
		[Instruction(nameof(A))]
		A,
		/// <summary>
		/// Action End - Denotes the end of a subroutine<br /><br />
		/// Payload: [&lt;next instruction&gt;]
		///</summary>
		[Instruction(nameof(AE))]
		AE,
		/// <summary>
		/// Block start
		///</summary>
		[Instruction(nameof(B))]
		B,
		/// <summary>
		/// Block End - Unrolls the stack until the start of the block
		///</summary>
		[Instruction(nameof(BE))]
		BE,
		/// <summary>
		/// Loop start
		///</summary>
		[Instruction(nameof(L))]
		L,
		/// <summary>
		/// Loop End - Jumps to the start of the loop<br /><br />
		/// Payload: &lt;Loop start instruction index&gt;
		///</summary>
		[Instruction(nameof(LE))]
		LE,
		/// <summary>
		/// Conditional Section start
		///</summary>
		[Instruction(nameof(CS))]
		CS,
		/// <summary>
		/// Conditional Section End - Jumps to the start of the conditional section<br /><br />
		/// Payload: &lt;Conditional section start instruction index&gt;
		///</summary>
		[Instruction(nameof(CSE))]
		CSE,
		/// <summary>
		/// Create Stack Pointer<br /><br />
		/// Payload: &lt;stack pointer name&gt;
		///</summary>
		[Instruction(nameof(CSP))]
		CSP,
		/// <summary>
		/// Create Group Pointer<br /><br />
		/// Payload: &lt;group pointer name&gt;
		///</summary>
		[Instruction(nameof(CGP))]
		CGP,
		/// <summary>
		/// Create Dynamic Pointer<br /><br />
		/// Payload: &lt;pointer location&gt; &lt;pointer name&gt;
		///</summary>
		[Instruction(nameof(CDP))]
		CDP,
		/// <summary>
		/// Stack Pointer to Register<br /><br />
		/// Payload: &lt;stack location&gt;
		///</summary>
		[Instruction(nameof(SPR))]
		SPR,
		/// <summary>
		/// Group Pointer to Register<br /><br />
		/// Payload: &lt;pointer index&gt;
		///</summary>
		[Instruction(nameof(GPR))]
		GPR,
		/// <summary>
		/// Dynamic Pointer to Register<br /><br />
		/// Payload: &lt;pointer location&gt; &lt;pointer name&gt;
		///</summary>
		[Instruction(nameof(DPR))]
		DPR,
		/// <summary>
		/// Value to Register<br /><br />
		/// Payload: &lt;value&gt;
		///</summary>
		[Instruction(nameof(VR))]
		VR,
		/// <summary>
		/// Combo Pointers to Register - Read multiple pointers into the register<br /><br />
		/// Payload: &lt;register offset&gt; &lt;pointers count&gt; [&lt;pointer instruction code&gt; &lt;pointer payload&gt;]...
		///</summary>
		[Instruction(nameof(CPR))]
		CPR,
		/// <summary>
		/// Register to Combo Pointers - Read the register into multiple pointers<br /><br />
		/// Register: [&lt;source pointer&gt;]...<br />
		/// Payload: &lt;pointers count&gt; [&lt;pointer instruction code&gt; &lt;pointer payload&gt;]...
		///</summary>
		[Instruction(nameof(RCP))]
		RCP,
		/// <summary>
		/// Undeclare Register Dynamic Pointer<br /><br />
		/// Register: &lt;pointer&gt;<br />
		///</summary>
		[Instruction(nameof(URDP))]
		URDP,
		/// <summary>
		/// Copy Group<br /><br />
		/// Register: &lt;source group statet&gt;<br />
		/// Payload: &lt;Copy flags&gt;
		///</summary>
		[Instruction(nameof(CG))]
		CG,
		/// <summary>
		/// Merge Groups<br /><br />
		/// Register: &lt;target group state&gt; &lt;source group state&gt;<br />E
		/// Payload: &lt;GroupMerge flags&gt;
		///</summary>
		[Instruction(nameof(MG))]
		MG,
		/// <summary>
		/// Merge Frame<br /><br />
		/// Payload: &lt;frame position&gt; &lt;FrameMerge flags&gt;
		///</summary>
		[Instruction(nameof(MF))]
		MF,
		/// <summary>
		/// Pull Register<br /><br />
		/// Register: &lt;to remove&gt;
		///</summary>
		[Instruction(nameof(PLR))]
		PLR,
		/// <summary>
		/// Push Register<br /><br />
		///</summary>
		[Instruction(nameof(PHR))]
		PHR,
		/// <summary>
		/// Copy Push Register<br /><br />
		/// Register: &lt;Pointer to duplicate&gt;<br />
		///</summary>
		[Instruction(nameof(CPHR))]
		CPHR,
		/// <summary>
		/// Register Move<br /><br />
		/// Register: &lt;pointer to move&gt;<br />
		/// Payload: &lt;Move offset&gt;
		///</summary>
		[Instruction(nameof(RM))]
		RM,
		/// <summary>
		/// Group to Register - Put the current executing group in the register
		///</summary>
		[Instruction(nameof(GR))]
		GR,
		/// <summary>
		/// Group Dependency to Register - Put the group dependency in the register<br /><br />
		/// Payload: &lt;Dependency index&gt;
		///</summary>
		[Instruction(nameof(GDR))]
		GDR,
		/// <summary>
		/// Register Group Dependency - Put the group from the register in the current group's dependencies<br /><br />
		/// Register: &lt;target group state&gt; &lt;dependee group state&gt;
		///</summary>
		[Instruction(nameof(RGD))]
		RGD,
		/// <summary>
		/// Register Group to Host - Store the group in the executor (hosted)<br /><br />
		/// Register: &lt;source group state&gt;<br />
		/// Payload: [&lt;hosted group identifier&gt;]
		///</summary>
		[Instruction(nameof(RGH))]
		RGH,
		/// <summary>
		/// Hosted Group to Register - Put the hosted group in the executor<br /><br />
		/// Payload: &lt;hosted group identifier&gt;
		///</summary>
		[Instruction(nameof(HGR))]
		HGR,
		/// <summary>
		/// Map Instruction<br /><br />
		/// Payload: &lt;name&gt; &lt;instruction index&gt;
		///</summary>
		[Instruction(nameof(MI))]
		MI,
		/// <summary>
		/// Map Pointer<br /><br />
		/// Payload: &lt;group pointer name&gt; &lt;group pointer index&gt;
		///</summary>
		[Instruction(nameof(MP))]
		MP,
		/// <summary>
		/// Mapped Instruction to Register<br /><br />
		/// Register: &lt;name&gt;<br />
		///</summary>
		[Instruction(nameof(MIR))]
		MIR,
		/// <summary>
		/// Mapped Pointer to Register<br /><br />
		/// Register: &lt;Group pointer name&gt;<br />
		///</summary>
		[Instruction(nameof(MPR))]
		MPR,
		/// <summary>
		/// Override Action - Executed group state actions targeting the instruction index will execute the target valuable<br /><br />
		/// Register: &lt;group state&gt; &lt;target valuable&gt;<br />
		/// Payload: &lt;instruction index&gt;
		///</summary>
		[Instruction(nameof(OA))]
		OA,
		/// <summary>
		/// Register Value Size<br /><br />
		/// Register: &lt;target valuable&gt;<br />
		///</summary>
		[Instruction(nameof(RVS))]
		RVS,
		/// <summary>
		/// Register Action Register - Create an action value<br /><br />
		/// Payload: &lt;instructions start index&gt; [&lt;action name&gt;]
		///</summary>
		[Instruction(nameof(AR))]
		AR,
		/// <summary>
		/// Action Register - Create an action value<br /><br />
		/// Register: &lt;instructions start index&gt;<br />
		/// Payload: [&lt;action name&gt;]
		///</summary>
		[Instruction(nameof(RAR))]
		RAR,
		/// <summary>
		/// Register Group Action Register - Create an action value from the given group<br /><br />
		/// Register: &lt;group state&gt; &lt;instructions start index&gt;<br />
		/// Payload: [&lt;action name&gt;]
		///</summary>
		[Instruction(nameof(RGAR))]
		RGAR,
		/// <summary>
		/// Register Group Group Pointer to Register<br /><br />
		/// Register: &lt;group state&gt;<br />
		/// Payload: &lt;group pointer index&gt;
		///</summary>
		[Instruction(nameof(RGGPR))]
		RGGPR,
		/// <summary>
		/// Register Group Dynamic Pointer to Register<br /><br />
		/// Register: &lt;group state&gt;<br />
		/// Payload: &lt;location&gt; [&lt;name&gt;]
		///</summary>
		[Instruction(nameof(RGDPR))]
		RGDPR,
		/// <summary>
		/// Stack Pointer to Stack Pointer<br /><br />
		/// Payload: &lt;source pointer index&gt; &lt;target pointer index&gt;
		///</summary>
		[Instruction(nameof(SPSP))]
		SPSP,
		/// <summary>
		/// Stack Pointer to Group Pointer<br /><br />
		/// Payload: &lt;source pointer index&gt; &lt;target pointer index&gt;
		///</summary>
		[Instruction(nameof(SPGP))]
		SPGP,
		/// <summary>
		/// Stack Pointer to Dynamic Pointer<br /><br />
		/// Payload: &lt;source pointer index&gt; &lt;target pointer location&gt; &lt;target pointer name&gt;
		///</summary>
		[Instruction(nameof(SPDP))]
		SPDP,
		/// <summary>
		/// Group Pointer to Stack Pointer<br /><br />
		/// Payload: &lt;source pointer index&gt; &lt;target pointer index&gt;
		///</summary>
		[Instruction(nameof(GPSP))]
		GPSP,
		/// <summary>
		/// Group Pointer to Group Pointer<br /><br />
		/// Payload: &lt;source pointer index&gt; &lt;target pointer index&gt;
		///</summary>
		[Instruction(nameof(GPGP))]
		GPGP,
		/// <summary>
		/// Group Pointer to Dynamic Pointer<br /><br />
		/// Payload: &lt;source pointer index&gt; &lt;target pointer location&gt; &lt;target pointer name&gt;
		///</summary>
		[Instruction(nameof(GPDP))]
		GPDP,
		/// <summary>
		/// Dynamic Pointer to Stack Pointer<br /><br />
		/// Payload: &lt;source pointer location&gt; &lt;source pointer name&gt; &lt;target pointer index&gt;
		///</summary>
		[Instruction(nameof(DPSP))]
		DPSP,
		/// <summary>
		/// Dynamic Pointer to Group Pointer<br /><br />
		/// Payload: &lt;source pointer location&gt; &lt;source pointer name&gt; &lt;target pointer index&gt;
		///</summary>
		[Instruction(nameof(DPGP))]
		DPGP,
		/// <summary>
		/// Dynamic Pointer to Dynamic Pointer<br /><br />
		/// Payload: &lt;source pointer location&gt; &lt;source pointer name&gt; &lt;target pointer location&gt; &lt;target pointer name&gt;
		///</summary>
		[Instruction(nameof(DPDP))]
		DPDP,
		/// <summary>
		/// Value to Stack Pointer<br /><br />
		/// Payload: &lt;value&gt; &lt;target pointer index&gt;
		///</summary>
		[Instruction(nameof(VSP))]
		VSP,
		/// <summary>
		/// Value to Group Pointer<br /><br />
		/// Payload: &lt;value&gt; &lt;target pointer index&gt;
		///</summary>
		[Instruction(nameof(VGP))]
		VGP,
		/// <summary>
		/// Value to Dynamic Pointer<br /><br />
		/// Payload: &lt;value&gt; &lt;target pointer location&gt; &lt;target pointer name&gt;
		///</summary>
		[Instruction(nameof(VDP))]
		VDP,
		/// <summary>
		/// Register (value to) Register - Set value from one pointer to another<br /><br />
		/// Register: &lt;target pointer&gt; &lt;source pointer&gt;<br />
		///</summary>
		[Instruction(nameof(RR))]
		RR,
		/// <summary>
		/// Stack Pointer to Create Stack Pointer<br /><br />
		/// Payload: &lt;source pointer index&gt; &lt;new pointer name&gt;
		///</summary>
		[Instruction(nameof(SPCSP))]
		SPCSP,
		/// <summary>
		/// Stack Pointer to Create Group Pointer<br /><br />
		/// Payload: &lt;source pointer index&gt; &lt;new pointer name&gt;
		///</summary>
		[Instruction(nameof(SPCGP))]
		SPCGP,
		/// <summary>
		/// Stack Pointer to Create Dynamic Pointer<br /><br />
		/// Payload: &lt;source pointer index&gt; &lt;new pointer location&gt; &lt;new pointer name&gt;
		///</summary>
		[Instruction(nameof(SPCDP))]
		SPCDP,
		/// <summary>
		/// Group Pointer to Create Stack Pointer<br /><br />
		/// Payload: &lt;source pointer index&gt; &lt;new pointer name&gt;
		///</summary>
		[Instruction(nameof(GPCSP))]
		GPCSP,
		/// <summary>
		/// Group Pointer to Create Group Pointer<br /><br />
		/// Payload: &lt;source pointer index&gt; &lt;new pointer name&gt;
		///</summary>
		[Instruction(nameof(GPCGP))]
		GPCGP,
		/// <summary>
		/// Group Pointer to Create Dynamic Pointer<br /><br />
		/// Payload: &lt;source pointer index&gt; &lt;new pointer location&gt; &lt;new pointer name&gt;
		///</summary>
		[Instruction(nameof(GPCDP))]
		GPCDP,
		/// <summary>
		/// Dynamic Pointer to Create Stack Pointer<br /><br />
		/// Payload: &lt;source pointer location&gt; &lt;source pointer name&gt; &lt;new pointer index&gt;
		///</summary>
		[Instruction(nameof(DPCSP))]
		DPCSP,
		/// <summary>
		/// Dynamic Pointer to Create Group Pointer<br /><br />
		/// Payload: &lt;source pointer location&gt; &lt;source pointer name&gt; &lt;new pointer index&gt;
		///</summary>
		[Instruction(nameof(DPCGP))]
		DPCGP,
		/// <summary>
		/// Dynamic Pointer to Create Dynamic Pointer<br /><br />
		/// Payload: &lt;source pointer location&gt; &lt;source pointer name&gt; &lt;new pointer location&gt; &lt;new pointer name&gt;
		///</summary>
		[Instruction(nameof(DPCDP))]
		DPCDP,
		/// <summary>
		/// Value to Create Stack Pointer<br /><br />
		/// Payload: &lt;value&gt; &lt;new pointer name&gt;
		///</summary>
		[Instruction(nameof(VCSP))]
		VCSP,
		/// <summary>
		/// Value to Create Group Pointer<br /><br />
		/// Payload: &lt;value&gt; &lt;new pointer name&gt;
		///</summary>
		[Instruction(nameof(VCGP))]
		VCGP,
		/// <summary>
		/// Value to Create Dynamic Pointer<br /><br />
		/// Payload: &lt;value&gt; &lt;new pointer location&gt; &lt;new pointer name&gt;
		///</summary>
		[Instruction(nameof(VCDP))]
		VCDP,
		/// <summary>
		/// Register (pointer to) List Register
		/// Payload: &lt;pointer count&gt;
		///</summary>
		[Instruction(nameof(RLR))]
		RLR,
		/// <summary>
		/// List Register (pointer to) Register
		/// Payload: &lt;pointer count&gt;
		///</summary>
		[Instruction(nameof(LRR))]
		LRR,
		/// <summary>
		/// List Register Action Stack<br /><br />
		/// Register: &lt;pointer&gt;...<br />
		/// Payload: &lt;pointer count&gt;
		///</summary>
		[Instruction(nameof(LRAS))]
		LRAS,
		/// <summary>
		/// Collection Value to Register<br /><br />
		/// Register: [&lt;initial capacity&gt;]<br />
		/// Payload: &lt;has capacity&gt; &lt;collection mode&gt;
		///</summary>
		[Instruction(nameof(CVR))]
		CVR,
		/// <summary>
		/// Register Value ( get entry at) Key - Get pointer from a keyable valuable<br />
		/// Register: &lt;keyable valuable&gt; &lt;key&gt;<br />
		/// Payload: &lt;create if undefined&gt;
		///</summary>
		[Instruction(nameof(RVK))]
		RVK,
		/// <summary>
		/// Register Value ( get entry at) Index<br /><br />
		/// Register: &lt;indexable valuable&gt; &lt;key&gt;
		///</summary>
		[Instruction(nameof(RVI))]
		RVI,
		/// <summary>
		/// Pointer Modifier Register - Get the modifier of a pointer<br /><br />
		/// Register: &lt;pointer&gt;
		///</summary>
		[Instruction(nameof(PMR))]
		PMR,
		/// <summary>
		/// Group Modifier Register - Get the modifier of a group<br /><br />
		/// Register: &lt;group state&gt;
		///</summary>
		[Instruction(nameof(GMR))]
		GMR,
		/// <summary>
		/// Register Pointer Modifier<br /><br />
		/// Register: &lt;pointer&gt; &lt;modifier&gt;
		///</summary>
		[Instruction(nameof(RPM))]
		RPM,
		/// <summary>
		/// Register Group Modifier<br /><br />
		/// Register: &lt;group state&gt; &lt;modifier&gt;
		///</summary>
		[Instruction(nameof(RGM))]
		RGM,
		/// <summary>
		/// Clear Register - Clear the stack register<br /><br />
		/// Register: &lt;pointers to remove&gt;...
		///</summary>
		[Instruction(nameof(CR))]
		CR,
		/// <summary>
		/// Unstack - Clear the execution stack until the given instruction origin value is reached<br /><br />
		/// Payload: &lt;instruction origin value&gt;
		///</summary>
		[Instruction(nameof(US))]
		US,
		/// <summary>
		/// Register Call Executable<br /><br />
		/// Register: &lt;valuable to execute on&gt;<br />
		///</summary>
		[Instruction(nameof(RCE))]
		RCE,
		/// <summary>
		/// Condition - Jump if the valuable is truthy<br /><br />
		/// Register: &lt;valuable&gt;<br />
		/// Payload: &lt;instruction index&gt;
		///</summary>
		[Instruction(nameof(C))]
		C,
		/// <summary>
		/// Not Condition - Jump if the valuable is falsy<br /><br />
		/// Register: &lt;valuable&gt;<br />
		/// Payload: &lt;instruction index&gt;
		///</summary>
		[Instruction(nameof(NC))]
		NC,
		/// <summary>
		/// Is Defined - Determine if a pointer is defined<br /><br />
		/// Register: &lt;pointer&gt;<br />
		///</summary>
		[Instruction(nameof(ID))]
		ID,
		/// <summary>
		/// Jump - Specify the next instruction to execute<br /><br />
		/// Payload: &lt;new instruction index&gt;
		///</summary>
		[Instruction(nameof(J))]
		J,
		/// <summary>
		/// Halt - Prevent further execution
		///</summary>
		[Instruction(nameof(H))]
		H,
		/// <summary>
		/// Add<br /><br />
		/// Register: &lt;value 1&gt; &lt;value 2&gt;<br />
		///</summary>
		[Instruction("R+")]
		RPlus,
		/// <summary>
		/// Add<br /><br />
		/// Register: &lt;minuend&gt; &lt;subtrahend&gt;<br />
		///</summary>
		[Instruction("R-")]
		RSubtract,
		/// <summary>
		/// Multiply<br /><br />
		/// Register: &lt;value 1&gt; &lt;value 2&gt;<br />
		///</summary>
		[Instruction("R*")]
		RMultiply,
		/// <summary>
		/// Divide<br /><br />
		/// Register: &lt;dividend&gt; &lt;divisor&gt;<br />
		///</summary>
		[Instruction("R/")]
		RDivide,
		/// <summary>
		/// Remainder<br /><br />
		/// Register: &lt;dividend&gt; &lt;divisor&gt;<br />
		///</summary>
		[Instruction("R%")]
		RRemainder,
		/// <summary>
		/// Left Shift<br /><br />
		/// Register: &lt;value&gt; &lt;shift value&gt;<br />
		///</summary>
		[Instruction("R<<")]
		RLeftShift,
		/// <summary>
		/// Right Shift<br /><br />
		/// Register: &lt;value&gt; &lt;shift value&gt;<br />
		///</summary>
		[Instruction("R>>")]
		RRightShift,
		/// <summary>
		/// Less Than<br /><br />
		/// Register: &lt;value 1&gt; &lt;value 2&gt;<br />
		///</summary>
		[Instruction("R<")]
		RLessThan,
		/// <summary>
		/// Greater Than<br /><br />
		/// Register: &lt;value 1&gt; &lt;value 2&gt;<br />
		///</summary>
		[Instruction("R>")]
		RGreaterThan,
		/// <summary>
		/// Less Than or Equal To<br />
		/// Register: &lt;value 1&gt; &lt;value 2&gt;<br />
		///</summary>
		[Instruction("R<=")]
		RLessThanOrEquals,
		/// <summary>
		/// Greater Than or Equal To<br /><br />
		/// Register: &lt;value 1&gt; &lt;value 2&gt;<br />
		///</summary>
		[Instruction("R>=")]
		RGreaterThanOrEquals,
		/// <summary>
		/// Equal to<br /><br />
		/// Register: &lt;value 1&gt; &lt;value 2&gt;<br />
		///</summary>
		[Instruction("R==")]
		REquals,
		/// <summary>
		/// Not Equal to<br /><br />
		/// Register: &lt;value 1&gt; &lt;value 2&gt;<br />
		///</summary>
		[Instruction("R!=")]
		RNotEquals,
		/// <summary>
		/// And<br /><br />
		/// Register: &lt;value 1&gt; &lt;value 2&gt;<br />
		///</summary>
		[Instruction("R&")]
		RAnd,
		/// <summary>
		/// Or<br /><br />
		/// Register: &lt;value 1&gt; &lt;value 2&gt;<br />
		///</summary>
		[Instruction("R|")]
		ROr,
		/// <summary>
		/// Not<br /><br />
		/// Register: &lt;value 1&gt;
		///</summary>
		[Instruction("R!")]
		RNot
	}
}

| Value | Instruction Code | Summary |
| --- | --- | --- |
| `0` | NON | NONE - No op |
| `1` | SPL | Special - Custom instruction<br />  Register: *<br />  Payload: &lt;instruction code&gt; [&lt;instruction playload&gt;...] |
| `2` | G | Group start - Creates a new Group/GroupState and execute its instructions. All instructions after this until the GE instruction are considered a part of the new Group.<br /><br />  Register:<br />  Payload: &lt;group name&gt; |
| `3` | GE | Group End - Denotes the end of instructions that belong to the new Group. |
| `4` | IFE | Instruction Feed End - Denotes the end of group instructions execution |
| `5` | A | Action start - Denotes the start of a subroutine<br /><br />  Payload: &lt;create frame&gt; |
| `6` | AE | Action End - Denotes the end of a subroutine<br /><br />  Payload: [&lt;next instruction&gt;] |
| `7` | B | Block start |
| `8` | BE | Block End - Unrolls the stack until the start of the block |
| `9` | L | Loop start |
| `10` | LE | Loop End - Jumps to the start of the loop<br /><br />  Payload: &lt;Loop start instruction index&gt; |
| `11` | CS | Conditional Section start |
| `12` | CSE | Conditional Section End - Jumps to the start of the conditional section<br /><br />  Payload: &lt;Conditional section start instruction index&gt; |
| `13` | CSP | Create Stack Pointer<br /><br />  Payload: &lt;stack pointer name&gt; |
| `14` | CGP | Create Group Pointer<br /><br />  Payload: &lt;group pointer name&gt; |
| `15` | CDP | Create Dynamic Pointer<br /><br />  Payload: &lt;pointer location&gt; &lt;pointer name&gt; |
| `16` | SPR | Stack Pointer to Register<br /><br />  Payload: &lt;stack location&gt; |
| `17` | GPR | Group Pointer to Register<br /><br />  Payload: &lt;pointer index&gt; |
| `18` | DPR | Dynamic Pointer to Register<br /><br />  Payload: &lt;pointer location&gt; &lt;pointer name&gt; |
| `19` | VR | Value to Register<br /><br />  Payload: &lt;value&gt; |
| `20` | CPR | Combo Pointers to Register - Read multiple pointers into the register<br /><br />  Payload: &lt;register offset&gt; &lt;pointers count&gt; [&lt;pointer instruction code&gt; &lt;pointer payload&gt;]... |
| `21` | RCP | Register to Combo Pointers - Read the register into multiple pointers<br /><br />  Register: [&lt;source pointer&gt;]...<br />  Payload: &lt;pointers count&gt; [&lt;pointer instruction code&gt; &lt;pointer payload&gt;]... |
| `22` | URDP | Undeclare Register Dynamic Pointer<br /><br />  Register: &lt;pointer&gt;<br /> |
| `23` | CG | Copy Group<br /><br />  Register: &lt;source group statet&gt;<br />  Payload: &lt;Copy flags&gt; |
| `24` | MG | Merge Groups<br /><br />  Register: &lt;target group state&gt; &lt;source group state&gt;<br />E  Payload: &lt;GroupMerge flags&gt; |
| `25` | MF | Merge Frame<br /><br />  Payload: &lt;frame position&gt; &lt;FrameMerge flags&gt; |
| `26` | PLR | Pull Register<br /><br />  Register: &lt;to remove&gt; |
| `27` | PHR | Push Register<br /><br /> |
| `28` | CPHR | Copy Push Register<br /><br />  Register: &lt;Pointer to duplicate&gt;<br /> |
| `29` | RM | Register Move<br /><br />  Register: &lt;pointer to move&gt;<br />  Payload: &lt;Move offset&gt; |
| `30` | GR | Group to Register - Put the current executing group in the register |
| `31` | GDR | Group Dependency to Register - Put the group dependency in the register<br /><br />  Register: [&lt;Dependency index if no payload&gt;]<br />  Payload: [&lt;Dependency index&gt;] |
| `32` | RGD | Register Group Dependency - Put the group from the register in the current group's dependencies<br /><br />  Register: &lt;target group state&gt; &lt;dependee group state&gt; |
| `33` | RGH | Register Group to Host - Store the group in the executor (hosted)<br /><br />  Register: &lt;source group state&gt;<br />  Payload: [&lt;hosted group identifier&gt;] |
| `34` | HGR | Hosted Group to Register - Put the hosted group in the executor<br /><br />  Payload: &lt;hosted group identifier&gt; |
| `35` | MI | Map Instruction<br /><br />  Payload: &lt;name&gt; &lt;instruction index&gt; |
| `36` | MP | Map Pointer<br /><br />  Payload: &lt;group pointer name&gt; &lt;group pointer index&gt; |
| `37` | MIR | Mapped Instruction to Register<br /><br />  Register: [&lt;group state&gt;] &lt;name&gt;<br />  Payload: [&lt;Use group state from register&gt;] |
| `38` | MPR | Mapped Pointer to Register<br /><br />  Register: [&lt;group state&gt;] &lt;Group pointer name&gt;<br />  Payload: [&lt;Use group state from register&gt;] |
| `39` | OA | Override Action - Executed group state actions targeting the instruction index will execute the target valuable<br /><br />  Register: &lt;group state&gt; &lt;target valuable&gt;<br />  Payload: &lt;instruction index&gt; |
| `40` | RVS | Register Value Size<br /><br />  Register: &lt;target valuable&gt;<br /> |
| `41` | AR | Register Action Register - Create an action value<br /><br />  Payload: &lt;instructions start index&gt; [&lt;action name&gt;] |
| `42` | RAR | Action Register - Create an action value<br /><br />  Register: &lt;instructions start index&gt;<br />  Payload: [&lt;action name&gt;] |
| `43` | RGAR | Register Group Action Register - Create an action value from the given group<br /><br />  Register: &lt;group state&gt; &lt;instructions start index&gt;<br />  Payload: [&lt;action name&gt;] |
| `44` | RGGPR | Register Group Group Pointer to Register<br /><br />  Register: &lt;group state&gt;<br />  Payload: &lt;group pointer index&gt; |
| `45` | RGDPR | Register Group Dynamic Pointer to Register<br /><br />  Register: &lt;group state&gt;<br />  Payload: &lt;location&gt; [&lt;name&gt;] |
| `46` | SPSP | Stack Pointer to Stack Pointer<br /><br />  Payload: &lt;source pointer index&gt; &lt;target pointer index&gt; |
| `47` | SPGP | Stack Pointer to Group Pointer<br /><br />  Payload: &lt;source pointer index&gt; &lt;target pointer index&gt; |
| `48` | SPDP | Stack Pointer to Dynamic Pointer<br /><br />  Payload: &lt;source pointer index&gt; &lt;target pointer location&gt; &lt;target pointer name&gt; |
| `49` | GPSP | Group Pointer to Stack Pointer<br /><br />  Payload: &lt;source pointer index&gt; &lt;target pointer index&gt; |
| `50` | GPGP | Group Pointer to Group Pointer<br /><br />  Payload: &lt;source pointer index&gt; &lt;target pointer index&gt; |
| `51` | GPDP | Group Pointer to Dynamic Pointer<br /><br />  Payload: &lt;source pointer index&gt; &lt;target pointer location&gt; &lt;target pointer name&gt; |
| `52` | DPSP | Dynamic Pointer to Stack Pointer<br /><br />  Payload: &lt;source pointer location&gt; &lt;source pointer name&gt; &lt;target pointer index&gt; |
| `53` | DPGP | Dynamic Pointer to Group Pointer<br /><br />  Payload: &lt;source pointer location&gt; &lt;source pointer name&gt; &lt;target pointer index&gt; |
| `54` | DPDP | Dynamic Pointer to Dynamic Pointer<br /><br />  Payload: &lt;source pointer location&gt; &lt;source pointer name&gt; &lt;target pointer location&gt; &lt;target pointer name&gt; |
| `55` | VSP | Value to Stack Pointer<br /><br />  Payload: &lt;value&gt; &lt;target pointer index&gt; |
| `56` | VGP | Value to Group Pointer<br /><br />  Payload: &lt;value&gt; &lt;target pointer index&gt; |
| `57` | VDP | Value to Dynamic Pointer<br /><br />  Payload: &lt;value&gt; &lt;target pointer location&gt; &lt;target pointer name&gt; |
| `58` | RR | Register (value to) Register - Set value from one pointer to another<br /><br />  Register: &lt;target pointer&gt; &lt;source pointer&gt;<br /> |
| `59` | SPCSP | Stack Pointer to Create Stack Pointer<br /><br />  Payload: &lt;source pointer index&gt; &lt;new pointer name&gt; |
| `60` | SPCGP | Stack Pointer to Create Group Pointer<br /><br />  Payload: &lt;source pointer index&gt; &lt;new pointer name&gt; |
| `61` | SPCDP | Stack Pointer to Create Dynamic Pointer<br /><br />  Payload: &lt;source pointer index&gt; &lt;new pointer location&gt; &lt;new pointer name&gt; |
| `62` | GPCSP | Group Pointer to Create Stack Pointer<br /><br />  Payload: &lt;source pointer index&gt; &lt;new pointer name&gt; |
| `63` | GPCGP | Group Pointer to Create Group Pointer<br /><br />  Payload: &lt;source pointer index&gt; &lt;new pointer name&gt; |
| `64` | GPCDP | Group Pointer to Create Dynamic Pointer<br /><br />  Payload: &lt;source pointer index&gt; &lt;new pointer location&gt; &lt;new pointer name&gt; |
| `65` | DPCSP | Dynamic Pointer to Create Stack Pointer<br /><br />  Payload: &lt;source pointer location&gt; &lt;source pointer name&gt; &lt;new pointer index&gt; |
| `66` | DPCGP | Dynamic Pointer to Create Group Pointer<br /><br />  Payload: &lt;source pointer location&gt; &lt;source pointer name&gt; &lt;new pointer index&gt; |
| `67` | DPCDP | Dynamic Pointer to Create Dynamic Pointer<br /><br />  Payload: &lt;source pointer location&gt; &lt;source pointer name&gt; &lt;new pointer location&gt; &lt;new pointer name&gt; |
| `68` | VCSP | Value to Create Stack Pointer<br /><br />  Payload: &lt;value&gt; &lt;new pointer name&gt; |
| `69` | VCGP | Value to Create Group Pointer<br /><br />  Payload: &lt;value&gt; &lt;new pointer name&gt; |
| `70` | VCDP | Value to Create Dynamic Pointer<br /><br />  Payload: &lt;value&gt; &lt;new pointer location&gt; &lt;new pointer name&gt; |
| `71` | RLR | Register (pointer to) List Register  Payload: &lt;pointer count&gt; |
| `72` | LRR | List Register (pointer to) Register  Payload: &lt;pointer count&gt; &lt;copy references&gt; |
| `73` | LRAS | List Register Action Stack<br /><br />  Register: &lt;pointer&gt;...<br />  Payload: &lt;pointer count&gt; |
| `74` | CVR | Collection Value to Register<br /><br />  Register: [&lt;initial capacity&gt;]<br />  Payload: &lt;has capacity&gt; &lt;collection mode&gt; |
| `75` | RVK | Register Value ( get entry at) Key - Get pointer from a keyable valuable<br /><br />  Register: &lt;keyable valuable&gt; &lt;key&gt;<br />  Payload: &lt;create if undefined&gt; |
| `76` | RVI | Register Value ( get entry at) Index<br /><br />  Register: &lt;indexable valuable&gt; &lt;key&gt; |
| `77` | PMR | Pointer Modifier Register - Get the modifier of a pointer<br /><br />  Register: &lt;pointer&gt; |
| `78` | GMR | Group Modifier Register - Get the modifier of a group<br /><br />  Register: &lt;group state&gt; |
| `79` | RPM | Register Pointer Modifier<br /><br />  Register: &lt;pointer&gt; &lt;modifier&gt; |
| `80` | RGM | Register Group Modifier<br /><br />  Register: &lt;group state&gt; &lt;modifier&gt; |
| `81` | CR | Clear Register - Clear the stack register<br /><br />  Register: &lt;pointers to remove&gt;... |
| `82` | US | Unstack - Clear the execution stack until the given instruction origin value is reached<br /><br />  Payload: &lt;instruction origin value&gt; |
| `83` | RCE | Register Call Executable<br /><br />  Register: &lt;valuable to execute on&gt;<br /> |
| `84` | C | Condition - Jump if the valuable is truthy<br /><br />  Register: &lt;valuable&gt;<br />  Payload: &lt;instruction index&gt; |
| `85` | NC | Not Condition - Jump if the valuable is falsy<br /><br />  Register: &lt;valuable&gt;<br />  Payload: &lt;instruction index&gt; |
| `86` | ID | Is Defined - Determine if a pointer is defined<br /><br />  Register: &lt;pointer&gt;<br /> |
| `87` | J | Jump - Specify the next instruction to execute<br /><br />  Payload: &lt;new instruction index&gt; |
| `88` | H | Halt - Prevent further execution |
| `89` | RPlus | Add<br /><br />  Register: &lt;value 1&gt; &lt;value 2&gt;<br /> |
| `90` | RSubtract | Add<br /><br />  Register: &lt;minuend&gt; &lt;subtrahend&gt;<br /> |
| `91` | RMultiply | Multiply<br /><br />  Register: &lt;value 1&gt; &lt;value 2&gt;<br /> |
| `92` | RDivide | Divide<br /><br />  Register: &lt;dividend&gt; &lt;divisor&gt;<br /> |
| `93` | RRemainder | Remainder<br /><br />  Register: &lt;dividend&gt; &lt;divisor&gt;<br /> |
| `94` | RLeftShift | Left Shift<br /><br />  Register: &lt;value&gt; &lt;shift value&gt;<br /> |
| `95` | RRightShift | Right Shift<br /><br />  Register: &lt;value&gt; &lt;shift value&gt;<br /> |
| `96` | RLessThan | Less Than<br /><br />  Register: &lt;value 1&gt; &lt;value 2&gt;<br /> |
| `97` | RGreaterThan | Greater Than<br /><br />  Register: &lt;value 1&gt; &lt;value 2&gt;<br /> |
| `98` | RLessThanOrEquals | Less Than or Equal To<br />  Register: &lt;value 1&gt; &lt;value 2&gt;<br /> |
| `99` | RGreaterThanOrEquals | Greater Than or Equal To<br /><br />  Register: &lt;value 1&gt; &lt;value 2&gt;<br /> |
| `100` | REquals | Equal to<br /><br />  Register: &lt;value 1&gt; &lt;value 2&gt;<br /> |
| `101` | RNotEquals | Not Equal to<br /><br />  Register: &lt;value 1&gt; &lt;value 2&gt;<br /> |
| `102` | RAnd | And<br /><br />  Register: &lt;value 1&gt; &lt;value 2&gt;<br /> |
| `103` | ROr | Or<br /><br />  Register: &lt;value 1&gt; &lt;value 2&gt;<br /> |
| `104` | RNot | Not<br /><br />  Register: &lt;value 1&gt; |
| `105` | SIR | Standard In to Register - Read from stdin |
| `106` | RSO | Register to Standard Out - Write to stdout<br /><br />  Register: &lt;value to write&gt; |

---
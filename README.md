# Nibbler
An interactive Turing machine simulator and Busy Beaver calculator using exponential tapes and macro transitions

Busy Beavers are n-state Turing machines which are started on an initially blank tape and produce a maximum number of symbols without entering an infinite loop. This number is called Σ(n) and is easily proved to be noncalculable.

Nibbler aims to provide an easy-to-use framework for working with Busy Beavers. Its class libraries can be imported and used by any .NET-compatible program. For an even quicker start, it is recommended to use Nibbler’s interactive command prompt. Any command entered is executed immediately and the results are shown on the screen as well as logged to a file. For visual clarity, colored text is used in both screen and file output.

## Output interpretation

During simulation, Nibbler prints two lines each step by default:

	0~ A>
	A>0 => 1 B> (1s)
	1~ 1 B>
	B>0 => <A 1 (1s)
	2~ 1 <A 1
	1<A => <B 1 (1s)
	...

Lines in the form of ‘`n~ C`’ denote the current shift count n and configuration C.

The form ‘`C1 => C2 (is)`’ conveys the employed transition from configuration C1 to C2 using i shifts. i refers to the number of simple operations aggregated into the macro operation. If the transition uses whole block operations, ‘`xr`’ is appended, where r is the number of individual operations combined in the block operation.

## Available commands

In the following we list the available commands. Apart from their syntax and description we also give an example of their usage.

The program itself includes the same information. An overview of all commands can be displayed by typing `help`. Help on individual commands can be viewed via `help x`, where `x` is the command in question. For instance, `help quit` displays: `quit: Quits menu processing.`

For convenience, the program allows abbreviating commands. As long as the entered command is uniquely identifiable, there is no need to spell out all words. For instance, `pr t a` is automatically expanded to `print trans all`.

### def – load, change or display TM definitions
Syntax: `def [k x | d]`

`def d` loads definition `d`, overwriting the current definition.

`def k x` changes the macro size of the loaded TM to `x`. `x` must be a positive integer between 1 and 8.

The command without argument displays the definition of the currently loaded TM.

	$ def 1RA 1RH
	Loaded definition: 1RA 1RH,
	$ def k 2
	Macro sized changed: 1RA 1RH, K = 2
	$ def
	1RA 1RH, K = 2

### lib – load or display TM definitions from the library
Syntax: `lib [name]`

Loads the specified TM definition `name` from the library, or displays all available definitions.

	$ lib
	BusyBeaver2         1RB 1LB, 1LA 1RH, K = 1
	BusyBeaver3         1RB 1RH, 0RC 1RB, 1LC 1LA, K = 1

	$ lib beaver 2
	Loading BusyBeaver2

### create – instantiate a TM
Syntax: `create [nohist] [silent]`

Instantiates a new TM from the currently loaded TM definition.

If option `nohist` is given, no history is created, disabling nontermination proofs by repetition.
If `silent` is specified, step and transition output is disabled.

	$ def 1RA 1RH
	Loaded definition: 1RA 1RH,
	$ create
	Selected machine: 1RA 1RH,

### print – change output options
Syntax: `print steps [show|hide]`
or `print trans [all|ctr|none]`

The `print` commands changes which configurations and transitions are displayed.

To display both the tape and the transitions:

	$ print steps show
	$ print trans all

To hide all regular output:

	$ print steps hide
	$ print trans none

### step – perform a number of steps in the simulation
Syntax: `step n`

Lets the currently selected TM run for `n` steps. A step is either a basic transition or a single whole block operation, if enabled.

	$ lib simple tree
	Loading SimpleChristmasTree
	$ create
	Selected machine: 1RB 1LA, 1LA 1RC, 1RH 1RB,
	$ print steps hide
	$ step 8
	A>0 => 1 B> (1s)
	B>0 => <A 1 (1s)
	1<A => <A 1 (1s)x1
	0<A => 1 B> (1s)
	B>1 => 1 C> (1s)
	C>1 => 1 B> (1s)
	B>0 => <A 1 (1s)
	1<A => <A 1 (1s)x3

### run – run the simulation
Syntax: `run [s]`

Simulates the machine until it halts. If an argument `s` is given, the machine stops once its shift count is at least `s`.

Example of running until a machine halts:

	$ lib mabu 2
	Loading BusyBeaver5_Mabu2
	$ create silent
	Selected machine: 1LB 1LA, 1RC 1RB, 1LA 1RD, 1LA 1RE, 1RH 0RC, K=3
	$ run
	Result of 1LB 1LA, 1RC 1RB, 1LA 1RD, 1LA 1RE, 1RH 0RC, K=3: HALTING WITH REJECT after 11798826 shifts. 4098 symbols on tape. Tape: 11798826~ 011 101^2047 110

Example of running up to a certain number of shifts (note that due to preceding block operations, slightly more shifts may be performed than specified):

	$ lib mabu 1
	Loading BusyBeaver5_Mabu1
	$ create silent
	Selected machine: 1LB 1RC, 1LC 1LB, 1LD 0RE, 1RA 1RD, 1LH 0RA, K=3
	$ run 100000
	$ print steps show
	$ step 1
	100304~ 110 010^188 E> 100
	100307~ 110 010^188 <C 110

### Additional commands
Additional commands, mostly related to automation and batch processing, include:

* `{`: Records commands to a batch file until } is entered
* `!`: Executes a batch file
* `echo`: Prints the specified text to stdout
* `pause`: Stops further operation until the enter key is pressed
* `if`: Executes a command if a condition is met
* `break`: Inserts a breakpoint checking if a certain shift count was reached
* `stopwatch`: Measures time difference

## Included TM library
The library provided by the program, accessible via the `lib` command, contains over 30 machines, including all known and conjectured Busy Beavers. These machines are stored in a newly developed JSON format.


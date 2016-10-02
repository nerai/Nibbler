# nibbler
An interactive Turing machine simulator and Busy Beaver calculator using exponential tapes and macro transitions

Busy Beavers are n-state Turing machines which are started on an initially blank tape and produce a maximum number of symbols without entering an infinite loop. This number is called Σ(n) and is easily proved to be noncalculable. Nibbler aims to provide an easy-to-use framework for working with Busy Beavers.

Nibbler’s class libraries can be imported and used by any .NET-compatible program. For an even quicker start, it is recommended to use Nibbler’s interactive command prompt. Any command entered is executed immediately and the results are shown on the screen as well as logged to a file. For visual clarity, colored text is used in both screen and file output.

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
todo

### lib – load or display TM definitions from the library
todo

### create – instantiate a TM
todo

### print – change output options
todo

### step – perform a number of steps in the simulation
todo

### run – run the simulation
todo

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
The library provided by the program, accessible via the ‘lib’ command, contains over 30 machines, including all known and conjectured Busy Beavers. These machines are stored in a newly developed JSON format.


#Unlog
A drop-in replacement for Console.WriteLine

Certainly, the most commonly used debugging tool has always been console output. This has a lot of drawbacks, especially for larger projects, and there exist various good alternatives. For C#, NLog or Log4Net would be such options. However, many C# programmers still use Console.Writeline for its simplicity. Being (often) as fast as possible, zero-configuration, and having some (limited) formatting options, like setting fore- and background colors, are also positive traits.

While I in no way endorse this style of logging as a general solution, I have sometimes found it sufficient for my purposes, especially if rather close coupling with the console is required anyway. The Unlog project is about extending the capabilities of console logging in a dead-simple, unobstrusive and effective way. In doing so, close control over output formatting will be granted, while also adding the ability to go back in time and remove logged text retroactively - a feature not found in any other major logging framework. Oh, and it's faster than using Console.WriteLine (magic).

Unlog is a byproduct of another (much) larger project of mine, which heavily uses console output and interaction, and for which I found other logging frameworks fitting poorly.

## Usage and features

### Drop-in replacement

For starters, it suffices to replace Console.X with Log.X, for X being any of Write, WriteLine, ForegroundColor, BackgroundColor, ResetColor. In this way, all functionality is preserved. For instance:

    Console.WriteLine ("i = " + i);

becomes

    Log.WriteLine ("i = " + i);

### CC console output to HTML file

To have the output written to a file, call AddDefaultFileTarget during your program's initialization. This will carbon copy all console output to a file in a format which can later be converted to e.g. a HTML file, which looks exactly like the console output did (yes, including all colors).

TODO details of running the conversion

TODO visual example, html and console pictures

### Nested logging

Unlog offers nested logging, i.e. you can atomically create a new log stream inside another log stream. Think of it like nested directories in a file system, with individual files being the lines of log output. Nested logs automatically indent all their output.

TODO examples

### Retroactive Un-Logging

You may decide that the block you logged to is invalid after all, and should not be written. In this case, you may simply leave the block without keeping it, and it will silently be discarded - it will neither be written to the console nor to any other log target.

TODO example

### Automatic scoping

TODO disposable block logging

### Performance optimizations

TODO delayed write

## Synopsis

This library helps integrate w/ R and Rserve using C#. The original project site can be found at [RserveLink - SourceForge].  My updated code can be found at [RserveLink - GitHub].

From the original description:
This is simple API for connections with Rserve program (making connections, file and variables transport, R-commands execute).

## Code Example

See the SourceForge project site [RserveLink - SourceForge].  Help documentation can be found at [RserveLinkHelp].

## Motivation

The original Source code on SourceForge was only in a zip, not conducive to contribution, and hasn't been updated since 2007.  I am publishing my updated code on GitHub so that people can fork and extend the library.

## Installation

The project has been updated to work with Visual Studio 2013 and targeting .Net 4.0 Client Profile and .Net 4.5.1, no reason it couldn't be 2.0 like the original project. 

Add this library to your via the [RserveLink NuGet Package]:
<div class="highlight highlight-powershell">
Install-Package RserveLink
</div>

## Contributors

Originally developed by Krzysztof Miodek and updated and added to GitHub by [Brock].

## License

Carrying forward the GPL v2.0 from the SourceForge respository.


## Changes

2015-07-06: Changed the RConnection.GetResponse class to allow response[4] to be 0 and to hinge the if statement on whether data was returned.  Fixes an issue where the RConnection.Eval would occasionally return an erroneous "Eval Failed" exception on a successful command.

2015-07-06: Created GitHub repo.


[RserveLink - SourceForge]:http://sourceforge.net/projects/rservelink/
[RserveLinkHelp]:http://rservelink.sourceforge.net/techhelp.htm
[Brock]:https://github.com/0xbrock/
[RserveLink - GitHub]:https://github.com/0xbrock/RserveLink
[RserveLink NuGet Package]:https://www.nuget.org/packages/RserveLink/

# GetOpts

This is a command line argument parser for `C#` and the `.NET Standard 2.0`. This parser is able to extract pre-defined options from a given enumerable of arguments.

## How to use

The main namespace of this library is `DD.GetOpts`.

First we need to define the required options:
```csharp
var option1 = new Option( "a", "alpha", Argument.NONE, Occur.ONCE );
var option2 = new Option( "b", "beta", Argument.REQUIRED, Occur.OPTIONAL );
var option2 = new Option( "g", "gamma", Argument.OPTIONAL, Occur.MULTIPLE );
```

Every option consists a short and long name, as well as various occurrence and argument rules. A short name has to prefixed with a `-` in the supplied arguments while a long name has to be prefixed with a `--`. In the above example `option1` can be parsed either via `-a` or `--alpha`. The argument rules of the above options are as follows:
* `option1` doesn't have an argument (Only `-a` or `--alpha` are valid).
* `option2` requires and argument (Only `-b argument` or `--beta argument` are valid).
* `option2` can either have an argument or omit it.

The occurance rules of the above options are as follows:
* `option1` must occur exactly once in the provided arguments.
* `option2` can occur once in the provided arguments but doesn't have to.
* `option3` can occur multiple times in the provided arguments but doesn't have to.

The next step is to add these options to the `Options` collection.
```csharp
var options = new Options();
options.Add( option1 ).Add( options2 ).Add( options3 );
```

`Options` inherits `IEnumerable<T>` so it can be iterated:
```csharp
foreach ( var option in options ) {
    Console.WriteLine( $"Included Option: {option}" );
}
```

Now we are ready to parse command line arguments. In the following snippet we assume `args` is `string[]` provided by the `Main` method:
```csharp
var matches = options.Parse( args );
```

Should a supplied argument not match any `Option` in the `Options` collection then the method will throw a `ArgumentException` with a detailed description. The exception are free-standing arguments without a short or long prefix. The return value of the `Parse()` method is a `IEnumerable<T>` where `T` is `Match`. Let's assume we parsed `first -a -g foo -g bar last` then the result would include three matches. The first one for the free standing arguments `first` and `last`, the second for `-a`, and the third for `-g`. A `Match` has the following properties:

* `ShortName`: The short name of the matched option (In this example either `a` or `g`).
* `LongName`: The long name of the matched option (In this example either `alpha` or `gamma`).
* `Count`: The number of times the `Option` was matched. (`1` for `a` and `2` for `g` ).
* `Arguments`: The read-only collection of all arguments supplied to the matched options. (Empty for `a` but would contain `"foo"` and `"bar"` for `g`).

A match for free-standing arguments doesn't have a `ShortName` or `LongName`. The `Count` is set to the amount of free-standing arguments. `Arguments` will be a list of all free-standing arguments encountered (In this case `"first"` and `"last"`).

The best way to query the matches is by using `Linq` extensions:
```csharp
// Get all arguments for -g/--gamma
var arguments = matches.Where( x => x.ShortName == "g" ).First().Arguments;
```

## Define custom prefixes

The default option prefixes `-` and `--` can be changed to something else during the initialization of `Options`:
```csharp
var options = new Options( "/", "//" );
```

Now short-name options would have to be prefixed with `/` and long-name options with `//` instead of `-` and `--`.

## How to build

To to build the library execute `dotnet build -c Release`. You can find the resulting .NET Standard 2.0 library in `GetOpts/bin/Release/netstandard2.0`. To run the unit tests execute `dotnet test GetOpts.Tests/GetOpts.Tests.csproj -f netcoreapp2.0 -v n`. To run the tests with code coverage add `/p:CollectCoverage=true` at the end.

## License

See [LICENSE](LICENSE)

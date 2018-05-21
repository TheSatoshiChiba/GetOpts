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

Every option consists a short and long name, as well as various occurance and argument rules. A short name has to prefixed with a `-` in the supplied arguments while a long name has to be prefixed with a `--`. In the above example `option1` can be parsed either via `-a` or `--alpha`. The argument rules of the above options are as follows:
* `option1` doesn't have an argument (Only `-a` or `--alpha` are valid).
* `option2` requires and argument (Only `-a argument` or `--alpha argument` are valid).
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

Should a supplied argument not match any `Option` in the `Options` collection then the method will throw a `ArgumentException` with a detailed description. The return value of the `Parse()` method is a `IEnumerable<T>` where `T` is `Match`. Let's assume we parsed `-a -g foo -g bar` then the result would include two matches. One for `-a` and one for `-g`. A `Match` has the following properties:

* `ShortName`: The short name of the matched option (In this example either `a` or `g`).
* `LongName`: The long name of the matched option (In this example either `alpha` or `gamma`).
* `Count`: The number of times the `Option` was matched. (`1` for `a` and `2` for `g` ).
* `Arguments`: The read-only collection of all arguments supplied to the matched options. (Empty for `a` but would contain `"foo"` and `"bar"` for `g`).

The best way to query the matches is by using `Linq` extensions:
```csharp
// Get all arguments for -g/--gamma
var arguments = matches.Where( x => x.ShortName == "g" ).First().Arguments;
```

## License

See [LICENSE](LICENSE)

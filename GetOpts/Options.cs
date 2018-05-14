using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DD.GetOpts {
    /// <summary>
    /// The command line <see cref="Option"/> parser.
    /// </summary>
    public sealed class Options : IEnumerable<Option> {
        private const string SHORT_PREFIX = "-";
        private const string LONG_PREFIX = "--";

        private readonly HashSet<Option> options = new HashSet<Option>();
        private readonly Dictionary<string, Option> shortOptions
            = new Dictionary<string, Option>();
        private readonly Dictionary<string, Option> longOptions
            = new Dictionary<string, Option>();

        /// <summary>
        /// Adds a <see cref="Option"/> to the current <see cref="Options"/>
        /// instance.
        /// </summary>
        /// <returns>
        /// The current <see cref="Options"/> instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="option"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// A <paramref name="option"/> name contains the short <c>-</c>
        /// or the long <c>--</c> prefix.
        /// A <paramref name="option"/> name already exists in this collection.
        /// </exception>
        public Options Add( Option option ) {
            if ( option == null ) {
                throw new ArgumentNullException( nameof( option ) );
            }
            CheckPrefix( option.ShortName, "ShortName" );
            CheckPrefix( option.LongName, "LongName" );

            if ( option.ShortName != string.Empty ) {
                try {
                    shortOptions.Add( option.ShortName, option );
                } catch ( ArgumentException ex ) {
                    throw new ArgumentException(
                        "Option with short name "
                            + option.ShortName
                            + " already exists",
                        ex );
                }
            }

            if ( option.LongName != string.Empty ) {
                try {
                    longOptions.Add( option.LongName, option );
                } catch ( ArgumentException ex ) {
                    throw new ArgumentException(
                        "Option with long name "
                            + option.LongName
                            + " already exists",
                        ex );
                }
            }

            options.Add( option );

            return this;

            void CheckPrefix( string name, string paramName ) {
                if ( name.StartsWith( LONG_PREFIX ) ) {
                    throw new ArgumentException(
                        paramName
                            + $" starts with the long prefix {LONG_PREFIX}",
                        nameof( option ) );
                }
                if ( name.StartsWith( SHORT_PREFIX ) ) {
                    throw new ArgumentException(
                        paramName
                            + $" starts with the short prefix {SHORT_PREFIX}",
                        nameof( option ) );
                }
            }
        }

        public IEnumerable<Match> Parse( IEnumerable<string> arguments ) {
            if ( arguments == null ) {
                throw new ArgumentNullException( nameof( arguments ) );
            }

            var enumerator = arguments.GetEnumerator();
            var matchedOptions = new HashSet<Option>();
            var matchedCount = new Dictionary<Option, int>();
            var matchedArguments = new Dictionary<Option, List<string>>();

            Option previous = null;

            // Start parsing.
            while ( enumerator.MoveNext() ) {
                var argument = enumerator.Current.Trim();
                var isShort = argument.StartsWith( SHORT_PREFIX );
                var isLong = argument.StartsWith( LONG_PREFIX );

                // Because we don't account for prefix precedence we can end up
                // in a situation of matching both prefixes.
                if ( isLong && isShort ) {
                    isLong = LONG_PREFIX.Length > SHORT_PREFIX.Length;
                    isShort = !isLong;
                }

                if ( isShort ) {
                    previous = ReadOption(
                        argument, SHORT_PREFIX, shortOptions )
                        ?? throw new ArgumentException(
                            $"Invalid argument {argument}",
                            nameof( arguments ) );

                } else if ( isLong ) {
                    previous = ReadOption(
                        argument, LONG_PREFIX, longOptions )
                        ?? throw new ArgumentException(
                            $"Invalid argument {argument}",
                            nameof( arguments ) );

                } else {
                    if ( previous?.Arguments != Argument.NONE ) {
                        matchedArguments[ previous ].Add( argument );
                    }
                    // else {
                    // TODO: Match free standing argument
                    // }

                    previous = null;
                }
            }

            // Check if last option required an argument.
            if ( previous?.Arguments == Argument.REQUIRED ) {
                throw new ArgumentException(
                    $"Expected argument for [{previous}]",
                    nameof( arguments ) );
            }

            var missing = options.Where( x => x.Occurs == Occur.ONCE )
                .Except( matchedOptions );

            if ( missing.Any() ) {
                throw new ArgumentException(
                    $"Expected [{missing.First()}]",
                    nameof( arguments ) );
            }

            // Returns matches
            return matchedOptions.Select(
                x => new Match(
                    x.ShortName,
                    x.LongName,
                    matchedCount[ x ],
                    matchedArguments[ x ] ) );

            Option ReadOption(
                string argument,
                string prefix,
                Dictionary<string, Option> op ) {

                if ( previous?.Arguments == Argument.REQUIRED ) {
                    throw new ArgumentException(
                        $"Expected argument for [{previous}]",
                        nameof( arguments ) );
                }

                var name = argument.Remove( 0, prefix.Length );
                if ( !op.TryGetValue( name, out var option ) ) {
                    return null; // Not a valid option.
                }

                // We found a valid option.
                matchedOptions.Add( option );

                // Update occurance.
                if ( !matchedCount.TryGetValue( option, out var count ) ) {
                    matchedCount.Add( option, 1 );
                } else {
                    count += 1;
                    if ( option.Occurs != Occur.MULTIPLE && count > 1 ) {
                        throw new ArgumentException(
                            $"Multiple occurance of [{option}]",
                            nameof( arguments ) );
                    }
                    matchedCount[ option ] = count;
                }

                // Add argument key if not present.
                if ( !matchedArguments.ContainsKey( option ) ) {
                    matchedArguments.Add( option, new List<string>() );
                }

                return option;
            }
        }

        /// <inheritdoc/>
        public IEnumerator<Option> GetEnumerator() => options.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => options.GetEnumerator();
    }

        /*
        private readonly HashSet<Option> required = new HashSet<Option>();

        public Options Add( string shortName, string longName, Argument arguments, Occur occurs ) {
            var option = new Option( shortName, longName, arguments, occurs );
            if ( option.Occurs == Occur.ONCE ) {
                required.Add( option );
            }
            return this;
        }

        public Matches Parse( IEnumerable<string> arguments ) {
            var shortMatches = new Dictionary<string, Match>();
            var longMatches = new Dictionary<string, Match>();

            var req = new HashSet<Option>( required );
            foreach ( var option in context.Options ) {
                var match = new Match(
                    option.ShortName,
                    option.LongName,
                    context.Count[ option ],
                    context.Arguments[ option ].AsReadOnly() );

                if ( match.ShortName != string.Empty ) {
                    shortMatches.Add( match.ShortName, match );
                }

                if ( match.LongName != string.Empty ) {
                    longMatches.Add( match.LongName, match );
                }

                req.Remove( option );
            }

            if ( req.Count > 0 ) {
                var arg = req.First();

                throw new ArgumentException(
                    CreateErrorMessage( "Expected argument ", arg.ShortName, arg.LongName ) );
            }

            return new Matches(
                context.Free.AsReadOnly(),
                new ReadOnlyDictionary<string, Match>( shortMatches ),
                new ReadOnlyDictionary<string, Match>( longMatches ) );
        }

        public Options( string shortPrefix = "-", string longPrefix = "--" ) {
            if ( shortPrefix == null ) {
                throw new ArgumentNullException( nameof( shortPrefix ) );
            }
            if ( longPrefix == null ) {
                throw new ArgumentNullException( nameof( longPrefix ) );
            }

            shortPrefix = shortPrefix.Trim();
            if ( shortPrefix == string.Empty ) {
                throw new ArgumentException( "Prefix must not be empty.", nameof( shortPrefix ) );
            }

            longPrefix = longPrefix.Trim();
            if ( longPrefix == string.Empty ) {
                throw new ArgumentException( "Prefix must not be empty.", nameof( longPrefix ) );
            }

            if ( shortPrefix.Any( x => char.IsWhiteSpace( x ) || char.IsControl( x ) ) ) {
                throw new ArgumentException( "Prefix must not contain control or white space characters.", nameof( shortPrefix ) );
            }
            if ( longPrefix.Any( x => char.IsWhiteSpace( x ) || char.IsControl( x ) ) ) {
                throw new ArgumentException( "Prefix must not contain control or white space characters.", nameof( longPrefix ) );
            }

            if ( shortPrefix == longPrefix ) {
                throw new ArgumentException( "Short and long prefix must not be the same." );
            }

            this.shortPrefix = shortPrefix;
            this.longPrefix = longPrefix;
        }
    }*/
}

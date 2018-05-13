using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DD.GetOpts {
    public sealed class Options {
        private const string SHORT_PREFIX = "-";
        private const string LONG_PREFIX = "--";

        private readonly HashSet<Option> required = new HashSet<Option>();
        private readonly HashSet<Option> allOptions = new HashSet<Option>();
        private readonly Dictionary<string, Option> shortOptions = new Dictionary<string, Option>();
        private readonly Dictionary<string, Option> longOptions = new Dictionary<string, Option>();

        public Options Add( string shortName, string longName, Argument arguments, Occur occurs ) {
            if ( shortName == null ) {
                throw new ArgumentNullException( nameof( shortName ) );
            }
            if ( longName == null ) {
                throw new ArgumentNullException( nameof( longName ) );
            }
            if ( ( byte )arguments > 0x02 ) {
                throw new ArgumentException( $"Invalid {nameof( arguments )} value {arguments}.", nameof( arguments ) );
            }
            if ( ( byte )occurs > 0x02 ) {
                throw new ArgumentException( $"Invalid {nameof( occurs )} value {occurs}.", nameof( occurs ) );
            }

            shortName = shortName.Trim();
            longName = longName.Trim();

            if ( shortName == string.Empty && longName == string.Empty ) {
                throw new ArgumentException( "Option must contain at least one valid name." );
            }
            if ( shortName.Any( x => char.IsWhiteSpace( x ) || char.IsControl( x ) ) ) {
                throw new ArgumentException( "Short name must not contain control or white space characters.", nameof( shortName ) );
            }
            if ( longName.Any( x => char.IsWhiteSpace( x ) || char.IsControl( x ) ) ) {
                throw new ArgumentException( "Long name must not contain control or white space characters.", nameof( longName ) );
            }
            if ( shortName.StartsWith( SHORT_PREFIX ) ) {
                throw new ArgumentException( $"Short name starts with the short prefix {SHORT_PREFIX}.", nameof( shortName ) );
            }
            if ( shortName.StartsWith( LONG_PREFIX ) ) {
                throw new ArgumentException( $"Short name starts with the long prefix {LONG_PREFIX}.", nameof( longName ) );
            }
            if ( longName.StartsWith( SHORT_PREFIX ) ) {
                throw new ArgumentException( $"Long name starts with the short prefix {SHORT_PREFIX}.", nameof( longName ) );
            }
            if ( longName.StartsWith( LONG_PREFIX ) ) {
                throw new ArgumentException( $"Long name starts with the long prefix {LONG_PREFIX}.", nameof( longName ) );
            }

            var option = new Option( shortName, longName, arguments, occurs );

            if ( shortName != string.Empty ) {
                try {
                    shortOptions.Add( option.ShortName, option );
                } catch ( ArgumentException ex ) {
                    throw new ArgumentException( $"Option with short name {option.ShortName} already exists.", ex );
                }
            }

            if ( longName != string.Empty ) {
                try {
                    longOptions.Add( option.LongName, option );
                } catch ( ArgumentException ex ) {
                    throw new ArgumentException( $"Option with long name {option.LongName} already exists.", ex );
                }
            }

            allOptions.Add( option );
            if ( option.Occurs == Occur.ONCE ) {
                required.Add( option );
            }
            return this;
        }

        public bool ContainsShort( string shortName )
            => shortOptions.ContainsKey( shortName );

        public bool ContainsLong( string longName )
            => longOptions.ContainsKey( longName );

        public Matches Parse( IEnumerable<string> arguments ) {
            var context = new Context( arguments.GetEnumerator() );

            Read( ref context, null );

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

        private void Read( ref Context context, Option previous ) {
            if ( !context.Enumerator.MoveNext() ) {
                if ( previous?.Arguments == Argument.REQUIRED ) {
                    throw new ArgumentException(
                        CreateErrorMessage( "Expected argument after ", previous.ShortName, previous.LongName ) );
                }
                return;
            }

            var argument = context.Enumerator.Current.Trim();
            var isShort = argument.StartsWith( SHORT_PREFIX );
            var isLong = argument.StartsWith( LONG_PREFIX );

            Option option = null;

            if ( isLong && isShort ) {
                if ( LONG_PREFIX.Length > SHORT_PREFIX.Length ) {
                    option = ReadOption( ref context, previous, argument, LONG_PREFIX, longOptions );
                } else {
                    option = ReadOption( ref context, previous, argument, SHORT_PREFIX, shortOptions );
                }

                previous = option ?? throw new ArgumentException( $"Invalid argument {argument}." );

            } else if ( isShort ) {
                previous = ReadOption( ref context, previous, argument, SHORT_PREFIX, shortOptions )
                    ?? throw new ArgumentException( $"Invalid argument {argument}." );

            } else if ( isLong ) {
                previous = ReadOption( ref context, previous, argument, LONG_PREFIX, longOptions )
                    ?? throw new ArgumentException( $"Invalid argument {argument}." );

            } else {
                if ( previous?.Arguments != Argument.NONE ) {
                    context.Arguments[ previous ].Add( argument );
                } else {
                    context.Free.Add( argument );
                }

                previous = null;
            }

            Read( ref context, previous );
        }

        private Option ReadOption(
            ref Context context,
            Option previous,
            string argument,
            string prefix,
            Dictionary<string, Option> options ) {

            if ( previous?.Arguments == Argument.REQUIRED ) {
                throw new ArgumentException( $"Expected argument after {argument}." );
            }

            var name = argument.Remove( 0, prefix.Length );
            if ( !options.TryGetValue( name, out var option ) ) {
                return null;
            }

            context.Options.Add( option );
            UpdateOptionOccurance( ref context, option );

            if ( !context.Arguments.ContainsKey( option ) ) {
                context.Arguments.Add( option, new List<string>() );
            }
            return option;
        }

        private void UpdateOptionOccurance( ref Context context, Option option ) {
            if ( !context.Count.TryGetValue( option, out var count ) ) {
                context.Count.Add( option, 1 );
                return;
            }

            count += 1;
            if ( option.Occurs != Occur.MULTIPLE && count > 1 ) {
                throw new ArgumentException(
                    CreateErrorMessage( "Multiple occurance of ", option.ShortName, option.LongName ) );
            }

            context.Count[ option ] = count;
        }

        private string CreateErrorMessage( string message, string shortName, string longName ) {
            var sb = new StringBuilder( message );

            if ( shortName != string.Empty && longName != string.Empty ) {
                sb.Append( shortName )
                    .Append( " or " )
                    .Append( longName );

            } else if ( shortName != string.Empty ) {
                sb.Append( shortName );

            } else if ( longName != string.Empty ) {
                sb.Append( longName );
            }

            return sb.Append( '.' ).ToString();
        }

        private ref struct Context {
            public List<string> Free {
                get;
            }

            public HashSet<Option> Options {
                get;
            }

            public Dictionary<Option, List<string>> Arguments {
                get;
            }

            public Dictionary<Option, int> Count {
                get;
            }

            public IEnumerator<string> Enumerator {
                get;
            }

            public Context( IEnumerator<string> enumerator )
                => (Enumerator, Free, Options, Arguments, Count)
                = (enumerator,
                new List<string>(),
                new HashSet<Option>(),
                new Dictionary<Option, List<string>>(),
                new Dictionary<Option, int>());
        }
    }

    /*
    public sealed class Options {
        private readonly string shortPrefix;
        private readonly string longPrefix;

        private readonly Dictionary<string, Option> shortOptions = new Dictionary<string, Option>();
        private readonly Dictionary<string, Option> longOptions = new Dictionary<string, Option>();

        /// <summary>
        /// Creates a new <see cref="Options"/> instance.
        /// </summary>
        /// <param name="shortPrefix">The case-sensitive prefix for short options.</param>
        /// <param name="longPrefix">The case-sensitive prefix for long options.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="shortPrefix"/> or <paramref name="longPrefix"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="shortPrefix"/> or <paramref name="longPrefix"/> is empty.
        /// <paramref name="shortPrefix"/> or <paramref name="longPrefix"/> contains white space, or control characters.
        /// <paramref name="shortPrefix"/> and <paramref name="longPrefix"/> are equal.
        /// </exception>
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

        /// <summary>
        /// Adds a new command-line option to the collection.
        /// </summary>
        /// <param name="shortName">
        /// The case-sensitive short-name of the command-line option. Or <c>string.Empty</c>.
        /// </param>
        /// <param name="longName">
        /// The case-sensitive long-name of the command-line option. Or <c>string.Empty</c>.
        /// </param>
        /// <returns>The current <see cref="Options"/> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="shortName"/> or <paramref name="longName"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="shortName"/> or <paramref name="longName"/> is empty.
        /// <paramref name="shortName"/> or <paramref name="longName"/> contains white space, or control characters.
        /// <paramref name="shortName"/> or <paramref name="longName"/> already exist in the current collection.
        /// </exception>
        public Options Add( string shortName, string longName ) {
            if ( shortName == null ) {
                throw new ArgumentNullException( nameof( shortName ) );
            }
            if ( longName == null ) {
                throw new ArgumentNullException( nameof( longName ) );
            }

            shortName = shortName.Trim();
            longName = longName.Trim();

            if ( shortName == string.Empty
                && longName == string.Empty ) {

                throw new ArgumentException( "Option must contain at least one valid name." );
            }

            if ( shortName.Any( x => char.IsWhiteSpace( x ) || char.IsControl( x ) ) ) {
                throw new ArgumentException( "Short name must not contain control or white space characters.", nameof( shortName ) );
            }
            if ( longName.Any( x => char.IsWhiteSpace( x ) || char.IsControl( x ) ) ) {
                throw new ArgumentException( "Long name must not contain control or white space characters.", nameof( longName ) );
            }

            var option = new Option( shortName, longName );
            AddShortOption( option );
            AddLongOption( option );
            return this;
        }

        private void AddShortOption( Option option ) {
            if ( option.ShortName == string.Empty ) {
                return;
            }

            try {
                shortOptions.Add( option.ShortName, option );
            } catch ( ArgumentException ex ) {
                throw new ArgumentException( $"Option with short name {option.ShortName} already exists.", ex );
            }
        }

        private void AddLongOption( Option option ) {
            if ( option.LongName == string.Empty ) {
                return;
            }

            try {
                longOptions.Add( option.LongName, option );
            } catch ( ArgumentException ex ) {
                throw new ArgumentException( $"Option with long name {option.LongName} already exists.", ex );
            }
        }

        public IEnumerable<string> Parse( IEnumerable<string> arguments ) {
            var matches = new List<string>();

            foreach ( var argument in arguments ) {
                var name = argument;

                if ( argument.StartsWith( shortPrefix ) ) { // Check for short option
                    name = name.Remove( 0, shortPrefix.Length );

                    if ( !shortOptions.TryGetValue( name, out var shortOption ) ) {
                        throw new ArgumentException( $"Unknown option {argument}.", nameof( arguments ) );
                    }

                } else if ( argument.StartsWith( longPrefix ) ) { // Check for long option
                   name = name.Remove( 0, longPrefix.Length );

                    if ( longOptions.TryGetValue( name, out var longOption ) ) {
                        throw new ArgumentException( $"Unknown option {argument}.", nameof( arguments ) );
                    }
                }

                matches.Add( name );
            }

            return matches;
        }

        /// <summary>
        /// The command line option.
        /// </summary>
        private sealed class Option {
            /// <summary>
            /// Gets the short name of the option.
            /// </summary>
            /// <returns>The short name as a <c>string</c>.</returns>
            public string ShortName {
                get;
            }

            /// <summary>
            /// Gets the long name of the option.
            /// </summary>
            /// <returns>The short name as a <c>string</c>.</returns>
            public string LongName {
                get;
            }

            /// <summary>
            /// Creates a new <see cref="Option"/> instance.
            /// </summary>
            /// <param name="shortName">The short name of the option.</param>
            /// <param name="longName">The long name of the option.</param>
            public Option( string shortName, string longName ) {
                ShortName = shortName;
                LongName = longName;
            }
        }
    }*/
}

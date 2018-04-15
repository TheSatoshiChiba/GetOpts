﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DD.GetOpts {
    public enum Occur : byte {
        ONCE = 0x00,
        OPTIONAL = 0x01,
        MULTIPLE = 0x02,
    }

    public enum Argument : byte {
        NONE = 0x00,
        REQUIRED = 0x01,
        OPTIONAL = 0x02,
    }

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

                shortMatches.Add( match.ShortName, match );
                longMatches.Add( match.LongName, match );

                req.Remove( option );
            }

            if ( req.Count > 0 ) {
                var arg = req.First();
                throw new ArgumentException( $"Expected argument {arg.ShortName} or {arg.LongName}." );
            }

            return new Matches(
                context.Free.AsReadOnly(),
                new ReadOnlyDictionary<string, Match>( shortMatches ),
                new ReadOnlyDictionary<string, Match>( longMatches ) );
        }

        private void Read( ref Context context, Option previous ) {
            var argument = context.Enumerator.Current;

            if ( !context.Enumerator.MoveNext() ) {
                if ( previous?.Arguments == Argument.REQUIRED ) {
                    throw new ArgumentException( $"Expected argument after {argument}." );
                }
                return;
            }

            argument = context.Enumerator.Current.Trim();
            if ( argument.StartsWith( SHORT_PREFIX ) ) {
                if ( previous?.Arguments == Argument.REQUIRED ) {
                    throw new ArgumentException( $"Expected argument after {argument}." );
                }

                var name = argument.Remove( 0, SHORT_PREFIX.Length );
                if ( shortOptions.TryGetValue( name, out var option ) ) {
                    context.Options.Add( option );
                    UpdateOptionOccurance( ref context, option );
                    previous = option;

                } else {
                    throw new ArgumentException( $"Invalid argument {argument}." );
                }

            } else if ( argument.StartsWith( LONG_PREFIX ) ) {
                if ( previous?.Arguments == Argument.REQUIRED ) {
                    throw new ArgumentException( $"Expected argument after {argument}." );
                }

                var name = argument.Remove( 0, LONG_PREFIX.Length );
                if ( longOptions.TryGetValue( name, out var option ) ) {
                    context.Options.Add( option );
                    UpdateOptionOccurance( ref context, option );
                    previous = option;

                } else {
                    throw new ArgumentException( $"Invalid argument {argument}." );
                }

            } else if ( argument != string.Empty ) {
                if ( previous?.Arguments != Argument.NONE ) {
                    if ( context.Arguments.ContainsKey( previous ) ) {
                        context.Arguments[ previous ].Add( argument );
                    } else {
                        context.Arguments.Add( previous, new List<string>() { argument } );
                    }

                } else {
                    context.Free.Add( argument );
                }

                previous = null;
            }

            Read( ref context, previous );
        }

        private void UpdateOptionOccurance( ref Context context, Option option ) {
            if ( !context.Count.TryGetValue( option, out var count ) ) {
                context.Count.Add( option, 1 );
                return;
            }

            count += 1;
            if ( option.Occurs != Occur.MULTIPLE && count > 1 ) {
                throw new ArgumentException( $"Multiple occurance of {option.ShortName} or {option.LongName}." );
            }

            context.Count[ option ] = count;
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

        private sealed class Option : IEquatable<Option> {
            public string ShortName {
                get;
            }

            public string LongName {
                get;
            }

            public Argument Arguments {
                get;
            }

            public Occur Occurs {
                get;
            }

            public Option( string shortName, string longName, Argument arguments, Occur occurs )
                => (ShortName, LongName, Arguments, Occurs)
                = (shortName ?? throw new ArgumentNullException( nameof( shortName ) ),
                longName ?? throw new ArgumentNullException( nameof( longName ) ),
                arguments,
                occurs);

            public bool Equals( Option other ) {
                if ( this == other ) {
                    return true;
                }

                return other != null
                    && ShortName == other.ShortName
                    && LongName == other.LongName
                    && Arguments == other.Arguments
                    && Occurs == other.Occurs;
            }

            public override bool Equals( object obj )
                => Equals( obj as Option );

            public override int GetHashCode() {
                unchecked {
                    int hash = 31;
                    hash = ( hash * 17 ) + ShortName.GetHashCode();
                    hash = ( hash * 17 ) + LongName.GetHashCode();
                    hash = ( hash * 17 ) + Arguments.GetHashCode();
                    hash = ( hash * 17 ) + LongName.GetHashCode();
                    return hash;
                }
            }

            public override string ToString()
                => $"{ShortName}, {LongName}, {Arguments}, {Occurs}";
        }
    }

    internal sealed class Match : IEquatable<Match> {
        public string ShortName {
            get;
        }

        public string LongName {
            get;
        }

        public int Count {
            get;
        }

        public ReadOnlyCollection<string> Arguments {
            get;
        }

        public Match( string shortName, string longName, int count, ReadOnlyCollection<string> arguments )
            => (ShortName, LongName, Count, Arguments)
            = (shortName ?? throw new ArgumentNullException( nameof( shortName ) ),
            longName ?? throw new ArgumentNullException( nameof( longName ) ),
            count >= 0 ? count : throw new ArgumentException( nameof( count ) ),
            arguments ?? throw new ArgumentNullException( nameof( arguments ) ));

        public bool Equals( Match other ) {
            if ( this == other ) {
                return true;
            }

            return other != null
                && ShortName == other.ShortName
                && LongName == other.LongName
                && Count == other.Count
                && Arguments.SequenceEqual( other.Arguments );
        }

        public override bool Equals( object obj )
            => Equals( obj as Match );

        public override int GetHashCode() {
            unchecked {
                int hash = 31;
                hash = ( hash * 17 ) + ShortName.GetHashCode();
                hash = ( hash * 17 ) + LongName.GetHashCode();
                hash = ( hash * 17 ) + Count.GetHashCode();

                foreach ( var argument in Arguments ) {
                    hash = ( hash * 17 ) + argument.GetHashCode();
                }

                return hash;
            }
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append( ShortName )
                .Append( ", " )
                .Append( LongName )
                .Append( ", Count: " )
                .Append( Count )
                .Append( ", Arguments: [" );

            bool first = true;
            foreach ( var argument in Arguments ) {
                if ( !first ) {
                    sb.Append( ", " );
                } else {
                    first = false;
                }

                sb.Append( '\"' )
                    .Append( argument )
                    .Append( '\"' );
            }

            sb.Append( ']' );

            return sb.ToString();
        }
    }

    public sealed class Matches {
        public ReadOnlyCollection<string> Arguments {
            get;
        }

        private readonly ReadOnlyDictionary<string, Match> shortMatches;
        private readonly ReadOnlyDictionary<string, Match> longMatches;

        internal Matches(
            ReadOnlyCollection<string> arguments,
            ReadOnlyDictionary<string, Match> shortMatches,
            ReadOnlyDictionary<string, Match> longMatches )
            => (Arguments, this.shortMatches, this.longMatches)
            = (arguments ?? throw new ArgumentNullException( nameof( arguments ) ),
            shortMatches ?? throw new ArgumentNullException( nameof( shortMatches ) ),
            longMatches ?? throw new ArgumentNullException( nameof( longMatches ) ));

        public bool ContainsShort( string name )
            => shortMatches.ContainsKey( name );

        public bool ContainsLong( string name )
            => longMatches.ContainsKey( name );

        public int ShortCount( string name )
            => shortMatches.TryGetValue( name, out var match ) ? match.Count : 0;

        public int LongCount( string name )
            => longMatches.TryGetValue( name, out var match ) ? match.Count : 0;

        public ReadOnlyCollection<string> GetShortArguments( string name )
            => shortMatches.TryGetValue( name, out var match ) ? match.Arguments : null;

        public ReadOnlyCollection<string> GetLongArguments( string name )
            => longMatches.TryGetValue( name, out var match ) ? match.Arguments : null;
    }
}

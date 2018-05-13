using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DD.GetOpts {
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
        private static readonly ReadOnlyCollection<string> EMPTY
            = new ReadOnlyCollection<string>( new List<string>() );

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
            => shortMatches.TryGetValue( name, out var match ) ? match.Arguments : EMPTY;

        public ReadOnlyCollection<string> GetLongArguments( string name )
            => longMatches.TryGetValue( name, out var match ) ? match.Arguments : EMPTY;
    }
}

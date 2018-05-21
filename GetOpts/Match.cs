using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace DD.GetOpts {
    /// <summary>
    /// The match of a command line <see cref="Option"/>.
    /// </summary>
    public sealed class Match : IEquatable<Match> {
        /// <summary>
        /// Gets the short name of the matched command line
        /// <see cref="Option"/>.
        /// </summary>
        /// <returns>
        /// The short name of the matched <see cref="Option"/>.
        /// </returns>
        public string ShortName { get; }

        /// <summary>
        /// Gets the long name of the matched command line
        /// <see cref="Option"/>.
        /// </summary>
        /// <returns>
        /// The long name of the matched <see cref="Option"/>.
        /// </returns>
        public string LongName { get; }

        /// <summary>
        /// Gets the non-negative non-zero number of times the command line
        /// <see cref="Option"/> was matched.
        /// </summary>
        /// <returns>The non-negative non-zero number of matches.</returns>
        public int Count { get; }

        /// <summary>
        /// Gets the read-only collection of arguments that have been passed to
        /// all matched command line options.
        /// </summary>
        /// <returns>
        /// The read-only collection of <see cref="string"/> arguments.
        /// </returns>
        public IReadOnlyCollection<string> Arguments { get; }

        /// <summary>
        /// Initializes a new <see cref="Match"/>.
        /// </summary>
        /// <param name="shortName">
        /// The short name of the matched command line <see cref="Option"/>.
        /// </param>
        /// <param name="longName">
        /// The long name of the matched command line <see cref="Option"/>.
        /// </param>
        /// <param name="count">
        /// The non-negative non-zero number of times the command line
        /// <see cref="Option"/> was matched.
        /// </param>
        /// <param name="arguments">
        /// The collection of matched arguments.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="shortName"/>, <paramref name="longName"/>, or
        /// <paramref name="arguments" is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="count"/> is either negative or zero.
        /// </exception>
        public Match(
            string shortName,
            string longName,
            int count,
            IReadOnlyCollection<string> arguments ) {

            ShortName = shortName
                ?? throw new ArgumentNullException( nameof( shortName ) );
            LongName = longName
                ?? throw new ArgumentNullException( nameof( longName ) );
            Arguments = arguments
                ?? throw new ArgumentNullException( nameof( arguments ) );

            Count = count > 0
                ? count
                : throw new ArgumentOutOfRangeException(
                    nameof( count ),
                    $"Invalid {nameof(Match)} count {count}" );
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override bool Equals( object obj ) => Equals( obj as Match );

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
}

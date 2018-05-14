using System;
using System.Linq;
using System.Text;

namespace DD.GetOpts {
    /// <summary>
    /// The occurance of a <see cref="Option"/>.
    /// </summary>
    public enum Occur : byte {
        /// <summary>
        /// The <see cref="Option"/> has to be present exactly once.
        /// </summary>
        ONCE = 0x00,

        /// <summary>
        /// The <see cref="Option"/> is optional and can be present once.
        /// </summary>
        OPTIONAL = 0x01,

        /// <summary>
        /// The <see cref="Option"/> is optional and can be present multiple
        /// times.
        /// </summary>
        MULTIPLE = 0x02,
    }

    /// <summary>
    /// The occurence of a argument after the <see cref="Option"/>.
    /// </summary>
    public enum Argument : byte {
        /// <summary>
        /// The <see cref="Option"/> has no argument.
        /// </summary>
        NONE = 0x00,

        /// <summary>
        /// The <see cref="Option"/> argument is required.
        /// </summary>
        REQUIRED = 0x01,

        /// <summary>
        /// The <see cref="Option"/> argument is optional.
        /// </summary>
        OPTIONAL = 0x02,
    }

    /// <summary>
    /// The command line option.
    /// </summary>
    public sealed class Option : IEquatable<Option> {
        /// <summary>
        /// Gets the short name of the current command line
        /// <see cref="Option"/>.
        /// </summary>
        /// <returns>
        /// The short name of the current <see cref="Option"/>.
        /// </returns>
        public string ShortName { get; }

        /// <summary>
        /// Gets the long name of the current command line
        /// <see cref="Option"/>.
        /// </summary>
        /// <returns>
        /// The long name of the current <see cref="Option"/>.
        /// </returns>
        public string LongName { get; }

        /// <summary>
        /// Gets the <see cref="Argument"/> occurance of the current command
        /// line <see cref="Option"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="Argument"/> occurance of the current command line
        /// <see cref="Option"/>.
        /// </returns>
        public Argument Arguments { get; }

        /// <summary>
        /// Gets the occurance option of the current command line
        /// <see cref="Option"/>.
        /// </summary>
        /// <returns>
        /// The occurance option of the current command line
        /// <see cref="Option"/>.
        /// </returns>
        public Occur Occurs { get; }

        /// <summary>
        /// Initializes a new <see cref="Option"/>.
        /// </summary>
        /// <param name="shortName">
        /// The short name of the command line option.
        /// </param>
        /// <param name="longName">
        /// The long name of the command line option.
        /// </param>
        /// <param name="arguments">
        /// The occurence of a argument after the command line option.
        /// </param>
        /// <param name="occurs">
        /// The occurance of the command line option.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="shortName"/> or <paramref name="longName"/> is
        /// <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="arguments"/> or <paramref name="occurs"/> is
        /// invalid.
        /// <paramref name="shortName"/> and <paramref name="longName"/> are
        /// empty.
        /// <paramref name="shortName"/> or <paramref name="longName"/> contain
        /// invalid whitespace or control characters.
        /// </exception>
        public Option(
            string shortName,
            string longName,
            Argument arguments,
            Occur occurs ) {

            shortName = Format( shortName, nameof( shortName ) );
            longName = Format( longName, nameof( longName ) );

            if ( shortName == string.Empty && longName == string.Empty ) {
                throw new ArgumentException(
                    $"{nameof( shortName )} and {nameof( longName )} are empty",
                    $"{nameof( shortName )}, {nameof( longName )}" );
            }

            ShortName = shortName;
            LongName = longName;

            Arguments = (byte)arguments <= 0x02
                ? arguments
                : throw new ArgumentException(
                    $"Invalid {nameof(Argument)} option {arguments}",
                    nameof( arguments ) );

            Occurs = (byte)occurs <= 0x02
                ? occurs
                : throw new ArgumentException(
                    $"Invalid {nameof(Occur)} option {occurs}",
                    nameof( occurs ) );

            string Format( string name, string paramName ) {
                if ( name == null ) {
                    throw new ArgumentNullException( paramName );
                }
                name = name.Trim();
                if ( name.Any(
                    x => char.IsWhiteSpace( x ) || char.IsControl( x ) ) ) {

                    throw new ArgumentException(
                        paramName
                        + " must not contain control or white space characters",
                        paramName );
                }
                return name;
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override bool Equals( object obj ) => Equals( obj as Option );

        /// <inheritdoc/>
        public override int GetHashCode() {
            unchecked {
                int hash = 31;
                hash = ( hash * 17 ) + ShortName.GetHashCode();
                hash = ( hash * 17 ) + LongName.GetHashCode();
                hash = ( hash * 17 ) + Arguments.GetHashCode();
                hash = ( hash * 17 ) + Occurs.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
            => $"{ShortName}, {LongName}, {Arguments}, {Occurs}";
    }
}

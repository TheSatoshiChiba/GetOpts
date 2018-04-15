using System;
using System.Collections.Generic;
using System.Linq;

namespace DD.GetOpts {
    /// <summary>
    /// The command-line options.
    /// </summary>
    /// <remarks>
    /// <para>Added in version 0.1.0.</para>
    /// </remarks>
    /// 
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

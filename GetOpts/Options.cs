using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DD.GetOpts {
    public sealed class Options {
        private readonly Dictionary<string, Option> shortOptions = new Dictionary<string, Option>();
        private readonly Dictionary<string, Option> longOptions = new Dictionary<string, Option>();

        public Options() {
        }

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

        private sealed class Option {
            public string ShortName {
                get;
            }

            public string LongName {
                get;
            }

            public Option( string shortName, string longName ) {
                ShortName = shortName;
                LongName = longName;
            }
        }
    }
}

// Option styles:
// short = -
// long = --
// short != long
// 
// Options:
// short name != null
// long name != null
// short name == long name

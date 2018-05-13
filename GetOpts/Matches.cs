using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DD.GetOpts {


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

        public IReadOnlyCollection<string> GetShortArguments( string name )
            => shortMatches.TryGetValue( name, out var match ) ? match.Arguments : EMPTY;

        public IReadOnlyCollection<string> GetLongArguments( string name )
            => longMatches.TryGetValue( name, out var match ) ? match.Arguments : EMPTY;
    }
}

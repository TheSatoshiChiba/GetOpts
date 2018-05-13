using NUnit.Framework;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace DD.GetOpts.Tests {
    public class ParsingTests {
        /*
        public static readonly char[] WHITE_SPACE_CHARACTERS = new[] {
            '\u0020', '\u00A0', '\u1680', '\u2000', '\u2001',
            '\u2002', '\u2003', '\u2004', '\u2005', '\u2006',
            '\u2007', '\u2008', '\u2009', '\u200A', '\u202F',
            '\u205F', '\u3000', '\u2028', '\u2029', '\u0009',
            '\u000A', '\u000B', '\u000C', '\u000D', '\u0085',
        };

        public static readonly char[] CONTROL_CHARACTERS = new[] {
            '\u0000', '\u0001', '\u0002', '\u0003', '\u0004',
            '\u0005', '\u0006', '\u0007', '\u0008', '\u000E',
            '\u000F', '\u0010', '\u0011', '\u0012', '\u0013',
            '\u0014', '\u0015', '\u0016', '\u0017', '\u0018',
            '\u0019', '\u001A', '\u001B', '\u001C', '\u001D',
            '\u001E', '\u001F', '\u007F', '\u0080', '\u0081',
            '\u0082', '\u0083', '\u0084', '\u0086', '\u0087',
            '\u0088', '\u0089', '\u008A', '\u008B', '\u008C',
            '\u008D', '\u008E', '\u008F', '\u0090', '\u0091',
            '\u0092', '\u0093', '\u0094', '\u0095', '\u0096',
            '\u0097', '\u0098', '\u0099', '\u009A', '\u009B',
            '\u009C', '\u009D', '\u009E', '\u009F',
        };

        private Options options;

        [SetUp]
        public void BeginTest() {
            options = new Options();
        }

        [TearDown]
        public void EndTest() {
            // no-op.
        }


        [TestCase( Argument.NONE, "a", "", "-a", "" )]
        [TestCase( Argument.OPTIONAL, "a", "", "-a", "hello" )]
        [TestCase( Argument.OPTIONAL, "a", "", "-a", "hello" )]



        [TestCase( Argument.NONE, "", "a", "--a", "" )]
        [TestCase( Argument.NONE, "", "a", "--a", "hello" )]
        [TestCase( Argument.NONE, "a", "a", "-a", "" )]
        [TestCase( Argument.NONE, "a", "a", "-a", "hello" )]
        [TestCase( Argument.NONE, "a", "a", "--a", "" )]
        [TestCase( Argument.NONE, "a", "a", "--a", "hello" )]
        public void ParseOnceTest(
            Argument argumentOption,
            string shortName,
            string longName,
            string argument1,
            string argument2 ) {

            options.Add( shortName, longName, argumentOption, Occur.ONCE );

            var matches = options.Parse( BuildArguments( argument ) );

            Assert.That( matches.Arguments.Count, Is.EqualTo( 0 ) );

            if ( shortName != string.Empty ) {
                AssertShortMatches( matches, shortName, true, 1, Enumerable.Empty<string>() );
            } else {
                AssertShortMatches( matches, string.Empty, false, 0, Enumerable.Empty<string>() );
            }

            if ( longName != string.Empty ) {
                AssertLongMatches( matches, longName, true, 1, Enumerable.Empty<string>() );
            } else {
                AssertLongMatches( matches, string.Empty, false, 0, Enumerable.Empty<string>() );
            }
        }

        [TestCase( "a", "", "-a", "" )]
        [TestCase( "", "a", "--a", "" )]
        [TestCase( "a", "", "-a", "hello" )]
        [TestCase( "", "a", "--a", "hello" )]
        public void ParseOnceOptionalTest(
            string shortName,
            string longName,
            string argument1,
            string argument2 ) {

            options.Add( shortName, longName, Argument.OPTIONAL, Occur.ONCE );

            var arguments = BuildArguments( argument1, argument2 );
            var matches = options.Parse( arguments );

            Assert.That( matches.Arguments.Count, Is.EqualTo( 0 ) );

            if ( shortName != string.Empty ) {
                AssertShortMatches( matches, shortName, true, 1, arguments.Last() );
            } else {
                AssertShortMatches( matches, string.Empty, false, 0, Enumerable.Empty<string>() );
            }

            if ( longName != string.Empty ) {
                AssertLongMatches( matches, longName, true, 1, arguments.Last() );
            } else {
                AssertLongMatches( matches, string.Empty, false, 0, Enumerable.Empty<string>() );
            }
        }

        [TestCase( "a", "", "b", "", "-b" )]
        [TestCase( "a", "", "", "b", "--b" )]
        [TestCase( "", "a", "b", "", "-b" )]
        [TestCase( "", "a", "", "b", "--b" )]
        public void ParsOnceMissingTest(
            string shortName1,
            string longName1,
            string shortName2,
            string longName2,
            string argument ) {

            options.Add( shortName1, longName1, Argument.NONE, Occur.ONCE );
            options.Add( shortName2, longName2, Argument.NONE, Occur.OPTIONAL );

            Assert.That(
                () => options.Parse( BuildArguments( argument ) ),
                Throws.ArgumentException
                .With.Message.EqualTo( "Expected argument a." ) );
        }

        [TestCase( "-a", "-a" )]
        [TestCase( "-a", "--b" )]
        [TestCase( "--b", "-a" )]
        [TestCase( "--b", "--b" )]
        public void ParseShortAndLongOnceTest( string argument1, string argument2 ) {
            options.Add( "a", "b", Argument.NONE, Occur.ONCE );

            Assert.That(
                () => options.Parse( BuildArguments( argument1, argument2 ) ),
                Throws.ArgumentException
                .With.Message.EqualTo( "Multiple occurance of a or b." ) );
        }

        [TestCase( "-a", "", "-a", "" )]
        [TestCase( "-a", "", "--b", "" )]
        [TestCase( "--b", "", "-a", "" )]
        [TestCase( "--b", "", "--b", "" )]
        public void ParseShortAndLongOnceOptionalTest( string argument1, string argument2 ) {
            options.Add( "a", "b", Argument.NONE, Occur.ONCE );

            Assert.That(
                () => options.Parse( new[] { argument1, argument2 } ),
                Throws.ArgumentException
                .With.Message.EqualTo( "Multiple occurance of a or b." ) );
        }

        private IEnumerable<string> BuildArguments( params string[] arguments )
            => arguments.Where( x => x != string.Empty );

        private void AssertShortMatches(
            Matches matches,
            string name,
            bool contains,
            int count,
            IEnumerable arguments ) {

            Assert.That( matches.ContainsShort( name ), Is.EqualTo( contains ) );
            Assert.That( matches.GetShortArguments( name ), Is.EquivalentTo( arguments ) );
            Assert.That( matches.ShortCount( name ), Is.EqualTo( count ) );
        }

        private void AssertLongMatches(
            Matches matches,
            string name,
            bool contains,
            int count,
            IEnumerable arguments ) {

            Assert.That( matches.ContainsLong( name ), Is.EqualTo( contains ) );
            Assert.That( matches.GetLongArguments( name ), Is.EquivalentTo( arguments ) );
            Assert.That( matches.LongCount( name ), Is.EqualTo( count ) );
        }

        /*
         * var arguments2 = new[] { "-a", "-a" };
            var arguments6 = new[] { "-x", "-y" };
         */
        //[Test]
        //public void CreationTest() {
        //    Assert.That(
        //        () => new Options(),
        //        Throws.Nothing );

        //    Assert.That(
        //        () => new Options( "\\" ),
        //        Throws.Nothing );

        //    Assert.That(
        //        () => new Options( "\\\\" ),
        //        Throws.Nothing );

        //    Assert.That(
        //        () => new Options( "a", "a" ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( "Short and long prefix must not be the same." ) );
        //}

        //[Test]
        //public void CreationNullTest() {
        //    Assert.That(
        //        () => new Options( shortPrefix: null ),
        //        Throws.ArgumentNullException
        //        .With.Message.Contain( "shortPrefix" ) );

        //    Assert.That(
        //        () => new Options( longPrefix: null ),
        //        Throws.ArgumentNullException
        //        .With.Message.Contain( "longPrefix" ) );

        //    Assert.That(
        //        () => new Options( shortPrefix: null, longPrefix: null ),
        //        Throws.ArgumentNullException
        //        .With.Message.Contain( "shortPrefix" ) );
        //}

        //[Test]
        //public void CreationEmptyTest() {
        //    var error = "Prefix must not be empty.";

        //    Assert.That(
        //        () => new Options( shortPrefix: string.Empty ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error )
        //        .And.Message.Contain( "shortPrefix" ) );

        //    Assert.That(
        //        () => new Options( longPrefix: string.Empty ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error )
        //        .And.Message.Contain( "longPrefix" ) );

        //    Assert.That(
        //        () => new Options( shortPrefix: string.Empty, longPrefix: string.Empty ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error )
        //        .And.Message.Contain( "shortPrefix" ) );
        //}

        //[TestCaseSource( nameof( WHITE_SPACE_CHARACTERS ) )]
        //public void CreationWhiteSpaceTest( char whiteSpace ) {
        //    var prefix = whiteSpace.ToString();
        //    var error = "Prefix must not be empty.";

        //    Assert.That(
        //        () => new Options( shortPrefix: prefix ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error )
        //        .And.Message.Contain( "shortPrefix" ) );

        //    Assert.That(
        //        () => new Options( longPrefix: prefix ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error )
        //        .And.Message.Contain( "longPrefix" ) );

        //    Assert.That(
        //        () => new Options( shortPrefix: prefix, longPrefix: prefix ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error )
        //        .And.Message.Contain( "shortPrefix" ) );

        //    var prefix2 = $"{whiteSpace}Foo{whiteSpace}Bar{whiteSpace}";
        //    var error2 = "Prefix must not contain control or white space characters.";

        //    Assert.That(
        //        () => new Options( shortPrefix: prefix2 ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error2 )
        //        .And.Message.Contain( "shortPrefix" ) );

        //    Assert.That(
        //        () => new Options( longPrefix: prefix2 ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error2 )
        //        .And.Message.Contain( "longPrefix" ) );

        //    Assert.That(
        //        () => new Options( shortPrefix: prefix2, longPrefix: prefix2 ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error2 )
        //        .And.Message.Contain( "shortPrefix" ) );
        //}

        //[TestCaseSource( nameof( CONTROL_CHARACTERS ) )]
        //public void CreationControlTest( char control ) {
        //    var prefix = control.ToString();
        //    var error = "Prefix must not contain control or white space characters.";

        //    Assert.That(
        //        () => new Options( shortPrefix: prefix ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error )
        //        .And.Message.Contain( "shortPrefix" ) );

        //    Assert.That(
        //        () => new Options( longPrefix: prefix ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error )
        //        .And.Message.Contain( "longPrefix" ) );

        //    Assert.That(
        //        () => new Options( shortPrefix: prefix, longPrefix: prefix ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error )
        //        .And.Message.Contain( "shortPrefix" ) );

        //    var prefix2 = $"Foo{control}Bar";

        //    Assert.That(
        //        () => new Options( shortPrefix: prefix2 ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error )
        //        .And.Message.Contain( "shortPrefix" ) );

        //    Assert.That(
        //        () => new Options( longPrefix: prefix2 ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error )
        //        .And.Message.Contain( "longPrefix" ) );

        //    Assert.That(
        //        () => new Options( shortPrefix: prefix2, longPrefix: prefix2 ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error )
        //        .And.Message.Contain( "shortPrefix" ) );
        //}

        //[Test]
        //public void AddNullTest() {
        //    var options = new Options();

        //    Assert.That(
        //        () => options.Add( null, string.Empty ),
        //        Throws.ArgumentNullException
        //        .With.Message.Contain( "shortName" ) );

        //    Assert.That(
        //        () => options.Add( string.Empty, null ),
        //        Throws.ArgumentNullException
        //        .With.Message.Contain( "longName" ) );

        //    Assert.That(
        //        () => options.Add( null, null ),
        //        Throws.ArgumentNullException
        //        .With.Message.Contain( "shortName" ) );
        //}

        //[TestCaseSource( nameof( WHITE_SPACE_CHARACTERS ) )]
        //public void AddWhiteSpaceTest( char whiteSpace ) {
        //    var options = new Options();
        //    var name = whiteSpace.ToString();
        //    var error = "Option must contain at least one valid name.";

        //    Assert.That(
        //        () => options.Add( name, string.Empty ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error ) );

        //    Assert.That(
        //        () => options.Add( string.Empty, name ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error ) );

        //    Assert.That(
        //        () => options.Add( name, name ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error ) );

        //    var name2 = $"{whiteSpace}Foo{whiteSpace}Bar{whiteSpace}";
        //    var error2 = "Short name must not contain control or white space characters.";
        //    var error3 = "Long name must not contain control or white space characters.";

        //    Assert.That(
        //        () => options.Add( name2, string.Empty ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error2 )
        //        .And.Message.Contain( "shortName" ) );

        //    Assert.That(
        //        () => options.Add( string.Empty, name2 ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error3 )
        //        .And.Message.Contain( "longName" ) );

        //    Assert.That(
        //        () => options.Add( name2, name2 ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error2 )
        //        .And.Message.Contain( "shortName" ) );
        //}

        //[TestCaseSource( nameof( CONTROL_CHARACTERS ) )]
        //public void AddControlTest( char control ) {
        //    var options = new Options();
        //    var name = control.ToString();
        //    var error = "Short name must not contain control or white space characters.";
        //    var error2 = "Long name must not contain control or white space characters.";

        //    Assert.That(
        //        () => options.Add( name, string.Empty ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error )
        //        .And.Message.Contain( "shortName" ) );

        //    Assert.That(
        //        () => options.Add( string.Empty, name ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error2 )
        //        .And.Message.Contain( "longName" ) );

        //    Assert.That(
        //        () => options.Add( name, name ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error )
        //        .And.Message.Contain( "shortName" ) );

        //    var name2 = $"Foo{control}Bar";

        //    Assert.That(
        //        () => options.Add( name2, string.Empty ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error )
        //        .And.Message.Contain( "shortName" ) );

        //    Assert.That(
        //        () => options.Add( string.Empty, name2 ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error2 )
        //        .And.Message.Contain( "longName" ) );

        //    Assert.That(
        //        () => options.Add( name2, name2 ),
        //        Throws.ArgumentException
        //        .With.Message.Contain( error )
        //        .And.Message.Contain( "shortName" ) );
        //}*/
    }
}

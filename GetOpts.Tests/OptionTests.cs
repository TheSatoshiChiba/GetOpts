using System;
using System.Linq;
using NUnit.Framework;

namespace DD.GetOpts.Tests {
    /// <summary>
    /// The <see cref="Option"/> tests.
    /// </summary>
    public class OptionTests {
        private static readonly char[] WHITE_SPACE_CHARACTERS = new[] {
            '\u0020', '\u00A0', '\u1680', '\u2000', '\u2001',
            '\u2002', '\u2003', '\u2004', '\u2005', '\u2006',
            '\u2007', '\u2008', '\u2009', '\u200A', '\u202F',
            '\u205F', '\u3000', '\u2028', '\u2029', '\u0009',
            '\u000A', '\u000B', '\u000C', '\u000D', '\u0085',
        };

        private static readonly char[] CONTROL_CHARACTERS = new[] {
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

        [Test]
        public void OccurValueTest() {
            Assert.That( (byte)Occur.ONCE, Is.EqualTo( 0x00 ) );
            Assert.That( (byte)Occur.OPTIONAL, Is.EqualTo( 0x01 ) );
            Assert.That( (byte)Occur.MULTIPLE, Is.EqualTo( 0x02 ) );

            Assert.That(
                Enum.GetValues( typeof( Occur ) ).Length,
                Is.EqualTo( 3 ) );
        }

        [Test]
        public void ArgumentValueTest() {
            Assert.That( (byte)Argument.NONE, Is.EqualTo( 0x00 ) );
            Assert.That( (byte)Argument.REQUIRED, Is.EqualTo( 0x01 ) );
            Assert.That( (byte)Argument.OPTIONAL, Is.EqualTo( 0x02 ) );

            Assert.That(
                Enum.GetValues( typeof( Argument ) ).Length,
                Is.EqualTo( 3 ) );
        }

        [Test]
        public void CreationTest(
            [Values( 0x00, 0x01, 0x02 )] byte argument,
            [Values( 0x00, 0x01, 0x02 )] byte occur ) {

            var arg = ( Argument )argument;
            var occ = ( Occur )occur;
            var option = new Option( "short", "long", arg, occ );

            Assert.That( option.ShortName, Is.EqualTo( "short" ) );
            Assert.That( option.LongName, Is.EqualTo( "long" ) );
            Assert.That( option.Arguments, Is.EqualTo( arg ) );
            Assert.That( option.Occurs, Is.EqualTo( occ ) );

            Assert.That(
                () => new Option( string.Empty, "long", arg, occ ),
                Throws.Nothing );

            Assert.That(
                () => new Option( "short", string.Empty, arg, occ ),
                Throws.Nothing );

            Assert.That(
                new Option( string.Empty, "long", arg, occ ).ShortName,
                Is.EqualTo( string.Empty ) );

            Assert.That(
                new Option( "short", string.Empty, arg, occ ).LongName,
                Is.EqualTo( string.Empty ) );
        }

        [Test]
        public void FailedCreationTest() {
            var arg = Argument.NONE;
            var occ = Occur.ONCE;

            Assert.That(
                () => new Option( null, "long", arg, occ ),
                Throws.ArgumentNullException
                    .With.Message.Contain( "Parameter name: shortName" ) );

            Assert.That(
                () => new Option( "short", null, arg, occ ),
                Throws.ArgumentNullException
                    .With.Message.Contain( "Parameter name: longName" ) );

            Assert.That(
                () => new Option( string.Empty, string.Empty, arg, occ ),
                Throws.ArgumentException
                    .With.Message.Contain(
                        "shortName and longName are empty" )
                    .And.Message.Contain(
                        "Parameter name: shortName, longName" ) );

            Assert.That(
                () => new Option( "short", "long", ( Argument )0x03, occ ),
                Throws.ArgumentException
                    .With.Message.Contain( "Invalid Argument option 3" )
                    .And.Message.Contain( "Parameter name: arguments" ) );

            Assert.That(
                () => new Option( "short", "long", arg, ( Occur )0x03 ),
                Throws.ArgumentException
                    .With.Message.Contain( "Invalid Occur option 3" )
                    .And.Message.Contain( "Parameter name: occurs" ) );
        }

        [Test]
        public void InvalidCharactersCreationTest() {
            var arg = Argument.NONE;
            var occ = Occur.ONCE;
            var expected = "must not contain control or white space characters";
            var sequence = CONTROL_CHARACTERS.Union( WHITE_SPACE_CHARACTERS );

            foreach ( var character in sequence ) {
                var name = $"foo{character}bar";

                Assert.That(
                    () => new Option( name, "long", arg, occ ),
                    Throws.ArgumentException
                        .With.Message.Contain( "shortName " + expected )
                        .And.Message.Contain( "Parameter name: shortName" ) );

                Assert.That(
                    () => new Option( "short", name, arg, occ ),
                    Throws.ArgumentException
                        .With.Message.Contain( "longName " + expected )
                        .And.Message.Contain( "Parameter name: longName" ) );
            }
        }

        [Test]
        public void TrimCreationTest() {
            var arg = Argument.NONE;
            var occ = Occur.ONCE;

            foreach ( var character in WHITE_SPACE_CHARACTERS ) {
                var prefix = $"{character}foo";
                var suffix = $"foo{character}";
                var both = $"{character}foo{character}";

                Assert.That(
                    () => new Option( prefix, "long", arg, occ ),
                    Throws.Nothing );

                Assert.That(
                    () => new Option( suffix, "long", arg, occ ),
                    Throws.Nothing );

                Assert.That(
                    () => new Option( both, "long", arg, occ ),
                    Throws.Nothing );

                Assert.That(
                    () => new Option( "short", prefix, arg, occ ),
                    Throws.Nothing );

                Assert.That(
                    () => new Option( "short", suffix, arg, occ ),
                    Throws.Nothing );

                Assert.That(
                    () => new Option( "short", both, arg, occ ),
                    Throws.Nothing );
            }
        }

        [Test]
        public void EqualityTest() {
            var arg = Argument.OPTIONAL;
            var occ = Occur.ONCE;
            var option = new Option( "short", "long", arg, occ );

            Assert.That( option.Equals( (object)null ), Is.False );
            Assert.That( option.Equals( (Option)null ), Is.False );
            Assert.That( option.Equals( this ), Is.False );
            Assert.That( option.Equals( option ), Is.True );
            Assert.That( option.Equals( (object)option ), Is.True );
            Assert.That(
                option.GetHashCode(), Is.EqualTo( option.GetHashCode() ) );

            var option2 = new Option( "short", "long", arg, occ );

            Assert.That( option2.Equals( option ), Is.True );
            Assert.That( option2.Equals( (object)option ), Is.True );
            Assert.That(
                option2.GetHashCode(), Is.EqualTo( option.GetHashCode() ) );

            var option3 = new Option( "different", "long", arg, occ );

            Assert.That( option3.Equals( option ), Is.False );
            Assert.That( option3.Equals( (object)option ), Is.False );
            Assert.That(
                option3.GetHashCode(), Is.Not.EqualTo( option.GetHashCode() ) );

            var option4 = new Option( "short", "different", arg, occ );

            Assert.That( option4.Equals( option ), Is.False );
            Assert.That( option4.Equals( (object)option ), Is.False );
            Assert.That(
                option4.GetHashCode(), Is.Not.EqualTo( option.GetHashCode() ) );

            var option5 = new Option( "short", "long", Argument.NONE, occ );

            Assert.That( option5.Equals( option ), Is.False );
            Assert.That( option5.Equals( (object)option ), Is.False );
            Assert.That(
                option5.GetHashCode(), Is.Not.EqualTo( option.GetHashCode() ) );

            var option6 = new Option( "short", "long", arg, Occur.MULTIPLE );

            Assert.That( option6.Equals( option ), Is.False );
            Assert.That( option6.Equals( (object)option ), Is.False );
            Assert.That(
                option6.GetHashCode(), Is.Not.EqualTo( option.GetHashCode() ) );

            var option7 = new Option( string.Empty, "a", arg, occ );
            var option8 = new Option( "a", string.Empty, arg, occ );

            Assert.That( option7.Equals( option8 ), Is.False );
            Assert.That( option7.Equals( (object)option8 ), Is.False );
            Assert.That(
                option7.GetHashCode(),
                Is.Not.EqualTo( option8.GetHashCode() ) );
        }

        [Test]
        public void ToStringTest() {
            var arg = Argument.OPTIONAL;
            var occ = Occur.ONCE;
            var option = new Option( "short", "long", arg, occ );

            Assert.That(
                option.ToString(),
                Is.EqualTo( $"short, long, {arg}, {occ}" ) );
        }
    }
}

using DD.GetOpts;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GetOpts.Tests {
    public class OptionsTests {
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

        [Test]
        public void CreationTest() {
            Assert.That(
                () => new Options(),
                Throws.Nothing );
        }

        [Test]
        public void AddNullTest() {
            var options = new Options();

            Assert.That(
                () => options.Add( null, string.Empty ),
                Throws.ArgumentNullException
                .With.Message.Contain( "shortName" ) );

            Assert.That(
                () => options.Add( string.Empty, null ),
                Throws.ArgumentNullException
                .With.Message.Contain( "longName" ) );

            Assert.That(
                () => options.Add( null, null ),
                Throws.ArgumentNullException
                .With.Message.Contain( "shortName" ) );
        }

        [TestCaseSource( nameof( WHITE_SPACE_CHARACTERS ) )]
        public void AddWhiteSpaceTest( char whiteSpace ) {
            var options = new Options();
            var name = whiteSpace.ToString();
            var error = "Option must contain at least one valid name.";

            Assert.That(
                () => options.Add( name, string.Empty ),
                Throws.ArgumentException
                .With.Message.Contain( error ) );

            Assert.That(
                () => options.Add( string.Empty, name ),
                Throws.ArgumentException
                .With.Message.Contain( error ) );

            Assert.That(
                () => options.Add( name, name ),
                Throws.ArgumentException
                .With.Message.Contain( error ) );

            var name2 = $"{whiteSpace}Foo{whiteSpace}Bar{whiteSpace}";
            var error2 = "Short name must not contain control or white space characters.";
            var error3 = "Long name must not contain control or white space characters.";

            Assert.That(
                () => options.Add( name2, string.Empty ),
                Throws.ArgumentException
                .With.Message.Contain( error2 )
                .And.Message.Contain( "shortName" ) );

            Assert.That(
                () => options.Add( string.Empty, name2 ),
                Throws.ArgumentException
                .With.Message.Contain( error3 )
                .And.Message.Contain( "longName" ) );

            Assert.That(
                () => options.Add( name2, name2 ),
                Throws.ArgumentException
                .With.Message.Contain( error2 )
                .And.Message.Contain( "shortName" ) );
        }

        [TestCaseSource( nameof( CONTROL_CHARACTERS ) )]
        public void AddControlTest( char control ) {
            var options = new Options();
            var name = control.ToString();
            var error = "Short name must not contain control or white space characters.";
            var error2 = "Long name must not contain control or white space characters.";

            Assert.That(
                () => options.Add( name, string.Empty ),
                Throws.ArgumentException
                .With.Message.Contain( error )
                .And.Message.Contain( "shortName" ) );

            Assert.That(
                () => options.Add( string.Empty, name ),
                Throws.ArgumentException
                .With.Message.Contain( error2 )
                .And.Message.Contain( "longName" ) );

            Assert.That(
                () => options.Add( name, name ),
                Throws.ArgumentException
                .With.Message.Contain( error )
                .And.Message.Contain( "shortName" ) );

            var name2 = $"Foo{control}Bar";

            Assert.That(
                () => options.Add( name2, string.Empty ),
                Throws.ArgumentException
                .With.Message.Contain( error )
                .And.Message.Contain( "shortName" ) );

            Assert.That(
                () => options.Add( string.Empty, name2 ),
                Throws.ArgumentException
                .With.Message.Contain( error2 )
                .And.Message.Contain( "longName" ) );

            Assert.That(
                () => options.Add( name2, name2 ),
                Throws.ArgumentException
                .With.Message.Contain( error )
                .And.Message.Contain( "shortName" ) );
        }
    }
}

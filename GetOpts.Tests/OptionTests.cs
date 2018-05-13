using System;
using NUnit.Framework;

namespace DD.GetOpts.Tests {
    /// <summary>
    /// The <see cref="Option" /> tests.
    /// </summary>
    public class OptionsTests {
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
        }

        [Test]
        public void InvalidCreationTest() {
            var arg = Argument.NONE;
            var occ = Occur.ONCE;

            Assert.That(
                () => new Option( null, "long", arg, occ ),
                Throws.ArgumentNullException
                .With.Message.Contain( "shortName" ) );

            Assert.That(
                () => new Option( "short", null, arg, occ ),
                Throws.ArgumentNullException
                .With.Message.Contain( "longName" ) );

            Assert.That(
                () => new Option( "short", "long", ( Argument )0x03, occ ),
                Throws.ArgumentException
                .With.Message.Contain( "Invalid Argument option 3" )
                .And.Message.Contain( "arguments" ) );

            Assert.That(
                () => new Option( "short", "long", arg, ( Occur )0x03 ),
                Throws.ArgumentException
                .With.Message.Contain( "Invalid Occur option 3" )
                .And.Message.Contain( "occurs" ) );
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

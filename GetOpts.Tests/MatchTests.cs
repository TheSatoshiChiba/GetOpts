using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NSubstitute;
using NUnit.Framework;

namespace DD.GetOpts.Tests {
    /// <summary>
    /// The <see cref="Match" /> tests.
    /// </summary>
    public class MatchTests {
        [Test]
        public void CreationTest() {
            var empty = Substitute.For<IReadOnlyCollection<string>>();
            var match = new Match( "short", "long", 42, empty );

            Assert.That( match.ShortName, Is.EqualTo( "short" ) );
            Assert.That( match.LongName, Is.EqualTo( "long" ) );
            Assert.That( match.Count, Is.EqualTo( 42 ) );
            Assert.That( match.Arguments, Is.SameAs( empty ) );

            Assert.That(
                () => new Match( null, "long", 1, empty ),
                Throws.ArgumentNullException
                .With.Message.Contain( "Parameter name: shortName" ) );

            Assert.That(
                () => new Match( "short", null, 1, empty ),
                Throws.ArgumentNullException
                .With.Message.Contain( "Parameter name: longName" ) );

            Assert.That(
                () => new Match( "short", "long", 0, empty ),
                Throws.InstanceOf<ArgumentOutOfRangeException>()
                .With.Message.Contain( $"Invalid Match count 0" )
                .And.Message.Contain( "Parameter name: count" ) );

            Assert.That(
                () => new Match( "short", "long", -1, empty ),
                Throws.InstanceOf<ArgumentOutOfRangeException>()
                .With.Message.Contain( $"Invalid Match count -1" )
                .And.Message.Contain( "Parameter name: count" ) );

            Assert.That(
                () => new Match( "short", "long", 1, null ),
                Throws.ArgumentNullException
                .With.Message.Contain( "Parameter name: arguments" ) );
        }

        [Test]
        public void EqualityTest() {
            var args = new ReadOnlyCollection<string>(
                new List<string>() { "a", "b", "c" } );
            var match = new Match( "short", "long", 42, args );

            Assert.That( match.Equals( (object)null ), Is.False );
            Assert.That( match.Equals( (Match)null ), Is.False );
            Assert.That( match.Equals( this ), Is.False );
            Assert.That( match.Equals( match ), Is.True );
            Assert.That( match.Equals( (object)match ), Is.True );
            Assert.That(
                match.GetHashCode(), Is.EqualTo( match.GetHashCode() ) );

            var match2 = new Match( "short", "long", 42, args );

            Assert.That( match2.Equals( match ), Is.True );
            Assert.That( match2.Equals( (object)match ), Is.True );
            Assert.That(
                match2.GetHashCode(), Is.EqualTo( match.GetHashCode() ) );

            var match3 = new Match( "different", "long", 42, args );

            Assert.That( match3.Equals( match ), Is.False );
            Assert.That( match3.Equals( (object)match ), Is.False );
            Assert.That(
                match3.GetHashCode(), Is.Not.EqualTo( match.GetHashCode() ) );

            var match4 = new Match( "short", "different", 42, args );

            Assert.That( match4.Equals( match ), Is.False );
            Assert.That( match4.Equals( (object)match ), Is.False );
            Assert.That(
                match4.GetHashCode(), Is.Not.EqualTo( match.GetHashCode() ) );

            var match5 = new Match( "short", "long", 1, args );

            Assert.That( match5.Equals( match ), Is.False );
            Assert.That( match5.Equals( (object)match ), Is.False );
            Assert.That(
                match5.GetHashCode(), Is.Not.EqualTo( match.GetHashCode() ) );

            var empty = new ReadOnlyCollection<string>( new List<string>() );
            var match6 = new Match( "short", "long", 42, empty );

            Assert.That( match6.Equals( match ), Is.False );
            Assert.That( match6.Equals( (object)match ), Is.False );
            Assert.That(
                match6.GetHashCode(), Is.Not.EqualTo( match.GetHashCode() ) );

            var args2 = new ReadOnlyCollection<string>(
                new List<string>() { "c", "b", "a" } );
            var match7 = new Match( "short", "long", 42, args2 );

            Assert.That( match7.Equals( match ), Is.False );
            Assert.That( match7.Equals( (object)match ), Is.False );
            Assert.That(
                match7.GetHashCode(), Is.Not.EqualTo( match.GetHashCode() ) );

            var args3 = new ReadOnlyCollection<string>(
                new List<string>() { "d", "e", "f" } );
            var match8 = new Match( "short", "long", 42, args3 );

            Assert.That( match8.Equals( match ), Is.False );
            Assert.That( match8.Equals( (object)match ), Is.False );
            Assert.That(
                match8.GetHashCode(), Is.Not.EqualTo( match.GetHashCode() ) );
        }

        [Test]
        public void ToStringTest() {
            var empty = new ReadOnlyCollection<string>( new List<string>() );
            var match = new Match( "short", "long", 42, empty );

            Assert.That(
                match.ToString(),
                Is.EqualTo( "short, long, Count: 42, Arguments: []" ) );

            var args = new ReadOnlyCollection<string>(
                new List<string>() { "a", "b", "c" } );
            var match2 = new Match( "short", "long", 42, args );
            var expected =
                "short, long, Count: 42, Arguments: [\"a\", \"b\", \"c\"]";

            Assert.That( match2.ToString(), Is.EqualTo( expected ) );
        }
    }
}

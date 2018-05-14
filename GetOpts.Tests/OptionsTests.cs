using System;
using System.Linq;
using NUnit.Framework;

namespace DD.GetOpts.Tests {
    /// <summary>
    /// The <see cref="Options"/> tests.
    /// </summary>
    public class OptionsTests {
        private Options options;

        [SetUp]
        public void BeginTest() {
            options = new Options();
        }

        [TearDown]
        public void EndTest() {
            // no-op.
        }

        [Test]
        public void AddOptionTest() {
            var arg = Argument.NONE;
            var occ = Occur.ONCE;
            var option = new Option( "short", "long", arg, occ );

            Assert.That( options.Add( option ), Is.SameAs( options ) );
            Assert.That( options.Count(), Is.EqualTo( 1 ) );
            Assert.That( options.First(), Is.SameAs( option ) );

            Assert.That(
                () => options.Add( null ),
                Throws.ArgumentNullException
                .With.Message.Contain( "Parameter name: option" ) );
        }

        [Test]
        public void AddOptionManyTest() {
            var arg = Argument.NONE;
            var occ = Occur.ONCE;
            var items = new [] {
                new Option( "a", string.Empty, arg, occ ),
                new Option( string.Empty, "a", arg, occ ),
                new Option( "b", "b", arg, occ ),
            };

            Assert.That( options.Add( items[ 0 ] ).Count(), Is.EqualTo( 1 ) );
            Assert.That( options.Add( items[ 1 ] ).Count(), Is.EqualTo( 2 ) );
            Assert.That( options.Add( items[ 2 ] ).Count(), Is.EqualTo( 3 ) );
            Assert.That( options, Is.EquivalentTo( items ) );
        }

        [Test]
        public void AddOptionExistingTest() {
            var arg = Argument.NONE;
            var occ = Occur.ONCE;
            var option = new Option( "a", string.Empty, arg, occ );

            Assert.That( options.Add( option ).Count(), Is.EqualTo( 1 ) );
            Assert.That(
                () => options.Add( option ),
                Throws.ArgumentException
                .With.Message.Contain(
                    "Option with short name a already exists" ) );

            var option2 = new Option( "a", "b", arg, occ );
            Assert.That(
                () => options.Add( option2 ),
                Throws.ArgumentException
                .With.Message.Contain(
                    "Option with short name a already exists" ) );

            var option3 = new Option( string.Empty, "a", arg, occ );
            Assert.That( options.Add( option3 ).Count(), Is.EqualTo( 2 ) );
            Assert.That(
                () => options.Add( option3 ),
                Throws.ArgumentException
                .With.Message.Contain(
                    "Option with long name a already exists" ) );

            var option4 = new Option( "b", "a", arg, occ );
            Assert.That(
                () => options.Add( option4 ),
                Throws.ArgumentException
                .With.Message.Contain(
                    "Option with long name a already exists" ) );

            var option5 = new Option( "a", "a", arg, occ );
            Assert.That(
                () => options.Add( option5 ),
                Throws.ArgumentException
                .With.Message.Contain(
                    "Option with short name a already exists" ) );

            Assert.That( options.Count(), Is.EqualTo( 2 ) );
        }

        [Test]
        public void AddOptionPrefixTest() {
            var arg = Argument.NONE;
            var occ = Occur.ONCE;
            var shortPrefix1 = new Option( "-short", "long", arg, occ );
            var shortPrefix2 = new Option( "short", "-long", arg, occ );
            var longPrefix1 = new Option( "--short", "long", arg, occ );
            var longPrefix2 = new Option( "short", "--long", arg, occ );

            Assert.That(
                () => options.Add( shortPrefix1 ),
                Throws.ArgumentException
                .With.Message.Contain(
                    "ShortName starts with the short prefix -" )
                .And.Message.Contain( "Parameter name: option" ) );

            Assert.That(
                () => options.Add( shortPrefix2 ),
                Throws.ArgumentException
                .With.Message.Contain(
                    "LongName starts with the short prefix -" )
                .And.Message.Contain( "Parameter name: option" ) );

            Assert.That(
                () => options.Add( longPrefix1 ),
                Throws.ArgumentException
                .With.Message.Contain(
                    "ShortName starts with the long prefix --" )
                .And.Message.Contain( "Parameter name: option" ) );

            Assert.That(
                () => options.Add( longPrefix2 ),
                Throws.ArgumentException
                .With.Message.Contain(
                    "LongName starts with the long prefix --" )
                .And.Message.Contain( "Parameter name: option" ) );
        }
    }
}

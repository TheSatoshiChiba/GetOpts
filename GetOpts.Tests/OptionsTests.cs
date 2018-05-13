using System;
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

            Assert.That(
                () => options.Add( null ),
                Throws.ArgumentNullException
                .With.Message.Contain( "Parameter name: option" ) );
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

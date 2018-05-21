using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NSubstitute;
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

        [Test]
        public void ParseNullTest() {
            var arg = Argument.NONE;
            var occ = Occur.ONCE;
            var option = new Option( "a", "b", arg, occ );

            options.Add( option );
            Assert.That(
                () => options.Parse( null ),
                Throws.ArgumentNullException
                    .With.Message.Contain( "Parameter name: arguments" ) );
        }

        [Test]
        public void ParseOnceTest() {
            var arg = Argument.NONE;
            var args = Substitute.For<IReadOnlyCollection<string>>();

            var alpha = new Option( "a", "alpha", arg, Occur.ONCE );
            options.Add( alpha )
                .Add( new Option( "b", "beta", arg, Occur.OPTIONAL ) )
                .Add( new Option( "g", "gamma", arg, Occur.MULTIPLE ) );

            var matches = new Match[] { new Match( "a", "alpha", 1, args ) };
            Assert.That(
                options.Parse( new string[] { "-a" } ),
                Is.EquivalentTo( matches ) );

            Assert.That(
                () => options.Parse( new string[] { } ),
                Throws.ArgumentException
                    .With.Message.Contain( $"Expected [{alpha}]" )
                    .And.Message.Contain( "Parameter name: arguments" ) );
        }

        [TestCase( "-a", "-a" )]
        [TestCase( "-a", "--alpha" )]
        [TestCase( "--alpha", "-a" )]
        [TestCase( "--alpha", "--alpha" )]
        public void ParseOnceInvalidTest( params string[] arguments ) {
            var arg = Argument.NONE;
            var args = Substitute.For<IReadOnlyCollection<string>>();

            var alpha = new Option( "a", "alpha", arg, Occur.ONCE );
            options.Add( alpha )
                .Add( new Option( "b", "beta", arg, Occur.OPTIONAL ) )
                .Add( new Option( "g", "gamma", arg, Occur.MULTIPLE ) );

            Assert.That(
                () => options.Parse( arguments ),
                Throws.ArgumentException
                    .With.Message.Contain( $"Multiple occurance of [{alpha}]" )
                    .And.Message.Contain( "Parameter name: arguments" ) );
        }

        [Test]
        public void ParseOptionalTest() {
            var arg = Argument.NONE;
            var args = Substitute.For<IReadOnlyCollection<string>>();

            var beta = new Option( "b", "beta", arg, Occur.OPTIONAL );
            options.Add( new Option( "a", "alpha", arg, Occur.ONCE ) )
                .Add( beta )
                .Add( new Option( "g", "gamma", arg, Occur.MULTIPLE ) );

            var matches = new Match[] {
                new Match( "a", "alpha", 1, args ),
                new Match( "b", "beta", 1, args ),
            };

            Assert.That(
                options.Parse( new string[] { "-b", "-a" } ),
                Is.EquivalentTo( matches ) );

            Assert.That(
                options.Parse( new string[] { "-a", "-b" } ),
                Is.EquivalentTo( matches ) );
        }

        [TestCase( "-a", "-b", "-b" )]
        [TestCase( "-a", "-b", "--beta" )]
        [TestCase( "-a", "--beta", "-b" )]
        [TestCase( "-a", "--beta", "--beta" )]
        public void ParseOptionalInvalidTest( params string[] arguments ) {
            var arg = Argument.NONE;
            var args = Substitute.For<IReadOnlyCollection<string>>();

            var beta = new Option( "b", "beta", arg, Occur.OPTIONAL );
            options.Add( new Option( "a", "alpha", arg, Occur.ONCE ) )
                .Add( beta )
                .Add( new Option( "g", "gamma", arg, Occur.MULTIPLE ) );

            Assert.That(
                () => options.Parse( arguments ),
                Throws.ArgumentException
                    .With.Message.Contain( $"Multiple occurance of [{beta}]" )
                    .And.Message.Contain( "Parameter name: arguments" ) );
        }

        [Test]
        public void ParseMultipleTest() {
            var arg = Argument.NONE;
            var args = Substitute.For<IReadOnlyCollection<string>>();

            var gamma = new Option( "g", "gamma", arg, Occur.MULTIPLE );
            options.Add( new Option( "a", "alpha", arg, Occur.ONCE ) )
                .Add( new Option( "b", "beta", arg, Occur.OPTIONAL ) )
                .Add( gamma );

            var matches = new Match[] {
                new Match( "a", "alpha", 1, args ),
                new Match( "g", "gamma", 3, args ),
            };

            Assert.That(
                options.Parse( new string[] { "-a", "-g", "-g", "-g" } ),
                Is.EquivalentTo( matches ) );

            Assert.That(
                options.Parse(
                    new string[] { "-a", "--gamma", "--gamma", "--gamma" } ),
                Is.EquivalentTo( matches ) );

            Assert.That(
                options.Parse(
                    new string[] { "-a", "--gamma", "-g", "--gamma" } ),
                Is.EquivalentTo( matches ) );
        }

        [Test]
        public void ParseInvalidPrefixSignsTest() {
            var arg = Argument.NONE;
            var args = Substitute.For<IReadOnlyCollection<string>>();
            var gamma = new Option( "g", "gamma", arg, Occur.MULTIPLE );

            options.Add( gamma );

            Assert.That(
                () => options.Parse( new string[] { "-gamma" } ),
                Throws.ArgumentException
                    .With.Message.Contain( $"Invalid argument -gamma" )
                    .And.Message.Contain( "Parameter name: arguments" ) );

            Assert.That(
                () => options.Parse( new string[] { "--g" } ),
                Throws.ArgumentException
                    .With.Message.Contain( $"Invalid argument --g" )
                    .And.Message.Contain( "Parameter name: arguments" ) );
        }

        [Test]
        public void ParseNoOptionsTest() {
            Assert.That(
                () => options.Parse( new string[] { "--gamma" } ),
                Throws.ArgumentException
                    .With.Message.Contain( $"Invalid argument --gamma" )
                    .And.Message.Contain( "Parameter name: arguments" ) );

            Assert.That(
                () => options.Parse( new string[] { "-g" } ),
                Throws.ArgumentException
                    .With.Message.Contain( $"Invalid argument -g" )
                    .And.Message.Contain( "Parameter name: arguments" ) );
        }

        [Test]
        public void ParseNoOptionsNoArgumentsTest() {
            Assert.That(
                options.Parse( new string[ 0 ] ),
                Is.EquivalentTo( new Match[ 0 ] ) );
        }

        [Test]
        public void ParseOptionalOnlyTest() {
            var arg = Argument.NONE;
            var args = Substitute.For<IReadOnlyCollection<string>>();

            var beta = new Option( "b", "beta", arg, Occur.OPTIONAL );
            options.Add( beta );

            var matches1 = new Match[] {
                new Match( "b", "beta", 1, args ),
            };

            var matches2 = new Match[ 0 ];

            Assert.That(
                options.Parse( new string[] { "-b" } ),
                Is.EquivalentTo( matches1 ) );

            Assert.That(
                options.Parse( new string[] { "--beta" } ),
                Is.EquivalentTo( matches1 ) );

            Assert.That(
                options.Parse( new string[ 0 ] ),
                Is.EquivalentTo( matches2 ) );
        }

        [Test]
        public void ParseWithArgumentsTest() {
            var args0 = Substitute.For<IReadOnlyCollection<string>>();
            var args1 = new ReadOnlyCollection<string>(
                new List<string>() { "foo" } );

            var alpha1 = new Option(
                "a1", "alpha1", Argument.REQUIRED, Occur.ONCE );
            var alpha2 = new Option(
                "a2", "alpha2", Argument.OPTIONAL, Occur.ONCE );

            options.Add( alpha1 ).Add( alpha2 );

            var matches1 = new Match[] {
                new Match( "a1", "alpha1", 1, args1 ),
                new Match( "a2", "alpha2", 1, args0 ),
            };
            var matches2 = new Match[] {
                new Match( "a1", "alpha1", 1, args1 ),
                new Match( "a2", "alpha2", 1, args1 ),
            };

            Assert.That(
                options.Parse( new string[] { "-a1", "foo", "-a2" } ),
                Is.EquivalentTo( matches1 ) );

            Assert.That(
                options.Parse( new string[] { "-a2", "-a1", "foo" } ),
                Is.EquivalentTo( matches1 ) );

            Assert.That(
                options.Parse( new string[] { "-a1", "foo", "--alpha2" } ),
                Is.EquivalentTo( matches1 ) );

            Assert.That(
                options.Parse( new string[] { "--alpha2", "-a1", "foo" } ),
                Is.EquivalentTo( matches1 ) );

            Assert.That(
                options.Parse( new string[] { "--alpha1", "foo", "-a2" } ),
                Is.EquivalentTo( matches1 ) );

            Assert.That(
                options.Parse( new string[] { "--alpha1", "foo", "--alpha2" } ),
                Is.EquivalentTo( matches1 ) );

            Assert.That(
                options.Parse( new string[] { "-a1", "foo", "-a2", "foo" } ),
                Is.EquivalentTo( matches2 ) );

            Assert.That(
                options.Parse( new string[] { "-a2", "foo", "-a1", "foo" } ),
                Is.EquivalentTo( matches2 ) );

            Assert.That(
                options.Parse( new string[] {
                    "-a1", "foo", "--alpha2", "foo" } ),
                Is.EquivalentTo( matches2 ) );

            Assert.That(
                options.Parse( new string[] {
                    "--alpha1", "foo", "-a2", "foo" } ),
                Is.EquivalentTo( matches2 ) );

            Assert.That(
                options.Parse( new string[] {
                    "--alpha1", "foo", "--alpha2", "foo" } ),
                Is.EquivalentTo( matches2 ) );

            Assert.That(
                () => options.Parse( new string[] { "-a1", "-a2" } ),
                Throws.ArgumentException
                    .With.Message.Contain( $"Expected argument for [{alpha1}]" )
                    .And.Message.Contain( "Parameter name: arguments" ) );

            Assert.That(
                () => options.Parse( new string[] { "-a2", "-a1" } ),
                Throws.ArgumentException
                    .With.Message.Contain( $"Expected argument for [{alpha1}]" )
                    .And.Message.Contain( "Parameter name: arguments" ) );
        }
    }
}

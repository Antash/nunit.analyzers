using System.Globalization;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ConstraintUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ConstraintsUsage
{
    public class EqualConstraintUsageAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new EqualConstraintUsageAnalyzer();
        private static readonly ExpectedDiagnostic isEqualToDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.EqualConstraintUsage,
                string.Format(CultureInfo.InvariantCulture, EqualConstraintUsageConstants.Message, "Is.EqualTo"));
        private static readonly ExpectedDiagnostic isNotEqualToDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.EqualConstraintUsage,
                string.Format(CultureInfo.InvariantCulture, EqualConstraintUsageConstants.Message, "Is.Not.EqualTo"));

        [Test]
        public void AnalyzeWhenEqualsOperatorUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓actual == ""abc"");");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenNotEqualsOperatorUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓actual != ""bcd"");");

            RoslynAssert.Diagnostics(analyzer, isNotEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsInstanceMethodUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓actual.Equals(""abc""));");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenNegatedEqualsInstanceMethodUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓!actual.Equals(""bcd""));");

            RoslynAssert.Diagnostics(analyzer, isNotEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsStaticMethodUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓Equals(actual,""abc""));");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenNegatedEqualsStaticMethodUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓!Equals(actual,""abc""));");

            RoslynAssert.Diagnostics(analyzer, isNotEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsMethodUsedWithIsTrue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓actual.Equals(""abc""), Is.True);");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsMethodUsedWithIsFalse()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓actual.Equals(""abc""), Is.False);");

            RoslynAssert.Diagnostics(analyzer, isNotEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsMethodUsedWithAssertTrue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.True(↓actual.Equals(""abc""));");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsMethodUsedWithAssertFalse()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.False(↓actual.Equals(""bcd""));");

            RoslynAssert.Diagnostics(analyzer, isNotEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsMethodUsedWithAssertIsTrue()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.IsTrue(↓actual.Equals(""abc""));");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsMethodUsedWithAssertIsFalse()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.IsFalse(↓actual.Equals(""bcd""));");

            RoslynAssert.Diagnostics(analyzer, isNotEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeDoubleNegationAsPositive()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.IsFalse(↓!actual.Equals(""bcd""));");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenAssertThatIsUsedWithMessage()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓actual == ""abc"", ""Assertion message"");");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsMethodUsedWithIsTrueWithMessage()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(↓actual.Equals(""abc""), Is.True, ""Assertion message"");");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsMethodUsedWithAssertTrueWithMessage()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.True(↓actual.Equals(""abc""), ""Assertion message"");");

            RoslynAssert.Diagnostics(analyzer, isEqualToDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenEqualsMethodUsedWithAssertFalseWithMessage()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.False(↓actual.Equals(""bcd""), ""Assertion message"");");

            RoslynAssert.Diagnostics(analyzer, isNotEqualToDiagnostic, testCode);
        }

        [Test]
        public void ValidWhenIsEqualToConstraintUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.EqualTo(""bcd""));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenIsNotEqualToConstraintUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual, Is.Not.EqualTo(""bcd""));");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidOtherMethodUsed()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""abc"";
                Assert.That(actual.Contains(""bc""));");

            RoslynAssert.Valid(analyzer, testCode);
        }
    }
}

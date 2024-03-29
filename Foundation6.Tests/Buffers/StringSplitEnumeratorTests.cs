﻿using NUnit.Framework;
using System;

namespace Foundation.Buffers
{
    [TestFixture]
    public class StringSplitEnumeratorTests
    {
        [Test]
        public void MoveNext_Should_Find_Part_When_Calling_MoveNext()
        {
            var span = "123.4567.89".AsSpan();
            var sut = new StringSplitEnumerator(span, ".4567".AsSpan(), StringComparison.InvariantCulture);

            Assert.IsTrue(sut.MoveNext());
            Assert.AreEqual(sut.Current.ToString(), "123");

            Assert.IsTrue(sut.MoveNext());
            Assert.AreEqual(sut.Current.ToString(), "4567.89");

            Assert.IsFalse(sut.MoveNext());
        }

        [Test]
        public void MoveNext_Should_Find_Separator_When_Used_With_Foreach()
        {
            var span = "123.4567.89".AsSpan();
            var sut = new SplitEnumerator<char>(span, new[] { '.' }.AsSpan());

            var i = 0;
            foreach (var entry in sut)
            {
                if (0 == i) Assert.AreEqual(entry.ToString(), "123");
                if (1 == i) Assert.AreEqual(entry.ToString(), "4567");
                if (2 == i) Assert.AreEqual(entry.ToString(), "89");
                i++;
            }
        }

        [Test]
        public void MoveNext_Should_Not_Find_Separator_When_Calling_MoveNext()
        {
            var span = "123.4567.89".AsSpan();
            var sut = new SplitEnumerator<char>(span, new[] { '_' }.AsSpan());

            Assert.IsFalse(sut.MoveNext());
        }

        [Test]
        public void MoveNext_Should_Not_Find_Separator_When_Used_With_Foreach()
        {
            var span = "123.4567.89".AsSpan();
            var sut = new SplitEnumerator<char>(span, new[] { '_' }.AsSpan());

            var passed = false;
            foreach (var _ in sut)
            {
                passed = true;
            }

            Assert.IsFalse(passed);
        }
    }
}

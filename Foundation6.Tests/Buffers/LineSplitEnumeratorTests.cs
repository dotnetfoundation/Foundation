﻿using NUnit.Framework;
using System;

namespace Foundation.Buffers
{
    [TestFixture]
    public class LineSplitEnumeratorTests
    {
        [Test]
        public void MoveNext_Should_Return4Lines_When_StringIncludes3Linebreaks()
        {
            var str = "this\nis\na\ntest";
            var sut = new LineSplitEnumerator(str.AsSpan());
            var enumerator = sut.GetEnumerator();
            {
                Assert.IsTrue(enumerator.MoveNext());
                var line = enumerator.Current;
                Assert.True("this".AsSpan().IsSameAs(line));
            }
            {
                Assert.IsTrue(enumerator.MoveNext());
                var line = enumerator.Current;
                Assert.True("is".AsSpan().IsSameAs(line));
            }
            {
                Assert.IsTrue(enumerator.MoveNext());
                var line = enumerator.Current;
                Assert.True("a".AsSpan().IsSameAs(line));
            }
            {
                Assert.IsTrue(enumerator.MoveNext());
                var line = enumerator.Current;
                Assert.True("test".AsSpan().IsSameAs(line));
            }
            Assert.IsFalse(enumerator.MoveNext());
        }
    }
}

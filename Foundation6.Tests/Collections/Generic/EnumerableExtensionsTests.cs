﻿using Foundation.ComponentModel;
using Foundation.Test.Collections.Generic;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Foundation.Collections.Generic
{
    [TestFixture]
    public class EnumerableExtensionsTests
    {
        [DebuggerDisplay("Name:{Name}")]
        public class A
        {
            private Guid _id;

            public A(string name)
            {
                Name = name;
            }

            public Guid Id
            {
                get
                {
                    if (Guid.Empty == _id)
                        _id = Guid.NewGuid();

                    return _id;
                }
                set { _id = value; }
            }

            public string Name { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        public class B : A
        {
            public B(string name) : base(name)
            {
            }
        }


        public class C : A
        {
            public C(string name) : base(name)
            {
            }

            public string? NickName { get; set; }
        }

        // ReSharper disable InconsistentNaming

        [Test]
        public void AfterEveryElement()
        {
            var items = new List<string> { "1", "2", "3" };
            var sb = new StringBuilder();

            foreach (var item in items.AfterEveryElement(() => sb.Append(',')))
            {
                sb.Append(item);
            }

            var actual = sb.ToString();
            Assert.AreEqual("1,2,3", actual);
        }

        [Test]
        public void Aggregate_Should_ReturnSome_When_HasElements()
        {
            var numbers = Enumerable.Range(1, 3);

            var minmax = numbers.Aggregate(number => (min: number, max: number), (acc, number) =>
            {
                if (number < acc.min) acc.min = number;
                if (number > acc.max) acc.max = number;
                return (acc.min, acc.max);
            });

            Assert.IsTrue(minmax.IsSome);

            var (min, max) = minmax.ValueOrThrow();
            Assert.AreEqual(1, min);
            Assert.AreEqual(3, max);
        }

        [Test]
        public void AtLeast()
        {
            var items = new List<string> { "1", "2", "3" }.ToArray();
            {
                var actual = items.AtLeast(4).ToArray();
                Assert.AreEqual(0, actual.Length);
            }
            {
                var actual = items.AtLeast(2).ToArray();
                Assert.AreEqual(3, actual.Length);
            }
            {
                var actual = items.AtLeast(3).ToArray();
                Assert.AreEqual(3, actual.Length);
            }
        }


        [Test]
        public void AverageMedian_ShouldReturnMedian_WhenUsingConverter()
        {
            //odd number of elements
            {
                IEnumerable<string> items = new List<string> { "a", "ab", "abc" };
                var median = items.AverageMedian(x => x.Length);
                Assert.AreEqual(2M, median);
            }
            //even number of elements
            {
                IEnumerable<string> items = new List<string> { "a", "ab", "abc", "abcd" };
                var median = items.AverageMedian(x => x.Length);
                Assert.AreEqual(2.5M, median);
            }
        }

        [Test]
        public void AverageMedian_ShouldReturnMedian_WhenUsingNumbers()
        {
            //odd number of elements
            {
                var numbers = Enumerable.Range(1, 7);
                var median = numbers.AverageMedian();
                Assert.AreEqual(4, median);
            }
            //even number of elements
            {
                var numbers = Enumerable.Range(1, 8);
                var median = numbers.AverageMedian();
                Assert.AreEqual(4.5, median);
            }
        }

        [Test]
        public void AverageMedian_ShouldThrowException_WhenUsingValuesNotConvertibleToDecimal()
        {
            IEnumerable<string> items = new List<string> { "one", "two", "three" };
            Assert.Throws<FormatException>(() => items.AverageMedian());
        }

        [Test]
        public void AverageTrueMedian_ShouldReturnTheMedianPositioned()
        {
            {
                var numbers = Enumerable.Range(1, 7);
                var (opt1, opt2) = numbers.AverageMedianPosition();
                Assert.IsFalse(opt2.IsSome);
                Assert.AreEqual(4, opt1.ValueOrThrow());
            }
            {
                var numbers = Enumerable.Range(1, 8);
                var (opt1, opt2) = numbers.AverageMedianPosition();
                Assert.IsTrue(opt2.IsSome);
                Assert.AreEqual(4, opt1.ValueOrThrow());
                Assert.AreEqual(5, opt2.ValueOrThrow());
            }
            {
                var items = Enumerable.Range(1, 7).Select(x => x.ToString());
                var (opt1, opt2) = items.AverageMedianPosition();
                Assert.IsFalse(opt2.IsSome);
                Assert.AreEqual("4", opt1.ValueOrThrow());
            }
            {
                var items = Enumerable.Range(1, 8).Select(x => x.ToString());
                var (opt1, opt2) = items.AverageMedianPosition();
                Assert.IsTrue(opt2.IsSome);
                Assert.AreEqual("4", opt1.ValueOrThrow());
                Assert.AreEqual("5", opt2.ValueOrThrow());
            }
        }

        [Test]
        public void CartesianProduct()
        {
            var items1 = new List<string> {"1", "2", "3"};
            var items2 = new List<string> {"a", "b", "c"};

            var erg = items1.CartesianProduct(items2).ToArray();
            Assert.AreEqual(("1", "a"), erg[0]);
            Assert.AreEqual(("1", "b"), erg[1]);
            Assert.AreEqual(("1", "c"), erg[2]);
            Assert.AreEqual(("2", "a"), erg[3]);
            Assert.AreEqual(("2", "b"), erg[4]);
            Assert.AreEqual(("2", "c"), erg[5]);
            Assert.AreEqual(("3", "a"), erg[6]);
            Assert.AreEqual(("3", "b"), erg[7]);
            Assert.AreEqual(("3", "c"), erg[8]);
        }
        
        [Test]
        public void Contains_AllNumbersWithinRange()
        {
            var items1 = Enumerable.Range(0, 9);
            var items2 = Enumerable.Range(0, 9).Where(i => (i % 2) == 0);
            Assert.IsTrue(items1.Contains(items2));

            var items3 = new List<int> { 10, 11 };
            Assert.IsFalse(items1.Contains(items3));
        }

        [Test]
        public void Contains_IncludingNumbersOutOfRange()
        {
            var items1 = Enumerable.Range(0, 9);
            IEnumerable<int> items2 = new List<int> { 1, 5, 12 };
            Assert.IsTrue(items1.Contains(items2));
        }

        [Test]
        public void CyclicEnumerate()
        {
            var items = new List<string> { "A", "B", "C" };
            var e = items.CycleEnumerate().GetEnumerator();

            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("A", e.Current);

            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("B", e.Current);

            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("C", e.Current);

            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("A", e.Current);

            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("B", e.Current);

            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("C", e.Current);
        }

        [Test]
        public void CyclicEnumerate_MinMax()
        {
            var items = new List<string> { "A", "B", "C", "D", "E" };
            var enumerated = items.CycleEnumerate(1, 2).ToList();
            var e = enumerated.GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual((1, "A"), e.Current);

            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual((2, "B"), e.Current);

            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual((1, "C"), e.Current);

            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual((2, "D"), e.Current);

            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual((1, "E"), e.Current);

            Assert.IsFalse(e.MoveNext());
        }

        [Test]
        public void Difference_CompletelyDifferent()
        {
            var items1 = Enumerable.Range(0, 10);
            var items2 = Enumerable.Range(10, 10);
            var diff = items1.Difference(items2).ToList();
            Assert.AreEqual(20, diff.Count);
        }

        [Test]
        public void Difference_NoDifference()
        {
            var items1 = Enumerable.Range(0, 10);
            var items2 = Enumerable.Range(0, 10);
            var diff = items1.Difference(items2).ToList();
            Assert.AreEqual(0, diff.Count);
        }

        [Test]
        public void Difference_PartiallyEqual()
        {
            var items1 = new List<int> { 1, 2, 3, 4, 5 };
            var items2 = new List<int> { 2, 4, 6 };
            var diff = items1.Difference(items2).ToList();
            Assert.AreEqual(4, diff.Count);
            CollectionAssert.AreEqual(new[] { 1, 3, 5, 6 }, diff);
        }

        [Test]
        public void Duplicates_DistinctIsFalse_WithMultipleDuplicateValues()
        {
            var items = new List<int> { 1, 2, 3, 4, 5, 2, 4, 2 };
            var result = items.Duplicates().ToList();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(2, result[0]);
            Assert.AreEqual(2, result[1]);
            Assert.AreEqual(4, result[2]);
        }

        [Test]
        public void Duplicates_DistinctIsTrue_OnlySingleDuplicateValues()
        {
            var items = new List<int> { 1, 2, 3, 4, 5, 2, 4, 2 };
            var result = items.Duplicates(true).ToList();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(2, result[0]);
            Assert.AreEqual(4, result[1]);
        }

        [Test]
        public void Duplicates_DistinctIsFalse_WithoutDuplicateValues()
        {
            var items = new List<int> { 1, 2, 3, 4, 5 };
            var result = items.Duplicates().ToList();
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Enumerate()
        {
            {
                var items1 = Enumerable.Range(0, 9).ToList();
                var enumerated = items1.Enumerate(n => n * 2).ToList();
                Assert.AreEqual(items1.Count, enumerated.Count);

                foreach (var tuple in enumerated)
                    Assert.AreEqual(tuple.Item1 * 2, tuple.Item2);
            }
            {
                var items = new[] { "one", "two", "three" };
                var i = 0;
                var enumerated = items.Enumerate(item => i++).ToList();
                Assert.AreEqual(("one", 0), enumerated[0]);
                Assert.AreEqual(("two", 1), enumerated[1]);
                Assert.AreEqual(("three", 2), enumerated[2]);
            }
        }

        [Test]
        public void Enumerate_WithMinMax()
        {
            var items = Enumerable.Range(1, 10).Select(x => x.ToString());
            var enumerated = items.Enumerate(MinMax.New(1, 3)).ToList();
            Assert.AreEqual(("1", 1), enumerated[0]);
            Assert.AreEqual(("2", 2), enumerated[1]);
            Assert.AreEqual(("3", 3), enumerated[2]);
            Assert.AreEqual(("4", 1), enumerated[3]);
            Assert.AreEqual(("5", 2), enumerated[4]);
            Assert.AreEqual(("6", 3), enumerated[5]);
            Assert.AreEqual(("7", 1), enumerated[6]);
            Assert.AreEqual(("8", 2), enumerated[7]);
            Assert.AreEqual(("9", 3), enumerated[8]);
            Assert.AreEqual(("10", 1), enumerated[9]);
        }

        [Test]
        public void Enumerate_WithSeed()
        {
            var items = new[] { "1", "2", "3" };
            var enumerated = items.Enumerate(5).ToList();
            Assert.AreEqual(("1", 5), enumerated[0]);
            Assert.AreEqual(("2", 6), enumerated[1]);
            Assert.AreEqual(("3", 7), enumerated[2]);
        }
[Test]
        public void Except()
        
        {
            var items1 = new[] 
            { 
                new A("1"),
                new A("2"),
                new A("3"),
            };

            var items2 = new[]
            {
                new C("1") { NickName = "3" },
                new C("2") { NickName = "1" },
                new C("3") { NickName = "1" },
            };

            var different = items1.Except(items2, i1 => i1.Name, i2 => i2.NickName, i1 => i1).ToArray();

            Assert.AreEqual(1, different.Length);
            Assert.AreEqual("2", different[0].Name);
        }
        
        [Test]
        public void FindUntil_Should_ReturnOneValueForEachMach_When_ListHasDuplicateValues()
        {
            var numbers = new [] { 1, 2, 3, 2, 4, 4, 5, 6 };
            var predicates = new Func<int, bool>[] { n => n == 2, n => n == 4, n => n == 6 };

            var foundNumbers = numbers.FindUntil(predicates).ToArray();
            Assert.AreEqual(3, foundNumbers.Length);
            Assert.AreEqual(2, foundNumbers[0]);
            Assert.AreEqual(4, foundNumbers[1]);
            Assert.AreEqual(6, foundNumbers[2]);
        }

        [Test]
        public void FindUntil_Should_StopIteration_When_AllPredicatesMatched()
        {
            var numbers = new TestEnumerable<int>(Enumerable.Range(1, 10));

            var calledMoveNext = 0;
            Action<bool> onMoveNext = hasNext => calledMoveNext++;

            numbers.OnMoveNext.Subscribe(onMoveNext);

            var predicates = new Func<int, bool>[] { n => n == 2, n => n == 5 };

            var foundNumbers = numbers.FindUntil(predicates).ToArray();
            Assert.AreEqual(6, calledMoveNext);
            Assert.AreEqual(2, foundNumbers.Length);
            Assert.AreEqual(2, foundNumbers[0]);
            Assert.AreEqual(5, foundNumbers[1]);
        }

        [Test]
        public void ForEach_Returning_number_of_processed_acctions()
        {
            var items = Enumerable.Range(0, 9);
            var iterationCounter = 0L;
            void action(int n) => iterationCounter++;
            var result = items.ForEach(action);
            Assert.AreEqual(9, result);
            Assert.AreEqual(iterationCounter, result);
        }

        [Test]
        public void ForEach_WithEmptyList()
        {
            var items = Enumerable.Empty<int>();
            var iterationCounter = 0L;
            Action<int> action = (n) => iterationCounter++;
            var result = items.ForEach(action);
            Assert.AreEqual(0, result);
            Assert.AreEqual(iterationCounter, result);
        }

        [Test]
        public void FromIndex()
        {
            var items = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            var selected = items.FromIndex(index => (index % 2) == 0).ToList();
            Assert.AreEqual(items.Count / 2, selected.Count);
            Assert.AreEqual("0", selected[0]);
            Assert.AreEqual("2", selected[1]);
            Assert.AreEqual("4", selected[2]);
            Assert.AreEqual("6", selected[3]);
            Assert.AreEqual("8", selected[4]);
        }

        [Test]
        public void If_Should_ExecuteAction_When_Predicate_IsTrue()
        {
            {
                var items = Enumerable.Range(1, 6);
                var ifItems = new List<int>();
                var elseItems = items.If(item => item < 4, ifItems.Add)
                                     .Else()
                                     .ToList();

                CollectionAssert.AreEqual(Enumerable.Range(1, 3), ifItems);
                CollectionAssert.AreEqual(Enumerable.Range(4, 3), elseItems);
            }
            {
                var items = Enumerable.Range(1, 6);
                var ifItems = new List<int>();
                var elseIfItems = new List<int>();
                var elseItems = items.If(item => item < 3, ifItems.Add)
                                     .ElseIf(item => item < 5, elseIfItems.Add)
                                     .Else()
                                     .ToList();

                CollectionAssert.AreEqual(Enumerable.Range(1, 2), ifItems);
                CollectionAssert.AreEqual(Enumerable.Range(3, 2), elseIfItems);
                CollectionAssert.AreEqual(Enumerable.Range(5, 2), elseItems);
            }

            {
                var items = Enumerable.Range(1, 6);
                var ifItems = new List<int>();
                var elseIfItems = new List<int>();

                items.If(item => item < 3, ifItems.Add)
                     .ElseIf(item => item < 5, elseIfItems.Add)
                     .EndIf();

                CollectionAssert.AreEqual(Enumerable.Range(1, 2), ifItems);
                CollectionAssert.AreEqual(Enumerable.Range(3, 2), elseIfItems);
            }
        }

        [Test]
        public void If_Should_ReturnMappedValues_When_Predicate_IsTrue()
        {
            {
                var numbers = Enumerable.Range(1, 6);

                var actual = numbers.If(n => n % 2 == 0, n => n * 10)
                                    .Else(n => n).ToArray();

                var expected = new[] { 1, 20, 3, 40, 5, 60 };

                Assert.AreEqual(expected.Length, actual.Length);
                Assert.AreEqual(expected[0], actual[0]);
                Assert.AreEqual(expected[1], actual[1]);
                Assert.AreEqual(expected[2], actual[2]);
                Assert.AreEqual(expected[3], actual[3]);
                Assert.AreEqual(expected[4], actual[4]);
                Assert.AreEqual(expected[5], actual[5]);
            }

            {
                var numbers = Enumerable.Range(1, 6);

                var strings = numbers.If(n => 3 > n, n => n.ToString())
                                     .Else(n => $"{n * 10}").ToArray();

            }
        }

        [Test]
        public void Ignore_Should_Ignore_Items_When_Match_On_Indices()
        {
            var numbers = Enumerable.Range(0, 10);
            var filtered = numbers.Ignore(new[] { 1, 3, 5, 7, 9 }).ToArray();
            Assert.AreEqual(5, filtered.Length);
            Assert.AreEqual(0, filtered[0]);
            Assert.AreEqual(2, filtered[1]);
            Assert.AreEqual(4, filtered[2]);
            Assert.AreEqual(6, filtered[3]);
            Assert.AreEqual(8, filtered[4]);
        }

        [Test]
        public void Ignore_Should_Ignore_Items_When_Matching_Predicate_Is_True()
        {
            var numbers = Enumerable.Range(0, 10);
            var filtered = numbers.Ignore(n => n % 2 == 0).ToArray();
            Assert.AreEqual(5, filtered.Length);
            Assert.AreEqual(1, filtered[0]);
            Assert.AreEqual(3, filtered[1]);
            Assert.AreEqual(5, filtered[2]);
            Assert.AreEqual(7, filtered[3]);
            Assert.AreEqual(9, filtered[4]);
        }

        [Test]
        public void IndexOf()
        {
            var items = Enumerable.Range(1, 5);
            Assert.AreEqual(0, items.IndexOf(1));
            Assert.AreEqual(1, items.IndexOf(2));
            Assert.AreEqual(2, items.IndexOf(3));
            Assert.AreEqual(3, items.IndexOf(4));
            Assert.AreEqual(4, items.IndexOf(5));
            Assert.AreEqual(-1, items.IndexOf(6));
        }

        [Test]
        public void Insert_Should_InsertAnItem_When_Using_Comparer()
        {
            var items = new List<int> { 1, 3, 5 };
            var item = 4;
            var newItems = items.Insert(item, Comparer<int>.Default).ToArray();
            Assert.AreEqual(4, newItems.Length);
            Assert.AreEqual(1, newItems[0]);
            Assert.AreEqual(3, newItems[1]);
            Assert.AreEqual(4, newItems[2]);
            Assert.AreEqual(5, newItems[3]);
        }

        [Test]
        public void Insert_Should_InsertAnItem_When_Using_Predicate()
        {
            var items = new List<int> { 1, 3, 5 };
            var item = 4;
            {
                var newItems = items.Insert(item, n => n > 3).ToArray();
                Assert.AreEqual(4, newItems.Length);
                Assert.AreEqual(1, newItems[0]);
                Assert.AreEqual(3, newItems[1]);
                Assert.AreEqual(4, newItems[2]);
                Assert.AreEqual(5, newItems[3]);
            }
            {
                var newItems = items.Insert(item, n => n > 3 && n <= 5).ToArray();
                Assert.AreEqual(4, newItems.Length);
                Assert.AreEqual(1, newItems[0]);
                Assert.AreEqual(3, newItems[1]);
                Assert.AreEqual(4, newItems[2]);
                Assert.AreEqual(5, newItems[3]);
            }
        }

        [Test]
        public void Insert_Should_InsertItem_When_EmptyEnumerable_UsingComparer()
        {
            var items = new List<int>();
            var item = 4;
            var newItems = items.Insert(item, Comparer<int>.Default).ToList();
            Assert.IsTrue(newItems.Contains(item));
        }

        [Test]
        public void Insert_Should_InsertItem_When_EmptyEnumerable_Predicate()
        {
            var items = new List<int>();
            var item = 4;
            var newItems = items.Insert(item, n => n > 3).ToList();
            Assert.IsTrue(newItems.Contains(item));
        }

        [Test]
        public void IntersectBy()
        {
            var items1 = new List<string> {"1", "2", "3"};
            var items2 = new List<string> {"4", "2", "5"};

            var erg = items1.IntersectBy(items2, (string? lhs, string? rhs) => lhs == rhs, null).ToList();
            Assert.Contains("2", erg);
            Assert.AreEqual(1, erg.Count);
        }

        [Test]
        public void IsEqualTo_Should_ReturnTrue_When_SameNumberOfElementsAndSameOrder()
        {
            var items1 = Enumerable.Range(0, 5);
            var items2 = Enumerable.Range(0, 5);
            Assert.IsTrue(items1.IsEqualTo(items2));
            Assert.IsTrue(items2.IsEqualTo(items1));
        }

        [Test]
        public void IsEqualTo_Should_ReturnTrue_When_Items_SameNumberOfElementsAndDifferentOrder()
        {
            var items1 = Enumerable.Range(0, 5);
            var items2 = Enumerable.Range(0, 5).Shuffle();
            Assert.IsTrue(items1.IsEqualTo(items2));
            Assert.IsTrue(items2.IsEqualTo(items1));
        }

        [Test]
        public void IsEqualTo_Should_ReturnFalse_When_DifferentNumberOfElements()
        {
            var items1 = Enumerable.Range(0, 5);
            var items2 = Enumerable.Range(0, 6);
            Assert.IsFalse(items1.IsEqualTo(items2));
            Assert.IsFalse(items2.IsEqualTo(items1));
        }

        [Test]
        public void IsInAscendingOrder()
        {
            {
                var numbers = Enumerable.Range(0, 5);
                Assert.IsTrue(numbers.IsInAscendingOrder((a, b) =>
                {
                    if (a < b) return CompareResult.Smaller;
                    if (a > b) return CompareResult.Greater;
                    return CompareResult.Equal;
                }));
            }
            {
                var numbers = new[] { 3, 4, 4, 7, 9 };
                Assert.IsTrue(numbers.IsInAscendingOrder((a, b) =>
                {
                    if (a < b) return CompareResult.Smaller;
                    if (a > b) return CompareResult.Greater;
                    return CompareResult.Equal;
                }));
            }
            {
                var numbers = new[] { 4, 3, 7, 9 };
                Assert.IsFalse(numbers.IsInAscendingOrder((a, b) =>
                {
                    if (a < b) return CompareResult.Smaller;
                    if (a > b) return CompareResult.Greater;
                    return CompareResult.Equal;
                }));
            }
        }

        [Test]
        public void KCombinations_Should_ReturnPermutations_WithoutRepetitions_When_RepetitionsIsNotSet_Using_No_Duplicates()
        {
            var numbers = Enumerable.Range(1, 3);
            var kCombinations = numbers.KCombinations(2).ToArray();
            Assert.AreEqual(3, kCombinations.Length);

            Assert.IsTrue(kCombinations.Any(g => g.IsEqualTo(new[] { 1, 2 })));
            Assert.IsTrue(kCombinations.Any(g => g.IsEqualTo(new[] { 1, 3 })));
            Assert.IsTrue(kCombinations.Any(g => g.IsEqualTo(new[] { 2, 3 })));
        }

        [Test]
        public void KCombinationsWithRepetition_Should_ReturnKCombinations_When_RepetitionsIsNotSet_Using_No_Duplicates()
        {
            var numbers = Enumerable.Range(1, 3);
            var kCombinations = numbers.KCombinationsWithRepetition(2).ToArray();
            Assert.AreEqual(6, kCombinations.Length);

            Assert.IsTrue(kCombinations.Any(g => g.IsEqualTo(new[] { 1, 1 })));
            Assert.IsTrue(kCombinations.Any(g => g.IsEqualTo(new[] { 1, 2 })));
            Assert.IsTrue(kCombinations.Any(g => g.IsEqualTo(new[] { 1, 3 })));
            Assert.IsTrue(kCombinations.Any(g => g.IsEqualTo(new[] { 2, 2 })));
            Assert.IsTrue(kCombinations.Any(g => g.IsEqualTo(new[] { 2, 3 })));
            Assert.IsTrue(kCombinations.Any(g => g.IsEqualTo(new[] { 3, 3 })));
        }

        [Test]
        public void MinMax_Should_ReturnMinMax_When_UsingSelectorWithDifferentValues()
        {
            var numbers = Enumerable.Range(1, 10);
            var actual = numbers.MinMax();

            var expected = MinMax.New(1, 10);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MinMax_Should_ReturnMinMax_When_RepeatingValues()
        {
            var numbers = new[] { 1, 2, 2, 2, 5, 3, 3, 3, 3, 4 };
            var actual = numbers.MinMax();

            var expected = MinMax.New(1, 5);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MostFrequent_Should_ReturnRightValue_When_MultipleMaxValue()
        {
            var numbers = new[] { 1, 2, 2, 2, 2, 3, 3, 3, 3, 4 };
            var (mostFrequent, count) = numbers.MostFrequent(x => x);
            var items = mostFrequent.ToArray();
            Assert.AreEqual(2, items.Length);
            Assert.AreEqual(2, items[0]);
            Assert.AreEqual(3, items[1]);
            Assert.AreEqual(4, count);
        }

        [Test]
        public void MostFrequent_Should_ReturnRightValue_When_SingleMaxValue()
        {
            var numbers = new[] { 1, 2, 2, 3, 3, 3, 3, 4 };
            var (mostFrequent, count) = numbers.MostFrequent(x => x);
            var items = mostFrequent.ToArray();
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual(3, items[0]);
            Assert.AreEqual(4, count);
        }

        [Test]
        public void Nth()
        {
            var items = new List<int> { 1, 2, 3, 4, 5 };
            Assert.AreEqual(1, items.Nth(0).ValueOrThrow());
            Assert.AreEqual(2, items.Nth(1).ValueOrThrow());
            Assert.AreEqual(3, items.Nth(2).ValueOrThrow());
            Assert.AreEqual(4, items.Nth(3).ValueOrThrow());
            Assert.AreEqual(5, items.Nth(4).ValueOrThrow());
        }

        [Test]
        public void Nths()
        {
            {
                var items = Enumerable.Range(0, 10);
                var selected = items.Nths(1, 2, 5, 7).ToList();
                Assert.AreEqual(4, selected.Count);
                Assert.AreEqual(1, selected[0]);
                Assert.AreEqual(2, selected[1]);
                Assert.AreEqual(5, selected[2]);
                Assert.AreEqual(7, selected[3]);
            }

            //with invalid indexes
            {
                var items = Enumerable.Range(0, 10);
                var selected = items.Nths(-1, 2, 5, 17).ToList();
                Assert.AreEqual(2, selected.Count);
                Assert.AreEqual(2, selected[0]);
                Assert.AreEqual(5, selected[1]);
            }
        }

        [Test]
        public void Nths_Using_Range()
        {
            {
                var items = Enumerable.Range(0, 10);
                var selected = items.Nths(2..4).ToList();
                Assert.AreEqual(3, selected.Count);
                Assert.AreEqual(2, selected[0]);
                Assert.AreEqual(3, selected[1]);
                Assert.AreEqual(4, selected[2]);
            }

            //with invalid indexes
            {
                var items = Enumerable.Range(0, 10);
                var selected = items.Nths(-1, 2, 5, 17).ToList();
                Assert.AreEqual(2, selected.Count);
                Assert.AreEqual(2, selected[0]);
                Assert.AreEqual(5, selected[1]);
            }
        }

        [Test]
        public void OnFirst_ShouldJumpIntoAction_When_UsedAction()
        {
            var numbers = Enumerable.Range(0, 10);
            var actionCounter = 0;
            var loopCounter = 0;
            void action() => actionCounter++;

            foreach (var n in numbers.OnFirst(action))
                loopCounter++;

            Assert.AreEqual(1, actionCounter);
            Assert.AreEqual(10, loopCounter);
        }

        [Test]
        public void OnFirst_ShouldJumpIntoAction_When_UsedAction_WithArgument()
        {
            var numbers = Enumerable.Range(0, 10);
            var actionCounter = 0;
            var actionValue = -1;
            var loopCounter = 0;

            void action(int x)
            {
                actionCounter++;
                actionValue = x;
            }

            foreach (var n in numbers.OnFirst(action))
                loopCounter++;

            Assert.AreEqual(1, actionCounter);
            Assert.AreEqual(0, actionValue);
            Assert.AreEqual(10, loopCounter);
        }

        [Test]
        public void OnLast()
        {
            var numbers = Enumerable.Range(0, 10);
            var actionCounter = 0;
            var loopCounter = 0;
            void action() => actionCounter++;

            foreach (var n in numbers.OnLast(action))
                loopCounter++;

            Assert.AreEqual(1, actionCounter);
            Assert.AreEqual(10, loopCounter);
        }

        [Test]
        public void OnLast_WithArgument()
        {
            var numbers = Enumerable.Range(0, 10);
            var actionCounter = 0;
            var actionValue = -1;
            var loopCounter = 0;
            Action<int> action = (x) =>
            {
                actionCounter++;
                actionValue = x;
            };

            foreach (var n in numbers.OnLast(action))
                loopCounter++;

            Assert.AreEqual(1, actionCounter);
            Assert.AreEqual(9, actionValue);
            Assert.AreEqual(10, loopCounter);
        }

        [Test]
        public void OnAdjacentElements()
        {
            var numbers = Enumerable.Range(0, 5);

            var tuples = new List<(int, int)>();

            foreach (var n in numbers.OnAdjacentElements((prev, curr) => tuples.Add((prev, curr))))
            {
            }

            Assert.AreEqual(4, tuples.Count);

            var it = tuples.GetEnumerator();

            Assert.IsTrue(it.MoveNext());
            Assert.AreEqual((0, 1), it.Current);

            Assert.IsTrue(it.MoveNext());
            Assert.AreEqual((1, 2), it.Current);

            Assert.IsTrue(it.MoveNext());
            Assert.AreEqual((2, 3), it.Current);

            Assert.IsTrue(it.MoveNext());
            Assert.AreEqual((3, 4), it.Current);

            Assert.IsFalse(it.MoveNext());
        }

        [Test]
        public void Permutations_Should_ReturnPermutations_WithoutRepetitions_When_RepetitionsIsFalse_Using_No_Duplicates()
        {
            var numbers = Enumerable.Range(1, 3);
            var permutations = numbers.Permutations(2).ToArray();
            Assert.AreEqual(9, permutations.Length);

            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 1, 1 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 1, 2 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 1, 3 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 2, 1 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 2, 2 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 2, 3 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 3, 1 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 3, 2 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 3, 3 })));
        }

        [Test]
        public void Permutations_Should_Return3Permutations_When_LengthIs1()
        {
            var numbers = Enumerable.Range(1, 3);
            var permutations = numbers.Permutations(1).ToArray();
            Assert.AreEqual(3, permutations.Length);

            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 1 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 2 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 3 })));
        }

        [Test]
        public void PermutationsWithoutRepetition_Should_ReturnPermutations_WithoutRepetitions_When_Includes_Duplicates()
        {
            var numbers = new[] { 1, 1, 2, 3 };
            var permutations = numbers.PermutationsWithoutRepetition(2).ToArray();

            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 1, 2 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 1, 3 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 2, 1 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 2, 3 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 3, 1 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 3, 2 })));
        }

        [Test]
        public void Permutations_Should_ReturnPermutations_WithRepetitions_When_ContainsRepetitionsIsSet()
        {
            var numbers = Enumerable.Range(1, 3);
            var permutations = numbers.Permutations(2, true).ToArray();

            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 1, 1 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 1, 2 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 1, 3 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 2, 1 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 2, 2 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 2, 3 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 3, 1 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 3, 2 })));
            Assert.IsTrue(permutations.Any(g => g.IsEqualTo(new[] { 3, 3 })));
        }

        [Test]
        public void QuantitativeDifference_Should_ReturnValues_When_DifferentElementsAndMultipleOccurence()
        {
            var items1 = new[] { 3, 2, 2, 1 };
            var items2 = new[] { 1, 3, 4, 3 };

            var diff = items1.QuantitativeDifference(items2).ToList();
            Assert.AreEqual(4, diff.Count);
            Assert.AreEqual(2, diff.Where(n => n == 2).Count());
            Assert.AreEqual(1, diff.Where(n => n == 3).Count());
            Assert.AreEqual(1, diff.Where(n => n == 4).Count());
        }

        [Test]
        public void QuantitativeDifference_Should_ReturnValues_When_DifferentElementsSameNumberOfElements()
        {
            var items1 = new[] { 1, 2, 3 };
            var items2 = new[] { 1, 3, 4 };

            var diff = items1.QuantitativeDifference(items2).ToList();
            Assert.AreEqual(2, diff.Count);
            Assert.Contains(2, diff);
            Assert.Contains(4, diff);
        }

        [Test]
        public void QuantitativeDifference_Should_ReturnValues_When_SameElementsAndMultipleOccurence()
        {
            var items1 = new[] { 1, 2, 2, 3 };
            var items2 = new[] { 1, 2, 3, 3 };

            var diff = items1.QuantitativeDifference(items2).ToList();
            Assert.AreEqual(2, diff.Count);
            Assert.Contains(2, diff);
            Assert.Contains(3, diff);
        }

        [Test]
        public void QuantitativeDifference_Should_ReturnValues_When_SameElementsAndSameOccurence()
        {
            var items1 = new[] { 1, 2, 2, 3 };
            var items2 = new[] { 1, 2, 2, 3 };

            var diff = items1.QuantitativeDifference(items2).ToList();
            Assert.AreEqual(0, diff.Count);
        }

        [Test]
        public void RandomSubset()
        {
            var numbers = Enumerable.Range(1, 5).ToList();
            {
                var subset = numbers.RandomSubset(3).ToList();
                Assert.AreEqual(3, subset.Count);
                foreach (var randomSelected in subset)
                {
                    CollectionAssert.Contains(numbers, randomSelected);
                }
            }
            {
                var subset = numbers.RandomSubset(6).ToList();
                Assert.AreEqual(5, subset.Count);
                Assert.IsTrue(subset.IsEqualTo(numbers));
            }
        }

        [Test]
        public void RemoveTail()
        {
            var numbers = Enumerable.Range(0, 5);
            var expected = Enumerable.Range(0, 4);
            var actual = numbers.RemoveTail();
            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Replace_ShouldReturnList_When_ReplaceFizzBuzz()
        {
            var numbers = EnumerableEx.Generator(n => ++n, 1).Take(20).Select(n => n.ToString());

            var fizzBuzz = "FizzBuzz";
            var fizz = "Fizz";
            var buzz = "Buzz";

            var all = numbers.Replace((n, index) =>
            {
                if (0 == index) return n;

                var pos = index + 1;

                if (0 == pos % 15) return fizzBuzz;
                if (0 == pos % 3) return fizz;
                if (0 == pos % 5) return buzz;

                return n;
            }).ToArray();

            foreach(var (item, counter) in all.Enumerate())
            {
                if(0 == counter)
                {
                    Assert.AreEqual(item, "1");
                    continue;
                }
                var pos = counter + 1;

                if (0 == pos % 15)
                {
                    Assert.AreEqual(fizzBuzz, item);
                    continue;
                }
                if (0 == pos % 3)
                {
                    Assert.AreEqual(fizz, item);
                    continue;
                }

                if (0 == pos % 5)
                {
                    Assert.AreEqual(buzz, item);
                    continue;
                }

                Assert.AreEqual(item, pos.ToString());
            }
        }

        [Test]
        public void Replace_Should_ReturnReplacedList_When_ListIsLongerThanMaxReplaceIndex()
        {
            var numbers = Enumerable.Range(1, 5);
            var replaced = numbers.Replace(new[] { (20, 1), (40, 3) }).ToArray();

            Assert.AreEqual(5, replaced.Length);
            Assert.AreEqual(1, replaced[0]);
            Assert.AreEqual(20, replaced[1]);
            Assert.AreEqual(3, replaced[2]);
            Assert.AreEqual(40, replaced[3]);
            Assert.AreEqual(5, replaced[4]);
        }

        [Test]
        public void Replace_Should_ReturnReplacedList_When_ListIsLongerThanMaxReplaceIndexAndProjectIsSet()
        {
            var numbers = Enumerable.Range(1, 5);
            var replaced = numbers.Replace(new[] { (20, 1), (40, 3) }, n => n.ToString()).ToArray();

            Assert.AreEqual(5, replaced.Length);
            Assert.AreEqual("1", replaced[0]);
            Assert.AreEqual("20", replaced[1]);
            Assert.AreEqual("3", replaced[2]);
            Assert.AreEqual("40", replaced[3]);
            Assert.AreEqual("5", replaced[4]);
        }

        [Test]
        public void Replace_Should_ReturnReplacedList_WhenListIsLongerThanMaxReplaceIndexAndUsingPredicate()
        {
            var numbers = Enumerable.Range(1, 5);
            var replaced = numbers.Replace((n, _) => 0 == n % 2 ? n * 10 : n).ToArray();

            Assert.AreEqual(5, replaced.Length);
            Assert.AreEqual(1, replaced[0]);
            Assert.AreEqual(20, replaced[1]);
            Assert.AreEqual(3, replaced[2]);
            Assert.AreEqual(40, replaced[3]);
            Assert.AreEqual(5, replaced[4]);
        }

        [Test]
        public void Replace_Should_ReturnReplacedList_When_ListIsShorterThanMaxReplaceIndex()
        {
            var numbers = Enumerable.Range(1, 5);
            var replaced = numbers.Replace(new[] { (20, 1), (40, 3), (60, 5) }).ToArray();

            Assert.AreEqual(5, replaced.Length);
            Assert.AreEqual(1, replaced[0]);
            Assert.AreEqual(20, replaced[1]);
            Assert.AreEqual(3, replaced[2]);
            Assert.AreEqual(40, replaced[3]);
            Assert.AreEqual(5, replaced[4]);
        }

        [Test]
        public void Shuffle()
        {
            var items = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            var shuffled = items.ToList().Shuffle().ToList();
            Assert.IsFalse(shuffled.IsSameAs(items));
            Assert.IsTrue(EnumerableExtensions.IsEqualTo(items, shuffled));
        }

        [Test]
        public void Slice_ShouldReturn2Lists_When_2_PredicatesAreUsed()
        {
            //{0, 1, 2, 3, 4, 5}
            var numbers = Enumerable.Range(0, 6);
            var spliced = numbers.Slice(n => n % 2 == 0, n => n % 2 != 0).ToArray();
            Assert.AreEqual(2, spliced.Length);

            var even = spliced[0].ToArray();
            Assert.AreEqual(3, even.Length);
            Assert.AreEqual(0, even[0]);
            Assert.AreEqual(2, even[1]);
            Assert.AreEqual(4, even[2]);

            var odd = spliced[1].ToArray();
            Assert.AreEqual(3, odd.Length);
            Assert.AreEqual(1, odd[0]);
            Assert.AreEqual(3, odd[1]);
            Assert.AreEqual(5, odd[2]);
        }


        [Test]
        public void Slice_Should_ReturnListOf5Enumerables_When_ListWith10ElemsAndLength2()
        {
            var numberOfItems = 10;
            var chopSize = 2;
            var start = 1;

            var items = Enumerable.Range(start, numberOfItems);
            var slices = items.Slice(chopSize).ToArray();
            Assert.AreEqual(5, slices.Length);
            var value = start;

            foreach (var slice in slices)
            {
                foreach (var v in slice)
                {
                    Assert.AreEqual(value, v);
                    value++;
                }
            }
        }

        [Test]
        public void Slice_Should_ReturnListOf6Enumerables_When_ListWith11ElemsAndLength2()
        {
            var numberOfItems = 11;
            var chopSize = 2;
            var start = 1;

            var items = Enumerable.Range(start, numberOfItems);
            var slices = items.Slice(chopSize).ToArray();
            Assert.AreEqual(6, slices.Length);
            var value = start;

            foreach (var slice in slices)
            {
                foreach (var v in slice)
                {
                    Assert.AreEqual(value, v);
                    value++;
                }
            }
        }

        [Test]
        public void ToBreakable()
        {
            {
                var items1 = Enumerable.Range(0, 3);
                var items2 = Enumerable.Range(0, 3);
 
                var i1 = 0;
                var i2 = 0;
                var stop = ObservableValue.Create(false);
                foreach(var item1 in items1.ToBreakable(ref stop))
                {
                    i1++;
                    foreach (var item2 in items2.ToBreakable(ref stop))
                    {
                        i2++;
                        
                        if (item2 == 2)
                            stop.Value = true;
                    }
                }

                Assert.AreEqual(1, i1);
                Assert.AreEqual(3, i2);
            }

            {
                var items1 = Enumerable.Range(0, 3);
                var items2 = Enumerable.Range(0, 3);
                var items3 = Enumerable.Range(0, 3);

                var i1 = 0;
                var i2 = 0;
                var i3 = 0;

                foreach (var item1 in items1)
                {
                    var stop = ObservableValue.Create(false);

                    i1++;
                    foreach (var item2 in items2.ToBreakable(ref stop))
                    {
                        i2++;
                        foreach (var item3 in items3.ToBreakable(ref stop))
                        {
                            i3++;
                            if (item3 == 1)
                                stop.Value = true;
                        }
                    }
                }

                Assert.AreEqual(3, i1);
                Assert.AreEqual(3, i2);
                Assert.AreEqual(6, i3);
            }
        }

        [Test]
        public void ToBreakable_Cascaded()
        {
            var items1 = Enumerable.Range(0, 3);
            var items2 = Enumerable.Range(0, 3);
            var items3 = Enumerable.Range(0, 3);

            var i1 = 0;
            var i2 = 0;
            var i3 = 0;
            var stop = ObservableValue.Create(false);
            var stopAll = ObservableValue.Create(false);
            foreach (var item1 in items1.ToBreakable(ref stopAll))
            {
                i1++;
                foreach (var item2 in items2.ToBreakable(ref stop)
                                            .ToBreakable(ref stopAll))
                {
                    i2++;
                    foreach (var item3 in items3.ToBreakable(ref stop)
                                                .ToBreakable(ref stopAll))
                    {
                        i3++;

                        if (item1 == 0 && item3 == 1)
                            stop.Value = true;

                        if (item2 == 1)
                            stopAll.Value = true;
                    }
                }
            }

            Assert.AreEqual(2, i1);
            Assert.AreEqual(3, i2);
            Assert.AreEqual(6, i3);
        }

        [Test]
        public void ToDualOrdinalStreams_Should_ReturnDualOrdinalStreams_When_PredicateIsFizzBuzz_And_IsExhaustiveIsTrue()
        {
            var numbers = EnumerableEx.Generator(n => ++n, 1).Take(50);

            var fizzBuzz = "FizzBuzz";
            var fizz = "Fizz";
            var buzz = "Buzz";

            var all = numbers.ToDualOrdinalStreams(n => 0 == n % 15, n => fizzBuzz, true)
                             .FilterLeft(n => 0 == n % 3, n => fizz, true)
                             .FilterLeft(n => 0 == n % 5, n => buzz, true)
                             .MergeStreams(n => n.ToString())
                             .ToArray();

            foreach (var (item, counter) in all.Enumerate())
            {
                if (0 == counter)
                {
                    Assert.AreEqual(item, "1");
                    continue;
                }
                var pos = counter + 1;

                if (0 == pos % 15)
                {
                    Assert.AreEqual(fizzBuzz, item);
                    continue;
                }
                if (0 == pos % 3)
                {
                    Assert.AreEqual(fizz, item);
                    continue;
                }

                if (0 == pos % 5)
                {
                    Assert.AreEqual(buzz, item);
                    continue;
                }

                Assert.AreEqual(item, pos.ToString());
            }

        }

        [Test]
        public void WhereByIndex_InRange_IsBetweenValue()
        {
            var list = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            var items = list.AsEnumerable();
            const int min = 2;
            const int max = 6;
            var foundItems = items.WhereByIndex(Range.Create(Is.Between<long>(min, max))).ToList();
            Assert.AreEqual(5, foundItems.Count);
            for (int i = min, j = 0; i <= max; i++, j++)
                Assert.AreEqual(list[i], foundItems[j]);
        }

        [Test]
        public void WhereByIndex_InRange_IsMatchingMaxValue()
        {
            var list = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            var items = list.AsEnumerable();
            const int max = 5;
            var foundItems = items.WhereByIndex(Range.Create(Is.Matching<long>(x => x <= max))).ToList();
            Assert.AreEqual(6, foundItems.Count);
            for (int i = 0, j = 0; i < max; i++, j++)
                Assert.AreEqual(list[i], foundItems[j]);
        }

        [Test]
        public void WhereByIndex_InRange_IsMatchingMinValue()
        {
            var list = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            var items = list.AsEnumerable();
            const int min = 2;
            var foundItems = items.WhereByIndex(Range.Create(Is.Matching<long>(x => x >= min))).ToList();
            Assert.AreEqual(8, foundItems.Count);
            for (int i = min, j = 0; i < list.Count; i++, j++)
                Assert.AreEqual(list[i], foundItems[j]);
        }

        [Test]
        public void WhereByIndex_InRange_IsMatchingMinAndMaxValue()
        {
            var list = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            var items = list.AsEnumerable();
            const int min = 2;
            const int max = 6;
            var foundItems = items.WhereByIndex(Range.Create(Is.Matching<long>(x => x >= min && x <= max))).ToList();
            Assert.AreEqual(5, foundItems.Count);
            for (int i = min, j = 0; i <= max; i++, j++)
                Assert.AreEqual(list[i], foundItems[j]);
        }

        [Test]
        public void Zip()
        {
            var items1 = new List<A> { new A("1"), new A("2"), new A("3") };
            var items2 = new List<A> { new A("a"), new A("b"), new A("c"), new A("1"), new A("3") };

            var mapping = items1.Zip(items2, (f, s) => f.Name == s.Name, (f, s) => (f, s)).ToList();
            Assert.AreEqual(2, mapping.Count);
            foreach (var (f, s) in mapping)
            {
                Assert.AreNotEqual(f.Id, s.Id);
                Assert.AreEqual(f.Name, s.Name);
            }
        }

        [Test]
        public void Zip_ListIncludesSameValues()
        {
            var items1 = new List<A> { new A("1"), new A("2"), new A("3") };
            var items2 = new List<A> { new A("a"), new A("b"), new A("c"), new A("1"), new A("3"), new A("1") };
            var mapping = items1.Zip(items2, (f, s) => f.Name == s.Name, (f, s) => (f, s)).ToList();
            Assert.AreEqual(3, mapping.Count);
            foreach (var (f, s) in mapping)
            {
                Assert.AreNotEqual(f.Id, s.Id);
                Assert.AreEqual(f.Name, s.Name);
            }
        }

        [Test]
        public void Zip_WithoutMappingValue()
        {
            var items1 = new List<A> { new A("1"), new A("2"), new A("3") };
            var items2 = new List<A> { new A("a"), new A("b"), new A("c") };
            var mapping = items1.Zip(items2, (f, s) => f.Name == s.Name, (f, s) => (f, s)).ToList();
            Assert.AreEqual(0, mapping.Count);
        }
    }

    // ReSharper restore InconsistentNaming
}

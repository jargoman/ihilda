using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Ripple.Testing.Utils
{
    public class PartitionedTests
    {
        public const string TestPrefixPattern = @"^(P(\d+)).*$";

        private readonly SortedList<int, MethodInfo> _tests;
        private int _expectedTest;
        private bool _failed;
        private Type _testAttributeType;

        private PartitionedTests(SortedList<int, MethodInfo> tests, Type testAttribute)
        {
            _tests = tests;
            _failed = false;
            _testAttributeType = testAttribute;
        }

        public void PreTest(string testName, Action<List<MethodInfo>> onAnyPriors)
        {
            if (_failed)
            {
                throw new InvalidOperationException("Dependent on prior state");
            }

            var nth = GetTextIndex(testName);

            if (nth > _expectedTest)
            {
                onAnyPriors(GetTestsPriorTo(nth));
            }
            _expectedTest++;
        }

        public void PostTest(bool failed)
        {
            if (failed)
            {
                _failed = true;
            }
        }

        public static PartitionedTests Create (Type type, Type testAttribute)
        {
            var nl = Environment.NewLine;
            var tests = GetTests(type, testAttribute);
            var matches = tests.Select(m =>
            {
                var match = Regex.Match(m.Name, TestPrefixPattern);
                return match;
            }).ToList();

            var matched = matches.Where(m => m.Success).ToList();
            var unmatched = matches.Select((m, i) => tests[i].Name)
                                   .Where((n, i) => !matches[i].Success)
                                   .ToList();

            if (unmatched.Count != 0)
            {
                throw new InvalidOperationException(
                    $"not all tests matched {TestPrefixPattern}:{nl}" +
                    $"{string.Join(nl, unmatched)} ");
            }

            var prefixes = matched.Select(m =>  m.Groups[1].Value).ToList();
            var uniqueLengths = prefixes.Select(p => p.Length).Distinct().Count();

            if (uniqueLengths != 1)
            {
                throw new InvalidOperationException(
                    "All test prefixes must be of equal length:" +
                    $"{nl}{string.Join(",", prefixes)}{nl}");
            }

            var sortedTests = new SortedList<int, MethodInfo>(
                      tests.ToDictionary(t => PNumberFromName(t.Name)));
            return new PartitionedTests(sortedTests, testAttribute);
        }

        public static int PNumberFromName(string testName)
        {
            var match = Regex.Match(testName, TestPrefixPattern);
            var groups = match.Groups;
            var nth = int.Parse(groups[2].Value);
            return nth;
        }

        public static List<MethodInfo> GetTests(Type type, Type testAttributeType)
        {
            return type.GetMethods()
                .Where(y => y.GetCustomAttributes()
                             .Any(a => a.GetType() == testAttributeType))
                .ToList();
        }

        public int GetTextIndex(string testName)
        {
            return _tests.IndexOfKey(PNumberFromName(testName));
        }

        public List<MethodInfo> GetTestsPriorTo(int nth)
        {
            // TODO: _tests.Values ?
            return _tests.Where((kv, i) => i < nth)
                        .Select(kv => kv.Value)
                        .ToList();
        }
    }
}
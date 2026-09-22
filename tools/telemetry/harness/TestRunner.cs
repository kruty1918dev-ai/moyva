using System;
using System.Collections.Generic;

namespace Kruty1918.Telemetry.Harness
{
    /// <summary>Zero-dependency test runner so core verification works without Unity or NuGet.</summary>
    public static class TestRunner
    {
        public sealed class Failure : Exception { public Failure(string m) : base(m) { } }

        public static void Assert(bool cond, string msg)
        {
            if (!cond) throw new Failure("ASSERT FAILED: " + msg);
        }

        public static void AssertEq(object expected, object actual, string what)
        {
            if (!Equals(expected, actual))
                throw new Failure($"ASSERT {what}: expected '{expected}' got '{actual}'");
        }

        public static int Run(IEnumerable<(string name, Action body)> tests)
        {
            int pass = 0, fail = 0;
            foreach (var (name, body) in tests)
            {
                try { body(); pass++; Console.WriteLine("PASS " + name); }
                catch (Exception e)
                {
                    fail++;
                    Console.WriteLine("FAIL " + name + " :: " + e.Message);
                }
            }
            Console.WriteLine($"--- {pass} passed, {fail} failed ---");
            return fail == 0 ? 0 : 1;
        }
    }
}

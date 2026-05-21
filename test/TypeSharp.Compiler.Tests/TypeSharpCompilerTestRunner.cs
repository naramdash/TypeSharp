using System;
using System.Collections.Generic;

internal static class TypeSharpCompilerTestRunner
{
    public static int Run(IReadOnlyList<TypeSharpCompilerTestCase> tests, string[] args)
    {
        var failures = 0;
        var runnerSettings = TestRunnerSettings.Create(args);
        var executed = 0;
        var testOrdinal = 0;

        foreach (var test in tests)
        {
            var currentOrdinal = testOrdinal++;
            if (!runnerSettings.Includes(test.Name, currentOrdinal))
            {
                continue;
            }

            executed++;
            try
            {
                test.Body();
                Console.WriteLine($"PASS {test.Name}");
                Console.Out.Flush();
            }
            catch (Exception ex)
            {
                failures++;
                Console.Error.WriteLine($"FAIL {test.Name}: {ex.Message}");
                Console.Error.Flush();
            }
        }

        if (executed == 0)
        {
            failures++;
            Console.Error.WriteLine(runnerSettings.FormatNoTestsMessage());
            Console.Error.Flush();
        }

        return failures == 0 ? 0 : 1;
    }
}

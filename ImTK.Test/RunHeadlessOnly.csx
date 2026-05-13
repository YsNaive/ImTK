using System;
using ImTK.Test.Framework;

bool result = HeadlessRunner.RunAllHeadlessTests();
Console.WriteLine(result ? "ALL PASSED" : "FAILED");
Environment.Exit(result ? 0 : 1);

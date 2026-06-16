// Copyright (c) 2026 Roger Brown.
// Licensed under the MIT License.

#if NETCOREAPP
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace RhubarbGeekNz.ItEscapesMe
{
    [TestClass]
    public class UnitTests
    {
        readonly InitialSessionState initialSessionState = InitialSessionState.CreateDefault();
        public UnitTests()
        {
            foreach (Type t in new Type[] {
                typeof(ConvertToEscapeString)
            })
            {
                CmdletAttribute ca = t.GetCustomAttribute<CmdletAttribute>();

                if (ca == null) throw new NullReferenceException();

                initialSessionState.Commands.Add(new SessionStateCmdletEntry($"{ca.VerbName}-{ca.NounName}", t, ca.HelpUri));
            }

            initialSessionState.Variables.Add(new SessionStateVariableEntry("ErrorActionPreference", ActionPreference.Stop, "Stop action"));
        }

        [TestMethod]
        public void TestPrintables()
        {
            using (PowerShell powerShell = PowerShell.Create(initialSessionState))
            {
                powerShell.AddScript("'1234' | ConvertTo-EscapeString");

                var outputPipeline = powerShell.Invoke();

                Assert.AreEqual(1, outputPipeline.Count);

                Assert.AreEqual("1234", outputPipeline[0].BaseObject.ToString());
            }
        }


        [TestMethod]
        public void TestNull()
        {
            using (PowerShell powerShell = PowerShell.Create(initialSessionState))
            {
                powerShell.AddScript("$null | ConvertTo-EscapeString");
                var outputPipeline = powerShell.Invoke();
                Assert.AreEqual(0, outputPipeline.Count);
            }
        }

        [TestMethod]
        public void TestControls()
        {
            using (PowerShell powerShell = PowerShell.Create(initialSessionState))
            {
                powerShell.AddScript("\"`0`a`b`t`r`n\" | ConvertTo-EscapeString");

                var outputPipeline = powerShell.Invoke();

                Assert.AreEqual(1, outputPipeline.Count);

                Assert.AreEqual("`0`a`b`t`r`n", outputPipeline[0].BaseObject.ToString());
            }
        }

        [TestMethod]
        public void TestDelete()
        {
            using (PowerShell powerShell = PowerShell.Create(initialSessionState))
            {
                powerShell.AddScript("[char]127 | ConvertTo-EscapeString");

                var outputPipeline = powerShell.Invoke();

                Assert.AreEqual(1, outputPipeline.Count);

#if NETCOREAPP
                Assert.AreEqual("`u{7F}", outputPipeline[0].BaseObject.ToString());
#else
                Assert.AreEqual("$([char]127)", outputPipeline[0].BaseObject.ToString());
#endif
            }
        }

        [TestMethod]
        public void TestEscape()
        {
            using (PowerShell powerShell = PowerShell.Create(initialSessionState))
            {
                powerShell.AddScript("[char]27 | ConvertTo-EscapeString");

                var outputPipeline = powerShell.Invoke();

                Assert.AreEqual(1, outputPipeline.Count);

#if NETCOREAPP
                Assert.AreEqual("`e", outputPipeline[0].BaseObject.ToString());
#else
                Assert.AreEqual("$([char]27)", outputPipeline[0].BaseObject.ToString());
#endif
            }
        }

#if NETCOREAPP
        [TestMethod]
        public void TestHappyShinyPeople()
        {
            using (PowerShell powerShell = PowerShell.Create(initialSessionState))
            {
                powerShell.AddScript("[Text.Rune]0x1F600 | ConvertTo-EscapeString");

                var outputPipeline = powerShell.Invoke();

                Assert.AreEqual(1, outputPipeline.Count);

                Assert.AreEqual("`u{1F600}", outputPipeline[0].BaseObject.ToString());
            }
        }
#endif

        [TestMethod]
        public void TestPolePosition()
        {
            using (PowerShell powerShell = PowerShell.Create(initialSessionState))
            {
                powerShell.AddScript("ConvertTo-EscapeString 'foo'");

                var outputPipeline = powerShell.Invoke();

                Assert.AreEqual(1, outputPipeline.Count);

                Assert.AreEqual("foo", outputPipeline[0].BaseObject.ToString());
            }
        }

        [TestMethod]
        public void TestBackTick()
        {
            using (PowerShell powerShell = PowerShell.Create(initialSessionState))
            {
                powerShell.AddScript("ConvertTo-EscapeString '`'");

                var outputPipeline = powerShell.Invoke();

                Assert.AreEqual(1, outputPipeline.Count);

                Assert.AreEqual("``", outputPipeline[0].BaseObject.ToString());
            }
        }

        [TestMethod]
        public void TestBigEndBom()
        {
            using (PowerShell powerShell = PowerShell.Create(initialSessionState))
            {
                powerShell.AddScript("[char]0xFEFF | ConvertTo-EscapeString");
                var outputPipeline = powerShell.Invoke();
                Assert.AreEqual("", outputPipeline[0].BaseObject.ToString());
            }
        }

        [TestMethod]
        public void TestLittleEndBom()
        {
            using (PowerShell powerShell = PowerShell.Create(initialSessionState))
            {
                powerShell.AddScript("[char]0xFFFE | ConvertTo-EscapeString");
                var outputPipeline = powerShell.Invoke();
                Assert.AreEqual("", outputPipeline[0].BaseObject.ToString());
            }
        }
    }
}

// Copyright (c) 2026 Roger Brown.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using System.Text;

namespace RhubarbGeekNz.ItEscapesMe
{
    [Cmdlet(VerbsData.ConvertTo, "EscapeString")]
    [OutputType(typeof(string))]
    sealed public class ConvertToEscapeString : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "String Data", Position = 0)]
        [AllowNull()]
        [AllowEmptyCollection()]
        public String InputString;

        private static readonly object[] EmptyList = { };
        private static readonly MethodInfo EnumerateRunes = typeof(String).GetMethod("EnumerateRunes");
        private static readonly PropertyInfo GetRuneValue = EnumerateRunes == null ? null : EnumerateRunes.ReturnType.GetProperty("Current").PropertyType.GetProperty("Value");
        private static readonly IDictionary<int, string> ControlCodes = CreateControlCodes();

        private static IDictionary<int, string> CreateControlCodes()
        {
            IDictionary<int, string> controlCodes = new Dictionary<int, string>
                {
                    { 0, "`0" },
                    { 7, "`a" },
                    { 8, "`b" },
                    { 9, "`t" },
                    { 10, "`n" },
                    { 11, "`v" },
                    { 12, "`f" },
                    { 13, "`r" }
                };

            if (EnumerateRunes != null)
            {
                controlCodes.Add(27, "`e");
            }

            return controlCodes;
        }

        protected override void ProcessRecord()
        {
            if (InputString != null)
            {
                StringBuilder stringBuilder = new StringBuilder();

                if (EnumerateRunes != null)
                {
                    using (IDisposable d = (IDisposable)EnumerateRunes.Invoke(InputString, EmptyList))
                    {
                        IEnumerator enumerator = (IEnumerator)d;

                        while (enumerator.MoveNext())
                        {
                            int value = (int)GetRuneValue.GetValue(enumerator.Current);

                            if (value > 126 || value < 32)
                            {
                                if (value < 32 && ControlCodes.TryGetValue(value, out string controlCode))
                                {
                                    stringBuilder.Append(controlCode);
                                }
                                else
                                {
                                    switch (value)
                                    {
                                        case 0xFEFF:
                                        case 0xFFFE:
                                            break;
                                        default:
                                            stringBuilder.Append("`u{");
                                            stringBuilder.Append(value.ToString("X"));
                                            stringBuilder.Append("}");
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                switch (value)
                                {
                                    case '`':
                                        stringBuilder.Append("``");
                                        break;
                                    case '$':
                                        stringBuilder.Append("`$");
                                        break;
                                    default:
                                        stringBuilder.Append((char)value);
                                        break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (UInt16 value in InputString)
                    {
                        if (value > 126 || value < 32)
                        {
                            if (value < 32 && ControlCodes.TryGetValue(value, out string controlCode))
                            {
                                stringBuilder.Append(controlCode);
                            }
                            else
                            {
                                switch (value)
                                {
                                    case 0xFEFF:
                                    case 0xFFFE:
                                        break;
                                    default:
                                        stringBuilder.Append("$([char]");
                                        stringBuilder.Append(value.ToString());
                                        stringBuilder.Append(")");
                                        break;
                                }
                            }
                        }
                        else
                        {
                            switch (value)
                            {
                                case '`':
                                    stringBuilder.Append("``");
                                    break;
                                case '$':
                                    stringBuilder.Append("`$");
                                    break;
                                default:
                                    stringBuilder.Append((char)value);
                                    break;
                            }
                        }
                    }
                }

                WriteObject(stringBuilder.ToString());
            }
        }
    }
}

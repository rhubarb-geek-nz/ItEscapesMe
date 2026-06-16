// Copyright (c) 2026 Roger Brown.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
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

        static object[] EmptyList= { };

        protected override void ProcessRecord()
        {
            if (InputString != null)
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
                StringBuilder stringBuilder=new StringBuilder();

                var method = InputString.GetType().GetMethod("EnumerateRunes");

                if (method != null)
                {
                    controlCodes.Add(27, "`e");

                    using (IDisposable d = (IDisposable)method.Invoke(InputString, EmptyList))
                    {
                        IEnumerator enumerator = (IEnumerator)d;
                        var property = enumerator.GetType().GetProperty("Current").PropertyType.GetProperty("Value");

                        while (enumerator.MoveNext())
                        {
                            int value = (int)property.GetValue(enumerator.Current);

                            if (value > 126 || value < 32)
                            {
                                if (value < 32 && controlCodes.ContainsKey(value))
                                {
                                    stringBuilder.Append(controlCodes[value]);
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
                    using (var enumerator = InputString.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            int value = enumerator.Current;

                            if (value > 126 || value < 32)
                            {
                                if (value < 32 && controlCodes.ContainsKey(value))
                                {
                                    stringBuilder.Append(controlCodes[value]);
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
                }

                WriteObject(stringBuilder.ToString());
            }
        }
    }
}

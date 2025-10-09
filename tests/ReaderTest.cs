/*
 * 2006 - 2018 Ted Spence, http://tedspence.com
 * License: http://www.apache.org/licenses/LICENSE-2.0 
 * Home page: https://github.com/tspence/csharp-csv-reader
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CSVFile;
#if HAS_ASYNC
using System.Threading.Tasks;
#endif

// ReSharper disable StringLiteralTypo

namespace CSVTestSuite
{
    [TestFixture]
    public class ReaderTest
    {
        [Test]
        public void TestBasicReader()
        {
            var source = "Name,Title,Phone\n" +
                         "JD,Doctor,x234\n" +
                         "Janitor,Janitor,x235\n" +
                         "\"Dr. Reed, " + Environment.NewLine + "Eliot\",\"Private \"\"Practice\"\"\",x236\n" +
                         "Dr. Kelso,Chief of Medicine,x100";

            // Skip header row
            var settings = new CSVSettings()
            {
                HeaderRowIncluded = false,
                LineSeparator = "\n",
            };

            // Convert into stream
            using (var cr = CSVReader.FromString(source, settings))
            {
                var i = 0;
                foreach (var line in cr)
                {
                    Assert.AreEqual(3, line.Length);
                    switch (i)
                    {
                        case 0:
                            Assert.AreEqual("Name", line[0]);
                            Assert.AreEqual("Title", line[1]);
                            Assert.AreEqual("Phone", line[2]);
                            break;
                        case 1:
                            Assert.AreEqual("JD", line[0]);
                            Assert.AreEqual("Doctor", line[1]);
                            Assert.AreEqual("x234", line[2]);
                            break;
                        case 2:
                            Assert.AreEqual("Janitor", line[0]);
                            Assert.AreEqual("Janitor", line[1]);
                            Assert.AreEqual("x235", line[2]);
                            break;
                        case 3:
                            Assert.AreEqual("Dr. Reed, " + Environment.NewLine + "Eliot", line[0]);
                            Assert.AreEqual("Private \"Practice\"", line[1]);
                            Assert.AreEqual("x236", line[2]);
                            break;
                        case 4:
                            Assert.AreEqual("Dr. Kelso", line[0]);
                            Assert.AreEqual("Chief of Medicine", line[1]);
                            Assert.AreEqual("x100", line[2]);
                            break;
                        default:
                            Assert.IsTrue(false, "Should not get here");
                            break;
                    }
                    i++;
                }
            }
        }

        [Test]
        public void TestDanglingFields()
        {
            var source = "Name,Title,Phone,Dangle\n" +
                "JD,Doctor,x234,\n" +
                "Janitor,Janitor,x235,\n" +
                "\"Dr. Reed, " + Environment.NewLine + "Eliot\",\"Private \"\"Practice\"\"\",x236,\n" +
                "Dr. Kelso,Chief of Medicine,x100,\n" +
                ",,,";

            // Skip header row
            var settings = new CSVSettings()
            {
                HeaderRowIncluded = false,
                LineSeparator = "\n",
            };

            // Convert into stream
            using (var cr = CSVReader.FromString(source, settings))
            {
                var i = 0;
                foreach (var line in cr)
                {
                    Assert.AreEqual(4, line.Length);
                    switch (i)
                    {
                        case 0:
                            Assert.AreEqual("Name", line[0]);
                            Assert.AreEqual("Title", line[1]);
                            Assert.AreEqual("Phone", line[2]);
                            Assert.AreEqual("Dangle", line[3]);
                            break;
                        case 1:
                            Assert.AreEqual("JD", line[0]);
                            Assert.AreEqual("Doctor", line[1]);
                            Assert.AreEqual("x234", line[2]);
                            Assert.AreEqual("", line[3]);
                            break;
                        case 2:
                            Assert.AreEqual("Janitor", line[0]);
                            Assert.AreEqual("Janitor", line[1]);
                            Assert.AreEqual("x235", line[2]);
                            Assert.AreEqual("", line[3]);
                            break;
                        case 3:
                            Assert.AreEqual("Dr. Reed, " + Environment.NewLine + "Eliot", line[0]);
                            Assert.AreEqual("Private \"Practice\"", line[1]);
                            Assert.AreEqual("x236", line[2]);
                            Assert.AreEqual("", line[3]);
                            break;
                        case 4:
                            Assert.AreEqual("Dr. Kelso", line[0]);
                            Assert.AreEqual("Chief of Medicine", line[1]);
                            Assert.AreEqual("x100", line[2]);
                            Assert.AreEqual("", line[3]);
                            break;
                        case 5:
                            Assert.AreEqual("", line[0]);
                            Assert.AreEqual("", line[1]);
                            Assert.AreEqual("", line[2]);
                            Assert.AreEqual("", line[3]);
                            break;
                        default:
                            Assert.IsTrue(false, "Should not get here");
                            break;
                    }

                    i++;
                }
            }
        }

        [Test]
        public void TestAlternateDelimiterQualifiers()
        {
            var source = "Name\tTitle\tPhone\n" +
                         "JD\tDoctor\tx234\n" +
                         "Janitor\tJanitor\tx235\n" +
                         "\"Dr. Reed, " + Environment.NewLine + "Eliot\"\t\"Private \"\"Practice\"\"\"\tx236\n" +
                         "Dr. Kelso\tChief of Medicine\tx100";

            // Convert into stream
            var settings = new CSVSettings() { HeaderRowIncluded = true, FieldDelimiter = '\t', LineSeparator = "\n", };
            using (var cr = CSVReader.FromString(source, settings))
            {
                Assert.AreEqual("Name", cr.Headers[0]);
                Assert.AreEqual("Title", cr.Headers[1]);
                Assert.AreEqual("Phone", cr.Headers[2]);
                var i = 1;
                foreach (var line in cr)
                {
                    Assert.AreEqual(3, line.Length);
                    switch (i)
                    {
                        case 1:
                            Assert.AreEqual("JD", line[0]);
                            Assert.AreEqual("Doctor", line[1]);
                            Assert.AreEqual("x234", line[2]);
                            break;
                        case 2:
                            Assert.AreEqual("Janitor", line[0]);
                            Assert.AreEqual("Janitor", line[1]);
                            Assert.AreEqual("x235", line[2]);
                            break;
                        case 3:
                            Assert.AreEqual("Dr. Reed, " + Environment.NewLine + "Eliot", line[0]);
                            Assert.AreEqual("Private \"Practice\"", line[1]);
                            Assert.AreEqual("x236", line[2]);
                            break;
                        case 4:
                            Assert.AreEqual("Dr. Kelso", line[0]);
                            Assert.AreEqual("Chief of Medicine", line[1]);
                            Assert.AreEqual("x100", line[2]);
                            break;
                    }

                    i++;
                }
            }
        }
        
        
        [Test]
        public void TestSepLineOverride()
        {
            // The "sep=" line is a feature of Microsoft Excel that makes CSV files work more smoothly with
            // European data sets where a comma is used in numerical separation.  If present, it overrides
            // the FieldDelimiter setting from the CSV Settings.
            var source = "sep=|\n" +
                         "Name|Title|Phone\n" +
                         "JD|Doctor|x234\n" +
                         "Janitor|Janitor|x235\n" +
                         "\"Dr. Reed, " + Environment.NewLine + "Eliot\"|\"Private \"\"Practice\"\"\"|x236\n" +
                         "Dr. Kelso|Chief of Medicine|x100";

            // Convert into stream
            var settings = new CSVSettings() { HeaderRowIncluded = true, FieldDelimiter = '\t', AllowSepLine = true, LineSeparator = "\n", };
            using (var cr = CSVReader.FromString(source, settings))
            {
                // The field delimiter should have been changed, but the original object should remain the same
                Assert.AreEqual('\t', settings.FieldDelimiter);
                Assert.AreEqual('|', cr.Settings.FieldDelimiter);
                Assert.AreEqual("Name", cr.Headers[0]);
                Assert.AreEqual("Title", cr.Headers[1]);
                Assert.AreEqual("Phone", cr.Headers[2]);
                var i = 1;
                foreach (var line in cr)
                {
                    switch (i)
                    {
                        case 1:
                            Assert.AreEqual("JD", line[0]);
                            Assert.AreEqual("Doctor", line[1]);
                            Assert.AreEqual("x234", line[2]);
                            break;
                        case 2:
                            Assert.AreEqual("Janitor", line[0]);
                            Assert.AreEqual("Janitor", line[1]);
                            Assert.AreEqual("x235", line[2]);
                            break;
                        case 3:
                            Assert.AreEqual("Dr. Reed, " + Environment.NewLine + "Eliot", line[0]);
                            Assert.AreEqual("Private \"Practice\"", line[1]);
                            Assert.AreEqual("x236", line[2]);
                            break;
                        case 4:
                            Assert.AreEqual("Dr. Kelso", line[0]);
                            Assert.AreEqual("Chief of Medicine", line[1]);
                            Assert.AreEqual("x100", line[2]);
                            break;
                    }

                    i++;
                }
            }
        }
        
                
        [Test]
        public void TestIssue53()
        {
            var settings = new CSVSettings()
            {
                HeaderRowIncluded = false
            };
            
            // This use case was reported by wvdvegt as https://github.com/tspence/csharp-csv-reader/issues/53
            var source = "\"test\",\"" + Environment.NewLine + "\",,,,\"Normal\",\"False\",,,\"Normal\",\"\"";
            using (var cr = CSVReader.FromString(source, settings))
            {
                foreach (var line in cr.Lines())
                {
                    Assert.AreEqual("test", line[0]);
                    Assert.AreEqual(Environment.NewLine, line[1]);
                    Assert.AreEqual("", line[2]);
                    Assert.AreEqual("", line[3]);
                    Assert.AreEqual("", line[4]);
                    Assert.AreEqual("Normal", line[5]);
                    Assert.AreEqual("False", line[6]);
                    Assert.AreEqual("", line[7]);
                    Assert.AreEqual("", line[8]);
                    Assert.AreEqual("Normal", line[9]);
                    Assert.AreEqual("", line[10]);
                }
            }
        }

        [Test]
        public void TestMultipleNewlines()
        {
            var settings = new CSVSettings()
            {
                HeaderRowIncluded = false,
                LineSeparator = "\r\n",
            };

            // This use case was reported by domdere as https://github.com/tspence/csharp-csv-reader/issues/59
            var source = "\"test\",\"blah\r\n\r\n\r\nfoo\",\"Normal\"";
            using (var cr = CSVReader.FromString(source, settings))
            {
                foreach (var line in cr.Lines())
                {
                    Assert.AreEqual("test", line[0]);
                    Assert.AreEqual("blah\r\n\r\n\r\nfoo", line[1]);
                    Assert.AreEqual("Normal", line[2]);
                }
            }

            // Test a few potential use cases here
            var source2 = "\"test\",\"\n\n\",\"\r\n\r\n\r\n\",\"Normal\",\"\",\"\r\r\r\r\r\"";
            using (var cr = CSVReader.FromString(source2, settings))
            {
                foreach (var line in cr.Lines())
                {
                    Assert.AreEqual("test", line[0]);
                    Assert.AreEqual("\n\n", line[1]);
                    Assert.AreEqual("\r\n\r\n\r\n", line[2]);
                    Assert.AreEqual("Normal", line[3]);
                    Assert.AreEqual("", line[4]);
                    Assert.AreEqual("\r\r\r\r\r", line[5]);
                }
            }

            // Test a false single CR within the text
            var source3 = "\"test\",\"\n\n\",\"\r\n\r\n\r\n\",\"Normal\",\"\",\"\r\r\r\r\r\",\r\r\r\n";
            using (var cr = CSVReader.FromString(source3, settings))
            {
                foreach (var line in cr.Lines())
                {
                    Assert.AreEqual("test", line[0]);
                    Assert.AreEqual("\n\n", line[1]);
                    Assert.AreEqual("\r\n\r\n\r\n", line[2]);
                    Assert.AreEqual("Normal", line[3]);
                    Assert.AreEqual("", line[4]);
                    Assert.AreEqual("\r\r\r\r\r", line[5]);
                    Assert.AreEqual("\r\r", line[6]);
                }
            }
        }

        [Test]
        public void TestIssue62()
        {
            var inputLines = File.ReadAllLines("PackageAssets.csv");
            var desiredLines = 53_543;
            var linesToRead = Enumerable
                .Repeat(inputLines, desiredLines / inputLines.Length + 1)
                .SelectMany(x => x)
                .Take(desiredLines)
                .ToArray();

            var config = new CSVSettings
            {
                HeaderRowIncluded = false,
            };

            var outputLines = 0;
            var rawText = string.Join(Environment.NewLine, linesToRead);
            var rawBytes = Encoding.UTF8.GetBytes(rawText);
            using (var memoryStream = new MemoryStream(rawBytes))
            {
                using (var streamReader = new StreamReader(memoryStream))
                {
                    using (var csvReader = new CSVReader(streamReader, config))
                    {
                        foreach (var row in csvReader)
                        {
                            outputLines++;
                        }
                    }
                }
            }
            Assert.AreEqual(desiredLines, outputLines);
        }

        [Test]
        public void TestEmptyHeaderRow()
        {
            var settings = new CSVSettings()
            {
                LineSeparator = "\n",
                FieldDelimiter = ',',
                HeaderRowIncluded = true
            };

            using (var cr = CSVReader.FromString("", settings))
            {
                Assert.AreEqual(0, cr.Headers.Length);
                Assert.AreEqual(0, cr.Count());
            }

            using (var cr = CSVReader.FromString(" ", settings))
            {
                Assert.AreEqual(1, cr.Headers.Length);
                Assert.AreEqual(" ", cr.Headers[0]);
                Assert.AreEqual(0, cr.Count());
            }

            using (var cr = CSVReader.FromString("\t", settings))
            {
                Assert.AreEqual(1, cr.Headers.Length);
                Assert.AreEqual("\t", cr.Headers[0]);
                Assert.AreEqual(0, cr.Count());
            }

            using (var cr = CSVReader.FromString("\n", settings))
            {
                Assert.AreEqual(0, cr.Headers.Length);
                Assert.AreEqual(0, cr.Count());
            }

            using (var cr = CSVReader.FromString(" \n", settings))
            {
                Assert.AreEqual(1, cr.Headers.Length);
                Assert.AreEqual(" ", cr.Headers[0]);
            }

            using (var cr = CSVReader.FromString("\n ", settings))
            {
                Assert.AreEqual(0, cr.Headers.Length);
                var lines = cr.ToArray();
                Assert.AreEqual(1, lines.Length);
                Assert.AreEqual(1, lines[0].Length);
                Assert.AreEqual(" ", lines[0][0]);
            }
        }

        [Test]
        public void TestSepLineEmptyHeaderRow()
        {
            var settings = new CSVSettings()
            {
                LineSeparator = "\n",
                FieldDelimiter = ',',
                HeaderRowIncluded = true,
                AllowSepLine = true
            };

            // if without an = sign, "sep" is just a header
            using (var cr = CSVReader.FromString("sep", settings))
            {
                Assert.AreEqual(1, cr.Headers.Length);
                Assert.AreEqual("sep", cr.Headers[0]);
                Assert.AreEqual(0, cr.Count());
            }

            // if with an = sign but no character specified, then "sep=" is also treated as a header
            // note that this is different from Excel's behaviour
            // Excel interprets this as a field delimiter setter, but it doesn't set it to any character
            // so all lines end up getting displayed in plaintext
            using (var cr = CSVReader.FromString("sep=", settings))
            {
                Assert.AreEqual(1, cr.Headers.Length);
                Assert.AreEqual("sep=", cr.Headers[0]);
                Assert.AreEqual(0, cr.Count());
            }

            using (var cr = CSVReader.FromString("sep= ", settings))
            {
                Assert.AreEqual(1, cr.Headers.Length);
                Assert.AreEqual("sep= ", cr.Headers[0]);
                Assert.AreEqual(0, cr.Count());
            }

            using (var cr = CSVReader.FromString("sep = ", settings))
            {
                Assert.AreEqual(1, cr.Headers.Length);
                Assert.AreEqual("sep = ", cr.Headers[0]);
                Assert.AreEqual(0, cr.Count());
            }

            using (var cr = CSVReader.FromString("sep=;", settings))
            {
                Assert.AreEqual(0, cr.Headers.Length);
                Assert.AreEqual(0, cr.Count());
            }

            using (var cr = CSVReader.FromString("sep=\n\nline1,line2", settings))
            {
                Assert.AreEqual(1, cr.Headers.Length);
                Assert.AreEqual("sep=", cr.Headers[0]);
                var lines = cr.ToArray();
                Assert.AreEqual(2, lines.Length);
                Assert.AreEqual(1, lines[0].Length);
                Assert.AreEqual("", lines[0][0]);
                Assert.AreEqual(2, lines[1].Length);
                Assert.AreEqual("line1", lines[1][0]);
                Assert.AreEqual("line2", lines[1][1]);
            }

            using (var cr = CSVReader.FromString("sep=;\n\nline1;line2", settings))
            {
                Assert.AreEqual(0, cr.Headers.Length);
                var lines = cr.ToArray();
                Assert.AreEqual(1, lines.Length);
                Assert.AreEqual(2, lines[0].Length);
                Assert.AreEqual("line1", lines[0][0]);
                Assert.AreEqual("line2", lines[0][1]);
            }

            using (var cr = CSVReader.FromString("sep=\n", settings))
            {
                Assert.AreEqual(1, cr.Headers.Length);
                Assert.AreEqual("sep=", cr.Headers[0]);
                Assert.AreEqual(0, cr.Count());
            }

            using (var cr = CSVReader.FromString("sep=;\n", settings))
            {
                Assert.AreEqual(0, cr.Headers.Length);
                Assert.AreEqual(0, cr.Count());
            }
        }

#if HAS_ASYNC_IENUM
        [Test]
        public async Task TestAsyncReader()
        {
            // The "sep=" line is a feature of Microsoft Excel that makes CSV files work more smoothly with
            // European data sets where a comma is used in numerical separation.  If present, it overrides
            // the FieldDelimiter setting from the CSV Settings.
            var source = "sep=|\n" +
                         "Name|Title|Phone\n" +
                         "JD|Doctor|x234\n" +
                         "Janitor|Janitor|x235\n" +
                         "\"Dr. Reed, " + Environment.NewLine + "Eliot\"|\"Private \"\"Practice\"\"\"|x236\n" +
                         "Dr. Kelso|Chief of Medicine|x100";

            // Convert into stream
            var settings = new CSVSettings() { HeaderRowIncluded = true, FieldDelimiter = '\t', LineSeparator = "\n", AllowSepLine = true };
            using (var cr = CSVReader.FromString(source, settings))
            {
                // The field delimiter should have been changed, but the original object should remain the same
                Assert.AreEqual('\t', settings.FieldDelimiter);
                Assert.AreEqual('|', cr.Settings.FieldDelimiter);
                Assert.AreEqual("Name", cr.Headers[0]);
                Assert.AreEqual("Title", cr.Headers[1]);
                Assert.AreEqual("Phone", cr.Headers[2]);
                var i = 1;
                await foreach (var line in cr)
                {
                    switch (i)
                    {
                        case 1:
                            Assert.AreEqual("JD", line[0]);
                            Assert.AreEqual("Doctor", line[1]);
                            Assert.AreEqual("x234", line[2]);
                            break;
                        case 2:
                            Assert.AreEqual("Janitor", line[0]);
                            Assert.AreEqual("Janitor", line[1]);
                            Assert.AreEqual("x235", line[2]);
                            break;
                        case 3:
                            Assert.AreEqual("Dr. Reed, " + Environment.NewLine + "Eliot", line[0]);
                            Assert.AreEqual("Private \"Practice\"", line[1]);
                            Assert.AreEqual("x236", line[2]);
                            break;
                        case 4:
                            Assert.AreEqual("Dr. Kelso", line[0]);
                            Assert.AreEqual("Chief of Medicine", line[1]);
                            Assert.AreEqual("x100", line[2]);
                            break;
                    }

                    i++;
                }
            }
        }
#endif
    }
}

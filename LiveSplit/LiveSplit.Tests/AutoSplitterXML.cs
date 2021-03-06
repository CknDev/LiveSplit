﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using LiveSplit.Model;

namespace LiveSplit.Tests
{
    [TestClass]
    public class AutoSplitterXML
    {
        [TestMethod]
        public void TestAutoSplittersXML()
        {
            var xmlPath = Path.Combine("..", "..", "..", "..", "LiveSplit.AutoSplitters.xml");

            Assert.IsTrue(File.Exists(xmlPath), "The Auto Splitters XML is missing");

            var document = new XmlDocument();
            document.Load(xmlPath);

            IDictionary<string, AutoSplitter> autoSplitters = document["AutoSplitters"].ChildNodes.OfType<XmlElement>().Where(element => element != null).Select(element =>
                    new AutoSplitter()
                    {
                        Description = element["Description"].InnerText,
                        URLs = element["URLs"].ChildNodes.OfType<XmlElement>().Select(x => x.InnerText).ToList(),
                        Type = (AutoSplitterType)Enum.Parse(typeof(AutoSplitterType), element["Type"].InnerText),
                        Games = element["Games"].ChildNodes.OfType<XmlElement>().Select(x => (x.InnerText ?? "").ToLower()).ToList(),
                        ShowInLayoutEditor = element["ShowInLayoutEditor"] != null
                    }).SelectMany(x => x.Games.Select(y => new KeyValuePair<string, AutoSplitter>(y, x))).ToDictionary(x => x.Key, x => x.Value);

            Assert.IsTrue(!autoSplitters.Any(x => string.IsNullOrWhiteSpace(x.Key)), "Empty Game Names are not allowed");
            Assert.IsTrue(!autoSplitters.Values.Any(x => string.IsNullOrWhiteSpace(x.Description)), "Auto Splitters need a description");
            Assert.IsTrue(!autoSplitters.Values.Any(x => !x.URLs.Any()), "Auto Splitters need to have at least one URL");
            Assert.IsTrue(!autoSplitters.Values.Any(x => x.URLs.Any(y => y.EndsWith(".asl")) && x.Type == AutoSplitterType.Component), 
                "ASL Script is downloaded even though Type \"Component\" is specified.");
            Assert.IsTrue(!autoSplitters.Values.Any(x => x.URLs.Any(y => y.EndsWith(".dll")) && x.Type == AutoSplitterType.Script), 
                "Component is downloaded even though Type \"Script\" is specified.");
            Assert.IsTrue(!autoSplitters.Values.Any(x => x.URLs.Any(y => !Uri.IsWellFormedUriString(y, UriKind.Absolute))),
                "Auto Splitters need to have valid URLs");
        }
    }
}

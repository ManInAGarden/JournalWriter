using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Markdown;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class UTFlowDocGenerator
    {
        [TestMethod]
        public void TestFlowDocGen()
        {
            DocLexElement.JumpPositions = new List<int>();

            FlowDocGenerator gen = new FlowDocGenerator();
            List<DocLexElement> lexes = new List<DocLexElement>();
            lexes.Add(new DocLexElement(DocLexElement.LexTypeEnum.word, text: "Hallo", spcCount: 1));
            lexes.Add(new DocLexElement(DocLexElement.LexTypeEnum.bold));
            lexes.Add(new DocLexElement(DocLexElement.LexTypeEnum.word, text:"fett"));
            lexes.Add(new DocLexElement(DocLexElement.LexTypeEnum.bold));
            lexes.Add(new DocLexElement(DocLexElement.LexTypeEnum.parabreak));
            string answ = gen.ProduceDoc(lexes);

            Assert.IsTrue(answ.StartsWith("<FlowDocument"));
            Assert.IsTrue(answ.Contains("Hallo <Bold>fett</Bold>"));
            Assert.IsTrue(answ.EndsWith("</FlowDocument>"));
        }
    }
}

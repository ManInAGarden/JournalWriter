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
            List<DocLexElement> lexes = new List<DocLexElement>()
            {
                new DocLexElement(DocLexElement.LexTypeEnum.word, text: "Hallo", spcCount: 1),
                new DocLexElement(DocLexElement.LexTypeEnum.bold),
                new DocLexElement(DocLexElement.LexTypeEnum.word, text: "fett"),
                new DocLexElement(DocLexElement.LexTypeEnum.bold),
                new DocLexElement(DocLexElement.LexTypeEnum.parabreak)
            };
            string answ = gen.ProduceDoc(lexes);

            Assert.IsTrue(answ.StartsWith("<FlowDocument"));
            Assert.IsTrue(answ.Contains("Hallo <Bold>fett</Bold>"));
            Assert.IsTrue(answ.EndsWith("</FlowDocument>"));
        }

        [TestMethod]
        public void TestFlowDocListGen()
        {
            DocLexElement.JumpPositions = new List<int>();

            FlowDocGenerator gen = new FlowDocGenerator();
            List<DocLexElement> lexes = new List<DocLexElement>()
            {
                new DocLexElement(DocLexElement.LexTypeEnum.word, text: "Vortext", spcCount: 1),
                new DocLexElement(DocLexElement.LexTypeEnum.parabreak),
                new DocLexElement(DocLexElement.LexTypeEnum.emphasize, spcCount: 1),
                new DocLexElement(DocLexElement.LexTypeEnum.word, text: "Hallo", spcCount: 1),
                new DocLexElement(DocLexElement.LexTypeEnum.linebreak),
                new DocLexElement(DocLexElement.LexTypeEnum.emphasize, spcCount: 1),
                new DocLexElement(DocLexElement.LexTypeEnum.word, text: "fett", spcCount: 1),
                new DocLexElement(DocLexElement.LexTypeEnum.linebreak),
                new DocLexElement(DocLexElement.LexTypeEnum.parabreak)
            };

            string answ = gen.ProduceDoc(lexes);

            Assert.IsTrue(answ.StartsWith("<FlowDocument"));
            Assert.IsTrue(answ.Contains("<List FontSize=\"\" FontFamily=\"\" MarkerStyle=\"Disc\">"));
            Assert.IsTrue(answ.Contains("<ListItem><Paragraph TextAlignment=\"\">Hallo")); 
            Assert.IsTrue(answ.EndsWith("</FlowDocument>"));
        }

    }
}

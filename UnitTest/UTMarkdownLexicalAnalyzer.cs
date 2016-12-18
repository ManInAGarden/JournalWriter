using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Markdown;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class UTMarkdownLexicalAnalyzer
    {
        [TestMethod]
        public void TestGetDocLexList()
        {
            MarkDownLexicalAnalyzer analyzer = new MarkDownLexicalAnalyzer("Hallo Welt");
            List<DocLexElement> reslts = analyzer.GetDocLexList();
            Assert.AreEqual(reslts[0].Text, "Hallo");
            Assert.AreEqual(reslts[1].Text, "Welt");

            analyzer.Text = "## Hallo Welt\nIch bin dann mal weg.";
            reslts = analyzer.GetDocLexList();
            Assert.IsTrue(reslts[0].Type == DocLexElement.LexTypeEnum.hashes && reslts[0].Level==2);
            Assert.AreEqual(reslts[1].Text, "Hallo");
            Assert.AreEqual(reslts[2].Text, "Welt");

            analyzer.Text = "```\nHallo Welt\nIch bin dann mal weg.\n```";
            reslts = analyzer.GetDocLexList();
     
            Assert.IsTrue(reslts[0].Type == DocLexElement.LexTypeEnum.codeblock);
            
        }

        [TestMethod]
        public void TestHashes()
        {
            MarkDownLexicalAnalyzer an = new MarkDownLexicalAnalyzer("# Hallo Welt\n## Bin dann mal weg\nAls ich noch jung war\n");
            List<DocLexElement> ergs = an.GetDocLexList();
            string ress = GetAsString(ergs);
            Assert.AreEqual(ress,
               "hashes(1),word(Hallo),word(Welt),linebreak,hashes(2),word(Bin),word(dann),word(mal),word(weg),linebreak,word(Als),word(ich),word(noch),word(jung),word(war),parabreak");
        }

        [TestMethod]
        public void TestCodeArea()
        {
            MarkDownLexicalAnalyzer an = new MarkDownLexicalAnalyzer("```x = y*3.0 + 12\n```\nHier gehts weiter");
            List<DocLexElement> ergs = an.GetDocLexList();
            string ress = GetAsString(ergs);
            Assert.AreEqual(ress,
                "codeblock(x = y*3.0 + 12\n),linebreak,word(Hier),word(gehts),word(weiter)");
        }

        [TestMethod]
        public void TestEmphasize()
        {
            MarkDownLexicalAnalyzer an = new MarkDownLexicalAnalyzer("Hallo *kursive* Welt");
            List<DocLexElement> ergs = an.GetDocLexList();
            string ress = GetAsString(ergs);
            Assert.AreEqual(ress,
                "word(Hallo),emphasize,word(kursive),emphasize,word(Welt)");
        }


        [TestMethod]
        public void TestBold()
        {
            MarkDownLexicalAnalyzer an = new MarkDownLexicalAnalyzer("Hallo **fette** Welt");
            List<DocLexElement> ergs = an.GetDocLexList();
            string ress = GetAsString(ergs);
            Assert.AreEqual(ress,
                "word(Hallo),bold,word(fette),bold,word(Welt)");
        }

        private string GetAsString(List<DocLexElement> ergs)
        {
            string answ = null;
            foreach (DocLexElement erg in ergs)
            {
                if (answ != null)
                    answ += "," + erg.ToString();
                else
                    answ += erg.ToString();
            }

            return answ;
        }
    }
}

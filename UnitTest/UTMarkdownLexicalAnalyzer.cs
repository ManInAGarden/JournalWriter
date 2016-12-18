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
     
            Assert.IsTrue(reslts[0].Type == DocLexElement.LexTypeEnum.code);
            Assert.IsTrue(reslts[1].Type == DocLexElement.LexTypeEnum.linebreak);
            Assert.IsTrue(reslts[2].Type == DocLexElement.LexTypeEnum.word && reslts[2].Text.Equals("Hallo"));
            Assert.IsTrue(reslts[2].Type == DocLexElement.LexTypeEnum.word && reslts[2].Text.Equals("Hallo"));
            Assert.IsTrue(reslts[4].Type == DocLexElement.LexTypeEnum.word && reslts[4].Text.Equals("Welt"));
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
                "code(x = y*3.0 + 12\n)\nword(Hier),word(gehts),word(weiter");
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

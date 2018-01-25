using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicScrapper.VO {
    public class DicItem {
        public int Num { get; set; }
        public string Word { get; set; }
        public string Yomi { get; set; }
        public string Type { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public int Stars { get; set; }
        public string JLPT { get; set; }

        public override string ToString() {
            return $"Num: {Num}, Word: {Word}, Yomi: {Yomi}, Type: {Type}, Summary: {Summary}, Stars: {Stars}, JLPT: {JLPT}\r\n{Content}";
        }

        private string SQLStyle(string input) {
            if (input == null) {
                return "NULL";
            } else {
                string done = "'" + input.Replace("'", "' || CHR(39) || '").Replace("\r", "' || CHR(10) || '").Replace("\n", "' || CHR(13) || '").Replace(";", ", ") + "'";
                return done.Replace(" || CHR(13) || ''", "");
            }
        }

        public string ToSQL() {
            return $"INSERT INTO jwords (num,yomi,word,type,summary,meaning,star,jlpt) values ({Num},{SQLStyle(Yomi)},{SQLStyle(Word)},{SQLStyle(Type)},{SQLStyle(Summary)},{SQLStyle(Content)},{Stars},{SQLStyle(JLPT)});";
        }
    }
}

using DicScrapper.VO;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DicScrapper {
    public class Communicate {

        private HttpClientHandler hHandler = null;
        private CookieContainer cookies = null;
        private static Dictionary<string, string> dHeaders = new Dictionary<string, string>() {
            { "User-Agent","Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:56.0) Gecko/20100101 Firefox/56.0" },
            { "Accept","text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" },
            { "Accept-Language","ko-KR,ko;q=0.8,en-US;q=0.5,en;q=0.3" },
            { "Accept-Encoding","gzip" },
            { "Referer","http://jpdic.naver.com/search.nhn?range=all&q=%E7%A9%BA&sm=jpd_hty" },
            { "Connection","keep-alive" }
        };

        public Communicate() {
            cookies = new CookieContainer();
            hHandler = new HttpClientHandler() {
                AutomaticDecompression = DecompressionMethods.GZip,
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = cookies                
            };
        }

        public async Task<Tuple<string,string>> HttpGet(string target, Dictionary<string,string> headers = null, string prefered_method = "GET") {
            try {
                HttpClient hc = new HttpClient(hHandler, false);
                hc.Timeout = new TimeSpan(0,0,10);
                var method = new HttpMethod(prefered_method);

                var request = new HttpRequestMessage(method, target) {
                    //Content = new StringContent(content, Encoding.UTF8, "application/json")
                };
                foreach (var oh in dHeaders) {
                    request.Headers.Add(oh.Key, oh.Value);
                }
                if (headers != null) {
                    foreach (var h in headers) {
                        request.Headers.Add(h.Key, h.Value);
                    }
                }

                HttpResponseMessage hrm = await hc.SendAsync(request);
                if (hrm.IsSuccessStatusCode) {
                    return new Tuple<string, string>(hrm.StatusCode.ToString(), await hrm.Content.ReadAsStringAsync());
                } else {
                    return new Tuple<string, string>(hrm.StatusCode.ToString(), null);
                }
            } catch(Exception ex) {
                Console.WriteLine(ex);
                return new Tuple<string, string>(null,null);
            }
        }

        public DicItem ParseDic(string raw) {
            HtmlDocument doc = new HtmlDocument();

            doc.LoadHtml(raw);

            HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//a[@href]");//the parameter is use xpath see: https://www.w3schools.com/xml/xml_xpath.asp 

            DicItem di = new DicItem();
            var fromwebnode = doc.DocumentNode.SelectSingleNode("//img[contains(@src,'/btn_collect.gif')]") ?? doc.DocumentNode.SelectSingleNode("//img[contains(@src,'/bg_opendict.gif')]");
            if(fromwebnode != null) {
                //웹수집은 건너뛰자
                return null;
            }

            var yominode = doc.DocumentNode.SelectSingleNode("//span[@class=\"maintitle\"]");
            if (yominode != null) {
                di.Yomi = WebUtility.HtmlDecode(yominode.InnerText);
                if(Regex.Match(di.Yomi, @"[^ぁ-んァ-ンー*]").Success) {
                    return null;
                }
            }

            var mainnode = doc.DocumentNode.SelectSingleNode("//h3/em");
            if(mainnode != null) {
                var words = WebUtility.HtmlDecode(mainnode.InnerText).Replace("[","").Replace("]","").Split(new string[] { "·" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                int i = 0;
                while(i < words.Count) {
                    if(words[i] == di.Yomi) {
                        words.Remove(di.Yomi);
                    } else {
                        if(di.Word != null) {
                            di.Word += ", ";
                        }
                        di.Word += words[i];
                        i++;
                    }
                } 
            }


            var summarynode = doc.DocumentNode.SelectSingleNode("//div[@class=\"spot_area\"]/p");
            if (summarynode != null) {
                di.Summary = WebUtility.HtmlDecode(summarynode.InnerText);
            }

            var typenode = doc.DocumentNode.SelectSingleNode("//div[@class=\"tc-panels\"]//h5");
            if (typenode != null) {
                di.Type = WebUtility.HtmlDecode(typenode.InnerText);
            }

            var meaning = doc.DocumentNode.SelectNodes("//li[@class=\"inner_lst\"]/span");
            if (meaning != null) {
                for(int i = 0; i < meaning.Count; i++) {
                    di.Content += string.Format("{0}. {1}",i+1,WebUtility.HtmlDecode(meaning[i].InnerText))+"\n";
                }
            }

            var starnode = doc.DocumentNode.SelectSingleNode("//div[@class=\"spot_area\"]//img[@class=\"star\"]");
            if (starnode != null) {
                string cname = starnode.Attributes["src"].Value;
                Match m = Regex.Match(cname, @"(?<star>\d).gif$");
                if (m.Success) {
                    di.Stars = Convert.ToInt32(m.Groups["star"].Value);
                } else {
                    di.Stars = 1;
                }
            }

            var jlptnode = doc.DocumentNode.SelectSingleNode("//div[@class=\"spot_area\"]//a[contains(@class,'jlpt')]/span");
            if (jlptnode != null) {
                string cname = jlptnode.InnerText;
                Match m = Regex.Match(cname, @"\d");
                if(m.Success) {
                    di.JLPT = m.Value;
                }
            }

            return di;
        }

        public List<string> ParseList(string raw) {
            HtmlDocument doc = new HtmlDocument();

            doc.LoadHtml(raw);

            HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//a[contains(@href,'/entry/jk/')]");//the parameter is use xpath see: https://www.w3schools.com/xml/xml_xpath.asp 

            if(links != null) {
                List<string> ls = new List<string>();
                foreach(var link in links) {
                    ls.Add(link.Attributes["href"].Value);
                }
                return ls;
            }
            return null;
        }
    }
}

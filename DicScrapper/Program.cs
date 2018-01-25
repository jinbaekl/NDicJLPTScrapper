using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicScrapper {
    class Program {
        const string DATE = "20171115";
        //http://jpdic.naver.com/entry/jk/JK000000052248.nhn
        const string URL_FORMAT = "http://jpdic.naver.com/entry/jk/JK{0:D12}.nhn";


        static void Main(string[] args) {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args) {
            Console.WriteLine("사전 스크래퍼 v0.7");
            Console.WriteLine("by 이진백 " + DATE);

            var comm = new Communicate();
            bool isContinuable = true;
            int i = 1; long iend = 1000000;

            Console.Write("시작할 nhn 번호(~96078?): ");
            string sNum = Console.ReadLine();
            if (!string.IsNullOrEmpty(sNum)) {
                i = Convert.ToInt32(sNum);
            }

            Console.Write("끝 nhn 번호(~96078?): ");
            sNum = Console.ReadLine();
            if (!string.IsNullOrEmpty(sNum)) {
                iend = Convert.ToInt32(sNum);
                if(iend < i) {
                    iend = 1000000;
                }
            }

            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar;

            if(i < 0) {
                // 음수 입력시 특수 기능 발동
                /*
                using (StreamWriter sw = new StreamWriter(path + "additional.sql", true, Encoding.UTF8)) {
                    using (OracleCon oc = new OracleCon()) {
                        
                    }
                }


                Console.ReadLine();*/
                return;
            }

            string sqlname = path + "data.sql";

            Console.Write("희망 SQL 이름: ");
            string sSQL = Console.ReadLine();
            if(!string.IsNullOrEmpty(sSQL)) {
                sqlname = path + sSQL;
            }
            

            if (File.Exists(sqlname)) {
                Console.Write("파일을 덮어쓰시겠습니까? (y/n) ");
                if(Console.Read() == (int)'y') {
                    File.Delete(sqlname);
                }
            }


            using (StreamWriter sw = new StreamWriter(sqlname,true,new UTF8Encoding(false))) {


                sw.WriteLine("SET SCAN OFF;");
                sw.WriteLine("SET DEFINE OFF;");
                sw.WriteLine("DROP TABLE jvoca;");
                sw.WriteLine("DROP TABLE jusers;");
                sw.WriteLine();
                sw.WriteLine("DROP TABLE jwords;");
                sw.WriteLine("CREATE TABLE jwords (num number primary key,yomi varchar2(100),word varchar2(100),type varchar2(50),summary varchar(250),meaning varchar2(2000),star number,jlpt varchar2(30));");
                sw.WriteLine("TRUNCATE TABLE jwords;");

                int failcount = 0;
                long count = 1;
                long wcount = 0;
                while (isContinuable) {
                    if(i > iend) {
                        break;
                    }

                    var requestURL = string.Format(URL_FORMAT, i);
                    var result = (await comm.HttpGet(requestURL));

                    if (result.Item1 == null || result.Item1 != "OK") {
                        /*failcount++;
                        Console.WriteLine($"서버에서 {result.Item1 ?? "치명적인 오류"} 결과 반환: {requestURL}");
                        if (failcount > 3) {
                            Console.Write("연속 3번 이상 오류. 계속하시겠습니까? (y/n) ");
                            isContinuable = Console.Read() == (int)'y';
                        }*/
                        //FailCount 무시
                        Console.WriteLine("{1}: {0}" ,requestURL, result.Item1);
                        if(i > 1000000) {
                            break;
                        }
                    } else {
                        failcount = 0;
                        var di = comm.ParseDic(result.Item2);
                        if(di != null) { //di가 null인 경우는 한자 같은 부적합한 단어인 경우
                            di.Num = i;
                            //Console.WriteLine(di);
                            Console.WriteLine(di.ToSQL());
                            sw.WriteLine(di.ToSQL());
                            wcount++;
                        }

                    }
                    i++;

                    /*if (i >= 5) {
                        break;
                    }*/
                    if(Console.KeyAvailable) {
                        if(Console.ReadKey().Key == ConsoleKey.Escape) {
                            isContinuable = false;
                            Console.WriteLine("Esc가 눌려서 중단!");
                        }
                    }
                    count++;
                    Console.Title = $"DicScrapper: {count}번 루프 ({i} 번호) {wcount}행 기록됨";
                }
                sw.WriteLine("COMMIT;");
                sw.WriteLine("CREATE TABLE jusers (username varchar2(150) primary key,registered date default SYSDATE,lastlogin date);");
                sw.WriteLine("CREATE TABLE jvoca(vnum number not null,inserted date default SYSDATE not null,remembered date,owner varchar2(100),constraint jvoca_jwords_fk foreign key(vnum) references jwords(num) on delete cascade,constraint jvoca_jusers_fk foreign key(owner) references jusers(username) on delete cascade);");
                sw.WriteLine("COMMIT;");
                sw.Flush();
            }

            Console.WriteLine("모든 작업이 끝났습니다.");

            Console.ReadLine();
        }
    }
}

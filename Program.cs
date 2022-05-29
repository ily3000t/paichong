using System.Net;
using HtmlAgilityPack;
using JiebaNet.Segmenter;
using System.Drawing;
using WordCloudSharp;
using JiebaNet.Segmenter.Common;
using JiebaNet.Analyser;
using System.Text;

string[] text = new string[400];
int cont = 0, line = 5;
string s = "";
string add;

//ExtractTagsDemo 方法为提取所有关键词。
void ExtractTagsDemo()
{
    var text = s;
    var extractor = new TfidfExtractor();
    var keywords = extractor.ExtractTags(text);
    foreach (var keyword in keywords)
    {
        Console.WriteLine(keyword);
    }
}

//ExtractTagsDemo2 方法为提取前十个仅包含名词和动词的关键词
void ExtractTagsDemo2()
{
    var text = s; 
    var extractor = new TfidfExtractor();
    var keywords = extractor.ExtractTags(text, 10, Constants.NounAndVerbPos);
    foreach (var keyword in keywords)
    {
        Console.WriteLine(keyword);
    }
}

//PosCutDemo 方法为词性标注。
void PosCutDemo(string s)
{
    var posSeg = new JiebaNet.Segmenter.PosSeg.PosSegmenter();  var tokens = posSeg.Cut(s);
    Console.WriteLine(string.Join(" ", tokens.Select(token => string.Format("{0}/{1}", token.Word, token.Flag))));
}

//调用 TokenizeDemo 方法会返回对应位置
 void TokenizeDemo(string s)
{
    var segmenter = new JiebaSegmenter(); var tokens = segmenter.Tokenize(s); foreach (var token in tokens)
    {
        Console.WriteLine("word {0,-12} start: {1,-3} end: {2,-3}", token.Word, token.StartIndex, token.EndIndex);
    }
}


// ExtractTagsWithWeight方法的返回结果中除了包含关键词，还包含了相应的权重值。
// 分词并统计词频：默认为精确模式，同时也使用HMM模型
static KeyValuePair<string, int>[] Counter(string text, WordWeightPair[] wordWeightAry)
{
    var segmenter = new JiebaSegmenter();
    var segments = segmenter.Cut(text);
    var freqs = new Counter<string>(segments);
    KeyValuePair<string, int>[] countAry = new KeyValuePair<string, int>[wordWeightAry.Length];
    for (int i = 0; i < wordWeightAry.Length; i++)
    {
        string key = wordWeightAry[i].Word;
        countAry[i] = new KeyValuePair<string, int>(key, freqs[key]);
    }
    StringBuilder sbr = new StringBuilder();
    sbr.Append("词语");
    sbr.Append(",");
    sbr.Append("词频");
    sbr.AppendLine(",");
    foreach (var pair in countAry)
    {
        sbr.Append(pair.Key);
        sbr.Append(",");
        sbr.Append(pair.Value);
        sbr.AppendLine(",");
    }
    string filename = "词频统计结果.csv";
    File.WriteAllText(filename, sbr.ToString(), Encoding.UTF8);
    Console.WriteLine("词频统计完成：" + filename);
    return countAry;
}

// 从指定文本中抽取关键词的同时得到其权重
static WordWeightPair[] ExtractTagsWithWeight(string text)
{
    var extractor = new TfidfExtractor();
    var wordWeight = extractor.ExtractTagsWithWeight(text, 50);
    System.Text.StringBuilder sbr = new StringBuilder();
    sbr.Append("词语");
    sbr.Append(",");
    sbr.Append("权重");
    sbr.AppendLine(",");
    foreach (var item in wordWeight)
    {
        sbr.Append(item.Word);
        sbr.Append(",");
        sbr.Append(item.Weight);
        sbr.AppendLine(",");
    }
    string filename = "关键词权重统计.csv";
    File.WriteAllText(filename, sbr.ToString(), Encoding.UTF8);
    Console.WriteLine("关键词提取完成：" + filename);
    return wordWeight.ToArray();
}

// 创建词云图
static void CreateWordCloud(KeyValuePair<string, int>[] countAry)
{
    //string markPath = "mask.jpg";
    string resultPath = "result.jpg";
    //Console.WriteLine("开始生成图片，读取蒙版：" + markPath);
    //Image mask = Image.FromFile(markPath);
    var wordCloud = new WordCloud(1000, 1000, false, null, -1, 1, null, false);
    var result = wordCloud.Draw(countAry.Select(it => it.Key).ToList(), countAry.Select(it => it.Value).ToList());
    result.Save(resultPath);
    Console.WriteLine("图片生成完成，保存图片：" + resultPath);
}

//发起请求
HttpWebRequest HttpWReq = (HttpWebRequest)WebRequest.Create("http://news.baidu.com/");
//获得响应
HttpWebResponse HttpWResp = (HttpWebResponse)HttpWReq.GetResponse();
//将响应用流保存，#httpWebResponse只能返回流
Stream dataStream = HttpWResp.GetResponseStream();
//将流文件进行编码
StreamReader streamReader = new StreamReader(dataStream, System.Text.Encoding.UTF8);
//Console.WriteLine(streamReader.ReadToEnd());
//streamReader.Close();

//用HAP解析html
HtmlDocument htmldoc = new HtmlDocument();
htmldoc.LoadHtml(streamReader.ReadToEnd());
//抓取百度新闻
HtmlNodeCollection newsListHot = htmldoc.DocumentNode.SelectNodes("//div[@id = 'pane-news']/div/ul/li/strong/a");
Console.WriteLine(" ");
Console.WriteLine("百度热点");
if (newsListHot != null)
{
    foreach (HtmlNode news in newsListHot)
    {
        Console.WriteLine("  ");
        text[cont] = news.InnerText;
        cont++;
        Console.WriteLine(news.InnerText);  //获取新闻标题
      //HtmlAttribute htmlAttribute1 = news.Attributes["href"];  //获取新闻链接
      //Console.WriteLine(htmlAttribute1.Value);
    }
}
else
{
    Console.WriteLine("出错了，建议检查Xpath是否出错");
}


while (line > 0)
{
    Console.WriteLine();
    line--;
}
line = 5;


//抓取百度要闻
HtmlNodeCollection newsImportant = htmldoc.DocumentNode.SelectNodes("//div[@id = 'pane-news']/ul/li/a");
Console.WriteLine("\n\n");
Console.WriteLine("百度要闻");
if (newsImportant != null)
{
    foreach (HtmlNode news in newsImportant)
    {
        Console.WriteLine("  ");
        Console.WriteLine(news.InnerText); //获取新闻标题
        text[cont] = news.InnerText;
        cont++;
        // HtmlAttribute htmlAttribute1 = news.Attributes["href"]; //获取新闻链接
        //Console.WriteLine(htmlAttribute1.Value);
    }
}
else
{
    Console.WriteLine("出错了，建议检查Xpath是否出错");
}

Console.WriteLine("{0}",text[0]);


while (line > 0)
{
    Console.WriteLine();
    line--;
}
line = 5;


//用s读取整个text文本
for (var n=0;  n<=cont; n++ )
{ 
    s=s+text[n];
}
Console.WriteLine("文本");
Console.WriteLine("{0}", s);

//JiebaSegmenter.AddWord(word, freq = 0, tag = null)
//JiebaSegmenter.DeleteWord(word)
//JiebaSegmenter.LoadUserDict("C:\Users\ASUS\Desktop\c#\paichong\word.txt");

while (line > 0)
{
    Console.WriteLine();
    line--;
}
line = 5;
//显示词性
PosCutDemo(s);

while (line > 0)
{
    Console.WriteLine();
    line--;
}
line = 5;



//显示分词
var segmenter = new JiebaSegmenter();
var segments = segmenter.Cut(s, cutAll: true);
Console.WriteLine("[分词]：{0}", string.Join("/ ", segments));



while (line>0) 
{
    Console.WriteLine();
    line--;
}
line = 5;


//高频词
Console.WriteLine("今日高频词");
var seg = new JiebaSegmenter();
var freqs = new Counter<string>(seg.Cut(s, cutAll: true));
foreach (var pair in freqs.MostCommon(36))
{
    if ((pair.Key.Length>=2))
    {
        Console.WriteLine($"{pair.Key} : {pair.Value}");
    }
}

while (line > 0)
{
    Console.WriteLine();
    line--;
}
line = 5;

//提取所有关键词
Console.WriteLine("提取所有关键词");
ExtractTagsDemo();


while (line > 0)
{
    Console.WriteLine();
    line--;
}
line = 5;

//为提取前十个仅包含名词和动词的关键词
Console.WriteLine("为提取前十个仅包含名词和动词的关键词");
ExtractTagsDemo2();



//调用 segmenter.AddWord添加新词
//如果想要添加词。在add中添加
add = "";
segmenter.AddWord(add);
//使用segmenter.LoadUserDict() 方法，传入词典路径
segmenter.LoadUserDict("C:\\Users\\ASUS\\Desktop\\c#\\paichong - 副本 (2)\\word.txt");//添加词典，加入需要词

//词典格式如下：词典格式与主词典格式相同，即一行包含：词、词频（可省略）、词性（可省略），用空格隔开。
//词频省略时，分词器将使用自动计算出的词频保证该词被分出。
/*****************词典格式*****************/
/*
台州 3 nz
云计算 5 nz
框架
机器模拟 3ai学习 8linezero 2
*/

//绘制词云图，图片存储在C:\Users\ASUS\Desktop\c#\paichong - 副本 (2)\bin\Debug\net6.0中查看；
var wordWeight = ExtractTagsWithWeight(s);
var wordFreqs = Counter(s, wordWeight);
CreateWordCloud(wordFreqs);
Console.Read();













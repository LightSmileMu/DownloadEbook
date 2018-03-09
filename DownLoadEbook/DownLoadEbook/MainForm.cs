using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace DownLoadEbook
{
    public partial class MainForm : Form
    {
        private static readonly object Locker = new object();
        int pageTotalCount = 763;
        int pageStep = 5;

        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Maximum = 763;
            bg_worker.RunWorkerAsync();
        }

        private void SetDataFromPage(object pageInfo)
        {
            List<BookInfo> books = new List<BookInfo>();

            PageInfo page = (PageInfo) pageInfo;
            if (page.StartPage == 0 || page.EndPage == 0)
            {
                return;
            }
            int startPage = page.StartPage;
            int endPage = page.EndPage;

            for (int pageIndex = startPage; pageIndex < endPage; pageIndex++)
            {
                books.AddRange(GetDataFromPage(pageIndex));
            }

            lock (Locker)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var bookInfo in books)
                {
                    sb.AppendLine(string.Format("{0}\t{1}\t{2}", bookInfo.Id, bookInfo.Name, bookInfo.ImageUrl));
                }
                File.AppendAllText(@"D:\books.txt", sb.ToString());
            }
        }

        private IList<BookInfo> GetDataFromPage(int pageIndex)
        {
            Console.WriteLine("pageindex" + pageIndex);

            List<BookInfo> books = new List<BookInfo>();

            //以Get方式调用  
            HttpWebRequest request = WebRequest.Create(string.Format("http://www.ebook-dl.com/cat/1/pg/{0}", pageIndex)) as HttpWebRequest;
            try
            {
                if (request != null)
                {
                    request.Timeout = 10 * 1000;
                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response != null)
                        {
                            Stream steam = response.GetResponseStream();                  

                            if (steam != null)
                            {
                                StreamReader reader = new StreamReader(steam);

                                string content = reader.ReadToEnd();
                                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                                doc.LoadHtml(content);
                                books.AddRange(GetDataFromHtml(doc));
                            }
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return books;

        }

        private IList<BookInfo> GetDataFromHtml(HtmlAgilityPack.HtmlDocument doc)
        {
            IList<BookInfo> books = new List<BookInfo>();

            try
            {
                if(doc == null)
                {
                    return books;
                }
                var productsNode = doc.DocumentNode.SelectSingleNode("html/body/div[@id='wrapper']/div[@class='container']/div[@class='twelve columns products']");
                if (productsNode == null)
                {
                    return books;
                }

                
                foreach (var node in productsNode.SelectNodes("div[@class='four shop columns']"))
                {
                    HtmlNode bookImgNode = node.SelectSingleNode("figure/div[@class='mediaholder']/a/img");
                    if (bookImgNode == null)
                    {
                        continue;
                    }
                    var bookName = bookImgNode.Attributes["alt"].Value;
                    var imgUrl = bookImgNode.Attributes["src"].Value;
                    if (string.IsNullOrEmpty(bookName))
                    {
                        continue;
                    }
                    var bookId = string.Empty;
                    HtmlNode bookIdNode =node.SelectSingleNode("figure/div[@class='mediaholder']/a[@class='product-button']");
                    if (bookIdNode != null && bookIdNode.Attributes.Contains("href"))
                    {
                        var href = bookIdNode.Attributes["href"].Value;
                        bookId = href.Split(new char[] { '/' },StringSplitOptions.RemoveEmptyEntries)[1];
                    }

                    books.Add(new BookInfo { Name = bookName, ImageUrl = imgUrl,Id = bookId});
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return books;
        }

        private void DownLoadFile(string url,string fileName)
        {
            try
            {
                WebClient myWebClient = new System.Net.WebClient();
                
                myWebClient.DownloadFile(url, fileName);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void DownLoadFileByStream(string url, string fileName)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            try
            {
                if (request != null)
                {
                    request.Timeout = 10 * 1000;
                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response != null)
                        {
                            Stream steam = response.GetResponseStream();

                            byte[] buffer = new byte[1024];
                            FileStream fstream = File.Create(fileName);
                            int len = 0;
                            do
                            {
                                len = steam.Read(buffer, 0, buffer.Length);
                                if (len > 0)
                                {
                                    fstream.Write(buffer, 0, len);
                                }
                            } while (len > 0);
                            fstream.Close();
                            steam.Close();
                        }
                    }
                }                
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var lines = File.ReadAllLines("booksName.txt");

            progressBar1.Maximum = lines.Length;
            progressBar1.Minimum = 0;
            int bookIndex = 1;
            foreach (var line in lines)
            {
                var infos = line.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                string url = infos[2].Replace(".jpg", "_large.jpg");
                DownLoadFile(url, string.Format(@"d:\bookImage\{0}_{1}{2}", infos[0], bookIndex, ".jpg"));
                progressBar1.Value += 1;
                bookIndex++;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void bg_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if (worker == null)
            {
                return;
            }

            for (int pageIndex = 1; pageIndex <= pageTotalCount; pageIndex += pageStep)
            {
                int endPage;
                if (pageIndex + pageStep > pageTotalCount)
                {
                    endPage = pageTotalCount;
                }
                else
                {
                    endPage = pageIndex + pageStep;
                }
                PageInfo page = new PageInfo() { StartPage = pageIndex, EndPage = endPage, Books = new List<BookInfo>() };
                SetDataFromPage(page);
                worker.ReportProgress(endPage  * 100/ pageTotalCount);
                Thread.Sleep(1000);
            }
        }

        private void bg_worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage * pageTotalCount /100;
        }

        private void bg_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show(@"操作完成！");
        }

        private void bg_downBookImg_DoWork(object sender, DoWorkEventArgs e)
        {
           
        }

        private void bg_downBookImg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void bg_downBookImg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
    }

    public struct BookInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }

    public struct PageInfo
    {
        public int StartPage { get; set; }
        public int EndPage { get; set; }
        public List<BookInfo> Books{ get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace DownLoadEbook
{
    public partial class MainForm : Form
    {
        
        private int _pageTotalCount;
        private string url = "http://www.ebook-dl.com/cat/1";
        private readonly List<BookInfo> _books = new List<BookInfo>();
        private readonly ImageList _imageList = new ImageList();
        

        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bg_worker.RunWorkerAsync();
        }

        private HtmlAgilityPack.HtmlDocument GetHtml(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            HtmlAgilityPack.HtmlDocument doc = null;

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

                            if (steam != null)
                            {
                                StreamReader reader = new StreamReader(steam);

                                string content = reader.ReadToEnd();
                                doc = new HtmlAgilityPack.HtmlDocument();
                                doc.LoadHtml(content);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return doc;
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
                var productsNode = doc.DocumentNode.SelectSingleNode("//div[@class='twelve columns products']");
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
                    var urlContent = imgUrl.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
                    string fileName = urlContent[urlContent.Length -1];
                    if (!File.Exists(fileName))
                    {
                        DownLoadFile(imgUrl, fileName);
                        
                    }
                    if (File.Exists(fileName))
                    {
                        if (!_imageList.Images.ContainsKey(fileName))
                        {
                            _imageList.Images.Add(fileName, Image.FromFile(fileName));
                        }
                    }
                    
                    if (string.IsNullOrEmpty(bookName))
                    {
                        continue;
                    }
                    var bookId = string.Empty;
                    HtmlNode bookIdNode =node.SelectSingleNode("figure/div[@class='mediaholder']/a[@class='product-button']");
                    if (bookIdNode != null && bookIdNode.Attributes.Contains("href"))
                    {
                        var href = bookIdNode.Attributes["href"].Value;
                        bookId = href.Split(new [] { '/' },StringSplitOptions.RemoveEmptyEntries)[1];
                    }

                    books.Add(new BookInfo { Name = bookName, ImageUrl = fileName, Id = bookId });
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
                WebClient myWebClient = new WebClient();
                
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
                            int len=0;
                            do
                            {
                                if (steam != null)
                                {
                                    len = steam.Read(buffer, 0, buffer.Length);
                                }
                                if (len > 0)
                                {
                                    fstream.Write(buffer, 0, len);
                                }
                            } while (len > 0);
                            fstream.Close();
                            if (steam != null)
                            {
                                steam.Close();
                            }
                        }
                    }
                }                
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void btnExportData_Click(object sender, EventArgs e)
        {
            if (_books.Count>0)
            {
                try
                {
                    using (SaveFileDialog dlg = new SaveFileDialog())
                    {
                        dlg.Filter = @"Excel文件|*.xls";
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (var item in _books)
                            {
                                sb.AppendLine(string.Format("{0}\twww.ebook-dl.com/book/{1}", item.Name, item.Id));
                            }

                            File.AppendAllText(dlg.FileName, sb.ToString());
                        }
                    }
                    MessageBox.Show(@"导出完成！");
                }
                catch (Exception exception)
                {
                    MessageBox.Show(@"导出出错：" + exception.Message);
                }
            }
            else
            {
                MessageBox.Show(@"当前无数据！");
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
            _books.Clear();
            var doc= GetHtml(url);

            List<string> urls = new List<string>();
            int maxPage;
            if (doc != null && doc.DocumentNode != null)
            {
               var paginationNode = doc.DocumentNode.SelectSingleNode("//nav[@class='pagination']");
               if (paginationNode != null)
               {
                  var pageNodes= paginationNode.SelectNodes("ul/li/a");
                  if (pageNodes.Count > 0)
                  {
                      var maxPageNode = pageNodes[pageNodes.Count - 1];
                      
                      if (int.TryParse(maxPageNode.InnerText, out maxPage))
                      {
                          _pageTotalCount = maxPage;
                          SetPicbarValueSafe(maxPage);
                          for (int pageIndex = 2; pageIndex <= maxPage; pageIndex++)
                          {
                              urls.Add(string.Format("{0}/pg/{1}", url,pageIndex));
                          }
                      }
                  }
               }
            }

            int process = 1;
            _books.AddRange(GetDataFromHtml(doc));
            worker.ReportProgress(process);
            
            foreach (var pageUrl in urls)
            {
                var  docTmp =GetHtml(pageUrl);
                _books.AddRange(GetDataFromHtml(docTmp));
                process++;
                worker.ReportProgress(process);
            }

            process = _pageTotalCount;
            worker.ReportProgress(process);
        }

        private void SetPicbarValueSafe(int value)
        {
            if (progressBar1.InvokeRequired)
            {
                Action<int> setAction = SetPicbarValueSafe;
                progressBar1.Invoke(setAction, value);
            }
            else
            {
                progressBar1.Maximum = value;
            }
        }

        private void bg_worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = Math.Min(e.ProgressPercentage, progressBar1.Maximum);
        }

        private void bg_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _imageList.ImageSize = new Size(240,256);
            listView1.LargeImageList = _imageList;
            foreach (var book in _books)
            {
                listView1.Items.Add(book.Id, book.Name, book.ImageUrl);
            }
            MessageBox.Show(@"操作完成！");
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

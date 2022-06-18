using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Net.Http.Headers;
using System.Web;
using System.Windows;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using static ChromeDriverInstaller;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms.Automation;
using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using System.Configuration;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.IO.Packaging;
using System.Diagnostics;
using Microsoft.Win32;
using HtmlAgilityPack.CssSelectors.NetCore;





namespace project
{
    public delegate void delWorkReport();
    public partial class Form1 : Form
    {
        delWorkReport _delReportDone;
        public Form1()
        {
            
            InitializeComponent();
            _delReportDone = new delWorkReport(ReportDone);

            readftp();
            //GetVer();
            Chromver();
            MaximizeBox = false;
            comboBox1.Items.Insert(0, "Furnace");
            comboBox1.Items.Insert(1, "Mill Line");
            comboBox1.Items.Insert(2, "ACC");
            comboBox1.Items.Insert(3, "Shear Line");
            label1.Visible = false;
            progressBar1.Visible = false;
            progressBar2.Visible = false;


            //string sAttr = ConfigurationManager.AppSettings.Get("Key0");
            

        }
        private void ReportDone()
        {
            MessageBox.Show("Done!", "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static async Task Mainan()
        {
            Console.WriteLine("Installing ChromeDriver");

            var chromeDriverInstaller = new ChromeDriverInstaller();
            // not necessary, but added for logging purposes
            var chromeVersion = await chromeDriverInstaller.GetChromeVersion();
            Console.WriteLine($"Chrome version {chromeVersion} detected");

            await chromeDriverInstaller.Install(chromeVersion);
            Console.WriteLine("ChromeDriver installed");

            Console.WriteLine("Enter URL to visit:");
            var url = Console.ReadLine();
            if (string.IsNullOrEmpty(url))
            {
                Console.WriteLine("No URL entered");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                return;
            }

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            using (var chromeDriver = new ChromeDriver(chromeOptions))
            {
                chromeDriver.Navigate().GoToUrl(url);
                Console.WriteLine($"Page title: {chromeDriver.Title}");
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
        private void readftp()
        {
            listBox1.Items.Clear();
            string ftpurl = ConfigurationManager.AppSettings.Get("ftpurl");
            string ftuser = ConfigurationManager.AppSettings.Get("ftpuser");
            string ftpass = ConfigurationManager.AppSettings.Get("ftppass");
           
            FtpWebRequest Request = (FtpWebRequest)WebRequest.Create(ftpurl);
            Request.Method = WebRequestMethods.Ftp.ListDirectory;
            Request.Credentials = new NetworkCredential(ftuser, ftpass);
            FtpWebResponse Response = (FtpWebResponse)Request.GetResponse();
            
            Stream ResponseStream = Response.GetResponseStream();
            StreamReader Reader = new StreamReader(ResponseStream);

            while (!Reader.EndOfStream)//Read file name   
            {
                listBox1.Items.Add(Reader.ReadLine().ToString());
            }
            Response.Close();
            ResponseStream.Close();
            Reader.Close();
        }

        //upload to Cloud
        private void upcloud()
        {
           
            String SelectedText = this.comboBox1.SelectedItem.ToString();
            
            label1.Text = "Login To CloudStorage";
            string cluser = ConfigurationManager.AppSettings.Get("clouduser");
            string clpass = ConfigurationManager.AppSettings.Get("cloudpass");

            string urlpm = ConfigurationManager.AppSettings.Get("urlpm");
            string urlfce = ConfigurationManager.AppSettings.Get("urlfce");
            string urlmill = ConfigurationManager.AppSettings.Get("urlmill");
            string urlacc = ConfigurationManager.AppSettings.Get("urlacc");
            string urlshear = ConfigurationManager.AppSettings.Get("urlshear");
            //progressBar1.Invoke(
            //               (MethodInvoker)(() => progressBar1.Maximum = 100));
            progressBar2.Invoke((MethodInvoker)(() => progressBar2.Maximum = 100));
            progressBar2.Invoke((MethodInvoker)(() => progressBar2.Value = 0));
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            var options = new ChromeOptions();
            options.AddArgument("--window-position=-32000,-32000");
            options.AddArgument("headless");

            //var driver = new ChromeDriver(service, options);
            var driver = new ChromeDriver(service, options);

            
            driver.Navigate().GoToUrl("yoururlcloud/login");

            IWebElement userx = driver.FindElement(By.Id("user"));
            IWebElement passwordx = driver.FindElement(By.Id("password"));
            IWebElement loginx = driver.FindElement(By.Id("submit"));
           
            userx.SendKeys(cluser);
            passwordx.SendKeys(clpass);


            loginx.Click();
            driver.Navigate().GoToUrl(urlpm);
      
            if (SelectedText == "Furnace")
            {
                driver.Navigate().GoToUrl(urlfce);
            }
            else if (SelectedText == "Mill Line")
            {
                driver.Navigate().GoToUrl(urlmill);
            }
            else if (SelectedText == "ACC")
            {
                driver.Navigate().GoToUrl(urlacc);
            }
            else if (SelectedText == "Shear Line")
            {
                driver.Navigate().GoToUrl(urlshear);
            }


            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            IWebElement tx = driver.FindElement(By.ClassName("icon-add"));
            tx.Click();
            IWebElement droparea = driver.FindElement(By.Id("file_upload_start"));
            
            string selected_file = listBox1.GetItemText(listBox1.SelectedItem);

            droparea.SendKeys("C:\\Local\\" + selected_file);
            progressBar2.Visible = true;
            int value = 0;
            bool compleateFlag = false;
            while (true) // Handle timeout somewhere
            {
                var ajaxIsComplete = (bool)(driver as IJavaScriptExecutor).ExecuteScript("return jQuery.active == 0");
                IWebElement element = driver.FindElement(By.Id("uploadprogressbar"));
                String elementval = element.GetAttribute("aria-valuenow");
                String speedVal = element.GetAttribute("original-title");
                double testX = Convert.ToDouble(elementval);
                int f = (int)Math.Round(testX);
                if (f < 0) f = 0;
                if (f > 0) compleateFlag = true;
                progressBar2.Invoke((MethodInvoker)(() => progressBar2.Value = f));

                progressBar2.CreateGraphics().DrawString(f + "  %", SystemFonts.SmallCaptionFont,
                                   Brushes.Black,
                                   new PointF(progressBar2.Width / 2 - (progressBar2.CreateGraphics().MeasureString(f + "  %",
                                   SystemFonts.DefaultFont).Width / 2.0F),
                                   progressBar2.Height / 2 - (progressBar2.CreateGraphics().MeasureString(f + "  %",
                                   SystemFonts.DefaultFont).Height / 2.0F)));


                //label1.Text = speedVal;
                if (ajaxIsComplete)
                    break;
                Thread.Sleep(100);
            }
            if (compleateFlag == true) {
                label1.Text = "Uploading Complete";
                Invoke(_delReportDone);
            } 
            else label1.Text = "Upload Failed, File Already Exist";

            progressBar2.Visible = false;
            driver.Quit();
            
            // Delete File From Local
            File.Delete("C:\\Local\\" + selected_file);
        }
        private void button1_Click(object sender, EventArgs e)
        {

            //read ftp
            readftp();
        }



        private void button2_Click(object sender, EventArgs e)
        {
            //String SelectedTexdt = this.comboBox1.SelectedItem.ToString();
            
            string selected_file = listBox1.GetItemText(listBox1.SelectedItem);
            if (selected_file.Contains("#"))
            {
                selected_file = selected_file.Replace("#", "%23");
            }

            if (selected_file == "" || comboBox1.SelectedItem ==  null)
            {
                String fs = "";
                String dd = "";
                String ee = "";
                if (selected_file == "")
                {
                    fs = " File ";
                }
                if (comboBox1.SelectedItem == null)
                {
                   dd = " Destination ";
                }
                if (selected_file == "" && comboBox1.SelectedItem == null)
                {
                    ee = " & ";
                }
                string message = "Please Select" + fs + ee + dd ;
                string title = "Alert Message";
                MessageBox.Show(message, title);


            }
            else
            {
                string chromever = label4.Text.Substring(label4.Text.Length - 3);
                string driverver = label5.Text.Substring(label5.Text.Length - 3);
                if (chromever != driverver)
                {
                    GetVer(chromever);
                }
                button2.Enabled = false;
                Thread.Sleep(50); //Just for test

                //Report back
                
                DownloadFtpDirectory();
                upcloud();
                //Invoke(_delReportDone);
                button2.Enabled = true;
            }

           
        }

        
        public static void ListDiractory()
        {

            //list dict
            const string baseurl = "yourcloudurl";
            CookieContainer cookie;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(baseurl);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            string login = string.Format("go=&Fuser={0}&Fpass={1}", "youruser", "yourpass");
            byte[] postbuf = Encoding.ASCII.GetBytes(login);
            req.ContentLength = postbuf.Length;
            Stream rs = req.GetRequestStream();
            rs.Write(postbuf, 0, postbuf.Length);
            rs.Close();

            cookie = req.CookieContainer = new CookieContainer();

            WebResponse resp = req.GetResponse();
            string t = new StreamReader(resp.GetResponseStream(), Encoding.Default).ReadToEnd();



            //resp.Close();
            

            ///adadadadadadawd
            string url = "http://cloudstorage.krakatauposco.co.id/apps/files/";
            List<string> files = new List<string>(500);
            HttpWebRequest requestd = (HttpWebRequest)WebRequest.Create(url);
            using (HttpWebResponse response = (HttpWebResponse)requestd.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string html = reader.ReadToEnd();

                    Regex regex = new Regex("<a href=\".*\">(?<name>.*)</a>");
                    MatchCollection matches = regex.Matches(html);

                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            if (match.Success)
                            {
                                string[] matchData = match.Groups[0].ToString().Split('\"');
                                files.Add(matchData[1]);
                            }
                        }
                    }
                }
            }
            //end

        }

        //client
        public class CookieAwareWebClient : WebClient
        {
            public string Method;
            public CookieContainer CookieContainer { get; set; }
            public Uri Uri { get; set; }

            public CookieAwareWebClient()
                : this(new CookieContainer())
            {
            }

            public CookieAwareWebClient(CookieContainer cookies)
            {
                this.CookieContainer = cookies;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest request = base.GetWebRequest(address);
                if (request is HttpWebRequest)
                {
                    (request as HttpWebRequest).CookieContainer = this.CookieContainer;
                    (request as HttpWebRequest).ServicePoint.Expect100Continue = false;
                    (request as HttpWebRequest).UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.71 Safari/537.36";
                    (request as HttpWebRequest).Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                    (request as HttpWebRequest).Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9");
                    (request as HttpWebRequest).Referer = "http://cloudstorage.krakatauposco.co.id";
                    (request as HttpWebRequest).KeepAlive = true;
                    (request as HttpWebRequest).AutomaticDecompression = DecompressionMethods.Deflate |
                                                                         DecompressionMethods.GZip;
                    if (Method == "POST")
                    {
                        (request as HttpWebRequest).ContentType = "application/x-www-form-urlencoded";
                    }

                }
                HttpWebRequest httpRequest = (HttpWebRequest)request;
                httpRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                return httpRequest;
            }

            protected override WebResponse GetWebResponse(WebRequest request)
            {
                WebResponse response = base.GetWebResponse(request);
                String setCookieHeader = response.Headers[HttpResponseHeader.SetCookie];

                if (setCookieHeader != null)
                {
                    //do something if needed to parse out the cookie.
                    try
                    {
                        if (setCookieHeader != null)
                        {
                            System.Net.Cookie cookie = new System.Net.Cookie(); //create cookie
                            this.CookieContainer.Add(cookie);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                return response;

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            label1.Visible = true;
            
            String SelectedTexdt = this.comboBox1.SelectedItem.ToString();
            if (SelectedTexdt == "")
            {
                string message = "Please Select Destination";
                string title = "Title";
                MessageBox.Show(message, title);
            }
            else
            {
                label1.Text = "Please Wait Login to Cloud";
                upcloud();
            }
            
        }
        
        //download from FTPfi
        void DownloadFtpDirectory()
        {

            label1.Visible = true;
            label3.Text = "Start Download From FTP";
            string ftuser = ConfigurationManager.AppSettings.Get("ftpuser");
            string ftpass = ConfigurationManager.AppSettings.Get("ftppass");
            string ftpurl = ConfigurationManager.AppSettings.Get("ftpurl");
            string selected_file = listBox1.GetItemText(listBox1.SelectedItem);
            string local = @"C:\Local";
           // String url = "ftp://172.21.73.118:10022/";
            NetworkCredential credentials = new NetworkCredential(ftuser,ftpass);
            if (!Directory.Exists(local))
            {
                Directory.CreateDirectory(local);
            }

            FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create(ftpurl);
            listRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            listRequest.Credentials = credentials;
            
            //listRequest.Method = WebRequestMethods.Ftp.GetFileSize;
            //int size = (int)listRequest.GetResponse().ContentLength;

            List<string> lines = new List<string>();

            using (var listResponse = (FtpWebResponse)listRequest.GetResponse())
            using (Stream listStream = listResponse.GetResponseStream())
            using (var listReader = new StreamReader(listStream))
            {
                while (!listReader.EndOfStream)
                {
                    lines.Add(listReader.ReadLine());
                }
            }

            foreach (string line in lines)
            {
                string[] tokens =
                    line.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                string name = tokens[8];
                string permissions = tokens[0];

                if (name == selected_file)
                {
                    string localFilePath = Path.Combine(local, name);
                    string fileUrl = ftpurl + name;

                    if (fileUrl.Contains("#"))
                    {
                        fileUrl = fileUrl.Replace("#", "%23");
                    }

                    if (permissions[0] == 'd')
                    {
                        if (!Directory.Exists(localFilePath))
                        {
                            Directory.CreateDirectory(localFilePath);
                        }

                        //DownloadFtpDirectory(fileUrl + "/", credentials, localFilePath);
                    }
                    else
                    {
                        // Query size of the file to be downloaded
                        WebRequest sizeRequest = WebRequest.Create(fileUrl);
                        sizeRequest.Credentials = credentials;
                        sizeRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                        int size = (int)sizeRequest.GetResponse().ContentLength;
                        progressBar1.Visible = true;
                        progressBar1.Invoke(
                            (MethodInvoker)(() => progressBar1.Maximum = size));


                        // Download the file
                        FtpWebRequest downloadRequest =
                            (FtpWebRequest)WebRequest.Create(fileUrl);
                        downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                        downloadRequest.Credentials = credentials;

                       
                        using (FtpWebResponse downloadResponse = (FtpWebResponse)downloadRequest.GetResponse())
                        using (Stream sourceStream = downloadResponse.GetResponseStream())
                        using (Stream targetStream = File.Create(localFilePath))
                        {
                            byte[] buffer = new byte[10240];
                            int read;
                            int x = 0;
                            
                            while ((read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                targetStream.Write(buffer, 0, read);
                                Console.WriteLine("Downloaded {0} bytes", targetStream.Length);
                                x = (int)targetStream.Position;
                                float d = x / 102400;
                                string g = d.ToString();
                                progressBar1.Invoke((MethodInvoker)(() => progressBar1.Value = x));

                                //progressBar1.CreateGraphics().DrawString(g+"  kb", SystemFonts.SmallCaptionFont,
                                //    Brushes.Black,
                                //    new PointF(progressBar1.Width / 2 - (progressBar1.CreateGraphics().MeasureString(g+ "  kb",
                                //    SystemFonts.DefaultFont).Width / 2.0F),
                                //    progressBar1.Height / 2 - (progressBar1.CreateGraphics().MeasureString(g + "  kb",
                                //    SystemFonts.DefaultFont).Height / 2.0F)));

                            }
                           
                        }
                    }
                    progressBar1.Visible = false;
                    break;
                }

                
            }
            label3.Text = "Downloading Complete";
            label3.Visible = false;
            //DeleteFile(selected_file);
        }
        
        //delete file from FTP
        private string DeleteFile()
        {
            label1.Text = "";
            string selected_file = listBox1.GetItemText(listBox1.SelectedItem);
            if(selected_file.Contains("#"))
            {
                selected_file = selected_file.Replace("#", "%23");
            }
            string ftuser = ConfigurationManager.AppSettings.Get("ftpuser");
            string ftpass = ConfigurationManager.AppSettings.Get("ftppass");
            string ftpurl = ConfigurationManager.AppSettings.Get("ftpurl");
            //FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://172.21.73.118:10022/" + fileName);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpurl + selected_file);
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Credentials = new NetworkCredential(ftuser,ftpass);
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                return response.StatusDescription;
            }


            label1.Text = "Delete Complete";
            label1.Update();
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //String SelectedText = this.comboBox1.SelectedItem.ToString();
            string selected_file = listBox1.GetItemText(listBox1.SelectedItem);
            if (selected_file == "")
            {

                string message = "Please Select File";
                string title = "Alert Message";
                MessageBox.Show(message, title);
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show("Are you Sure Delete this File ?", "Alert Message", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    //do something
                    DeleteFile();
                    readftp();
                }
                else if (dialogResult == DialogResult.No)
                {
                    //do something else
                }
                
            }
            
            //var doc = new HtmlAgilityPack.HtmlDocument();
            //doc.LoadHtml("http://cloudstorage.krakatauposco.co.id/apps/files/?dir=/Plate%20Mill%20L2%20Data%20Request&fileid=1138107");

            //// InnerHtml 
            //var innerHtml = doc.DocumentNode.InnerHtml;

            //// InnerText 
            //var innerText = doc.DocumentNode.InnerText;



        }
       
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            string selected_file = listBox1.GetItemText(listBox1.SelectedItem);
            if (selected_file == "")
            {

                string message = "Please Select File";
                string title = "Alert Message";
                MessageBox.Show(message, title);
            }
            else
            {
                DownloadFtpDirectory();
                Invoke(_delReportDone);
            }
        }
        public void GetVer(String ChVer)
        {
            string dr = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            Process[] processCollection = Process.GetProcesses();
            foreach (Process p in processCollection)
            {
                string c = p.ProcessName;
                if (c == "chromedriver")
                {
                    p.Kill();
                    break;
                }
            }
           
                label1.Visible = true;
                label1.Text = "Please Wait For Updating Driver";
                string url = "https://www.nuget.org/packages/Selenium.WebDriver.ChromeDriver/";
                var web = new HtmlAgilityPack.HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = web.Load(url);
                string getversion = "#version-history > table > tbody > tr:nth-child(1) > td:nth-child(1) > a";
           
                string getlink = "https://www.nuget.org/api/v2/package/Selenium.WebDriver.ChromeDriver/";

            bool flag = false;
            int j = 1;
            while (flag == false)
            {
                var nodesd = doc.QuerySelectorAll(getversion);
                string cd = nodesd[0].InnerHtml.ToString().Trim().Remove(3);

                if (cd == ChVer)
                {
                    flag = true; 
                    string ds = nodesd[0].InnerHtml.TrimStart().TrimEnd();
                    string newlink = getlink + ds;

                    WebClient client = new WebClient();
                    client.DownloadFile(newlink, "chromedriver.nupkg");
                    break;
                }
                else
                {
                    j++;
                    getversion = getversion.Replace("tr:nth-child(1)", "tr:nth-child(" + j + ")");
                }
                //j++;
            }
            //for (int i = 2; i < 5; i++)
            //{
            //    var nodesd = doc.QuerySelectorAll(getversion);
            //    string cd = nodesd[0].InnerHtml.ToString().Trim().Remove(3);
                
            //    if (cd == ChVer)
            //      {
            //        string ds = nodesd[0].InnerHtml.TrimStart().TrimEnd();
            //        string newlink = getlink + ds;
                   
            //        WebClient client = new WebClient();
            //        client.DownloadFile(newlink, "chromedriver.nupkg");
            //        break;
            //    }
            //      else
            //      {
            //           getversion = getversion.Replace("tr:nth-child(1)", "tr:nth-child(" + i + ")");    
            //      }
            //}

                //string ddc = "/html/body/div[3]/section/div/div/div[3]/div[5]/div/table/tbody/tr[1]/td[1]/a/text()";
                //string urll = "/html/body/div[3]/section/div/div/div[3]/div[5]/div/table/tbody/tr[1]/td[1]/a";

                //string selecr = "#version-history > table > tbody > tr:nth-child(1)";
                //var nodexs = doc.DocumentNode.SelectNodes(selecr);
                //for (int i = 1; i < 5; i++)
                //{

                //    string newstr = ddc.Replace("tr[1]", "tr[" + i + "]");
                //    var node = doc.DocumentNode.SelectSingleNode(newstr);

                //    string vs = node.InnerHtml.ToString().Trim().Remove(3);
                //    var fg = doc.GetElementbyId("href");
                //    if (vs == ChVer)
                //    {
                //        string newurl = urll.Replace("tr[1]", "tr[" + i + "]");
                //        var nodex = doc.DocumentNode.SelectSingleNode(newurl);
                //        string fe = nodex.OuterHtml;
                //        doc.LoadHtml(fe);
                //        var anchor = doc.DocumentNode.SelectSingleNode("//a");
                //        if (anchor != null)
                //        {
                //            string link = "https://www.nuget.org" + anchor.Attributes["href"].Value;
                //            HtmlAgilityPack.HtmlDocument doc2 = web.Load(link);
                //            string gw = "/html/body/div[3]/section/div/aside/div[2]/ul/li[5]/a";
                //            var nodexx = doc2.DocumentNode.SelectSingleNode(gw);
                //            string fex = nodexx.OuterHtml;
                //            doc2.LoadHtml(fex);
                //            var anchorx = doc2.DocumentNode.SelectSingleNode("//a");
                //            if (anchorx != null)
                //            {
                //                string linkx = anchorx.Attributes["href"].Value;
                //                WebClient client = new WebClient();
                //                client.DownloadFile(linkx, "chromedriver.nupkg");
                //            }

                //            Console.WriteLine(link);
                //        }
                //        break;
                //    }

                //}
                
                using (FileStream file = File.OpenRead("chromedriver.nupkg"))
                {
                    using (ZipArchive zip = new ZipArchive(file, ZipArchiveMode.Read))
                    {
                        foreach (ZipArchiveEntry entry in zip.Entries)
                        {
                            if (entry.Name == "chromedriver.exe")
                            {
                                string[] fileEntries = Directory.GetFiles(dr, "chromedriver.exe");
                                if (fileEntries[0] != null)
                                {    
                                    File.Delete(fileEntries[0]);
                                }
                                entry.ExtractToFile("chromedriver.exe");
                                break;
                            }
                        }
                    }
                }
                string[] fileEntriesx = Directory.GetFiles(dr, "chromedriver.nupkg");
                if (fileEntriesx[0] != null)
                {
                    File.Delete(fileEntriesx[0]);
                }

            //}
            label1.Text = "Update Complete";
            Chromver();
            //driver.Close();
            //driver.Quit();

        }
        public void Chromver()
        {
            string chromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
            string chromeVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(chromePath).ProductVersion[0..3];
            string dr = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            //cmd.StartInfo.WorkingDirectory = @"C:\Users\kp1hmi01\source\repos\project\project\bin\Debug\net6.0-windows";
            cmd.StartInfo.WorkingDirectory = dr;
            cmd.Start();

            /* execute "dir" */

            cmd.StandardInput.WriteLine("chromeDriver -v");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            int i = 0;
            string[] tr = new string[10];
            while (!cmd.StandardOutput.EndOfStream)
            {
                tr[i] = cmd.StandardOutput.ReadLine();
                i++;
            }
            string Driverver = "";
            Driverver = tr[4][13..16];

            label4.Text = "Chrome Ver : " + chromeVersion;
            label5.Text = "Driver Ver : " + Driverver;
            //if (chromeVersion != Driverver)
            //{
            //    GetVer(chromeVersion);
            //}
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }


    }
}

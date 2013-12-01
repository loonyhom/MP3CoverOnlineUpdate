using System;
using System.Data;
using System.Text;
using System.Net;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using Mp3Lib;
using Id3Lib;
using System.IO.Compression;
namespace Mp3AlbumCoverUpdater
{
    public partial class frmMp3Album : Form
    {
        public frmMp3Album()
        {
            InitializeComponent();
        }

        WebClient myWebClient = new WebClient();
        string SourceCode = "";
        ArrayList httpList;
        int iThread;
        Mp3File Mp3File = null;
        private string strSelectPaht="";
        private DataTable dtResult = null;
        private List<string> listError = new List<string>();
        private string strEngine = "";
       
        private delegate void TempDelegate(Image image);
        private delegate void ChangeControlEnable(Button bt);

        public class ThreadInfo
        {
            private int _iStart;
            private int _iEnd;
            public int iStart { get { return _iStart; } set { _iStart = value; } }
            public int iEnd { get { return _iEnd; } set { _iEnd = value; } }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Text = "加载中..";            
            btnStart.Enabled = false;
        
            string strurlBaidu = "http://image.baidu.com/i?tn=baiduimage&ipn=r&ct=201326592&cl=2&lm=-1&st=-1&fm=result&fr=&sf=1&fmq=&pv=&ic=0&nc=1&z=&se=1&showtab=0&fb=0&width=&height=&face=0&istype=2&ie=utf-8&word=";
            string strurlQQ = "http://soso.music.qq.com/fcgi-bin/music_json.fcg?mid=1&catZhida=1&lossless=0&json=1&w=Key&num=30&t=8&p=1&utf8=1&searchid=216648286573551810&remoteplace=sizer.yqqlist.album&g_tk=5381&loginUin=0&hostUin=0&format=yqq&jsonpCallback=MusicJsonCallback&needNewCode=0";
            string strurlGoogle = "http://www.google.com.hk/search?newwindow=1&safe=strict&hl=zh-CN&biw=1366&bih=654&site=imghp&tbm=isch&sa=1&q=";
            string strurl163 = "http://music.163.com/#/m/search?s=key&_page=search&type=10";
            string strurlXiaMi = "http://www.xiami.com/search?spm=a1z1s.3521873.23310045.1.AKUtUf&key=";
            string strurlSouGou = "http://pic.sogou.com/pics?ie=utf8&p=40230504&interV=kKIOkrELjboMmLkEk7oTkKIMkbELjbgQmLkElbcTkKILmrELjboLmLkEkr4TkKIRmLkEk78TkKILkbELjboN_1861238217&query=";
            string strurl360 = "http://image.so.com/i?ie=utf-8&q=";
            string strurl = "";         
            switch (cobEngine.Text)
            {
                case"BaiDu":
                    strurl = strurlBaidu + txtKeyWord.Text;
                    strEngine = cobEngine.Text;
                    SourceCode += GetWebClient(strurl);
                    break;
                case"QQ":
                    strurl = strurlQQ.Replace("Key", System.Web.HttpUtility.UrlEncode(txtKeyWord.Text, System.Text.Encoding.GetEncoding("UTF-8")));
                    strEngine = cobEngine.Text;
                    SourceCode = GetWebClient(strurl);
                    SourceCode = CreateImageUrl(SourceCode);
                    break;
                case"Google":
                    strurl = strurlGoogle + txtKeyWord.Text;
                    strEngine = cobEngine.Text;
                    SourceCode = GetWebClient(strurl);
                    break;
                case"163":
                    strEngine = cobEngine.Text;
                    SourceCode = PostData("http://music.163.com/api/search/get/web?csrf_token=", "hlpretag=%3Cspan%20class%3D%22s-fc7%22%3E&hlposttag=%3C%2Fspan%3E&s=" + GuessAlbumNameBy163(txtKeyWord.Text) + "&_page=search&type=10&offset=0&total=true&limit=75", "http://music.163.com");
                    break;
                case "XiaMi":
                    strurl = strurlXiaMi+txtKeyWord.Text;
                    strEngine = cobEngine.Text;
                    SourceCode = GetWebClient(strurl);
                    break;
                case "SouGou":
                    strurl = strurlSouGou + txtKeyWord.Text;
                    strEngine = cobEngine.Text;
                    SourceCode = GetWebClient(strurl);
                    break;
                case "360":
                    strurl = strurl360 + txtKeyWord.Text;
                    strEngine = cobEngine.Text;
                    SourceCode = GetWebClient(strurl);
                    break;
                default:
                    break;
            }
            
            
            flpPicture.Controls.Clear();
            httpList = GetHyperLinks(SourceCode);
            iThread = 10;
            int iSum = httpList.Count;
            int iAve = iSum / iThread;
            int iMod = iSum % iThread;

            try
            {
                
                for (int i = 0; i < iThread; i++)
                {
                    ThreadInfo ti = new ThreadInfo();
                    if(i==0)
                    {
                        ti.iStart = 0;
                        ti.iEnd = iAve - 1;
                    }
                    
                    else
                    {
                        ti.iStart = i * iAve;
                        ti.iEnd = (i * iAve) + iAve - 1;
                    }
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Mp3AlbumCoverUpdaterToForm), ti);
                }
                if (iMod != 0)
                {
                    ThreadInfo ti = new ThreadInfo();
                    ti.iStart = iAve * iThread;
                    ti.iEnd = iSum - 1;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Mp3AlbumCoverUpdaterToForm), ti);

                }
                //AutoResetEvent mainAutoResetEvent = new AutoResetEvent(false);
                RegisteredWaitHandle registeredWaitHandle = null;
                registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(false), new WaitOrTimerCallback(delegate(object obj, bool timeout)
                {
                    int workerThreads = 0;
                    int maxWordThreads = 0;
                    int compleThreads = 0;
                    ThreadPool.GetAvailableThreads(out workerThreads, out compleThreads);
                    ThreadPool.GetMaxThreads(out maxWordThreads, out compleThreads);                   
                    if (workerThreads == maxWordThreads)
                    {
                      
                        //mainAutoResetEvent.Set();
                        registeredWaitHandle.Unregister(null);
                        btnStart.Invoke(new ChangeControlEnable(ChangeButtonEnable), new object[] { btnStart });

                    }
                }), null, 1000, false);
                //mainAutoResetEvent.WaitOne();
               
                this.Cursor = Cursors.WaitCursor;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                this.Cursor = Cursors.Default;
              

            }
        }

        private void ChangeButtonEnable(Button bt)
        {
            bt.Text = "开始";
            bt.Enabled = true;
        }

        private void Mp3AlbumCoverUpdaterToForm(Object threadinfo)
        {
            ThreadInfo ti = (ThreadInfo)threadinfo;
            
            for (int i = 0; i < ti.iEnd-ti.iStart+1; i++)
            {
                Image image = null;
                try
                {
                    image = GetUrlImage(httpList[ti.iStart + i].ToString());
                    flpPicture.Invoke(new TempDelegate(AddPictureBox), new object[] { image });
                }
                catch (Exception)
                {
                    
                    throw;
                }
                
                
            }
        
        }

        private void AddPictureBox(Image image)
        {
            if (image != null)
            {
                PictureBox picbox = new PictureBox();
                picbox.Size = new Size(100, 100);
                picbox.Click += new EventHandler(picbox_Click);
                picbox.SizeMode = PictureBoxSizeMode.StretchImage;
                picbox.Image = image;
                flpPicture.Controls.Add(picbox);
            }
        }

        void picbox_Click(object sender, EventArgs e)
        {
            PictureBox picbox = sender as PictureBox;
            ptbNew.Image = picbox.Image;
            if (picbox.Image == null) return;
            label1.Text = picbox.Image.Size.ToString();
        }

        void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            Point aPoint = new Point(e.X, e.Y);
            aPoint.Offset(this.Location.X, this.Location.Y);
            Rectangle aRec1 = new Rectangle(flpPicture.Location.X, flpPicture.Location.Y, flpPicture.Width, flpPicture.Height);
          

            if (RectangleToScreen(aRec1).Contains(aPoint))
                flpPicture.AutoScrollPosition = new Point(0, flpPicture.VerticalScroll.Value - e.Delta / 20);
        
        }
        private string GetWebClient(string url)
        {
            try
            {
                string strHTML = "";
                Stream myStream = myWebClient.OpenRead(url);
                StreamReader sr = new StreamReader(myStream, System.Text.Encoding.GetEncoding("gb2312"));
                strHTML = sr.ReadToEnd();
                strHTML=strHTML.Replace("\\","");
                
                myStream.Close();
                return strHTML;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
                return "";
            }
           
        }

        private string CreateImageUrl(string Source)
        {
            ArrayList al = new ArrayList();
            string strImageUrl = "";
            string strRegex = @"albumMID.*?,";
            Regex r = new Regex(strRegex, RegexOptions.IgnoreCase);
            MatchCollection m = r.Matches(Source);
            for (int i = 0; i < m.Count; i++)
            {
                strImageUrl+="http://imgcache.qq.com/music/photo/mid_album_500/"+m[i].ToString().Substring(22,1)+"/"+m[i].ToString().Substring(23,1)+"/"+m[i].ToString().Substring(10,14)+".jpg   ";
            }
            return strImageUrl;
        }

        private ArrayList GetHyperLinks(string htmlCode)
        {
            ArrayList al = new ArrayList();

            string strRegex = @"http:\/\/.[^""]*?\.jpg";           
            Regex r = new Regex(strRegex, RegexOptions.IgnoreCase);
            MatchCollection m = r.Matches(htmlCode);

            for (int i = 0; i <= m.Count - 1; i++)
            {
                bool rep = false;
                string strNew = m[i].ToString();
                foreach (string str in al)
                {
                    if (strEngine == "163" || strEngine == "BaiDu")
                    {
                        if (strNew.Substring(10, 38) == str.Substring(10, 38) || strNew.Contains("fm=21&gp"))
                        {
                            rep = true;
                            break;
                        }
                    }
                    else if (strEngine == "SouGou" || strEngine == "360")
                    {
                        if (strNew.Contains("sogou.com") || strNew.Contains("so.qhimg.com"))
                        {
                            rep = true;
                            break;
                        }
                    }
                }
                if (strEngine == "XiaMi")
                {
                    strNew = strNew.Replace("_1", "_4");
                }
                if (!rep) al.Add(strNew);
            }           
            return al;
        }    

        private Image GetUrlImage(string strurl)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strurl);
            request.Method = "GET";
            string strReferer = "";
            switch (strEngine)
            {
                case "BaiDu":
                    strReferer = "http://www.baidu.com";
                    break;
                case "QQ":
                    strReferer = "http://y.qq.com/";
                    break;
                case "Google":
                    strReferer = "http://www.google.hk";
                    break;
                case "163":
                    strReferer = "http://music.163.com/";
                    break;
                case "XiaMi":
                    strReferer = "http://www.xiami.com/";
                    break;
                case "SouGou":
                    strReferer = "http://www.sogou.com/";
                    break;
                case "360":
                    strReferer = "http://www.so.com/";
                    break;
                default:
                    break;
            }
            request.Referer = strReferer;
            request.ContentType = "application/x-www-form-urlencoded";
            Image image;
            Stream myStream;
            try
            {
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

             myStream = response.GetResponseStream();

             image = Image.FromStream(myStream); 
                myStream.Close();
            }
            catch (Exception)
            {
                listError.Add(strurl);
                image = null;
            }          
            
           
            return image;
        }

        private void Form1_Load(object sender, EventArgs e)
        {           
            cobEngine.Text = "QQ";
            this.MouseWheel += new MouseEventHandler(Form1_MouseWheel);

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            Mp3File.TagHandler.Picture = ptbNew.Image;
            Mp3File.Update();
            dgvList.SelectedRows[0].Cells[2].Value = ptbNew.Image;
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                
               
                try
                {
                    strSelectPaht = fbd.SelectedPath;
                    frmWaitingBox frm = new frmWaitingBox(new EventHandler<EventArgs>(GetFiles), 30 * 60 * 1000, "", false, true);
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        dgvList.DataSource = dtResult;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                   
                }
              
               
            }
           
        }

        private void GetFiles(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            DataColumn dc1 = new DataColumn("标题", typeof(string));
            DataColumn dc2 = new DataColumn("路径", typeof(string));
            DataColumn dc3 = new DataColumn("专辑封面", typeof(Image));
           
            dt.Columns.Add(dc1);
            dt.Columns.Add(dc2);
            dt.Columns.Add(dc3);
            DirectoryInfo di = new DirectoryInfo(strSelectPaht);
            FileInfo[] files1 = di.GetFiles("*.mp3");
            string strTitel = "";
            try
            {
                foreach (FileInfo fi in files1)
                {
                    DataRow dr = dt.NewRow();
                    dr["标题"] = fi.Name;
                    dr["路径"] = fi.FullName;
                    strTitel = fi.Name;
                    Mp3File = new Mp3File(fi.FullName);
                    dr["专辑封面"] = Mp3File.TagHandler.Picture;
                    dt.Rows.Add(dr.ItemArray);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(strTitel + " " + ex.Message);
            }
            finally
            {
                dtResult = dt;
            }           
            
        }

        private void dgvList_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvList.SelectedRows.Count <= 0) return;

            try
            {
                Mp3File = new Mp3File(dgvList.SelectedRows[0].Cells["路径"].Value.ToString());
               
                Mp3FileInfo mp3fileinfo = new Mp3FileInfo(dgvList.SelectedRows[0].Cells["路径"].Value.ToString());
                txtKeyWord.Text = mp3fileinfo.Title.Trim() + " " + mp3fileinfo.Artist.Trim();
                ptpOld.Image = mp3fileinfo.AlbumCover;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ptpOld.Image = null;
            }

        }       
       

        private void btnAoutUpdate_Click(object sender, EventArgs e)
        {
            MessageBox.Show(".....待续");
        }

        private void ptbNew_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ptbNew.Image = Image.FromFile(ofd.FileName);
            }
        }              

        private void ShowErrorList()
        {

            File.WriteAllLines("E:\\ERROR.TXT", listError.ToArray(), Encoding.Default);
            //Process.Start("E:\\ERROR.TXT");
            listError.Clear();
        }

        private string PostData(string targetUrl,string postDataStr,string refererUrl)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(targetUrl);
            request.CookieContainer = new CookieContainer();
            CookieContainer cookie = request.CookieContainer;
            request.Referer = refererUrl;
            request.Accept = "*/*";
            request.Headers["Accept-Language"] = "zh-cn";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/6.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E)";
            request.KeepAlive = true;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";

            Encoding encoding = Encoding.UTF8;
            byte[] postData = encoding.GetBytes(postDataStr);
            request.ContentLength = postData.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(postData, 0, postData.Length);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            if (response.Headers["Content-Encoding"] != null && response.Headers["Content-Encoding"].ToLower().Contains("gzip"))
            {
                responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
            }

            StreamReader streamReader = new StreamReader(responseStream, encoding);
            string retString = streamReader.ReadToEnd();

            streamReader.Close();
            responseStream.Close();
            return retString;
        }

        private string GuessAlbumNameByQQ(string KeyWord)
        {
            string strTemp = GetWebClient("http://soso.music.qq.com/fcgi-bin/multiple_music_search.fcg?mid=1&p=1&catZhida=1&lossless=0&t=100&searchid=40993740618700273&remoteplace=txt.yqqlist.all&utf8=1&w=" + System.Web.HttpUtility.UrlEncode(KeyWord, System.Text.Encoding.GetEncoding("UTF-8")));
            string strRegex = "<a class=\"data\" style=\"display:none;\">.*?</a>";
            Regex r = new Regex(strRegex, RegexOptions.IgnoreCase);
            MatchCollection m = r.Matches(strTemp);
            string[] strTemps = m[0].ToString().Split(new char[] { '|' });
            if (strTemps.Length > 6)
            {
                return strTemps[5].ToString();
            }
            else
            {
                return "";
            }

        }

        private string GuessAlbumNameBy163(string KeyWord)
        {
            string strTemp = PostData("http://music.163.com/api/search/get/web?csrf_token=", "hlpretag=%3Cspan%20class%3D%22s-fc7%22%3E&hlposttag=%3C%2Fspan%3E&s=Key&_page=search&type=1&offset=0&total=true&limit=30".Replace("Key",System.Web.HttpUtility.UrlEncode(KeyWord, System.Text.Encoding.GetEncoding("UTF-8"))), "http://music.163.com");
            string strRegex = "artists\".*?artist\"";
            Regex r = new Regex(strRegex, RegexOptions.IgnoreCase);
            MatchCollection m = r.Matches(strTemp);
            string[] strTemps = m[0].ToString().Split(new char[] { '"' });

            if (strTemps.Length > 7)
            {
                return strTemps[6].ToString()+" "+strTemps[18].ToString();
            }
            else
            {
                return "";
            }

        }
       
    }
}
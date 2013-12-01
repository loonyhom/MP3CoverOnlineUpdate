using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Mp3AlbumCoverUpdater
{
    /// <summary>
    /// 获取js执行之后的网页html标签body部分的代码
    /// </summary>
    public class FinalHtml
    {
        private String htmlString;
        private String url;
        private String htmlTitle;
        // 获得html title标签的内容
        public String HtmlTitle
        {
            get
            {
                if (success == false) return null;
                return htmlTitle;
            }
        }
        private List<String> linkList;
        private List<String> imageList;
        private bool success; // 是否成功运行
        /// <summary>
        /// 获得网页所有链接的链表， 一定要在Run之后进行
        /// </summary>
        public List<String> LinkList
        {
            get
            {
                if (success == false) return null;
                return linkList;
            }
        }
        /// <summary>
        /// 获得所有图像的标签， 一定要在Run之后进行
        /// </summary>
        public List<String> ImageList
        {
            get
            {
                if (success == false) return null;
                return imageList;
            }
        }
        /// <summary>
        /// 获得执行完js之后的网页body 部分的html代码
        /// </summary>
        public String HtmlBody
        {
            get
            {
                if (success == false) return null;
                return htmlString;
            }
        }
        public FinalHtml()
        {
            linkList = new List<String>();
            imageList = new List<String>();
            htmlString = "";
            success = false;
        }
        /// <summary>
        /// 检查并补充设置url
        /// </summary>
        /// <param name="url"></param>
        private void CheckURL(String url)
        {
            if (!url.StartsWith("http://") && !url.StartsWith("https://") && !url.StartsWith("file:///"))
                url = "http://" + url;
            this.url = url;
        }
        /// <summary>
        /// 加载指定文件
        /// </summary>
        /// <param name="url">文件URL</param>
        /// <param name="timeOut">超时时限</param>
        /// <returns>是否成功运行，没有超时</returns>
        public bool Run(String url, int timeOut)
        {
            timeOut = 10000;
            CheckURL(url);
            Thread newThread = new Thread(NewThread);
            newThread.SetApartmentState(ApartmentState.STA);/// 为了创建WebBrowser类的实例 必须将对应线程设为单线程单元
            newThread.Start();
            //监督子线程运行时间
            while (newThread.IsAlive && timeOut > 0)
            {
                Thread.Sleep(100);
                timeOut -= 100;
            }
            // 超时处理
            if (newThread.IsAlive)
            {
                if (success) return true;
                newThread.Abort();
                return false;
            }
            return true;
        }

        private void NewThread()
        {
            new FinalHtmlPerThread(this);
            Application.Run();// 循环等待webBrowser 加载完毕 调用 DocumentCompleted 事件
        }
        /// <summary>
        ///  用于处理一个url的核心类
        /// </summary>
        class FinalHtmlPerThread : IDisposable
        {
            FinalHtml master;
            WebBrowser web;

            public FinalHtmlPerThread(FinalHtml master)
            {
                this.master = master;
                DealWithUrl();
            }
            private void DealWithUrl()
            {
                String url = master.url;
                web = new WebBrowser();
                bool success = false;
                try
                {
                    web.Url = new Uri(url);
                    web.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(web_DocumentCompleted); // 对事件加委托
                    success = true;
                }
                finally
                {
                    if (!success)
                        Dispose();
                }

            }
            public void Dispose()
            {
                if (!web.IsDisposed)
                    web.Dispose();
            }
            private void ToList(HtmlElementCollection collection, List<String> list)
            {
                System.Collections.IEnumerator it = collection.GetEnumerator();
                while (it.MoveNext())
                {
                    HtmlElement htmlElement = (HtmlElement)it.Current;
                    list.Add(htmlElement.OuterHtml);
                }
            }
            private void web_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
            {
                //微软官方回答 一个网页有多个Ifram元素就有可能触发多次此事件， 并且提到了
                // vb 和 C++ 的解决方案， C# 没有提及， 经本人尝试，发现下面的语句可以判断成功
                // 如果未完全加载 web.ReadyState = WebBrowserReadyState.Interactive
                if (web.ReadyState != WebBrowserReadyState.Complete) return;
                master.htmlTitle = web.Document.Title;
                ToList(web.Document.Links, master.linkList);
                ToList(web.Document.Images, master.imageList);
                master.htmlString = web.DocumentText;
                master.success = true;
                Thread.CurrentThread.Abort();
            }
        }
    }
}
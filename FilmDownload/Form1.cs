using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Reflection;

namespace FilmDownload
{
    public partial class Form1 : Form
    {
        private static List<FilmUpdate> films;

        string path = "film.xml";

        public Form1()
        {
            InitializeComponent();
        }

        private void FormUpdate_Load(object sender, EventArgs e)
        {
            try
            {
                FilmUpdate fu = new FilmUpdate();

                foreach (FieldInfo fi in fu.GetType().GetFields())
                {
                    this.listView1.Columns.Add(fi.Name, -2, HorizontalAlignment.Left); //一步添加
                }

                listView1.KeyDown += listView1_KeyDown;

                //XmlHelper xml = new XmlHelper(path);
                //DataTable dtList = xml.GetData(rootNodeName);'

                FileStream fs = new FileStream(path, FileMode.Open);
                films = XmlHelper.Deserialize(typeof(List<FilmUpdate>), fs) as List<FilmUpdate>;
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    //将复制的内容放入剪切板中
                    if (listView1.SelectedItems[0].Text != "")
                        Clipboard.SetDataObject(listView1.SelectedItems[0].SubItems[4].Text);
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string url = textBox1.Text;

                //创建http链接
                var request = (HttpWebRequest)WebRequest.Create(url);

                request.Timeout = 1000 * 5;    //5s过期

                var response = (HttpWebResponse)request.GetResponse();

                Stream stream = response.GetResponseStream();

                StreamReader sr = new StreamReader(stream, Encoding.GetEncoding("GB2312"));

                string content = sr.ReadToEnd();

                var list = GetHtmlFtpUrl(content);

                showURL.Text = list.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                lblShowInfo.Text = string.Empty;
                if (Directory.Exists(txtFilePathDownload.Text))
                {
                    DirectoryInfo di = new DirectoryInfo(txtFilePathDownload.Text);

                    SearchDirectory(di);
                }
                else
                {
                    MessageBox.Show("文件夹不存在！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        static Semaphore sem = new Semaphore(2, 10);
        private void btnFindUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                //sem.Release(1);
                listView1.Items.Clear();

                foreach (FilmUpdate film in films)
                {
                    string filmPath = txtFilePathDownload.Text + "\\" + film.Name;
                    if (Directory.Exists(filmPath))
                    {
                        DirectoryInfo dir = new DirectoryInfo(filmPath);
                        film.CountDownloaded = dir.GetFiles().Length;
                    }
                    Thread thread = new Thread(new ParameterizedThreadStart(CheckFilmState));
                    thread.Start(film);

                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region 工具函数
        public static List<string> GetHtmlFtpUrlList(string sHtmlText)
        {
            // 定义正则表达式用来匹配 img 标签 
            Regex regImg = new Regex(@"^ftp://\W:\W@dq.dl1234.com.+</a>$", RegexOptions.IgnoreCase);
            //Regex regImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);

            // 搜索匹配的字符串 
            MatchCollection matches = regImg.Matches(sHtmlText);

            List<string> sUrlList = new List<string>();

            // 取得匹配项列表 
            foreach (Match match in matches)
                sUrlList.Add(match.Groups["imgUrl"].Value);
            return sUrlList;
        }

        public static StringBuilder GetHtmlFtpUrl(string sHtmlText)
        {
            // 定义正则表达式用来匹配 img 标签 
            Regex regImg = new Regex(@">ftp://[a-z]{1}:[a-z]{1}@dq.dl1234.com.+</a>", RegexOptions.IgnoreCase);
            //Regex regImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);

            // 搜索匹配的字符串 
            MatchCollection matches = regImg.Matches(sHtmlText);

            StringBuilder sUrl = new StringBuilder();

            // 取得匹配项列表 
            foreach (Match match in matches)
            {
                string sMatch = match.Value.TrimStart('>');
                sMatch = sMatch.Substring(0, sMatch.IndexOf("</a>"));
                sUrl.AppendLine(sMatch);
            }
            return sUrl;
        }

        public static StringBuilder GetFilmUpdateInfo(string sHtmlText)
        {
            // 定义正则表达式用来匹配 img 标签 
            Regex regImg = new Regex(@"<title>.+</title>", RegexOptions.IgnoreCase);
            //Regex regImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);

            // 搜索匹配的字符串 
            MatchCollection matches = regImg.Matches(sHtmlText);

            StringBuilder sUrl = new StringBuilder();

            // 取得匹配项列表 
            foreach (Match match in matches)
            {
                string sMatch = match.Value;
                if (sMatch.Contains("更新"))
                {
                    sMatch = sMatch.Substring(sMatch.IndexOf("更新"));
                }
                else if (sMatch.Contains("全"))
                {
                    sMatch = sMatch.Substring(sMatch.IndexOf("全"));
                }
                sMatch = sMatch.Substring(0, sMatch.IndexOf("集") + 1);
                sUrl.AppendLine(sMatch);
            }
            return sUrl;
        }

        private void SearchDirectory(DirectoryInfo di)
        {
            bool bShowInfo = false;
            foreach (FileInfo fi in di.GetFiles())
            {
                foreach (string s in listEx.Items)
                {
                    if (fi.Name.Contains(s))
                    {
                        fi.MoveTo(fi.FullName.Substring(0, fi.FullName.Length - fi.Name.Length) 
                            + fi.Name.Replace(s, string.Empty));
                        bShowInfo = true;
                    }
                }
            }

            if (bShowInfo)
            {
                lblShowInfo.Text += di.FullName + "完成\r\n";
            }

            DirectoryInfo[] dis = di.GetDirectories();
            if (dis.Length > 0)
            {
                foreach (DirectoryInfo subDi in dis)
                {
                    SearchDirectory(subDi);
                }
            }
        }

        public void CheckFilmState(object film)
        {
            try
            {
                FilmUpdate curFilm = (FilmUpdate)film;

                sem.WaitOne();
                var request = (HttpWebRequest)WebRequest.Create(curFilm.Url);
                request.Timeout = 1000 * 5;
                var response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader sr = new StreamReader(stream, Encoding.GetEncoding("GB2312"));
                string content = sr.ReadToEnd();

                StringBuilder sb = GetFilmUpdateInfo(content);
                curFilm.UpdateState = sb.ToString();

                curFilm.CountWaitDownload = int.Parse(new Regex(@"\d+").Match(sb.ToString()).Value) - curFilm.CountDownloaded;

                //UpdateListview();
                UpdateListview(curFilm);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            { 
                sem.Release();
            }
        }

        public void UpdateListview(FilmUpdate film)
        {
            if (listView1.InvokeRequired)
            {
                listView1.Invoke(new Action<FilmUpdate>(UpdateListview), new object[] { film });
            }
            else
            {
                listView1.BeginUpdate();
                ListViewItem lvi = new ListViewItem();

                foreach (FieldInfo fi in film.GetType().GetFields())
                {
                    if (fi.Name ==  "Name")
                    {
                        lvi.Text = fi.GetValue(film).ToString();
                    }
                    else
                    {
                        lvi.SubItems.Add(fi.GetValue(film).ToString());

                        if (fi.Name == "CountWaitDownload" && ((int)fi.GetValue(film)) > 0)
                        {
                            lvi.BackColor = Color.LightPink;
                        }
                    }
                }
                listView1.Items.Add(lvi);
                listView1.EndUpdate();

                for (int i = 0; i < listView1.Columns.Count; i++)
                {
                    listView1.Columns[i].Width = -1;
                }
            }
        }
        
        public void UpdateListview()
        {
            if (listView1.InvokeRequired)
            {
                listView1.Invoke(new Action(UpdateListview), new object[] {  });
            }
            else
            {
                listView1.Items.Clear();

                listView1.BeginUpdate();
                ListViewItem lvi = new ListViewItem();

                foreach (FilmUpdate film in films)
                {
                    foreach (FieldInfo fi in film.GetType().GetFields())
                    {
                        if (fi.Name == "Name")
                        {
                            lvi.Text = fi.GetValue(film).ToString();
                        }
                        else
                        {
                            lvi.SubItems.Add(fi.GetValue(film).ToString());

                            if (fi.Name == "CountWaitDownload" && ((int)fi.GetValue(film)) > 0)
                            {
                                lvi.BackColor = Color.LightPink;
                            }
                        }
                    }
                    listView1.Items.Add(lvi);
                    listView1.EndUpdate();
                }

                for (int i = 0; i < listView1.Columns.Count; i++)
                {
                    listView1.Columns[i].Width = -1;
                }
            }
        }
        #endregion

        
    }

    public class FilmUpdate
    {
        public string Name;
        public string UpdateState;
        public int CountDownloaded;
        public int CountWaitDownload;
        public string Url;

        public FilmUpdate()
        { 
        }

        public FilmUpdate(string Name,string Url)
        {
            this.Name = Name;
            this.Url = Url;
        }
    }
}

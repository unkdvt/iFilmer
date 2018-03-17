using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;

namespace iFilmer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //   textBox3.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "iFilmer\\Posters\\");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //   checkBox1.CheckState = CheckState.Checked;
        }

        TMDbClient client = new TMDbClient("0bbad6ec9cff1a17b613d802f0eac5fd");
        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync(lstsearchresult);
        }


        string mvname;
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem li in lstsearchresult.SelectedItems)
            {
                Movie movie = client.GetMovieAsync(li.SubItems[1].Text).Result;
                mvname = movie.Title;
                var imgurl = "http://image.tmdb.org/t/p/w500/" + movie.PosterPath;
                textBox2.Text = pictureBox1.ImageLocation = imgurl;
                txtlng.Text = movie.OriginalLanguage;
                foreach (Genre g in movie.Genres)
                {
                    txty.Text = g.Name;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //find illigal characters in file name and remove them
            string illegal = mvname + ".jpg";
            string regexSearch = new string(Path.GetInvalidFileNameChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            illegal = r.Replace(illegal, "");  //new file name after remove illigal characters.

            savePoster.FileName = illegal;
            savePoster.Filter = "JPG Image (*.jpg)|*.jpg";
            if (savePoster.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image.Save(savePoster.FileName);
                sts.Text = "Poster saved..";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            lstFilms.Items.Clear();
            if (selectFilmFolder.ShowDialog() == DialogResult.OK)
            {
                DirectoryInfo d = new DirectoryInfo(selectFilmFolder.SelectedPath);
                textBox3.Text = d.Name;
                foreach (DirectoryInfo s in d.GetDirectories())
                {
                    string name = s.FullName.Remove(0, s.FullName.LastIndexOf('\\') + 1).Trim();
                    string pattern = @"\(\d{4}\)";
                    name = Regex.Replace(name, pattern, "");
                    name = name.Replace('.', ' ').Replace('(', ' ').Replace(')', ' ').Replace("Zoom lk Sinhala Subtitles", "").Replace(" bold", "").Trim();

                    lstFilms.Items.Add(name);
                }

            }
        }

        private void lstFilms_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = lstFilms.SelectedItem.ToString();

        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Invoke(new Action(() => sts.Text = "Getting Result..."));
            var lsts = (ListView)e.Argument;
            if (lsts.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    lsts.Items.Clear();

                    SearchContainer<SearchMovie> results = client.SearchMovieAsync(textBox1.Text).Result;

                    foreach (SearchMovie result in results.Results)
                        Invoke(new Action(() =>
                        lstsearchresult.Items.Add(new ListViewItem(new string[] {
                    result.Title, result.Id.ToString() }))));

                    List<string> adress = new List<string>();

                    foreach (ListViewItem li in lsts.Items)
                    {
                        Movie mv = client.GetMovieAsync(li.SubItems[1].Text).Result;
                        var imgurl = "http://image.tmdb.org/t/p/w500/" + mv.PosterPath;
                        adress.Add(imgurl);
                    }

                    ImageList il = new ImageList();


                    foreach (string img in adress)
                    {

                        try
                        {
                            WebRequest request = WebRequest.Create(img);
                            WebResponse resp = request.GetResponse();
                            Stream respStream = resp.GetResponseStream();
                            Bitmap bmp = new Bitmap(respStream);
                            respStream.Dispose();
                            il.Images.Add(bmp);
                        }

                        catch { }


                    }


                    il.ImageSize = new Size(32, 32);
                    int count = 0;
                    lsts.LargeImageList = il;
                    lsts.SmallImageList = il;
                    foreach (ListViewItem s in lstsearchresult.Items)
                    {
                        s.ImageIndex = count++;
                    }

                    Invoke(new Action(() => sts.Text = String.Format("Total of {0} result found", results.Results.Count)));
                    if (lstsearchresult.Items.Count == 1)
                    {
                        lsts.Items[0].Selected = true;
                        lsts.Items[0].Focused = true;
                    }
                });
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {

        }
    }
}

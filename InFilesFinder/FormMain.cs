using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace InFilesFinder
{
    public partial class FormMain : Form
    {
        List<string> filesList = new List<string>();
        string logOut = pathAddSlash(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)) + "_foundlist.txt";
        string lastFolder = null;

        public FormMain()
        {
            InitializeComponent();
        }

        private void searchFolder(string path)
        {
            getInFilesFinder(path);
            foreach (string line in Directory.GetDirectories(path))
            {
                if (!new DirectoryInfo(line).Attributes.HasFlag(FileAttributes.System))
                {
                    searchFolder(line);
                }
            }
        }

        private void getInFilesFinder(string path)
        {
            foreach (string file in Directory.GetFiles(path))
            {
                int linenum = 0;
                string fline = null;
                StreamReader sr = new StreamReader(file);
                while ((fline = sr.ReadLine()) != null)
                {
                    linenum++;
                    foreach (string line in textBox1.Lines)
                    {
                        if (fline.IndexOf(textBox2.Text + line + textBox3.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            filesList.Add(file + "\t" + line + "\t" + linenum.ToString());
                        }
                    }
                }
                sr.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (lastFolder != null)
            {
                if (Directory.Exists(lastFolder))
                {
                    folderBrowserDialog1.SelectedPath = lastFolder;
                }
            }
            else
            {
                folderBrowserDialog1.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK && Directory.Exists(folderBrowserDialog1.SelectedPath) && textBox1.Lines.Length > 0)
            {
                button1.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;
                lastFolder = folderBrowserDialog1.SelectedPath;
                searchFolder(folderBrowserDialog1.SelectedPath);
                try
                {
                    File.WriteAllLines(logOut, filesList);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось записать файл: " + logOut + Environment.NewLine + ex.Message);
                }
                textBox3.Enabled = true;
                textBox2.Enabled = true;
                button1.Enabled = true;
            }
            filesList.Clear();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                if (sender != null)
                    ((TextBox)sender).SelectAll();
            }
        }

        private static string pathAddSlash(string path)
        {
            if (!path.EndsWith("/") && !path.EndsWith(@"\"))
            {
                if (path.Contains("/"))
                {
                    path += "/";
                }
                else if (path.Contains(@"\"))
                {
                    path += @"\";
                }
            }
            return path;
        }
    }
}

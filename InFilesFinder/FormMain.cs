using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace InFilesFinder
{
    public partial class FormMain : Form
    {
        List<string> filesList = new List<string>();
        List<string> searchStrings = new List<string>();
        string temppath = null;

        public FormMain()
        {
            InitializeComponent();
            toolTip1.SetToolTip(checkBox1, "Производит поиск каждой строки в дополнительных символах в начале и в конце.");
        }

        private void searthFolder(string path)
        {
            foreach (string line in Directory.GetDirectories(path))
            {
                searthFolder(line);
                getInFilesFinder(line);
            }
        }

        private void getInFilesFinder(string path)
        {
            foreach (string line in Directory.GetFiles(path))
            {
                using (StreamReader sr = new StreamReader(line))
                {
                    sr.BaseStream.Position = 0;
                    string line1;
                    int linenum = 0;
                    while ((line1 = sr.ReadLine()) != null)
                    {
                        linenum++;
                        bool find = false;
                        foreach (string line2 in searchStrings)
                        {
                            if (!checkBox1.Checked)
                            {
                                if (line1.IndexOf(line2, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    find = true;
                                }
                            }
                            else
                            {
                                if (line1.IndexOf("=" + line2, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    find = true;
                                }
                                else if (line1.IndexOf("(" + line2 + ")", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    find = true;
                                }
                                else if (line1.IndexOf("[" + line2 + "]", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    find = true;
                                }
                                else if (line1.IndexOf("\"" + line2 + "\"", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    find = true;
                                }
                                else if (line1.IndexOf("'" + line2 + "'", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    find = true;
                                }
                                else if (line1.IndexOf("," + line2 + ",", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    find = true;
                                }
                                else if (line1.IndexOf(" " + line2 + ",", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    find = true;
                                }
                            }
                            if (find)
                            {
                                filesList.Add(line + "\t" + line2 + "\t" + linenum.ToString());
                                find = false;
                            }
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (temppath != null)
            {
                folderBrowserDialog1.SelectedPath = temppath;
            }
            else
            {
                folderBrowserDialog1.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            searchStrings.AddRange(textBox1.Lines);
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK && searchStrings.Count > 0)
            {
                if (Directory.Exists(folderBrowserDialog1.SelectedPath))
                {
                    button1.Enabled = false;
                    checkBox1.Enabled = false;
                    temppath = folderBrowserDialog1.SelectedPath;
                    getInFilesFinder(folderBrowserDialog1.SelectedPath);
                    searthFolder(folderBrowserDialog1.SelectedPath);
                    File.WriteAllLines(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\_foundlist.txt", filesList);
                    checkBox1.Enabled = true;
                    button1.Enabled = true;
                }
            }
            searchStrings.Clear();
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
    }
}

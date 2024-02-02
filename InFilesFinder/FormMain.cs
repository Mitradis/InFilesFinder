using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace InFilesFinder
{
    public partial class FormMain : Form
    {
        List<string> filesList = new List<string>();
        string logOut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "_foundlist.txt");
        byte[] searchBytesUpper;
        byte[] searchBytesLower;
        int searchLength = 0;

        public FormMain()
        {
            InitializeComponent();
        }

        void button1_Click(object sender, EventArgs e)
        {
            button1.Text = "Работает";
            button1.Enabled = false;
            textBox1.Enabled = false;
            checkBox1.Enabled = false;
            checkBox2.Enabled = false;
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK && Directory.Exists(folderBrowserDialog1.SelectedPath) && textBox1.Lines.Length > 0)
            {
                if (checkBox1.Checked)
                {
                    searchBytesUpper = Encoding.ASCII.GetBytes(textBox1.Text.ToUpper());
                    searchBytesLower = Encoding.ASCII.GetBytes(textBox1.Text.ToLower());
                    searchLength = searchBytesLower.Length;
                }
                searchFolder(folderBrowserDialog1.SelectedPath);
                try
                {
                    File.WriteAllLines(logOut, filesList);
                }
                catch
                {
                    MessageBox.Show("Не удалось записать файл: " + logOut);
                }
                searchBytesLower = null;
                searchLength = 0;
            }
            checkBox2.Enabled = true;
            checkBox1.Enabled = true;
            textBox1.Enabled = true;
            button1.Enabled = true;
            button1.Text = "Путь";
            filesList.Clear();
        }

        void searchFolder(string path)
        {
            getInFilesFinder(path);
            foreach (string line in getDirectories(path))
            {
                searchFolder(line);
            }
        }

        void getInFilesFinder(string path)
        {
            foreach (string file in getFiles(path))
            {
                if (getAccessFile(file))
                {
                    bool found = false;
                    if (checkBox1.Checked)
                    {
                        Stream st = File.OpenRead(file);
                        long length = 0;
                        int read = 0;
                        int pos = 0;
                        bool find = false;
                        bool skip = false;
                        while ((read = st.ReadByte()) != -1)
                        {
                            length++;
                            if (find)
                            {
                                if (skip)
                                {
                                    if (read == 0)
                                    {
                                        skip = false;
                                    }
                                    else
                                    {
                                        pos = 0;
                                        find = false;
                                    }
                                }
                                else
                                {
                                    if (read == searchBytesUpper[pos] || read == searchBytesLower[pos])
                                    {
                                        pos++;
                                        if (pos == searchLength)
                                        {
                                            pos = 0;
                                            find = false;
                                            filesList.Add(file + "\toffset: " + (length - searchLength * 2).ToString());
                                            found = true;
                                        }
                                        else
                                        {
                                            skip = true;
                                        }
                                    }
                                    else
                                    {
                                        pos = 0;
                                        find = false;
                                    }
                                }
                            }
                            else if (read == searchBytesUpper[0] || read == searchBytesLower[0])
                            {
                                pos++;
                                find = true;
                                skip = true;
                            }
                        }
                        st.Close();
                    }
                    else
                    {
                        string read;
                        int num = 0;
                        StreamReader sr = new StreamReader(file);
                        while ((read = sr.ReadLine()) != null)
                        {
                            num++;
                            foreach (string line in textBox1.Lines)
                            {
                                if (read.IndexOf(line, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    filesList.Add(file + "\tline: " + num.ToString());
                                    found = true;
                                }
                            }
                        }
                        sr.Close();
                    }
                    if (checkBox2.Checked && !found)
                    {
                        File.Delete(file);
                    }
                }
            }
        }

        bool getAccessFile(string path)
        {
            try
            {
                FileStream fs = File.OpenRead(path);
                fs.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        string[] getFiles(string path)
        {
            try
            {
                return Directory.GetFiles(path);
            }
            catch
            {
                return new string[] { };
            }
        }

        string[] getDirectories(string path)
        {
            try
            {
                return Directory.GetDirectories(path);
            }
            catch
            {
                return new string[] { };
            }
        }

        void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Multiline = !checkBox1.Checked;
        }

        void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                if (sender != null)
                {
                    ((TextBox)sender).SelectAll();
                }
            }
        }
    }
}

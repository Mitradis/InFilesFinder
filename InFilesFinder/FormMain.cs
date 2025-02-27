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
        List<byte[]> searchBytesUpper = new List<byte[]>();
        List<byte[]> searchBytesLower = new List<byte[]>();
        List<int> linesLength = new List<int>();
        string lastPath = null;
        int listCount = 0;

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
            checkBox3.Enabled = false;
            if (lastPath != null)
            {
                folderBrowserDialog1.SelectedPath = lastPath;
            }
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK && Directory.Exists(folderBrowserDialog1.SelectedPath) && textBox1.Lines.Length > 0)
            {
                lastPath = folderBrowserDialog1.SelectedPath;
                foreach (string line in textBox1.Lines)
                {
                    if (!String.IsNullOrEmpty(line))
                    {
                        if (checkBox2.Checked)
                        {
                            searchBytesUpper.Add(getBytes(line.ToUpper()));
                            searchBytesLower.Add(getBytes(line.ToLower(), true));
                        }
                        else
                        {
                            searchBytesUpper.Add(getBytes(line));
                        }
                    }
                }
                listCount = searchBytesUpper.Count;
                searchFolder(folderBrowserDialog1.SelectedPath);
                try
                {
                    File.WriteAllLines(logOut, filesList);
                }
                catch
                {
                    MessageBox.Show("Не удалось записать файл: " + logOut);
                }
                searchBytesUpper.Clear();
                searchBytesLower.Clear();
                linesLength.Clear();
            }
            checkBox3.Enabled = true;
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
                    byte[] bytesFile = null;
                    bool found = false;
                    try
                    {
                        bytesFile = File.ReadAllBytes(file);
                    }
                    catch (Exception e)
                    {
                        filesList.Add(file + "\tIGNORED(" + e.Message + ")");
                        return;
                    }
                    int fileSize = bytesFile.Length;
                    for (int i = 0; i < fileSize; i++)
                    {
                        for (int j = 0; j < listCount; j++)
                        {
                            for (int l = 0; l <= linesLength[j]; l++)
                            {
                                if (i + l >= fileSize || (checkBox2.Checked ? (bytesFile[i + l] != searchBytesUpper[j][l] && bytesFile[i + l] != searchBytesLower[j][l]) : bytesFile[i + l] != searchBytesUpper[j][l]))
                                {
                                    break;
                                }
                                if (l == linesLength[j])
                                {
                                    found = true;
                                    filesList.Add(file + "\toffset: " + i + "\t" + (checkBox1.Checked ? Encoding.Unicode.GetString(searchBytesUpper[j]) : Encoding.UTF8.GetString(searchBytesUpper[j])));
                                }
                            }
                        }
                    }
                    bytesFile = null;
                    if (checkBox3.Checked && !found)
                    {
                        File.Delete(file);
                    }
                }
            }
        }

        byte[] getBytes(string line, bool skip = false)
        {
            byte[] bytes = checkBox1.Checked ? Encoding.Unicode.GetBytes(line) : Encoding.UTF8.GetBytes(line);
            if (!skip)
            {
                linesLength.Add(bytes.Length - 1);
            }
            return bytes;
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

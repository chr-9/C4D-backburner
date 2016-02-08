using System;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace C4D_bb
{
    public partial class Form1 : Form
    {
        public string cmd = "";
        public string path = "";
        public string server_ip;
        public string server_group;
        public string server_c4dloc;
        public string server_count;
        public string tasklist = "";
        public string saveDir = "";

        public string fstr = "";
        public string rstr = "";

        public string fstep = "";
        public bool b_fstep = false;

        public bool mp = false;

        Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\c4dbb");
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            server_ip = (string)regkey.GetValue("server_ip", "");
            if (server_ip == "")
                server_ip = "192.168.0.12";
            server_group = (string)regkey.GetValue("server_group", "");
            if (server_group == "")
                server_group = "RENDER";
            server_c4dloc = (string)regkey.GetValue("server_c4dloc", "");
            if (server_c4dloc == "")
                server_c4dloc = @"C:\Program Files\MAXON\CINEMA 4D R14\CINEMA 4D 64 Bit.exe";
            server_count = (string)regkey.GetValue("server_count", "");
            if (server_count == "")
                server_count = "5";
            bb = (string)regkey.GetValue("bb_path", "");
            if (bb == "")
                bb = "C:\\Program Files (x86)\\Autodesk\\Backburner\\cmdjob.exe";

            fstr = (string)regkey.GetValue("fstr", "");
            if (fstr == "")
                fstr = " ";

            rstr = (string)regkey.GetValue("rstr", "");
            if (rstr == "")
                rstr = " ";

            if (!File.Exists(bb))
            {
                MessageBox.Show("cmdjob.exeが存在しません", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                OpenFileDialog d3 = new OpenFileDialog();
                d3.Filter = "cmdjob.exe|cmdjob.exe";
                d3.Title = "Select cmdjob.exe";
                d3.RestoreDirectory = true;

                if (d3.ShowDialog() == DialogResult.OK)
                {
                    bb = d3.FileName;
                    textBox10.Text = bb;
                }
                else
                {
                    Application.Exit();
                }
            }
            textBox10.Text = bb;

            textBox3.Text = server_ip;
            textBox5.Text = server_group;
            textBox7.Text = server_c4dloc;
            textBox8.Text = server_count;
            AcceptButton = button1;
            Size = new System.Drawing.Size(415, 465);

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 5)
            {
                if (!args[1].Contains(fstr))
                {
                    button5_Click(null, null);
                }

                if(args[4] == "1")
                {
                    mp = true;
                }

                path = args[1].Replace(fstr, rstr);
                textBox11.Text = path;
                textBox1.Text = Path.GetFileName(path).Replace(".c4d", "");
                saveDir = Path.GetDirectoryName(args[1]) + "\\results\\";
                saveDir = saveDir.Replace(fstr, rstr);
                textBox9.Text = saveDir;

                textBox4.Text = args[2];
                textBox6.Text = args[3];

                if (args[3].Contains("_"))
                {
                    // fstep enable
                    b_fstep = true;
                    fstep = args[3].Split('_')[1];
                    textBox6.Text = args[3].Split('_')[0];

                }
                else
                {
                    // disable
                    b_fstep = false;
                }


            }
            else
            {
                OpenFileDialog d = new OpenFileDialog();
                d.Filter = "Cinema4D Project(*.c4d)|*.c4d|All files(*.*)|*.*";
                d.Title = "Select Project";
                d.RestoreDirectory = true;

                if (d.ShowDialog() == DialogResult.OK)
                {
                    path = d.FileName.Replace(fstr, rstr);
                    textBox11.Text = path;
                    textBox1.Text = Path.GetFileName(path).Replace(".c4d", "");
                    saveDir = Path.GetDirectoryName(d.FileName).Replace(fstr, rstr) + "\\results\\";
                    textBox9.Text = saveDir;
                    ActiveControl = textBox4;
                }
                else
                {
                    Application.Exit();
                }
            }

            Text = "C4D Backburner - " + textBox1.Text+".c4d";

            Activate();
            toolStripStatusLabel1.Text = "";

            if (fstr == " " || rstr == " " || !rstr.StartsWith("\\\\"))
            {
                button5_Click(null,null);
            }
        }

        public string jobName = "";
        private void button1_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "";

            textBox4.BackColor = System.Drawing.Color.White;
            textBox6.BackColor = System.Drawing.Color.White;
            textBox9.BackColor = System.Drawing.Color.White;
            textBox11.BackColor = System.Drawing.Color.White;

            if (textBox4.Text == "")
            {
                System.Media.SystemSounds.Beep.Play();
                ActiveControl = textBox4;
                textBox4.BackColor = System.Drawing.Color.LightPink;
                return;
            }
            else if (textBox6.Text == "")
            {
                System.Media.SystemSounds.Beep.Play();
                ActiveControl = textBox6;
                textBox6.BackColor = System.Drawing.Color.LightPink;
                return;
            }

            server_ip = textBox3.Text;
            server_group = textBox5.Text;
            server_c4dloc = textBox7.Text;
            server_count = textBox8.Text;
            string priority = textBox2.Text;

            path = textBox11.Text;
            saveDir = textBox9.Text;

            jobName = textBox1.Text;

            int startFrame = int.Parse(textBox4.Text);
            int endFrame = int.Parse(textBox6.Text);

            if (!path.StartsWith("\\\\"))
            {
                System.Media.SystemSounds.Beep.Play();
                ActiveControl = textBox11;
                textBox11.BackColor = System.Drawing.Color.LightPink;
                toolStripStatusLabel1.Text = "ネットワークパスを指定して下さい";
                return;
            }

            if (!saveDir.StartsWith("\\\\"))
            {
                System.Media.SystemSounds.Beep.Play();
                ActiveControl = textBox9;
                textBox9.BackColor = System.Drawing.Color.LightPink;
                toolStripStatusLabel1.Text = "ネットワークパスを指定して下さい";
                return;
            }

            genTasklist(startFrame, endFrame, int.Parse(server_count), jobName);

            cmd = "";
            addCmd("jobname \"" + jobName + "\"");
            addCmd("manager " + server_ip);
            addCmd("priority " + priority);
            addCmd("serverCount " + server_count);
            addCmd("group \"" + server_group + "\"");
            #if DEBUG
                addCmd("suspended");
            #endif

            if (!Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);

            addCmd("taskList \"" + Path.GetDirectoryName(path) + "\\" + jobName + "_tasklist.txt" + "\" -taskname 1");
            if (mp)
            {
                // マルチパス有効
                if (b_fstep)
                {
                    cmd += "\"" + server_c4dloc + "\" -nogui -render \"" + path + "\" -frame %tp2 %tp3 " + fstep + "-oimage \"" + saveDir + jobName + "\" -omultipass \"" + saveDir + jobName + "\"";
                }
                else
                {
                    cmd += "\"" + server_c4dloc + "\" -nogui -render \"" + path + "\" -frame %tp2 %tp3 -oimage \"" + saveDir + jobName + "\" -omultipass \"" + saveDir + jobName + "\"";
                }
            }
            else
            {
                if (b_fstep)
                {
                    cmd += "\"" + server_c4dloc + "\" -nogui -render \"" + path + "\" -frame %tp2 %tp3 " + fstep + " -oimage \"" + saveDir + jobName + "\"";
                }
                else
                {
                    cmd += "\"" + server_c4dloc + "\" -nogui -render \"" + path + "\" -frame %tp2 %tp3 -oimage \"" + saveDir + jobName + "\"";
                }
            }

            StreamWriter taskw = new StreamWriter(Path.GetDirectoryName(path) + "\\" + jobName + "_tasklist.txt", false, Encoding.GetEncoding("Shift_JIS"));
            taskw.WriteLine(tasklist);
            taskw.Close();

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = false;
            p.StartInfo.RedirectStandardOutput = false;
            p.StartInfo.FileName = bb;
            p.StartInfo.Arguments = cmd;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.WaitForExit();
            p.Close();

#if DEBUG
                  MessageBox.Show("Send: \n" + cmd, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
#endif

            regkey.SetValue("server_ip", server_ip);
            regkey.SetValue("server_group", server_group);
            regkey.SetValue("server_c4dloc", server_c4dloc);
            regkey.SetValue("server_count", server_count);
            regkey.SetValue("bb_path", bb);

            if (fstr == " ")
                fstr = "";
            if (rstr == " ")
                rstr = "";

            regkey.SetValue("fstr", fstr);
            regkey.SetValue("rstr", rstr);
            Application.Exit();
        }

        private void genTasklist(int start, int end, int count, string job)
        {
            if (end >= count)
            {
                // 通常
                double pt = (end - start + 0.00000) / count;
                int pt_ce = (int)Math.Round(pt) - 1;

                int pt_tmp = start;
                tasklist = "";
                for (int i = 0; i < count - 1; i++)
                {
                    tasklist += job + "_" + pt_tmp + "-" + (pt_tmp + pt_ce) + "\t" + pt_tmp + "\t" + (pt_tmp + pt_ce) + "\r\n";
                    pt_tmp += pt_ce + 1;
                }

                tasklist += job + "_" + pt_tmp + "-" + end + "\t" + pt_tmp + "\t" + end;
            }
            else
            {
                // フレーム数<サーバー台数
                count = end;
                int pt_tmp = start;

                tasklist = "";
                for (int i = 0; i < count+1; i++)
                {
                    tasklist += job + "_" + pt_tmp + "\t" + pt_tmp + "\t" + pt_tmp + "\r\n";
                    pt_tmp += 1;
                }
            }

            /*for (int i = start; i < end; i++)
            {
                tasklist += "frame" + i + "-" + (i+1) + "\t" + i + "\t" + (i + 1) + "\r\n";
            }*/
            return;
        }

        private void addCmd(string _cmd)
        {
            cmd += "-" + _cmd + " ";
            return;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.Description = "出力先";

            folderBrowserDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
            folderBrowserDialog1.SelectedPath = Path.GetDirectoryName(path);
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                saveDir = folderBrowserDialog1.SelectedPath+"\\";
                textBox9.Text = saveDir;
            }
            folderBrowserDialog1.Dispose();

            return;
        }

        public string bb = "";
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "cmdjob.exe|cmdjob.exe";
            d.Title = "Select cmdjob.exe";
            d.RestoreDirectory = true;

            if (d.ShowDialog() == DialogResult.OK)
            {
                bb = d.FileName;
                textBox10.Text = bb;
            }
            return;
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                System.Media.SystemSounds.Beep.Play();
                e.Handled = true;
            }
        }

        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                System.Media.SystemSounds.Beep.Play();
                e.Handled = true;
            }

        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                System.Media.SystemSounds.Beep.Play();
                e.Handled = true;
            }
        }

        private void textBox8_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                System.Media.SystemSounds.Beep.Play();
                e.Handled = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "Cinema4D Project(*.c4d)|*.c4d|All files(*.*)|*.*";
            d.Title = "Select Project";
            d.RestoreDirectory = true;

            if (d.ShowDialog() == DialogResult.OK)
            {
                path = d.FileName.Replace(fstr, rstr);
                textBox11.Text = path;
                textBox1.Text = Path.GetFileName(path).Replace(".c4d", "");
                saveDir = Path.GetDirectoryName(d.FileName).Replace(fstr, rstr) + "\\results\\";
                textBox9.Text = saveDir;
            }
        }

        private bool b5 = false;
        private void button5_Click(object sender, EventArgs e)
        {
            if (!b5)
            {
                textBox12.Text = fstr;
                textBox13.Text = rstr;
                label14.Text = saveDir;

                if (fstr == " ")
                    textBox12.Text = "";
                if (rstr == " ")
                    textBox13.Text = "";

                button5.Text = "<<<";
                Size = new System.Drawing.Size(700, 465);
                button1.Enabled = false;
                textBox11.Enabled = false;
                button4.Enabled = false;
                textBox1.Enabled = false;
                textBox4.Enabled = false;
                textBox6.Enabled = false;
                textBox9.Enabled = false;
                button2.Enabled = false;
                textBox2.Enabled = false;
                groupBox1.Enabled = false;
                textBox10.Enabled = false;
                button3.Enabled = false;
                label11.Enabled = false;
                label1.Enabled = false;
                label3.Enabled = false;
                label4.Enabled = false;
                label9.Enabled = false;
                label2.Enabled = false;
                label10.Enabled = false;

                b5 = true;
            }
            else
            {
                textBox12.BackColor = System.Drawing.Color.White;
                textBox13.BackColor = System.Drawing.Color.White;
                toolStripStatusLabel1.Text = "";

                fstr = textBox12.Text;
                rstr = textBox13.Text;

                label14.Text = saveDir;

                if (fstr == "")
                {
                    System.Media.SystemSounds.Beep.Play();
                    ActiveControl = textBox12;
                    textBox12.BackColor = System.Drawing.Color.LightPink;
                    return;
                }

                if (rstr == "")
                {
                    System.Media.SystemSounds.Beep.Play();
                    ActiveControl = textBox13;
                    textBox13.BackColor = System.Drawing.Color.LightPink;

                    return;
                }

                if (!rstr.StartsWith("\\\\"))
                {
                    System.Media.SystemSounds.Beep.Play();
                    ActiveControl = textBox13;
                    textBox13.BackColor = System.Drawing.Color.LightPink;
                    toolStripStatusLabel1.Text = "ネットワークパスを指定して下さい";
                    return;
                }

                textBox12.BackColor = System.Drawing.Color.White;
                textBox13.BackColor = System.Drawing.Color.White;

                path = path.Replace(fstr, rstr);
                textBox11.Text = path;
                saveDir = saveDir.Replace(fstr, rstr);
                textBox9.Text = saveDir;

                button1.Enabled = true;
                textBox11.Enabled = true;
                button4.Enabled = true;
                textBox1.Enabled = true;
                textBox4.Enabled = true;
                textBox6.Enabled = true;
                textBox9.Enabled = true;
                button2.Enabled = true;
                textBox2.Enabled = true;
                groupBox1.Enabled = true;
                textBox10.Enabled = true;
                button3.Enabled = true;
                label11.Enabled = true;
                label1.Enabled = true;
                label3.Enabled = true;
                label4.Enabled = true;
                label9.Enabled = true;
                label2.Enabled = true;
                label10.Enabled = true;

                button5.Text = "置換設定 >>>";
                Size = new System.Drawing.Size(415, 465);
                b5 = false;
                ActiveControl = textBox4;
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            server_ip = textBox3.Text;
            server_group = textBox5.Text;
            server_c4dloc = textBox7.Text;
            server_count = textBox8.Text;
            bb = textBox10.Text;

            regkey.SetValue("server_ip", server_ip);
            regkey.SetValue("server_group", server_group);
            regkey.SetValue("server_c4dloc", server_c4dloc);
            regkey.SetValue("server_count", server_count);
            regkey.SetValue("bb_path", bb);

            if (fstr == " ")
                fstr = "";
            if (rstr == " ")
                rstr = "";

            regkey.SetValue("fstr", fstr);
            regkey.SetValue("rstr", rstr);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox12.BackColor = System.Drawing.Color.White;
            textBox13.BackColor = System.Drawing.Color.White;

            fstr = textBox12.Text;
            rstr = textBox13.Text;

            if (fstr == "")
            {
                System.Media.SystemSounds.Beep.Play();
                ActiveControl = textBox12;
                textBox12.BackColor = System.Drawing.Color.LightPink;
                return;

            }

            if (rstr == "")
            {
                System.Media.SystemSounds.Beep.Play();
                ActiveControl = textBox13;
                textBox13.BackColor = System.Drawing.Color.LightPink;
                return;
            }

            if (!rstr.StartsWith("\\\\"))
            {
                System.Media.SystemSounds.Beep.Play();
                ActiveControl = textBox13;
                textBox13.BackColor = System.Drawing.Color.LightPink;
                toolStripStatusLabel1.Text = "ネットワークパスを指定して下さい";

                return;
            }

            textBox12.BackColor = System.Drawing.Color.White;
            textBox13.BackColor = System.Drawing.Color.White;

            path = path.Replace(fstr, rstr);
            textBox11.Text = path;
            saveDir = saveDir.Replace(fstr, rstr);
            textBox9.Text = saveDir;

            button1.Enabled = true;
            textBox11.Enabled = true;
            button4.Enabled = true;
            textBox1.Enabled = true;
            textBox4.Enabled = true;
            textBox6.Enabled = true;
            textBox9.Enabled = true;
            button2.Enabled = true;
            textBox2.Enabled = true;
            groupBox1.Enabled = true;
            textBox10.Enabled = true;
            button3.Enabled = true;
            label11.Enabled = true;
            label1.Enabled = true;
            label3.Enabled = true;
            label4.Enabled = true;
            label9.Enabled = true;
            label2.Enabled = true;
            label10.Enabled = true;

            ActiveControl = textBox4;

            button5.Text = "置換設定 >>>";
            Size = new System.Drawing.Size(415, 465);
            b5 = false;
        }

        private void textBox13_TextChanged(object sender, EventArgs e)
        {
            label14.Text = saveDir.Replace(textBox12.Text, textBox13.Text);
        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            label14.Text = saveDir.Replace(textBox12.Text, textBox13.Text);
        }
    }
}

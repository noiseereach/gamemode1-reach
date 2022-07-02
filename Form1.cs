using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Memory;


//https://www.youtube.com/c/NoiseReach

namespace testgamemode
{
    public partial class Form1 : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,    
            int nBottomRect,   
            int nWidthEllipse, 
            int nHeightEllipse 
        );
        [DllImport("user32.dll")] public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        const int OPEN_BUTTON = 0x401;
        const int OFF_BUTTON = 0x402;
        string gamemode_address;
        int GAMEMODE_CREATIVE = 1;
        int GAMEMODE_SURVIVAL = 0;
        int current_gamemode = 0;
        public Mem m = new Mem();
        public void creativeOn()
        {
            string mask = "00 00 00 0? 00 00 00 ?? ?? ?? E2 ?? ?? ?? E3 ?? ?? ?? E? ?? ?? ?? ?? ?? ?? ?? E? 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? 0? 00 00 00 00 00 00 00 ?? ?? ?? F8";
            bool checkprocesses = m.OpenProcess("javaw");
            if (checkprocesses == false)
            {
                scanlabel.Text = "game not found";
                MessageBox.Show("Oyun bulunamadı (error: pid 0)", "noise error");
                return;
            }
            scanlabel.Text = "scan starting..";
            Task<IEnumerable<long>> task = m.AoBScan(mask, true, false);
            task.Wait();
            IEnumerable<long> pack = task.Result;
            for (int i = 0; i < pack.Count(); i++)
            {
                gamemode_address = string.Format("{0:X}", pack.ElementAt(i));
                valuelabel.Text = "" + gamemode_address;
                byte[] buffer = m.ReadBytes(gamemode_address, 30);
                buffer[23] = ((byte)(buffer[23] + 4));
                m.WriteBytes(gamemode_address, buffer);
                scanlabel.Text = "scan completed";
                current_gamemode = GAMEMODE_CREATIVE;
            }
        }

        public void changeGamemode()
        {
            if (m == null) return;
            if (gamemode_address == null)
            {
                MessageBox.Show("gamemode_adress bulunamadi hileyi aktif et");
                return;
            }
            if (gamemode_address == "") return;

            if (current_gamemode == GAMEMODE_CREATIVE)
            {
                bool checkprocesses = m.OpenProcess("javaw");
                if (checkprocesses == false)
                {
                    scanlabel.Text = "game not found";
                    MessageBox.Show("Oyun bulunamadı (error: pid 0)", "noise error");
                    return;
                }
                valuelabel.Text = "" + gamemode_address;
                byte[] buffer = m.ReadBytes(gamemode_address, 30);
                buffer[23] = ((byte)(buffer[23] - 4));
                m.WriteBytes(gamemode_address, buffer);
                scanlabel.Text = "mode changed (0)";
                current_gamemode = GAMEMODE_SURVIVAL;
                return;
            }

            if (current_gamemode == GAMEMODE_SURVIVAL)
            {
                bool checkprocesses = m.OpenProcess("javaw");
                if (checkprocesses == false)
                {
                    scanlabel.Text = "game not found";
                    MessageBox.Show("Oyun bulunamadı (error: pid 0)", "noise error");
                    return;
                }
                valuelabel.Text = "" + gamemode_address;
                byte[] buffer = m.ReadBytes(gamemode_address, 30);
                buffer[23] = ((byte)(buffer[23] + 4));
                m.WriteBytes(gamemode_address, buffer);
                scanlabel.Text = "mode changed (1)";
                current_gamemode = GAMEMODE_CREATIVE;
                return;
            }

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(changeGamemode);
            th.Start();
        }

        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 28, 28));
            Control.CheckForIllegalCrossThreadCalls = false;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            RegisterHotKey(this.Handle, OPEN_BUTTON, 2, (int)Keys.F);
            RegisterHotKey(this.Handle, OFF_BUTTON, 2, (int)Keys.X);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }


        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x84:
                    base.WndProc(ref m);
                    if ((int)m.Result == 0x1)
                        m.Result = (IntPtr)0x2;
                    return;
            }

            if (m.Msg == 0x0312 && m.WParam.ToInt32() == OPEN_BUTTON)
            {
                guna2Button1.PerformClick();
            }

            if (m.Msg == 0x0312 && m.WParam.ToInt32() == OFF_BUTTON)
            {
                guna2Button2.PerformClick();
            }

            base.WndProc(ref m);
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            Thread thh = new Thread(creativeOn);
            thh.Start();
        }
    }
}

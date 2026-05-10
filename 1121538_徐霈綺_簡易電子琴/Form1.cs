using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _1121538_徐霈綺_簡易電子琴
{
    public partial class Form1 : Form
    {
        [DllImport("winmm.dll")]
        private static extern int midiOutOpen(ref IntPtr handle, int deviceID, IntPtr proc, IntPtr instance, int flags);

        [DllImport("winmm.dll")]
        private static extern int midiOutClose(IntPtr handle);

        [DllImport("winmm.dll")]
        private static extern int midiOutShortMsg(IntPtr handle, int message);

        private IntPtr midiOutHandle;

        public Form1()
        {
            InitializeComponent();
            midiOutOpen(ref midiOutHandle, 0, IntPtr.Zero, IntPtr.Zero, 0);
            InitializePiano();
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            midiOutClose(midiOutHandle);
        }

        private void InitializePiano()
        {
            this.Text = "簡易電子琴";
            this.Size = new Size(900, 300);
            this.BackColor = Color.White;
            this.StartPosition = FormStartPosition.CenterScreen;

            int whiteKeyWidth = 40;
            int whiteKeyHeight = 200;
            int blackKeyWidth = 26;
            int blackKeyHeight = 120;
            int startX = 15;
            int startY = 20;

            string[] whiteKeyNames = { "C", "D", "E", "F", "G", "A", "B" };
            int[] whiteKeyOffsets = { 0, 2, 4, 5, 7, 9, 11 };

            string[] blackKeyNames = { "升C\n降D", "升D\n降E", "升F\n降G", "升G\n降A", "升A\n降B" };
            int[] blackKeyOffsets = { 1, 3, 6, 8, 10 };
            int[] blackKeyIndices = { 0, 1, 3, 4, 5 }; // 黑鍵跟隨在哪個白鍵後面

            int startNote = 60; // 從中央C (C4) 開始
            List<Button> blackKeys = new List<Button>();

            for (int oct = 0; oct < 3; oct++)
            {
                // 建立白鍵
                for (int i = 0; i < 7; i++)
                {
                    Button wBtn = new Button();
                    wBtn.Text = whiteKeyNames[i];
                    wBtn.TextAlign = ContentAlignment.BottomCenter;
                    wBtn.Padding = new Padding(0, 0, 0, 10);
                    wBtn.Size = new Size(whiteKeyWidth, whiteKeyHeight);
                    wBtn.Location = new Point(startX + (oct * 7 + i) * whiteKeyWidth, startY);
                    wBtn.BackColor = Color.White;
                    wBtn.Tag = startNote + oct * 12 + whiteKeyOffsets[i];
                    wBtn.MouseDown += Btn_MouseDown;
                    wBtn.MouseUp += Btn_MouseUp;
                    wBtn.Font = new Font("Arial", 10);
                    this.Controls.Add(wBtn);
                }

                // 建立黑鍵
                for (int i = 0; i < 5; i++)
                {
                    Button bBtn = new Button();
                    bBtn.Text = blackKeyNames[i];
                    bBtn.TextAlign = ContentAlignment.BottomCenter;
                    bBtn.ForeColor = Color.White;
                    bBtn.BackColor = Color.Black;
                    bBtn.Size = new Size(blackKeyWidth, blackKeyHeight);
                    int whiteLeft = startX + (oct * 7 + blackKeyIndices[i]) * whiteKeyWidth;
                    bBtn.Location = new Point(whiteLeft + whiteKeyWidth - (blackKeyWidth / 2), startY);
                    bBtn.Tag = startNote + oct * 12 + blackKeyOffsets[i];
                    bBtn.MouseDown += Btn_MouseDown;
                    bBtn.MouseUp += Btn_MouseUp;
                    bBtn.Font = new Font("Microsoft JhengHei", 7);
                    blackKeys.Add(bBtn);
                    this.Controls.Add(bBtn);
                }
            }

            // 確保黑鍵顯示在最上層
            foreach (var bBtn in blackKeys)
            {
                bBtn.BringToFront();
            }
        }

        private void Btn_MouseDown(object sender, MouseEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                int note = (int)btn.Tag;
                int msg = 0x090 | (note << 8) | (127 << 16); // Note On 訊息 (發聲)
                midiOutShortMsg(midiOutHandle, msg);
            }
        }

        private void Btn_MouseUp(object sender, MouseEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                int note = (int)btn.Tag;
                int msg = 0x080 | (note << 8) | (0 << 16); // Note Off 訊息 (停止)
                midiOutShortMsg(midiOutHandle, msg);
            }
        }
    }
}

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
        private Dictionary<int, Button> pianoKeys = new Dictionary<int, Button>();
        private Dictionary<Button, Color> originalColors = new Dictionary<Button, Color>();

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
            this.Size = new Size(880, 320);
            this.BackColor = Color.White;
            this.StartPosition = FormStartPosition.CenterScreen;

            Button autoPlayBtn = new Button();
            autoPlayBtn.Text = "自動演奏：小星星";
            autoPlayBtn.Size = new Size(150, 40);
            autoPlayBtn.Location = new Point(15, 230);
            autoPlayBtn.Click += AutoPlayBtn_Click;
            this.Controls.Add(autoPlayBtn);

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

            int startNote = 48; // 從C3開始，讓中央C (C4) 落在中間的八度
            List<Button> blackKeys = new List<Button>();

            for (int oct = 0; oct < 3; oct++)
            {
                // 建立白鍵
                for (int i = 0; i < 7; i++)
                {
                    Button wBtn = new Button();
                    int note = startNote + oct * 12 + whiteKeyOffsets[i];

                    if (note == 60) // 標示中央C (C4)
                    {
                        wBtn.Text = whiteKeyNames[i] + "\n(中央Do)";
                        wBtn.BackColor = Color.LightYellow;
                    }
                    else
                    {
                        wBtn.Text = whiteKeyNames[i];
                        wBtn.BackColor = Color.White;
                    }

                    wBtn.TextAlign = ContentAlignment.BottomCenter;
                    wBtn.Padding = new Padding(0, 0, 0, 10);
                    wBtn.Size = new Size(whiteKeyWidth, whiteKeyHeight);
                    wBtn.Location = new Point(startX + (oct * 7 + i) * whiteKeyWidth, startY);
                    wBtn.Tag = note;
                    wBtn.MouseDown += Btn_MouseDown;
                    wBtn.MouseUp += Btn_MouseUp;
                    wBtn.Font = new Font("Arial", 10);
                    this.Controls.Add(wBtn);

                    pianoKeys[note] = wBtn;
                    originalColors[wBtn] = wBtn.BackColor;
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

                    int bNote = (int)bBtn.Tag;
                    pianoKeys[bNote] = bBtn;
                    originalColors[bBtn] = bBtn.BackColor;
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

        private async void AutoPlayBtn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            btn.Enabled = false;

            int[] melody = {
                60, 60, 67, 67, 69, 69, 67, // Do Do Sol Sol La La Sol
                65, 65, 64, 64, 62, 62, 60, // Fa Fa Mi Mi Re Re Do
                67, 67, 65, 65, 64, 64, 62, // Sol Sol Fa Fa Mi Mi Re
                67, 67, 65, 65, 64, 64, 62, // Sol Sol Fa Fa Mi Mi Re
                60, 60, 67, 67, 69, 69, 67, // Do Do Sol Sol La La Sol
                65, 65, 64, 64, 62, 62, 60  // Fa Fa Mi Mi Re Re Do
            };

            int[] durations = {
                500, 500, 500, 500, 500, 500, 1000,
                500, 500, 500, 500, 500, 500, 1000,
                500, 500, 500, 500, 500, 500, 1000,
                500, 500, 500, 500, 500, 500, 1000,
                500, 500, 500, 500, 500, 500, 1000,
                500, 500, 500, 500, 500, 500, 1000
            };

            for (int i = 0; i < melody.Length; i++)
            {
                int note = melody[i];
                int msgOn = 0x090 | (note << 8) | (127 << 16);
                midiOutShortMsg(midiOutHandle, msgOn);

                Button keyBtn = null;
                if (pianoKeys.TryGetValue(note, out keyBtn))
                {
                    keyBtn.BackColor = originalColors[keyBtn] == Color.Black ? Color.DimGray : Color.Gray;
                }

                await Task.Delay(durations[i] - 50); // 扣掉些微時間做為音符間距

                int msgOff = 0x080 | (note << 8) | (0 << 16);
                midiOutShortMsg(midiOutHandle, msgOff);

                if (keyBtn != null)
                {
                    keyBtn.BackColor = originalColors[keyBtn]; // 恢復原色
                }

                await Task.Delay(50);
            }

            btn.Enabled = true;
        }
    }
}

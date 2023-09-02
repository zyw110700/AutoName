using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection.Emit;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Runtime.InteropServices;
using NPinyin;
using System.Media;
using System.Diagnostics;


namespace AutoName
{
    public partial class Form1 : Form

    {
        private List<string> names = new List<string>();
        private Point mouseOffset;
        private bool isMouseDown = false;

        public Form1()
        {
            // 从文本文件中读取名字
            ReadNamesFromFile("names.txt");
            // 初始化随机数种子
            InitializeComponent();


        }

        private void ReadNamesFromFile(string filename) //读取文件
        {
            try
            {
                string[] lines = File.ReadAllLines(filename);
                names.AddRange(lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开文件 {filename}：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }





        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.MouseDown += pictureBox1_MouseDown;
            pictureBox1.MouseMove += pictureBox1_MouseMove;
            pictureBox1.MouseUp += pictureBox1_MouseUp;

        }

        private void PickAndDisplayRandomName() //抽取
        {
            // 检查是否有可用的名字
            if (names.Count == 0)
            {
                MessageBox.Show("没有找到名字", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 生成随机索引
            Random rand = new Random();
            int randomIndex = rand.Next(names.Count);
            // 将名字显示在 Label 控件中
            label1.Text = names[randomIndex];


            string name = names[randomIndex];
            string piny = Pinyin.GetPinyin(name);
            string outputDirectory = "sources";


            //播放名字音频
            foreach (char character in name)
            {
                string pinyin = Pinyin.GetPinyin(character.ToString());
                string fileName = $"{pinyin}.wav";
                string filePath = Path.Combine(outputDirectory, fileName);

                // 在这里需要根据实际情况获取对应的音频文件路径
                // 例如，如果使用文件名匹配的方式，可以根据 pinyin 获取对应的音频文件

                if (File.Exists(filePath))
                {
                    try
                    {
                        using (SoundPlayer player = new SoundPlayer(filePath))
                        {
                            player.PlaySync(); // 播放声音
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error playing sound: " + ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine($"Audio file not found for: {character}");
                }
            }


        }
        private void label1_Click(object sender, EventArgs e)
        {

        }


        public class KeyboardHook
        {
            private const int WH_KEYBOARD_LL = 13;
            private const int WM_KEYDOWN = 0x0100;
            private const int VK_C = 0x43;

            private IntPtr hookId = IntPtr.Zero;
            private LowLevelKeyboardProc hookCallback;

            public event EventHandler CKeyPressed;

            public void Start()
            {
                hookCallback = HookCallback;
                using (Process process = Process.GetCurrentProcess())
                using (ProcessModule module = process.MainModule)
                {
                    IntPtr hModule = GetModuleHandle(module.ModuleName);
                    hookId = SetWindowsHookEx(WH_KEYBOARD_LL, hookCallback, hModule, 0);
                }
            }

            public void Stop()
            {
                UnhookWindowsHookEx(hookId);
            }

            private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    if (vkCode == VK_C)
                    {
                        CKeyPressed?.Invoke(this, EventArgs.Empty);
                    }
                }

                return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            }

            private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string lpModuleName);
        }

        public class ProgramWindow
        {
            [DllImport("user32.dll")]
            private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            private const int SW_SHOW = 5;

            public void Show()
            {
                var handle = Process.GetCurrentProcess().MainWindowHandle;
                ShowWindow(handle, SW_SHOW);
            }
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // 记录鼠标按下时的位置和PictureBox的位置
            if (e.Button == MouseButtons.Left)
            {
                int xOffset = -e.X - SystemInformation.FrameBorderSize.Width;
                int yOffset = -e.Y - SystemInformation.CaptionHeight -
                    SystemInformation.FrameBorderSize.Height;
                mouseOffset = new Point(xOffset, yOffset);
                isMouseDown = true;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            // 移动窗口和PictureBox
            if (isMouseDown)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                Location = mousePos;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            // 清除记录的位置信息
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PickAndDisplayRandomName();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Close();
        }
    }
}


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

namespace FeribsFisherboat
{
    public partial class Form1 : Form
    {
        //This was used to keep track of all the game clients that are open
        private uint[] FishermansCave = new uint[64];
        private int[] FisherbotProcc = new int[64];
        private IntPtr[] ThreadList = new IntPtr[64];
        private int AmountOfFishermans = 0;

        //Ignore the imports, i shold have done this diffrent
        #region LoadDLL
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess,
          int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress,
          byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        const int PROCESS_WM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_OPERATION = 0x0008;

        [DllImport("kernel32.dll")]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId);

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000,
            All = 0x001F0FFF
        }

        [Flags]
        public enum AllocationType
        {
            Commit = 0x00001000,
            Reserve = 0x00002000,
            Decommit = 0x00004000,
            Release = 0x00008000,
            Reset = 0x00080000,
            TopDown = 0x00100000,
            WriteWatch = 0x00200000,
            Physical = 0x00400000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            NoAccess = 0x0001,
            ReadOnly = 0x0002,
            ReadWrite = 0x0004,
            WriteCopy = 0x0008,
            Execute = 0x0010,
            ExecuteRead = 0x0020,
            ExecuteReadWrite = 0x0040,
            ExecuteWriteCopy = 0x0080,
            GuardModifierflag = 0x0100,
            NoCacheModifierflag = 0x0200,
            WriteCombineModifierflag = 0x0400
        }

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32")]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //This searches for World of Warcraft (Wow)
            string Target = "Wow";
            Process[] process = Process.GetProcessesByName(Target);

            dataGridView1.Rows.Clear();

            int i = 0;
            foreach(Process proc in process)
            {
                IntPtr hHandle = OpenProcess(0x1F0FFF, false, proc.Id);
                int bytesWritten = 0;
                int Data = (int)(proc.MainModule.BaseAddress + Offsets.LocalPlayerName);
                byte[] buffer = new byte[12];
                ReadProcessMemory((int)hHandle, (int)Data, buffer, buffer.Length, ref bytesWritten);
                string[] name = System.Text.Encoding.UTF8.GetString(buffer).Split(' ');
                dataGridView1.Rows.Add(false, System.Text.Encoding.UTF8.GetString(buffer), process[i].Id, "Testing",i);
                i++;
            }
        }

        private void DeployFishermansCave(int iProcessId)
        {
            //This is used to write the code cave into memory
            Process process = Process.GetProcessById(iProcessId);
            IntPtr hHandle = OpenProcess(0x1F0FFF, false, iProcessId);


            #region ASMcode
            
            byte[] asm = new byte[] {
                0x55,                                                               //Push ebp
                0x8B, 0xEC,                                                         //mov ebp,esp
                0xB8, 0xDE, 0xAD, 0xBE, 0xEF,                                       //mov eax,DataMem
                0xC7, 0x80, 0xF8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,         //mov [eax+000000F8],00000000
                0x50,                                                               //push eax
                0x6A, 0x00,                                                         //push 00
                0x8D, 0x58, 0x24,                                                   //lea ebx,[eax+24]
                0x53,                                                               //push ebx
                0x8D, 0x58, 0x14,                                                   //lea ebx,[eax+14]        
                0x53,                                                               //push ebx
                0x6A, 0x00,                                                         //push 00
                0x8B, 0x58, 0x10,                                                   //mov ebx,[eax+10]
                0x53,                                                               //push ebx
                0x8B, 0x58, 0x04,                                                   //mov ebx,[eax+04]
                0x8B, 0x1B,                                                         //mov ebx,[ebx]
                0x53,                                                               //push ebx
                0x8B, 0x18,                                                         //mov ebx,[eax]
                0xFF, 0xD3,                                                         //call ebx
                0x83, 0xC4, 0x18,                                                   //add esp,18
                0x68, 0x40, 0x06, 0x00, 0x00,                                       //push 00000640
                0xE8, 0xDE, 0xAD, 0xBE, 0xEF,                                       //call KERNEL32.Sleep       +0x36
                0x58,                                                               //pop eax
                0x8B, 0x58, 0x08,                                                   //mov ebx,[eax+08]
                0x8B, 0x1B,                                                         //mov ebx,[ebx]
                0x8B, 0x5B, 0x0C,                                                   //mov ebx,[ebx+0C]
                0x8B, 0x5B, 0x44,                                                   //mov ebx,[ebx+44]
                0x8B, 0xCB,                                                         //mov ecx,ebx
                0x8B, 0x51, 0x10,                                                   //mov edx,[ecx+10]
                0x81, 0xFA, 0x00, 0x02, 0x00, 0x00,                                 //cmp edx,00000200
                0x0F, 0x87, 0x62, 0x00, 0x00, 0x00,                                 //ja halloc + 0xB9
                0x83, 0xFA, 0x05,                                                   //edx,05
                0x75, 0xE7,                                                         //jne halloc + 0x43
                0x8B, 0x8B, 0x6C, 0x02, 0x00, 0x00,                                 //mov ecx,[ebx+0000026C]
                0x8B, 0x89, 0xB4, 0x00, 0x00, 0x00,                                 //mov ecx,[ecx+000000B4]
                0x8B, 0x09,                                                         //mov ecx,[ecx]
                0x3B, 0x88, 0xFC, 0x00, 0x00, 0x00,                                 //cmp ecx,[eax+000000FC]
                0x75, 0xD1,                                                         //jne halloc + 0x43

                0x50,                                                               //push eax
                0x8B, 0x8B, 0x28, 0x03, 0x00, 0x00,                                 //mov ecx,[ebx+00000328]
                0x8B, 0x90, 0xF4, 0x00, 0x00, 0x00,                                 //edx,[eax+000000F4]
                0x3B, 0x0A,                                                         //cmp ecx,[edx]
                0x58,                                                               //pop eax
                0x75, 0xBF,                                                         //jne halloc + 0x43

                0xB9, 0x00, 0x00, 0x00, 0x00,                                       //mov ecx,00000000
                0x41,                                                               //inc ecx        <--------------------------------------#
                0x83, 0xF9, 0x42,                                                   //cmp ecx,42                                            |
                0x0F, 0x84, 0x26, 0x00, 0x00, 0x00,                                 //je hAlloc + A9                Exit:                   |
                0x50,                                                               //push eax                                              |
                0x53,                                                               //push ebx                                              |
                0x51,                                                               //push ecx                                              |
                0x68, 0x60, 0x01, 0x00, 0x00,                                       //push 00000160                                         |
                0xE8, 0xDE, 0xAD, 0xBE, 0xEF,                                       //call KERNEL32.Sleep         +0x9C                     |
                0x59,                                                               //pop ecx                                               |
                0x5B,                                                               //pop ebx                                               |
                0x58,                                                               //pop eax                                               |
                0x8B, 0x93, 0xF8, 0x00, 0x00, 0x00,                                 //mov edx,[ebx+000000F8]                                |
                0x80, 0xFA, 0x01,                                                   //cmp dl,01                                             |
                0x75, 0xDB,                                                         //jne --------------------------------------------------#

                0x8B, 0xD3,                                                         //mov edx,ebx
                0x83, 0xC2, 0x30,                                                   //add edx,30
                0x52,                                                               //push edx
                0x8B, 0x50, 0x0C,                                                   //mov edx,[eax+0C]
                0xFF, 0xD2,                                                         //call edx
                //0xC7, 0x80, 0xF4, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,         //mov [eax+000000F4],00000001
                //0xFF, 0x80, 0xF8, 0x00, 0x00, 0x00,                                 //inc [eax+000000F8]
                0x8B, 0xE5,                                                         //mov esp,ebp
                0x5D,                                                               //pop ebp
                0xC3,                                                               //Ret   
                0xBE, 0x83, 0x54, 0x00,                                             ///Spell_C_CastSpell() func 
                0x50, 0x99, 0x0D, 0x01,                                             ///PlayerBase Ptr 
                0x5C, 0xD2, 0x03, 0x01,                                             ///ObjMgr Ptr 
                0x02, 0x31, 0x2F, 0x00,                                             ///InteratObjByGUID() Func 
                0x92, 0x01, 0x02, 0x00,                                             ///Spell id 
                0x00, 0x00, 0x00, 0x00,                                             ///TargetGUID[4*DWORD]
                0x00, 0x00, 0x00, 0x00,                                             ///D2
                0x00, 0x00, 0x00, 0x00,                                             ///D3
                0x00, 0x00, 0x00, 0x00,                                             ///D4
                0x00, 0x00, 0x00, 0x00,                                             ///DummyBuffer[50*DWORD]
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x92, 0x01, 0x02, 0x00,                                             ///DummyBuffer[5].. Fishing Spell again
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,                                             ///PlayerGUID ptr           0xF4
                0x00, 0x00, 0x00, 0x00,                                             ///status                   0xF8
                0x46, 0x69, 0x73, 0x68,                                             ///'Fishin Bobber'          0xFC
                0x69, 0x6E, 0x67, 0x20,
                0x42, 0x6F, 0x62, 0x62,
                0x65, 0x72 };
                            
            #endregion ASMcode


            if (hHandle == IntPtr.Zero)
                throw new ApplicationException("Cannot get process handle.");

            IntPtr hAlloc = VirtualAllocEx(hHandle, IntPtr.Zero, (uint)asm.Length, AllocationType.Commit, MemoryProtection.ExecuteReadWrite);
            Console.WriteLine("CodeCave: " + (hAlloc).ToString() + " ]");

            if (hAlloc == IntPtr.Zero)
                throw new ApplicationException("Cannot allocate memory.");

            int bytesWritten = 0;

            if (!WriteProcessMemory((int)hHandle, (int)hAlloc, asm, asm.Length, ref bytesWritten))
                throw new ApplicationException("Cannot write process memory.");

            byte[] buffer = { 0x00, 0x00, 0x00, 0x00 };
            buffer = BitConverter.GetBytes((int)hAlloc + Offsets.CodeCaveDataSize);

            if (!WriteProcessMemory((int)hHandle, (int)hAlloc + 0x04, buffer, buffer.Length, ref bytesWritten))
                throw new ApplicationException("Cannot write process memory.");

            IntPtr Base32Sleep = GetProcAddress(GetModuleHandle("kernel32.dll"), "Sleep");
            Console.WriteLine(Base32Sleep.ToString());
            int baseAddr = (int)process.MainModule.BaseAddress;

            buffer = BitConverter.GetBytes((uint)Base32Sleep - ((uint)hAlloc + 0x36 + 4));
            WriteProcessMemory((int)hHandle, (int)hAlloc + 0x36, buffer, buffer.Length, ref bytesWritten);
            buffer = BitConverter.GetBytes((uint)Base32Sleep - ((uint)hAlloc + 0x9C + 4));
            WriteProcessMemory((int)hHandle, (int)hAlloc + 0x9C, buffer, buffer.Length, ref bytesWritten);

            buffer = BitConverter.GetBytes((uint)(baseAddr + Offsets.Spell_C_CastSpell));    //Spell_C_CastSpell() func
            WriteProcessMemory((int)hHandle, (int)hAlloc + Offsets.CodeCaveDataSize, buffer, buffer.Length, ref bytesWritten);

            buffer = BitConverter.GetBytes((uint)(baseAddr + Offsets.PlayerBasePtr));    //PlayerBase ptr
            WriteProcessMemory((int)hHandle, (int)hAlloc + Offsets.CodeCaveDataSize + 0x04, buffer, buffer.Length, ref bytesWritten);

            buffer = BitConverter.GetBytes((uint)(baseAddr + Offsets.ObjMgrPtr));    //ObjMgr
            WriteProcessMemory((int)hHandle, (int)hAlloc + Offsets.CodeCaveDataSize + 0x08, buffer, buffer.Length, ref bytesWritten);

            buffer = BitConverter.GetBytes((uint)(baseAddr + Offsets.InteractObjByGUID));    //InteratObjByGUID
            WriteProcessMemory((int)hHandle, (int)hAlloc + Offsets.CodeCaveDataSize + 0x0C, buffer, buffer.Length, ref bytesWritten);

            buffer = BitConverter.GetBytes((uint)(baseAddr + Offsets.LocalPlayerGuid));    //LocalPlayerGUID
            WriteProcessMemory((int)hHandle, (int)hAlloc + Offsets.CodeCaveDataSize + 0xF4, buffer, buffer.Length, ref bytesWritten);

            uint iThreadId = 0;
            IntPtr hThread = CreateRemoteThread(hHandle, IntPtr.Zero, 0, hAlloc, IntPtr.Zero, 0, out iThreadId);
            Console.WriteLine("Thread= " + hThread.ToString());
            

            if (hThread == IntPtr.Zero)
                throw new ApplicationException("Cannot create and execute remote thread.");

            if (hThread == IntPtr.Zero) { Console.WriteLine(" .. Failed; "); }

            CloseHandle(hThread);
            CloseHandle(hHandle);

            int id = (int)dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[4].Value;

            FishermansCave[id] = (uint)hAlloc;
            FisherbotProcc[id] = iProcessId;
            ThreadList[id] = (IntPtr)iThreadId;
            AmountOfFishermans++;
            timer1.Enabled = true;
        }

        private void FishermanWorkorder(int iProcessId)
        {
            //This one blows life into the code cave we made before, we re-use the code cave every time
            try
            {
                int id = 0;
                for (int j = 0; j != dataGridView1.RowCount; j++)
                {
                    if ((int)dataGridView1.Rows[j].Cells[2].Value == iProcessId)
                    {
                        id = (int)dataGridView1.Rows[j].Cells[4].Value;
                    }
                }


                Console.WriteLine("Doing fishing...");
                IntPtr hAlloc = IntPtr.Zero;
                int i = 0;
                foreach (uint proc in FisherbotProcc)
                {
                    if (iProcessId == proc)
                    {
                        hAlloc = (IntPtr)FishermansCave[i];
                    }
                    i++;
                }

                IntPtr hHandle = OpenProcess(0x1F0FFF, false, iProcessId);
                uint iThreadId = 0;
                IntPtr hThread = CreateRemoteThread(hHandle, IntPtr.Zero, 0, hAlloc, IntPtr.Zero, 0, out iThreadId);

                if (hThread == IntPtr.Zero)
                    timer1.Enabled = false;
                dataGridView1.Select();

                if (hThread == IntPtr.Zero) { Console.WriteLine(" .. Failed; "); }
                ThreadList[id] = (IntPtr)iThreadId;
                CloseHandle(hThread);
                CloseHandle(hHandle);
            }
            catch
            {
                MessageBox.Show("Sumthing went wrong :(");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Sets some apperances
            this.BackColor = Color.Wheat;
            this.TopMost = true;
            this.TransparencyKey = Color.Wheat;
            this.FormBorderStyle = FormBorderStyle.None;

            DataGridViewColumn column = dataGridView1.Columns[0];
            column.Width = 40;
            DataGridViewColumn column1 = dataGridView1.Columns[1];
            column1.Width = 205;

            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();
            dataGridView1.AllowUserToAddRows = false;
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            ActionOnChange();
        }

        private void ActionOnChange()
        {
            //Keeps track of changes
            bool found = false;
            for (int i = 0; i != dataGridView1.Rows.Count; i++)
            {
                if ((bool)dataGridView1.Rows[i].Cells[0].Value == true)
                {
                    for (int j = 0; j != FisherbotProcc.Length; j++)
                    {
                        if ((int)dataGridView1.Rows[i].Cells[2].Value == FisherbotProcc[j])
                        {
                            found = true;
                        }
                    }
                    if (found == false)
                    {
                        DeployFishermansCave(Convert.ToInt32(dataGridView1.Rows[i].Cells[2].Value));
                    }
                }
            }

        }

        private void AvtivateFishOMatic(int id)
        {
            DeployFishermansCave((int)dataGridView1.Rows[id].Cells[2].Value);

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            ActionOnChange();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AvtivateFishOMatic(0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int id = 0;
            FishermanWorkorder((int)dataGridView1.Rows[id].Cells[2].Value);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //This keeps tracks of user activity on the GUI
            for(int i = 0; i != dataGridView1.RowCount; i++)
            {
                if ((bool)dataGridView1.Rows[i].Cells[0].Value)
                {
                    CheckFisherman((int)dataGridView1.Rows[i].Cells[4].Value);
                }
            }
        }

        private void CheckFisherman(int id)
        {
            //Check if thread is still alive, if not we make new ones where needed.
            int iProcessId = FisherbotProcc[id];
            IntPtr thread = ThreadList[id];
            bool found = false;

            try
            {
                Process.GetProcessById(iProcessId);
            }
            catch
            {
                MessageBox.Show("Wow was not found.");
                dataGridView1.Rows.Clear();
                this.Close();
                return;
            }

            Process process = Process.GetProcessById(iProcessId);
            IntPtr hHandle = OpenProcess(0x1F0FFF, false, iProcessId);
            ProcessThreadCollection CList = process.Threads;
            foreach(ProcessThread CThread in CList)
            {
                Console.WriteLine(CThread.Id.ToString() + " == " + ThreadList[id].ToString());
                if(CThread.Id == (int)ThreadList[id])
                {
                    found = true;
                }
            }
            Console.WriteLine(dataGridView1.Rows[id].Cells[0].Value.ToString());
            if (found == false && (bool)dataGridView1.Rows[id].Cells[0].Value == true)
            {
                FishermanWorkorder(FisherbotProcc[id]);
            }

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var dataGridView = (DataGridView)sender;
            var cell = dataGridView[0, e.RowIndex];

            if (cell.Value == null) cell.Value = false;
            cell.Value = !(bool)cell.Value;
            dataGridView.EndEdit();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                this.TopMost = true;
            }else
            {
                this.TopMost = false;
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            //This was used to drag around the GUI, It was a must for a design like this
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void PatchCRTAntiCheat(int id, bool status)
        {
            byte[] Patch = { 0xFF, 0xE0, 0xCC, 0xCC, 0xCC };  //JMP RAX
            byte[] Patch2 = { 0x48, 0xFF, 0xC0, 0xFF, 0xE0 }; //INC RAX, JMP RAX

            //Blizzard will add 0xC3 (ret) at the begin of our code cave, So what we do is start our code cave with 0x90 (NOP) and then add the code cave under it.
            //We will patch a DLL function (Cuz i don't like touching Wow.exe) so it start executing our code cave from the second byte.

            int PatchOffset = Offsets.CreateRemoteThreadPatchOffset;
            if (status){
                Patch = Patch2;
            }

            int iProcessId = FisherbotProcc[id];
            IntPtr hHandle = OpenProcess(0x1F0FFF, false, iProcessId);
            int bytesWritten = 0;

            if (!WriteProcessMemory((int)hHandle, (int)PatchOffset, Patch, Patch.Length, ref bytesWritten))
                throw new ApplicationException("Cannot write process memory.");

        }

    }
}

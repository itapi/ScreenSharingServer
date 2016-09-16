using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Drawing.Imaging;
using System.Text;
using System.Threading;
using Open.Nat;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;

using System.Net.Sockets;

namespace DesktopDuplication.Demo
{
    public unsafe partial class FormDemo : Form
    {
        #region DLLS
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int memcpy(IntPtr dst, IntPtr src, uint count);
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int memcpy(byte* dst, byte* src, uint count);
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int memcpy(void* dst, void* src, uint count);
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern unsafe int memcmp(void* ptr1, void* ptr2, int count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(IntPtr ptr1, IntPtr ptr2, int count);
        [DllImport("user32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SystemParametersInfo(uint uiAction,
                                                       uint uiParam,
                                                       ref ANIMATIONINFO pvParam,
                                                       uint fWinIni);
        #endregion
        private DesktopDuplicator desktopDuplicator;
        DesktopFrame frame = null;
        private TcpListener listener;
        private Socket sck;
        const int Max_Block_Width = 250;
        const int Max_Block_Height = 100;

        private MemoryStream ms;
        private Bitmap block;
        private Thread thread;
        private Rectangle Bounds = Screen.PrimaryScreen.Bounds;
        private byte[] buffer;
        private ulong mask = 0xFFFCFCFCFFFCFCFC;
        #region ReducingTraffic

        [StructLayout(LayoutKind.Sequential)]
        public struct ANIMATIONINFO
        {
            public uint cbSize;
            public int iMinAnimate;
        };


        public static uint SPIF_SENDCHANGE = 0x02;
        public static uint SPI_SETANIMATION = 0x0049;


        public bool SwitchAero(int state)
        {
            ANIMATIONINFO ai;
            ai.cbSize = 8;
            ai.iMinAnimate = state;   // turn all animation off
            return SystemParametersInfo(SPI_SETANIMATION, 0, ref ai, SPIF_SENDCHANGE);
        }
        #endregion

        private unsafe void Xor(Bitmap bmp1, Bitmap bmp2)
        {
            BitmapData bmData = bmp1.LockBits(new Rectangle(0, 0, bmp1.Width, bmp1.Height),
                                              System.Drawing.Imaging.ImageLockMode.ReadWrite,
                                bmp1.PixelFormat);
            BitmapData bmData2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height),
                                           System.Drawing.Imaging.ImageLockMode.ReadWrite,
                             bmp2.PixelFormat);
            IntPtr scan0 = bmData.Scan0;
            IntPtr scan02 = bmData2.Scan0;
            int stride = bmData.Stride;
            int stride2 = bmData2.Stride;

            foreach (var region in frame.UpdatedRegions)
            {
                int sy = 0;
                Console.WriteLine(frame.UpdatedRegions.Length.ToString());

                int destX = region.X + region.Width;
                int destY = region.Y + region.Height;

                for (int y = region.Y; y < destY; y++, sy++)
                {
                    byte* p = (byte*)scan0.ToPointer();
                    p += y * stride;

                    byte* p2 = (byte*)scan02.ToPointer();
                    p2 += sy * stride2;
                    int sx = 0;
                    for (int x = region.X; x < destX; x++, sx++)
                    {

                        p2[(x * 4) + 0] ^= p[(sx * 4) + 0];   //blue
                        p2[(x * 4) + 1] ^= p[(sx * 4) + 1]; //green
                        p2[(x * 4) + 2] ^= p[(sx * 4) + 2]; //red   

                    }
                }

                block = bmp2.Clone(region, PixelFormat.Format32bppRgb);
                position = BlockToJpeg();

                WriteBlockPoint(region.X, region.Y);

                try
                {
                    SendData(position + 8);
                }
                catch
                {
                    Exiting();
                }

            }
            bmp1.UnlockBits(bmData);
            bmp2.UnlockBits(bmData2);


        }
        bool[] visited;
        public FormDemo()
        {
            InitializeComponent();
            buffer = new byte[Bounds.Width * Bounds.Height * 3];
            ms = new MemoryStream(buffer, 4, buffer.Length - 4);
            block = new Bitmap(Bounds.Width, Bounds.Height);
            bmp = new Bitmap(1, 1);
            BlockBitmap = new Bitmap(Bounds.Width / 10, Bounds.Height/ 10, PixelFormat.Format32bppRgb);
            BlockSize = new Size(BlockBitmap.Width, BlockBitmap.Height);
            Changed = new Rectangle();

            //BlockBitmap = new Bitmap(Bounds.Width / 10, Bounds.Height / 10, PixelFormat.Format32bppRgb);
            //   BlockSize.Width=Max_Block_Width;
            // BlockSize.Height = Max_Block_Height;

            try
            {

                desktopDuplicator = new DesktopDuplicator(0);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Exiting()
        {
            SwitchAero(1);
            MessageBox.Show("Exiting");
            Environment.Exit(0);
        }

        private int InitialToJpeg()
        {

            ms.SetLength(0);
            block.Save(ms, ImageFormat.Jpeg);
            return (int)ms.Position;
        }

        private int BlockToJpeg()
        {

            ms.SetLength(0);
            BlockBitmap.Save(ms, ImageFormat.Jpeg);
            return (int)ms.Position;
        }

        private int Bmp2ToJpeg()
        {

            ms.SetLength(0);
            bmp.Save(ms, ImageFormat.Jpeg);
            return (int)ms.Position;
        }
        private void PrintData()
        {

            Console.WriteLine("DataLength :" + position);
            Console.WriteLine("Block X : {0}  Y : {1}", x, y);
            Console.WriteLine("--------");
        }
        private void SendData(int position)
        {
            // this.Invoke(new Action(() => this.Text = position.ToString()));
            sck.Send(BitConverter.GetBytes(position));
            int sent = sck.Send(buffer, position, SocketFlags.None);
        }


        private unsafe void CutHelper(int x, int y, int height)
        {
            CurrPtr -= (height + 1) * stride;

            BlockPtr = (byte*)bmDataBlock.Scan0;
            for (int i = 0; i <= height; i++)
            {
                memcpy(BlockPtr, CurrPtr, (uint)stride2);
                CurrPtr += stride;
                BlockPtr += stride2;

            }
        }

        BitmapData tempData;
        private unsafe void CutHelper(Bitmap bmp, int x, int y, int height)
        {
            CurrPtr -= (height + 1) * stride;

            BlockPtr = (byte*)tempData.Scan0;

            for (int i = 0; i <= height; i++)
            {
                memcpy(BlockPtr, CurrPtr, (uint)tempData.Stride);
                CurrPtr += stride;
                BlockPtr += tempData.Stride;

            }
        }
        private unsafe void FindChangeStart(int x)
        {
            int height;
            CurrPtr += x * 4 ;
            PrevPtr += x * 4 ;

            for (; lasty < block.Height -1; lasty++)
            {

                if (memcmp(CurrPtr, PrevPtr, stride2) != 0)//found different pixels line.
                {
                   // counter++;
                    HeightCheck = Math.Min(block.Height - lasty, BlockSize.Height) - 1;
                    height = GetChangeEnd(x, y);
                    Changed.X = x;
                    Changed.Y = lasty;
                    lasty += height;
                    Changed.Width = BlockSize.Width;
                    Changed.Height = height;

                    //Console.WriteLine("X :{0} , Y :{1}  , Height :{2}", Changed.X,Changed.Y,Changed.Height);
                    if (height + 1 == BlockSize.Height)
                    {
                        CutHelper(Changed.X, Changed.Y, height);
                        position = BlockToJpeg();

                    }
                    else
                        if (height + 1 == bmp.Height)
                        {

                            CutHelper(bmp, Changed.X, Changed.Y, height);
                            position = Bmp2ToJpeg();
                        }
                        else
                        {
                            bmp.Dispose();
                            bmp = new Bitmap(BlockSize.Width, height + 1);
                            tempData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);

                            CutHelper(bmp, Changed.X, Changed.Y, height);
                            position = Bmp2ToJpeg();
                            

                        }
                    WriteBlockPoint(Changed.X, Changed.Y);
                    udpServer.Send(buffer, position + 4, remoteEP);

                }
                CurrPtr += stride;
                PrevPtr += stride;

            }


        }
        //64443553445
        int lasty = 0;
        int counter = 0;
        Bitmap bmp;
        int HeightCheck;

        private unsafe int GetChangeEnd(int x, int y)
        {

            // Console.WriteLine("X :{0} , Y :{1}",x,y);


            int total = HeightCheck;
            for (; y < total; y++)
            {
                if (memcmp(CurrPtr, PrevPtr, stride2) == 0)
                    return y;
                CurrPtr += stride;
                PrevPtr += stride;

            }
            return y;
        }
        IntPtr scan02, scan0;
        byte* CurrPtr, BlockPtr, PrevPtr;
        int stride, stride2;
        Size BlockSize;
        BitmapData bmDataBlock;
        BitmapData bmDataPrev;
        Bitmap BlockBitmap;
        Rectangle Changed;

        private unsafe void UnsafeMem()
        {
            stride2 = bmDataBlock.Stride;


            fixed (byte* FixedPrevPtr = frame.PreviousDesktopBuffer, FixedCurrPtr = frame.CurrentDesktopBuffer)
            {
                stride = block.Width * 4;

                for (int x = 0; x < block.Width - 1; x += BlockSize.Width)
                {
                    lasty = 0;

                    PrevPtr = FixedPrevPtr;
                    CurrPtr = FixedCurrPtr;
                    FindChangeStart(x);

                }

            }
        }


        private void WriteBlockPoint(int x, int y)
        {
            buffer[0] = (byte)(x);
            buffer[1] = (byte)(x >> 8);
            buffer[2] = (byte)(y);
            buffer[3] = (byte)(y >> 8);
        }


        int position;
        int x, y;

        private void MainScreenThread()
        {


            SwitchAero(0);
            frame = desktopDuplicator.GetLatestFrame();
            WriteBitmap(frame.CurrentDesktopBuffer);

            position = InitialToJpeg();
            //Console.WriteLine(position + 8);
            SendData(position);

            listener.Stop();
            bmDataBlock = BlockBitmap.LockBits(new Rectangle(0, 0, BlockBitmap.Width, BlockBitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);

            while (true)
            {
                frame = desktopDuplicator.GetLatestFrame();
              
               
                if (frame != null)
                {
                    UnsafeMem();
                    /* foreach (var region in frame.UpdatedRegions)
                     {
                         block = frame.CurrentDesktopImage.Clone(region, PixelFormat.Format32bppRgb);
                        position= BlockToJpeg();
                        WriteBlockPoint(region.X, region.Y);
                        SendData(position + 8);
                     }*/
                    //GetBoundingBoxForChanges(frame.PreviousDesktopImage, frame.CurrentDesktopImage);//approach #2

                }
            }



        }

        int GetDividedWidth(int num)
        {
            for (int i = 1; i < Max_Block_Width; i++)
            {
                if (num % i == 0 && num / i < Max_Block_Width)
                    return num / i;
            }
            return num;
        }

        int GetDivideHeight(int num)
        {
            for (int i = 1; i < Max_Block_Height; i++)
            {
                if (num % i == 0 && num / i < Max_Block_Height)
                    return num / i;
            }
            return num;
        }

        private void WriteBitmap(byte[] imageData)
        {


            BitmapData bmpData = block.LockBits(new Rectangle(0, 0, block.Width, block.Height), ImageLockMode.WriteOnly, block.PixelFormat);

            Marshal.Copy(imageData, 0, bmpData.Scan0, imageData.Length);

            block.UnlockBits(bmpData);




        }

        private void FormDemo_Load(object sender, EventArgs e)
        {

            /*    Bitmap bmp;//תחשבו שיש פה תמונה מסוימת
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Jpeg);

                byte[] buffer = ms.GetBuffer();
                int x = 150;
                int y = 810;
                int width = 300;
                int height = 400;
                */

        }

        UdpClient udpServer;
        IPEndPoint remoteEP;
        private async void button1_Click(object sender, EventArgs e)
        {

            try
            {

                listener = new TcpListener(IPAddress.Any, 25655);
                listener.Start();
                sck = listener.AcceptTcpClient().Client;
                Console.WriteLine("tcp connected");
                udpServer = new UdpClient(1100);
                remoteEP = new IPEndPoint(IPAddress.Any, 1100);
                var data = udpServer.Receive(ref remoteEP);

                button1.Text = "Sharing started";
                button1.Enabled = false;
                thread = new Thread(MainScreenThread);
                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void FormDemo_FormClosed(object sender, FormClosedEventArgs e)
        {
            SwitchAero(1);
            thread.Abort();
            Environment.Exit(0);
        }
    }
}
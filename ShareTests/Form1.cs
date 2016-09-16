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
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using DesktopDuplication;

using System.Net.Sockets;

namespace ShareTests
{
    public partial class Form1 : Form
    {
        private DesktopDuplicator desktopDuplicator;
        private Bitmap prev, curr;
        private DesktopFrame frame;

        private unsafe void Xor(Bitmap bmp1, Bitmap bmp2)
        {

            BitmapData bmData = bmp1.LockBits(new Rectangle(0, 0, bmp1.Width, bmp1.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp1.PixelFormat);
            BitmapData bmData2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp2.PixelFormat);
            IntPtr scan0 = bmData.Scan0;
            IntPtr scan02 = bmData2.Scan0;
            int stride = bmData.Stride;

            for (int y = 0; y < bmp1.Height; y++)
            {
                int* row1 = (int*)(scan0 + stride * y);
                int* row2 = (int*)(scan02 + stride * y);
                for (int x = 0; x < bmp1.Width; x++)

                    row1[x] ^= row2[x];
            }
            bmp1.UnlockBits(bmData);
            bmp2.UnlockBits(bmData2);
            pictureBox1.Invoke(new Action(() => pictureBox1.Image = bmp1));

        }

        public Form1()
        {


            try
            {

                desktopDuplicator = new DesktopDuplicator(0);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }



        int position;
        int x, y;

        private void MainScreenThread()
        {
            frame = desktopDuplicator.GetLatestFrame();

            while (true)
            {
                frame = desktopDuplicator.GetLatestFrame();

                Xor(frame.PreviousDesktopImage, frame.CurrentDesktopImage);

            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            MessageBox.Show("D");
            Thread th = new Thread(MainScreenThread);
            th.Start();
        }


        private void FormDemo_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}

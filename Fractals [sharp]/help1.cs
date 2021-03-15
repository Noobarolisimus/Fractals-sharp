using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Fractals__sharp_
{
    public partial class help1 : Form
    {
        public help1()
        {
            InitializeComponent();
            ImageAnimator.Animate(pictureBox1.BackgroundImage, OnFrameChanged);
        }

        private void OnFrameChanged(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => OnFrameChanged(sender, e)));
                return;
            }
            ImageAnimator.UpdateFrames();
            pictureBox1.Refresh();
            Invalidate(false);
        }
    }
}

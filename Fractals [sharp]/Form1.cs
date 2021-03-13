using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace Fractals__sharp_
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			Icon = Fractals__sharp_.Properties.Resources.logo;
			BitmapCanvas = new Bitmap(1920, 1080);
			CanvasGraphics = Graphics.FromImage(BitmapCanvas);
			BackgroundImage = new Bitmap(1920, 1080);
			MainImage.BackgroundImage = BitmapCanvas;
			color1.BackColor = FirstColor;
			color2.BackColor = SecondColor;
			numericUpDown1.Value = Depth;
			timer1.Interval = 500;


			panels[0] = panelDefault;
			panels[1] = panelTree;
			panels[2] = panelDefault;
			panels[3] = panelDefault;


		}

		Point NewPointByAngle(Point Position, double Angle, double Radius)
        {
			return new Point((int)Math.Round(Position.X + Radius * Math.Sin(Angle / 180.0 * Math.PI)),
							 (int)Math.Round(Position.Y + Radius * Math.Cos(Angle / 180.0 * Math.PI)));
        }

		int Lerp(float t, int from, int to)
        {
			return from + (int)((to - from) * t);
        }
		Point Lerp(float t, Point from, Point to)
        {
			from.X += (int)((to.X - from.X) * t);
			from.Y += (int)((to.Y - from.Y) * t);
			return from;
		}
		Color Lerp(float t, Color from, Color to)
        {
			return Color.FromArgb(Lerp(t, from.A, to.A),
								  Lerp(t, from.R, to.R),
								  Lerp(t, from.G, to.G),
								  Lerp(t, from.B, to.B));
        }

		#region Variables
		private Bitmap BitmapCanvas;
		private Color FirstColor = Color.Red;// Color.Brown;
		private Color SecondColor = Color.DarkGreen;
		private int Depth = 3;
		private int Angle1 = 45;
		private int Angle2 = 45;
		private int Longness = 300;
		private Graphics CanvasGraphics;
		private CancellationTokenSource cts = new CancellationTokenSource();
		private Task ActiveTask;
		private Mutex mutex = new Mutex();
		private int lastFractal = 2;
		private int lastPanel = 0;
		private Panel[] panels = new Panel[4];
		//private Graphics ImageGraphics;
		//private Rectangle CanvasRect;
		#endregion

		async Task TreeFract(Point Pos, int Radius, int Ang, int Step = 0)
        {
			if (Step > Depth)
				return ;
			Point NewPoint = NewPointByAngle(Pos, Ang, Radius);
			mutex.WaitOne();
			CanvasGraphics.DrawLine(new Pen(Lerp((Step / (float)Depth), FirstColor, SecondColor), 2), Pos, NewPoint);
			mutex.ReleaseMutex();
			await TreeFract(NewPoint, (int)(Radius / 1.5), Ang + Angle1, Step + 1);
			await TreeFract(NewPoint, (int)(Radius / 1.5), Ang - Angle2, Step + 1);
		}
		
		async Task SierpinskiTriangle(Point[] triangle, int step = 0)
        {
			if (step >= Depth)
				return;
			Point[] subTr = new Point[3];
			subTr[0] = Lerp(.5f, triangle[0], triangle[1]);
			subTr[1] = Lerp(.5f, triangle[1], triangle[2]);
			subTr[2] = Lerp(.5f, triangle[0], triangle[2]);
			Point[] childTr1 = new Point[3];
			Point[] childTr2 = new Point[3];
			Point[] childTr3 = new Point[3];
			childTr1[0] = triangle[0];
			childTr1[1] = subTr[0];
			childTr1[2] = subTr[2];
			childTr2[0] = triangle[1];
			childTr2[1] = subTr[0];
			childTr2[2] = subTr[1];
			childTr3[0] = triangle[2];
			childTr3[1] = subTr[1];
			childTr3[2] = subTr[2];
			await SierpinskiTriangle(childTr1, step + 1);
			await SierpinskiTriangle(childTr2, step + 1);
			await SierpinskiTriangle(childTr3, step + 1);
			mutex.WaitOne();
			CanvasGraphics.DrawPolygon(new Pen(Lerp((step / (float)Depth), FirstColor, SecondColor), 1), triangle);
			mutex.ReleaseMutex();
		}

		async Task TestFract()
        {
			for(int i = 0; i < Depth; i++)
            {
                CanvasGraphics.FillRectangle(new SolidBrush(Lerp(i / (float)Depth, FirstColor, SecondColor)), i * 1920 / (float)Depth, 0, 1920 / (float)Depth, 1080);
				//CanvasGraphics.FillRectangle(new SolidBrush(FirstColor), i * 1920 / (Depth - 1), 0, 1920 / (Depth - 1), 1080);
			}
		}

		private float RotatingAngle = MathF.PI / 3; // 60 градусов
		async Task KochCurve(Point p1, Point p5, int colorIndex = 0, int step = 0)
        {
			if (step + 1 == Depth || Math.Pow(p1.X - p5.X, 2) + Math.Pow(p1.Y - p5.Y, 2) <= 1)
			{
				CanvasGraphics.DrawLine(new Pen(Lerp(((float)colorIndex / (float)Depth), FirstColor, SecondColor), 2), p1, p5);
				return;
			}
			Point p2 = Lerp(0.33333333334f, p1, p5);
			Point p4 = Lerp(0.66666666667f, p1, p5);
			Point p3 = new Point(
				(int)(p2.X + (p4.X - p2.X) * MathF.Cos(RotatingAngle) + MathF.Sin(RotatingAngle) * (p4.Y - p2.Y)),
				(int)(p2.Y - (p4.X - p2.X) * MathF.Sin(RotatingAngle) + MathF.Cos(RotatingAngle) * (p4.Y - p2.Y))
			);
			//Point p3 = NewPointByAngle(Lerp(0.5f, p2, p4), 30 )

			await KochCurve(p1, p2, colorIndex, step + 1);
			await KochCurve(p2, p3, colorIndex + 1, step + 1);
			await KochCurve(p3, p4, colorIndex + 1, step + 1);
			await KochCurve(p4, p5, colorIndex, step + 1);
		}

		double Magnitude(System.Numerics.Complex num)
        {
			return Math.Sqrt(num.Real * num.Real + num.Imaginary * num.Imaginary);
        }
		async Task MaldebrotSet()
		{
			CanvasGraphics.Clear(Color.Black);
			Point start = new Point(1920 / 2 - 1080 / 2, 0);
			int size = 1080;
			for (int i = 0; i < size; i++)
				for (int j = 0; j < size; j++)
				{
					System.Numerics.Complex z = new System.Numerics.Complex(0, 0), 
											c = new System.Numerics.Complex((i - size / 2f) / size * 2f - .5f, (j - size / 2f) / size * 2f - 0);
					int it = 0;
					while (++it < 100)
					{
						z *= z;//.square();
						z += c;
						if (Magnitude(z) > 2)
							break;
					}
					if (it < 100)
					{
						CanvasGraphics.DrawRectangle(new Pen(Lerp((it / 100.0f), Color.Black, FirstColor), 1), (i + start.X), (j + start.Y), 1f, 1f);
						//SDL_SetRenderDrawColor(ren, 0xFF * it / 100., 0x00, 0x00, 0x00);
						//SDL_RenderDrawPoint(ren, (i + start.x), (j + start.y));
					}
					//else
					//	CanvasGraphics.DrawRectangle(new Pen(Color.Black, 1), (i + start.X), (j + start.Y), 1f, 1f);
				}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			saveFileDialog1.Filter = "PNG файл|*.png|JPG файл|*.jpg";
			if(saveFileDialog1.ShowDialog() != DialogResult.Cancel)
            {
				BitmapCanvas.Save(saveFileDialog1.FileName);
				//MessageBox.Show("Изображение сохранено.");
            }
		}

		private void prepareCanvas()
        {
			cts.Cancel();
			mutex.Close();
			cts = new CancellationTokenSource();
			mutex = new Mutex();
			CanvasGraphics.Clear(Color.FromArgb(255, 255, 255));
		}
		
		private void drawAFractal(int id)
        {
			prepareCanvas();
			ActiveTask = Task.Run(async () =>
			{
				switch (id)
				{
					case 1:
						Point[] mainTriangle = new Point[3];
						for (int i = 0; i < 3; i++)
							mainTriangle[i] = NewPointByAngle(new Point(1920 / 2, (int)(1080 / 2.5)), 360 / 3 * i, 1080 / 2);
						await SierpinskiTriangle(mainTriangle);
						break;
					case 2:
						await TreeFract(new Point(1920 / 2, 1080 / 4), Longness, 0);
						break;
					case 3:
						await KochCurve(new Point(1920 / 100, 1080 - 1080 / 5), new Point(1920 - 1920 / 100, 1080 - 1080 / 5));
						break;
					case 4:
						await MaldebrotSet();
						break;
					default:
						await TestFract();
						break;
				}
			}, cts.Token);
		}

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
			panels[lastFractal - 1].Visible = false;
			lastFractal = Convert.ToInt32((string)(e.ClickedItem.Tag));
			panels[lastFractal - 1].Visible = true;
			drawAFractal(Convert.ToInt32((string)(e.ClickedItem.Tag)));

			/**ActiveTask = Task.Run(async () =>
			{
				switch (Convert.ToInt32((string)(e.ClickedItem.Tag)))
				{
					case 1:
						Point[] mainTriangle = new Point[3];
						for (int i = 0; i < 3; i++)
							mainTriangle[i] = NewPointByAngle(new Point(1920 / 2, (int)(1080 / 2.5)), 360 / 3 * i, 1080 / 2);
						await SierpinskiTriangle(mainTriangle);
						break;
					case 2:
						await TreeFract(new Point(1920 / 2, 1080 / 4), Longness, 0);
						break;
					case 3:
						await KochCurve(new Point(1920 / 100, 1080 - 1080 / 5), new Point(1920 - 1920 / 100, 1080 - 1080 / 5));
						break;
					case 4:
						await MaldebrotSet();
						break;
					default:
						await TestFract();
						break;
				}
			}, cts.Token);
			Width++;
			Width--;**/
        }

        private void color1_Click(object sender, EventArgs e)
        {
			if (colorDialog1.ShowDialog() != DialogResult.Cancel)
			{
				FirstColor = colorDialog1.Color;
				color1.BackColor = FirstColor;
				drawAFractal(lastFractal);
			}
		}

		private void color2_Click(object sender, EventArgs e)
		{
			if (colorDialog1.ShowDialog() != DialogResult.Cancel)
			{
				SecondColor = colorDialog1.Color;
				color2.BackColor = SecondColor;
				drawAFractal(lastFractal);
			}
		}

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
			Depth = (int)((NumericUpDown)sender).Value;
			drawAFractal(lastFractal);
		}

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
			Angle1 = (int)((NumericUpDown)sender).Value;
			drawAFractal(lastFractal);
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
		{
			Angle2 = (int)((NumericUpDown)sender).Value;
			drawAFractal(lastFractal);
		}

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
			Longness = (int)((NumericUpDown)sender).Value * 50;
			drawAFractal(lastFractal);
		}

        private void timer1_Tick(object sender, EventArgs e)
        {
			MainImage.Refresh();
        }

        private void MainImage_Click(object sender, EventArgs e)
        {
			MessageBox.Show("Сделал Вавилов Роман для школьного проекта\n15.03.2021");
        }
    }
}

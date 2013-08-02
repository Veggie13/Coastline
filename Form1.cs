using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FlexDraw;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Coastline
{
    public partial class Form1 : Form
    {
        GCViewport _viewport = new GCViewport();
        DrawSurface _surface = new DrawSurface();
        Landmass _landmass = new Landmass();

        class Landmass : IDrawable
        {
            public Landmass()
            {
                var points = new ObservableCollection<PointD>();
                points.CollectionChanged += new NotifyCollectionChangedEventHandler(points_CollectionChanged);
                Points = points;
            }

            private void points_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (Modified != null)
                    Modified(this);
            }

            public IList<PointD> Points
            {
                get;
                private set;
            }

            public void Fractalize()
            {
                PointD vec = new PointD(), perp = new PointD(), midpoint = new PointD();
                Random rand = new Random();
                int i;
                for (i = 1; i < Points.Count; i++)
                {
                    vec.X = Points[i].X - Points[i - 1].X;
                    vec.Y = Points[i].Y - Points[i - 1].Y;

                    double mag = Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
                    double mid = 0.5;// rand.NextDouble();
                    double offset = 0.5 * (rand.NextDouble() - 0.5);

                    perp.X = -vec.Y * offset;
                    perp.Y = vec.X * offset;

                    midpoint.X = Points[i - 1].X + mid * vec.X + perp.X;
                    midpoint.Y = Points[i - 1].Y + mid * vec.Y + perp.Y;
                    Points.Insert(i++, midpoint);
                }
            }

            public void Draw(IDrawAPI api)
            {
                int i;
                for (i = 1; i < Points.Count; i++)
                {
                    api.DrawLine(Points[i - 1], Points[i], Color.Green);
                }
                if (Points.Count > 1)
                {
                    //api.DrawLine(Points[0], Points[i - 1], Color.Green);
                }
            }

            public bool Visible
            {
                get { return true; }
            }

            public PointD Origin
            {
                get { return new PointD(0, 0); }
            }

            public RectangleD Bounds
            {
                get { return new RectangleD(-100, 100, -100, 100); }
            }

            public RectangleD LastBounds
            {
                get { return Bounds; }
            }

            public event DrawableModifiedEvent Modified;
        }

        public Form1()
        {
            InitializeComponent();

            panel1.Paint += new PaintEventHandler(panel1_Paint);
            panel1.MouseClick += new MouseEventHandler(panel1_MouseClick);

            this.KeyPress += new KeyPressEventHandler(Form1_KeyPress);
        }

        void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ' ')
            {
                _landmass.Fractalize();
                panel1.Invalidate();
                e.Handled = true;
            }
        }

        void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            PointD click = _viewport.Transform(e.Location);
            _landmass.Points.Add(click);
            panel1.Invalidate();
        }

        void panel1_Paint(object sender, PaintEventArgs e)
        {
            _viewport.GC = e.Graphics;
            _viewport.FillRectangle(_viewport.View, Color.Black);
            _surface.Draw();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _viewport.Window = new Rectangle(0, 0, panel1.Width, panel1.Height);
            _viewport.View = new RectangleD(-100, 100, -100, 100);

            _surface.Viewports.Add(_viewport);
            _surface.Items.Add(_landmass);
        }
    }
}

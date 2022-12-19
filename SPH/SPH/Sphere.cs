using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPH
{
    internal class Sphere
    {
        public double radius;
        public Vector3d position, speed;
        private int nx;
        private int ny;
        public Vector3d[] vertdata;
        public Vector3d[] normals;
        public int[] indexes;
        public Vector4 color;
        public Vector2[] textCoords;

        public List<Sphere> neighbours = new List<Sphere>();
        public bool isInContainer = true;

        public Sphere(double radius, Vector3d position, Vector3d speed, Vector4 color,
            int nx = 32, int ny = 32)
        {
            this.radius = radius;
            this.position = position;
            this.speed = speed;
            this.nx = nx;
            this.ny = ny;
            this.color = color;

            InitVertex();
            GenerateIndexArray();
            InitTexCoords();
        }

        public void InitVertex()
        {
            vertdata = new Vector3d[2 * (nx + 1) * ny];
            int ix, iy, num = 0;
            double x, y, z, sy, cy, sy1, cy1, sx, cx,
                piy, pix, ay, ay1, ax, tx, ty, ty1, dnx, dny, diy;
            dnx = 1.0 / (double)nx;
            dny = 1.0 / (double)ny;
            piy = Math.PI * dny;
            pix = Math.PI * dnx;

            for (iy = 0; iy < ny; iy++)
            {
                diy = (double)iy;
                ay = diy * piy;
                sy = Math.Sin(ay);
                cy = Math.Cos(ay);
                ty = diy * dny;
                ay1 = ay + piy;
                sy1 = Math.Sin(ay1);
                cy1 = Math.Cos(ay1);
                ty1 = ty + dny;
                for (ix = 0; ix <= nx; ix++)
                {
                    ax = 2.0 * ix * pix;
                    sx = Math.Sin(ax);
                    cx = Math.Cos(ax);
                    x = radius * sy * cx;
                    y = radius * sy * sx;
                    z = radius * cy;
                    tx = (double)ix * dnx;

                    vertdata[num] = new Vector3d(x, y, z);
                    num++;

                    x = radius * sy1 * cx;
                    y = radius * sy1 * sx;
                    z = radius * cy1;

                    vertdata[num] = new Vector3d(x, y, z);
                    num++;
                }
            }

            normals = vertdata;
        }

        private void GenerateIndexArray()
        {
            int length = 3 * (vertdata.Length - 2);
            indexes = new int[length];
            int n = 0;

            for (int i = 0; i < length - 2; i += 3)
            {
                indexes[i] = n;
                indexes[i + 1] = n + 1;
                indexes[i + 2] = n + 2;
                n++;
            }
        }

        private void InitTexCoords()
        {
            textCoords = new Vector2[indexes.Length];

            for (int i = 0; i < textCoords.Length - 3; i += 4)
            {
                textCoords[i] = new Vector2(0f, 0f);
                textCoords[i + 1] = new Vector2(1f, 0f);
                textCoords[i + 2] = new Vector2(1f, 1f);
                textCoords[i + 3] = new Vector2(0f, 1f);
            }
        }
    }
}
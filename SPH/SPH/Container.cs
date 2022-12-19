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
    internal class Container
    {
        public double bottom, top, left, right, front, back;
        public Vector3d[] vertdata, normals;
        public int[] indexes;
        public Vector4 color;

        public Container(Vector4 color,
            double bottom, double top, double left, double right, double front, double back)
        {
            this.color = color;
            this.bottom = bottom;
            this.top = top;
            this.left = left;
            this.right = right;
            this.front = front;
            this.back = back;

            InitVertex();
        }

        private void InitVertex()
        {
            vertdata = new Vector3d[20]
            {
                new Vector3d(left, bottom, front),new Vector3d(left, bottom, back),
                    new Vector3d(right, bottom, back),new Vector3d(right, bottom, front),   // bottom
                new Vector3d(left, bottom, front),new Vector3d(left, top, front),
                    new Vector3d(right, top, front),new Vector3d(right, bottom, front),     // front
                new Vector3d(left, bottom, back),new Vector3d(left, top, back),
                    new Vector3d(right, top, back),new Vector3d(right, bottom, back),       // back
                new Vector3d(left, bottom, front),new Vector3d(left, top, front),
                    new Vector3d(left, top, back),new Vector3d(left, bottom, back),         // left
                new Vector3d(right, bottom, front),new Vector3d(right, top, front),
                    new Vector3d(right, top, back),new Vector3d(right, bottom, back)        // right

            };

            normals = new Vector3d[20]
            {
                new Vector3d(0, bottom, 0),new Vector3d(0, bottom, 0),
                    new Vector3d(0, -bottom, 0),new Vector3d(0, bottom, 0),      // bottom
                new Vector3d(0, 0, -front),new Vector3d(0, 0, front),
                    new Vector3d(0, 0, front),new Vector3d(0, 0, front),        // front
                new Vector3d(0, 0, back),new Vector3d(0, 0, -back),
                    new Vector3d(0, 0, -back),new Vector3d(0, 0,-back),            //back
                new Vector3d(-left, 0, 0),new Vector3d(-left, 0, 0),
                    new Vector3d(-left, 0, 0),new Vector3d(-left, 0, 0),            // left
                new Vector3d(right, 0, 0),new Vector3d(right, 0, 0),
                    new Vector3d(right, 0, 0),new Vector3d(right, 0, 0)
            };

            indexes = new int[]
            {
                0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19
            };
        }
    }
}

using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SPH
{
    internal class SPH
    {
        private const double mass = 0.2;
        private Vector3d g = new Vector3d(0, -9.8, 0);

        private static double rad = 0.25, cell = 0.8;
        private static double ratioCoefficient = 210;
        private double bottom, top, left, right, front, back;

        private List<Sphere> spheresSet = new List<Sphere>();
        private List<Sphere> borderSpheres = new List<Sphere>();
        private Container container;

        private static double currTime;
        private static double p0;

        CollisionsResolver collisionsResolver;
        Drawing draw;

        public SPH(double bottom, double top,
            double left, double right, double front, double back)
        {
            this.bottom = bottom;
            this.top = top;
            this.left = left;
            this.right = right;
            this.front = front;
            this.back = back;

            SetDefaultScene();
            p0 = CalculateInitDensity();
        }

        private double DiffKernel(double r)
        {
            if (r >= 0 && r <= cell)
            {
                return 945 * Math.Pow(Math.Pow(cell, 2) - Math.Pow(r, 2), 2)
                    / (32 * Math.PI * Math.Pow(cell, 9));
            }

            return 0;
        }

        private double Diff2Kernel(double r)
        {
            if (r >= 0 && r <= cell)
            {
                return 45 * (cell - r) / (Math.PI * Math.Pow(cell, 6));
            }

            return 0;
        }

        private double Kernel(double r)
        {
            if (r >= 0 && r <= cell)
            {
                return 315 * Math.Pow(Math.Pow(cell, 2) - Math.Pow(r, 2), 3)
                    / (64 * Math.PI * Math.Pow(cell, 9));
            }

            return 0;
        }

        private void SetDefaultScene()
        {
            container = new Container(new(0f, 1f, 1f, 0.3f),
                bottom, top, left, right, front, back);

            SetBorders();

            double netBorder = -1;
            double offset = 2e-2;
            int layersX = Convert.ToInt32((netBorder - left) / (2 * rad + offset));
            int layersY = Convert.ToInt32((top - bottom) / (2 * rad + offset));
            int layersZ = Convert.ToInt32((front - back) / (2 * rad + offset));

            for (int x = 1; x <= layersX; x++)
            {
                for (int y = 1; y <= layersY; y++)
                {
                    for (int z = 1; z <= layersZ; z++)
                    {
                        spheresSet.Add(new Sphere(rad,
                        new Vector3d(left + rad * (2 * (x + offset) - 1),
                                     bottom + rad * (2 * (y + offset) - 1),
                                     back + rad * (2 * (z + offset) - 1)),
                        new Vector3d(0, 0, 0), new(1f, 0f, 0f, 1f), 16, 16));
                    }
                }
            }

            DefineNeighbours();
        }

        private void SetBorders()
        {
            int width = 3;
            int countX = Convert.ToInt32((right - left) / (2 * rad));
            int countY = Convert.ToInt32((top - bottom) / (2 * rad));
            int countZ = Convert.ToInt32((front - back) / (2 * rad));

            // floor
            for (int x = 1 - width; x <= countX + width; x++)
            {
                for (int y = 1; y <= width; y++)
                {
                    for (int z = 1 - width; z <= countZ + width; z++)
                    {
                        borderSpheres.Add(new Sphere(rad,
                            new Vector3d(left + rad * (2 * x - 1),
                                     bottom - rad * (2 * y - 1),
                                     back + rad * (2 * z - 1)),
                            new Vector3d(0, 0, 0), new(0f, 1f, 0f, 0.2f), 16, 16));
                    }
                }
            }

            // left and right wall
            for (int x = 1; x <= width; x++)
            {
                for (int y = 1; y <= countY; y++)
                {
                    for (int z = 1; z <= countZ; z++)
                    {
                        borderSpheres.Add(new Sphere(rad,
                            new Vector3d(left - rad * (2 * x - 1),
                                     bottom + rad * (2 * y - 1),
                                     back + rad * (2 * z - 1)),
                            new Vector3d(0, 0, 0), new(0f, 1f, 0f, 0.2f), 16, 16));

                        borderSpheres.Add(new Sphere(rad,
                            new Vector3d(right + rad * (2 * x - 1),
                                     bottom + rad * (2 * y - 1),
                                     back + rad * (2 * z - 1)),
                            new Vector3d(0, 0, 0), new(0f, 1f, 0f, 0.2f), 16, 16));
                    }
                }
            }

            // back and front wall
            for (int x = 1 - width; x <= countX + width; x++)
            {
                for (int y = 1; y <= countY; y++)
                {
                    for (int z = 1; z <= width; z++)
                    {
                        borderSpheres.Add(new Sphere(rad,
                            new Vector3d(left + rad * (2 * x - 1),
                                     bottom + rad * (2 * y - 1),
                                     back - rad * (2 * z - 1)),
                            new Vector3d(0, 0, 0), new Vector4(0, 1, 0, 0.2f), 16, 16));

                        borderSpheres.Add(new Sphere(rad,
                            new Vector3d(left + rad * (2 * x - 1),
                                     bottom + rad * (2 * y - 1),
                                     front + rad * (2 * z - 1)),
                            new Vector3d(0, 0, 0), new Vector4(0, 1, 0, 0.2f), 16, 16));
                    }
                }
            }
        }

        public void DefineNeighbours()
        {
            foreach (Sphere sphere in spheresSet)
            {
                sphere.neighbours.Clear();
                Vector3d currentPosition = sphere.position;

                foreach (Sphere part in spheresSet)
                {
                    Vector3d distance = currentPosition - part.position;
                    if (distance.Length <= cell && sphere != part)
                    {
                        sphere.neighbours.Add(part);
                    }
                }
                foreach (Sphere part in borderSpheres)
                {
                    Vector3d distance = currentPosition - part.position;
                    if (distance.Length <= cell && sphere != part)
                    {
                        sphere.neighbours.Add(part);
                    }
                }
            }
            foreach (Sphere border in borderSpheres)
            {
                border.neighbours.Clear();
                Vector3d currentPosition = border.position;

                /*var allSpheres = spheresSet.Zip(borderSpheres,
                    (s, b) => new { Set = s, Boarder = b });*/

                foreach (Sphere part in spheresSet)
                {
                    Vector3d distance = currentPosition - part.position;
                    if (distance.Length <= cell && border != part)
                    {
                        border.neighbours.Add(part);
                    }
                }
                foreach (Sphere part in borderSpheres)
                {
                    Vector3d distance = currentPosition - part.position;
                    if (distance.Length <= cell && border != part)
                    {
                        border.neighbours.Add(part);
                    }
                }
            }
        }

        private double Density(Sphere sphere)
        {
            double density = 0;
            foreach (Sphere neighbour in sphere.neighbours)
            {
                Vector3d distance = sphere.position - neighbour.position;
                density += mass * Kernel(distance.Length);
            }
            return density;
        }

        private double CalculateInitDensity()
        {
            double initDensity = 0;

            foreach (Sphere sphere in spheresSet)
            {
                initDensity += Density(sphere);
            }

            initDensity /= spheresSet.Count;

            return initDensity;
        }

        private double Pressure(Sphere sphere, double c0 = 57.5)
        {
            if (Density(sphere) <= p0)
                return 0;

            return c0 * (Density(sphere) - p0);
        }

        private Vector3d PressureForce(Sphere sphere)
        {
            Vector3d pressureForce = new Vector3d(0, 0, 0);

            foreach (Sphere neighbour in sphere.neighbours)
            {
                Vector3d distance = sphere.position - neighbour.position;

                double d1 = Density(sphere), d2 = Density(neighbour);
                double p1 = Pressure(sphere), p2 = Pressure(neighbour);

                if (d1 != 0 && d2 != 0)
                {
                    pressureForce += mass * mass * (p1 / Math.Pow(d1, 2) + p2 / Math.Pow(d2, 2))
                        * DiffKernel(distance.Length) * distance;
                }
            }

            return pressureForce;
        }

        private Vector3d ViscosityForce(Sphere sphere, double nu = 4)
        {
            Vector3d viscosityForce = new Vector3d(0, 0, 0);

            foreach (Sphere neighbour in sphere.neighbours)
            {
                Vector3d distance = sphere.position - neighbour.position;

                double d1 = Density(sphere), d2 = Density(neighbour);

                if (d1 != 0 && d2 != 0)
                {
                    viscosityForce += mass * mass * (neighbour.speed - sphere.speed)
                             * Diff2Kernel(distance.Length) / (d1 * d2);
                }
            }
            viscosityForce *= nu;

            return viscosityForce;
        }

        private Vector3d ResultForse(Sphere sphere)
        {
            Vector3d v1 = PressureForce(sphere);
            Vector3d v2 = ViscosityForce(sphere);
            Vector3d v3 = mass * g;
            Vector3d res = v1 + v2 + v3;

            if (Double.IsNaN(res.X) || Double.IsNaN(res.Y) ||
                    Double.IsNaN(res.Z))
            {
                Console.WriteLine("NaN!!");
            }

            return res;
        }

        public void Simulate(FrameEventArgs e, int program,
            Matrix4 _viewMatrix, Matrix4 _projMatrix, Matrix4 _normalMatrix, Matrix4 _rotateMatrix,
            Bitmap texture)
        {
            currTime = e.Time;

            List<Vector3d> speeds = new List<Vector3d>();

            foreach (Sphere sphere in spheresSet)
            {
                Vector3d newSpeed = ResultForse(sphere) * currTime / (ratioCoefficient * mass)
                    + sphere.speed;
                speeds.Add(newSpeed);
            }

            for (int i = 0; i < spheresSet.Count; i++)
            {
                spheresSet[i].speed = speeds[i];
                spheresSet[i].position += spheresSet[i].speed * currTime;

                collisionsResolver = new CollisionsResolver(spheresSet[i]);
                collisionsResolver.WithBorders(container, cell);
            }

            draw = new Drawing(program,
                    _projMatrix, _viewMatrix, _normalMatrix, _rotateMatrix);
            draw.Draw(spheresSet, texture);
            draw.Draw(container);
        }
    }
}

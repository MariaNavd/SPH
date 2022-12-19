using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPH
{
    internal class CollisionsResolver
    {
        private Sphere sphere;

        public CollisionsResolver(Sphere sphere)
        {
            this.sphere = sphere;
        }

        public void WithBorders(Container container, double cell)
        {
            IsInContainer(container);

            if (true)
            {
                if (sphere.position.Y < container.bottom + sphere.radius)
                {
                    sphere.position.Y = container.bottom + sphere.radius;
                }
                if (sphere.position.X < container.left + sphere.radius)
                {
                    sphere.position.X = container.left + sphere.radius;
                }
                if (sphere.position.X > container.right - sphere.radius)
                {
                    sphere.position.X = container.right - sphere.radius;
                }
                if (sphere.position.Z < container.back + sphere.radius)
                {
                    sphere.position.Z = container.back + sphere.radius;
                }
                if (sphere.position.Z > container.front - sphere.radius)
                {
                    sphere.position.Z = container.front - sphere.radius;
                }
            }
        }

        private void IsInContainer(Container container)
        {
            if (sphere.position.Y >= container.top)
            {
                sphere.isInContainer = false;
            }
            if (sphere.position.Y <= container.top - sphere.radius &&
                (sphere.position.Y >= container.bottom + sphere.radius ||
                 sphere.position.X >= container.left + sphere.radius ||
                 sphere.position.X <= container.right - sphere.radius ||
                 sphere.position.Z >= container.back + sphere.radius ||
                 sphere.position.Z <= container.front - sphere.radius))
            {
                sphere.isInContainer = true;
            }
        }
    }
}
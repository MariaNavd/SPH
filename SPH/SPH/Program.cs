using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK;
using System.Drawing;
using OpenTK.Input;
using System.Text;
using OpenTK.Platform;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace SPH
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var window = new Window())
            {
                window.Title = "Smoothed Particle Hydrodynamics";
                window.Run(60);
            }
        }
    }

    class Window : GameWindow
    {
        private Matrix4 _projMatrix;
        private Matrix4 _viewMatrix;
        private Matrix4 _normalMatrix;
        private Matrix4 _modelMatrix;
        private Matrix4 _rotateMatrix;
        private bool _isUpdating = false;

        private static int count = 0;
        private static System.Diagnostics.Stopwatch watch;

        private static bool moveUp, moveDown, moveLeft, moveRight, snapshot = false, autoRotate = false;
        private static float xangle = 0, yangle = 0;
        private static float deltaWheel;

        private const double bottom = -3, top = 2, left = -4, right = 4, front = 2, back = -4;
        int program;
        public SPH sph;
        public Bitmap texture;

        private float frameTime = 0.0f;
        private int fps = 0;

        Drawing dr;

        public Window() : base(600, 600, new GraphicsMode(32, 24, 0, 8)) { }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var vShaderSource =
                @"
                    #version 330
                     
                    in vec3 aPosition;
                    in vec4 aNormal;
                    in vec2 textPosition;

                    uniform mat4 uMvpMatrix;
                    uniform mat4 uNormalMatrix;

                    out float vNdotL;
                    out vec2 uv;
 
                    void main()
                    {
                        uv = textPosition;
                        gl_Position = uMvpMatrix * vec4(aPosition, 1.0);

                        vec4 normal = uNormalMatrix * aNormal;
                        vec3 lightDir = vec3(10, 50, 30);
                        vNdotL = max(dot(normalize(normal.xyz), normalize(lightDir)), 0.0);
                    }
                ";
            var fShaderSource =
                @"
                    #version 330
                    precision mediump float;

                    in float vNdotL;
                    in vec2 uv;

                    uniform vec4 uColor;
                    uniform sampler2D texture;
                    uniform bool isTextured;

                    out vec4 fragColor;
 
                    void main()
                    {
                        if (isTextured)
                        {
                            float ambient = 0.5;
                            float lighting = max(vNdotL, ambient);

                            fragColor.rgb = lighting * texture2D(texture, uv).rgb;
                            fragColor.a = 1;
                        }
                        else
                        {
                            vec3 diffuseLight = vec3(1.0, 1.0, 1.0);
                            vec3 diffuseColor = diffuseLight * uColor.rgb * vNdotL;

                            vec3 ambientLight = vec3(0.5, 0.5, 0.5);
                            vec3 ambientColor = ambientLight * uColor.rgb;

                            fragColor = vec4(diffuseColor + ambientColor, uColor.a);
                        }
                    }
                ";
            var vShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vShader, vShaderSource);
            GL.CompileShader(vShader);
            Console.WriteLine(GL.GetShaderInfoLog(vShader));
            var fShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fShader, fShaderSource);
            GL.CompileShader(fShader);
            Console.WriteLine(GL.GetShaderInfoLog(fShader));

            program = GL.CreateProgram();
            GL.AttachShader(program, vShader);
            GL.AttachShader(program, fShader);
            GL.LinkProgram(program);
            GL.UseProgram(program);

            _viewMatrix = Matrix4.LookAt(
                eye: new Vector3(3f, 7f, 10f),
                target: new Vector3(0f, 0f, 0f),
                up: new Vector3(0f, 1f, 0f));
            _modelMatrix = Matrix4.Identity;
            _normalMatrix = Matrix4.Identity;

            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Texture2D);

            watch = System.Diagnostics.Stopwatch.StartNew();

            sph = new SPH(bottom, top, left, right, front, back);
            dr = new Drawing(program, _projMatrix, _viewMatrix, _normalMatrix, _rotateMatrix);

            GL.DetachShader(program, vShader);
            GL.DeleteShader(vShader);
            GL.DetachShader(program, fShader);
            GL.DeleteShader(fShader);

            texture = new Bitmap("..\\..\\..\\data\\text2.jpg");
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            frameTime += (float)e.Time;
            fps++;
            if (frameTime >= 1.0f)
            {
                Title = $"LearnOpenTK FPS - {fps}";
                frameTime = 0.0f;
                fps = 0;
            }

            base.OnUpdateFrame(e);

            if (!_isUpdating)
            {
                return;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            if (!_isUpdating)
            {
                _isUpdating = true;
            }

            watch.Stop();
            float deltaTime = (float)watch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
            watch.Restart();

            if (autoRotate)
            {
                xangle += deltaTime / 2;
                yangle += deltaTime;
            }
            if (moveRight) yangle -= deltaTime;
            if (moveLeft) yangle += deltaTime;
            if (moveUp) xangle += deltaTime;
            if (moveDown) xangle -= deltaTime;


            _rotateMatrix = Matrix4.CreateRotationY(yangle) * Matrix4.CreateRotationX(xangle);

            if (!snapshot)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                if (count == 4)
                {
                    sph.DefineNeighbours();
                    count = 0;
                }
                count++;

                sph.Simulate(e, program, _viewMatrix, _projMatrix, _normalMatrix, _rotateMatrix, texture);
                SwapBuffers();
            }
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            if (e.Key.ToString() == "W") moveUp = false;
            else if (e.Key.ToString() == "S") moveDown = false;
            else if (e.Key.ToString() == "D") moveRight = false;
            else if (e.Key.ToString() == "A") moveLeft = false;
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            //Console.WriteLine("key press " + e.Key);
            if (e.Key.ToString() == "W") moveUp = true;
            else if (e.Key.ToString() == "S") moveDown = true;
            else if (e.Key.ToString() == "D") moveRight = true;
            else if (e.Key.ToString() == "A") moveLeft = true;
            else if (e.Key.ToString() == "C")
            {
                if (snapshot)
                    snapshot = false;
                else
                    snapshot = true;
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            deltaWheel = e.DeltaPrecise;
            Console.WriteLine(deltaWheel.ToString() + " куку епта");
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);

            float aspect = (float)Width / Height;
            _projMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(40f), aspect, 0.1f, 100f);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            GL.UseProgram(0);
            GL.DeleteProgram(program);
        }
    }
}
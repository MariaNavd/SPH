using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace SPH
{
    internal class Drawing
    {
        private int program;
        private Matrix4 _projMatrix;
        private Matrix4 _viewMatrix;
        private Matrix4 _normalMatrix;
        private Matrix4 _rotateMatrix;
        private Matrix4 _modelMatrix;

        private int _uMvpMatrixLocation;
        private int _uNormalMatrixLocation;
        private int _uColor;
        private int _textureLocation;
        private int _isTexturedLocation;
        private Matrix4 _mvpMatrix;
        private int _amountOfVertices = 0;

        public Drawing(int program,
            Matrix4 _projMatrix, Matrix4 _viewMatrix, Matrix4 _normalMatrix, Matrix4 _rotateMatrix)
        {
            this.program = program;
            this._projMatrix = _projMatrix;
            this._viewMatrix = _viewMatrix;
            this._normalMatrix = _normalMatrix;
            this._rotateMatrix = _rotateMatrix;
        }

        private void InitTexture(Bitmap texture)
        {
            _textureLocation = GL.GetUniformLocation(program, "texture");

            int tt;
            GL.GenTextures(1, out tt);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _textureLocation);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            BitmapData data = texture.LockBits(new System.Drawing.Rectangle(0, 0, texture.Width, texture.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            texture.UnlockBits(data);
        }

        private int InitBuffers(List<Sphere> spheresSet, int isTextured)
        {
            InitArrayBuffer(spheresSet, "aPosition");
            InitArrayBuffer(spheresSet, "aNormal");
            if (Convert.ToBoolean(isTextured))
            {
                InitArrayBuffer(spheresSet, "textPosition");
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            int indexBuffer;
            GL.CreateBuffers(1, out indexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);

            int length = 0;
            foreach (Sphere sphere in spheresSet)
            {
                GL.BufferData(BufferTarget.ElementArrayBuffer,
                    sizeof(int) * sphere.indexes.Length, sphere.indexes, BufferUsageHint.StaticDraw);
                length += sphere.indexes.Length;
            }

            return length;
        }

        private void InitArrayBuffer(List<Sphere> spheresSet, string attributeName)
        {
            int vbo;
            GL.CreateBuffers(1, out vbo);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            foreach (Sphere sphere in spheresSet)
            {
                if (attributeName == "aPosition")
                {
                    GL.BufferData(BufferTarget.ArrayBuffer,
                        (IntPtr)(sphere.vertdata.Length * Vector3d.SizeInBytes), sphere.vertdata, BufferUsageHint.StaticDraw);
                }
                else if (attributeName == "aNormal")
                {
                    GL.BufferData(BufferTarget.ArrayBuffer,
                        (IntPtr)(sphere.normals.Length * Vector3d.SizeInBytes), sphere.normals, BufferUsageHint.StaticDraw);
                }
                else if (attributeName == "textPosition")
                {
                    GL.BufferData(BufferTarget.ArrayBuffer,
                        (IntPtr)(sphere.textCoords.Length * Vector2.SizeInBytes), sphere.textCoords, BufferUsageHint.StaticDraw);
                }
            }

            int attributeLocation = GL.GetAttribLocation(program, attributeName);
            GL.VertexAttribPointer(attributeLocation, 3, VertexAttribPointerType.Double, false, 0, 0);
            GL.EnableVertexAttribArray(attributeLocation);
        }

        private int InitBuffers(Container container)
        {
            InitArrayBuffer(container, "aPosition");
            InitArrayBuffer(container, "aNormal");

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            int indexBuffer;
            GL.CreateBuffers(1, out indexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);

            GL.BufferData(BufferTarget.ElementArrayBuffer,
                sizeof(int) * container.indexes.Length, container.indexes, BufferUsageHint.StaticDraw);

            return container.indexes.Length;
        }

        private void InitArrayBuffer(Container container, string attributeName)
        {
            int vbo;
            GL.CreateBuffers(1, out vbo);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            if (attributeName == "aPosition")
            {
                GL.BufferData(BufferTarget.ArrayBuffer,
                    (IntPtr)(container.vertdata.Length * Vector3d.SizeInBytes), container.vertdata, BufferUsageHint.StaticDraw);
            }
            else if (attributeName == "aNormal")
            {
                GL.BufferData(BufferTarget.ArrayBuffer,
                    (IntPtr)(container.normals.Length * Vector3d.SizeInBytes), container.normals, BufferUsageHint.StaticDraw);
            }

            int attributeLocation = GL.GetAttribLocation(program, attributeName);
            GL.VertexAttribPointer(attributeLocation, 3, VertexAttribPointerType.Double, false, 0, 0);
            GL.EnableVertexAttribArray(attributeLocation);
        }

        private Matrix4 ModelMatrix(Sphere sphere)
        {
            return Matrix4.CreateScale(1f, 1f, 1f) *
                   Matrix4.CreateTranslation((float)sphere.position.X,
                        (float)sphere.position.Y, (float)sphere.position.Z) * _rotateMatrix;
        }

        private Matrix4 ModelMatrix(Container container)
        {
            return Matrix4.CreateScale(1f, 1f, 1f) * _rotateMatrix;
        }

        public void Draw(List<Sphere> spheresSet, Bitmap texture = null)
        {
            int isTextured = 0;
            if (texture != null)
            {
                InitTexture(texture);
                isTextured = 1;
            }
            else
            {
                _uColor = GL.GetUniformLocation(program, "uColor");
                GL.Uniform4(_uColor, 1f, 0f, 0f, 1f);
            }

            _uMvpMatrixLocation = GL.GetUniformLocation(program, "uMvpMatrix");
            _uNormalMatrixLocation = GL.GetUniformLocation(program, "uNormalMatrix");
            _isTexturedLocation = GL.GetUniformLocation(program, "isTextured");
            GL.Uniform1(_isTexturedLocation, isTextured);

            _amountOfVertices = InitBuffers(spheresSet, isTextured);

            foreach (Sphere sphere in spheresSet)
            {
                _modelMatrix = ModelMatrix(sphere);

                _mvpMatrix = _modelMatrix * _viewMatrix * _projMatrix;

                Matrix4.Invert(ref _modelMatrix, out _normalMatrix);
                Matrix4.Invert(_modelMatrix);
                Matrix4.Transpose(_modelMatrix);
                GL.UniformMatrix4(_uNormalMatrixLocation, false, ref _modelMatrix);
                GL.UniformMatrix4(_uMvpMatrixLocation, false, ref _mvpMatrix);

                GL.DrawElements(PrimitiveType.Triangles, _amountOfVertices, DrawElementsType.UnsignedInt, 0);
            }
        }

        public void Draw(Container container)
        {
            _uMvpMatrixLocation = GL.GetUniformLocation(program, "uMvpMatrix");
            _uNormalMatrixLocation = GL.GetUniformLocation(program, "uNormalMatrix");
            _uColor = GL.GetUniformLocation(program, "uColor");
            _isTexturedLocation = GL.GetUniformLocation(program, "isTextured");

            _amountOfVertices = InitBuffers(container);

            _modelMatrix = ModelMatrix(container);
            GL.Uniform4(_uColor, container.color);
            GL.Uniform1(_isTexturedLocation, 0);

            _mvpMatrix = _modelMatrix * _viewMatrix * _projMatrix;

            Matrix4.Invert(ref _modelMatrix, out _normalMatrix);
            Matrix4.Invert(_modelMatrix);
            Matrix4.Transpose(_modelMatrix);
            GL.UniformMatrix4(_uNormalMatrixLocation, false, ref _modelMatrix);
            GL.UniformMatrix4(_uMvpMatrixLocation, false, ref _mvpMatrix);

            GL.DrawElements(PrimitiveType.Quads, _amountOfVertices, DrawElementsType.UnsignedInt, 0);
        }
    }
}

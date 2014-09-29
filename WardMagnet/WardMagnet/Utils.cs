using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Resources;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Toolkit.Graphics;
using Color = SharpDX.Color;
using Font = SharpDX.Direct3D9.Font;
using PrimitiveType = SharpDX.Direct3D9.PrimitiveType;
using Rectangle = SharpDX.Rectangle;
using Texture = SharpDX.Direct3D9.Texture;

namespace WardMagnet
{
    static class Common
    {
        public static bool IsOnScreen(Vector3 vector)
        {
            Vector2 screen = Drawing.WorldToScreen(vector);
            if (screen[0] < 0 || screen[0] > Drawing.Width || screen[1] < 0 || screen[1] > Drawing.Height)
                return false;
            return true;
        }

        public static bool IsOnScreen(Vector2 vector)
        {
            Vector2 screen = vector;
            if (screen[0] < 0 || screen[0] > Drawing.Width || screen[1] < 0 || screen[1] > Drawing.Height)
                return false;
            return true;
        }

        public static Size ScaleSize(this Size size, float scale, Vector2 mainPos = default(Vector2))
        {
            size.Height = (int)(((size.Height - mainPos.Y) * scale) + mainPos.Y);
            size.Width = (int)(((size.Width - mainPos.X) * scale) + mainPos.X);
            return size;
        }

        public static bool IsInside(Vector2 mousePos, Size windowPos, int width, int height)
        {
            return Utils.IsUnderRectangle(mousePos, windowPos.Width, windowPos.Height, width, height);
        }
    }


    static class DirectXDrawer
    {

        public struct PositionColored
        {
            public static readonly VertexFormat FVF = VertexFormat.Position | VertexFormat.Diffuse;
            public static readonly int Stride = Vector3.SizeInBytes + sizeof(int);

            public Vector3 Position;
            public int Color;

            public PositionColored(Vector3 pos, int col)
            {
                Position = pos;
                Color = col;
            }
        }

        private static void InternalRender(Vector3 target)
        {
            Drawing.Direct3DDevice.VertexShader = null;
            Drawing.Direct3DDevice.PixelShader = null;
            Drawing.Direct3DDevice.SetRenderState(RenderState.AlphaBlendEnable, true);
            Drawing.Direct3DDevice.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
            Drawing.Direct3DDevice.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            Drawing.Direct3DDevice.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            Drawing.Direct3DDevice.SetRenderState(RenderState.Lighting, 0);
            Drawing.Direct3DDevice.SetRenderState(RenderState.ZEnable, true);
            Drawing.Direct3DDevice.SetRenderState(RenderState.AntialiasedLineEnable, true);
            Drawing.Direct3DDevice.SetRenderState(RenderState.Clipping, true);
            Drawing.Direct3DDevice.SetRenderState(RenderState.EnableAdaptiveTessellation, true);
            Drawing.Direct3DDevice.SetRenderState(RenderState.MultisampleAntialias, true);
            Drawing.Direct3DDevice.SetRenderState(RenderState.ShadeMode, ShadeMode.Gouraud);
            Drawing.Direct3DDevice.SetTexture(0, null);
            Drawing.Direct3DDevice.SetRenderState(RenderState.CullMode, Cull.None);
        }

        public static void DrawLine(Vector3 from, Vector3 to, System.Drawing.Color color)
        {
            var vertices = new PositionColored[2];
            vertices[0] = new PositionColored(Vector3.Zero, color.ToArgb());
            from = from.SwitchYZ();
            to = to.SwitchYZ();
            vertices[1] = new PositionColored(to - from, color.ToArgb());

            InternalRender(from);

            Drawing.Direct3DDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices.Length / 2, vertices);
        }

        public static void DrawLine(Line line, Vector3 from, Vector3 to, ColorBGRA color, Size size = default(Size), float[] scale = null, float rotation = 0.0f)
        {
            if (line != null)
            {
                from = from.SwitchYZ();
                to = to.SwitchYZ();
                Matrix nMatrix = (scale != null ? Matrix.Scaling(scale[0], scale[1], 0) : Matrix.Scaling(1)) *
                                 Matrix.RotationZ(rotation) * Matrix.Translation(from);
                Vector3[] vec = { from, to };
                line.DrawTransform(vec, nMatrix, color);
            }

        }
    }
}
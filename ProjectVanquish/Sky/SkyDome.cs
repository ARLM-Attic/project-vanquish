﻿/*
 * Skydome Component
 * 
 * Alex Urbano Álvarez
 * XNA Community Coordinator
 * 
 * goefuika@gmail.com
 * 
 * http://elgoe.blogspot.com
 * http://www.codeplex.com/XNACommunity
 *
 * Modified by Neil Knight to work with Project Vanquish and converted to XNA 4.0
 */

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using ProjectVanquish.Renderers;
using ProjectVanquish.Core;

namespace ProjectVanquish.Sky
{
    public class SkyDome
    {
        #region Fields
        /// <summary>
        /// Theta
        /// </summary>
        private float theta;

        /// <summary>
        /// Phi
        /// </summary>
        private float phi;

        /// <summary>
        /// Previous Theta and Phi
        /// </summary>
        private float previousTheta, previousPhi;

        /// <summary>
        /// Use Realtime
        /// </summary>
        private bool realTime;

        /// <summary>
        /// Textures
        /// </summary>
        Texture2D mieTex, rayleighTex, moonTex, glowTex, starsTex, permTex, gradTex;

        /// <summary>
        /// RenderTargets
        /// </summary>
        RenderTarget2D mieRT, rayleighRT;
        
        /// <summary>
        /// Effects
        /// </summary>
        Effect scatterEffect, texturedEffect, noiseEffect;

        /// <summary>
        /// Fullscreen Quad
        /// </summary>
        QuadRenderer quad;

        /// <summary>
        /// SkyDome Parameters
        /// </summary>
        SkyDomeParameters parameters;

        VertexPositionTexture[] domeVerts, quadVerts;
        short[] ib, quadIb;

        int DomeN;
        int DVSize;
        int DISize;

        Vector4 sunColor;

        private float inverseCloudVelocity;
        private float cloudCover;
        private float cloudSharpness;
        private float numTiles;
        #endregion

        #region Properties
        /// <summary>
        /// Gets/Sets Theta value
        /// </summary>
        public float Theta { get { return theta; } set { theta = value; } }

        /// <summary>
        /// Gets/Sets Phi value
        /// </summary>
        public float Phi { get { return phi; } set { phi = value; } }

        /// <summary>
        /// Gets/Sets actual time computation
        /// </summary>
        public bool RealTime
        {
            get { return realTime; }
            set { realTime = value; }
        }

        /// <summary>
        /// Gets/Sets the SkyDome parameters
        /// </summary>
        public SkyDomeParameters Parameters { get { return parameters; } set { parameters = value; } }

        /// <summary>
        /// Gets the Sun color
        /// </summary>
        public Vector4 SunColor { get { return sunColor; } }

        /// <summary>
        /// Gets/Sets InverseCloudVelocity value
        /// </summary>
        public float InverseCloudVelocity { get { return inverseCloudVelocity; } set { inverseCloudVelocity = value; } }

        /// <summary>
        /// Gets/Sets CloudCover value
        /// </summary>
        public float CloudCover { get { return cloudCover; } set { cloudCover = value; } }

        /// <summary>
        /// Gets/Sets CloudSharpness value
        /// </summary>
        public float CloudSharpness { get { return cloudSharpness; } set { cloudSharpness = value; } }

        /// <summary>
        /// Gets/Sets CloudSharpness value
        /// </summary>
        public float NumTiles { get { return numTiles; } set { numTiles = value; } }

        #endregion

        #region Contructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SkyDome"/> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="camera">The camera.</param>
        public SkyDome(GraphicsDevice device, ContentManager content, bool useRealTime = false)
        {
            realTime = useRealTime;
            parameters = new SkyDomeParameters();

            quad = new QuadRenderer(device);

            DomeN = 32;

            GeneratePermTex(device);

            // You can use SurfaceFormat.Color to increase performance / reduce quality
            mieRT = new RenderTarget2D(device, 128, 64, false, SurfaceFormat.Color, DepthFormat.None);
            rayleighRT = new RenderTarget2D(device, 128, 64, false, SurfaceFormat.Color, DepthFormat.None);

            // Clouds constants
            inverseCloudVelocity = 16.0f;
            CloudCover = -0.1f;
            CloudSharpness = 0.5f;
            numTiles = 16.0f;

            // Load Effects
            scatterEffect = content.Load<Effect>("Shaders/Sky/scatter");
            texturedEffect = content.Load<Effect>("Shaders/Sky/Textured");
            noiseEffect = content.Load<Effect>("Shaders/Sky/SNoise");

            // Load Textures
            moonTex = content.Load<Texture2D>("Textures/moon");
            glowTex = content.Load<Texture2D>("Textures/moonglow");
            starsTex = content.Load<Texture2D>("Textures/starfield");

            GenerateDome();
            GenerateMoon();
        }
        #endregion

        #region Members
        /// <summary>
        /// Applies the changes.
        /// </summary>
        public void ApplyChanges(GraphicsDevice device)
        {
            UpdateMieRayleighTextures(device);
        }

        /// <summary>
        /// Draws the component.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw(GraphicsDevice device,GameTime gameTime)
        {
            Matrix View = CameraManager.GetActiveCamera().ViewMatrix;
            Matrix Projection = CameraManager.GetActiveCamera().ProjectionMatrix;
            Matrix World = Matrix.CreateTranslation(CameraManager.GetActiveCamera().Position.X,
                                                    CameraManager.GetActiveCamera().Position.Y,
                                                    CameraManager.GetActiveCamera().Position.Z);

            if (previousTheta != theta || previousPhi != phi)
                UpdateMieRayleighTextures(device);

            this.sunColor = this.GetSunColor(-theta, 2);

            // Clear the Device
            device.Clear(ClearOptions.Target, new Vector4(1.0f), 1.0f, 0);
            device.RasterizerState = RasterizerState.CullNone;

            // Set the Scatter Effect parameters
            scatterEffect.CurrentTechnique = scatterEffect.Techniques["Render"];
            scatterEffect.Parameters["txMie"].SetValue(mieRT);
            scatterEffect.Parameters["txRayleigh"].SetValue(rayleighRT);
            scatterEffect.Parameters["WorldViewProjection"].SetValue(World * View * Projection);
            scatterEffect.Parameters["v3SunDir"].SetValue(new Vector3(-parameters.LightDirection.X, -parameters.LightDirection.Y, -parameters.LightDirection.Z));
            scatterEffect.Parameters["NumSamples"].SetValue(parameters.NumberOfSamples);
            scatterEffect.Parameters["fExposure"].SetValue(parameters.Exposure);
            scatterEffect.Parameters["StarsTex"].SetValue(starsTex);

            if (theta < Math.PI / 2.0f || theta > 3.0f * Math.PI / 2.0f)
                scatterEffect.Parameters["starIntensity"].SetValue((float)Math.Abs(Math.Sin(Theta + (float)Math.PI / 2.0f)));
            else
                scatterEffect.Parameters["starIntensity"].SetValue(0.0f);

            // Apply each pass
            foreach (EffectPass pass in scatterEffect.CurrentTechnique.Passes)
            {
                // Apply Effect
                pass.Apply();

                // Draw Primitives
                device.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, domeVerts, 0, this.DVSize, ib, 0, this.DISize);
            }

            // Draw Glow, Moon and Clouds
            DrawGlow(device);
            DrawMoon(device);
            DrawClouds(device, gameTime);

            // Reset RasterizerState
            device.RasterizerState = RasterizerState.CullCounterClockwise;

            // Store the old theta and phi values
            previousTheta = theta;
            previousPhi = phi;
        }

        /// <summary>
        /// Draws the clouds.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="gameTime">The game time.</param>
        private void DrawClouds(GraphicsDevice device, GameTime gameTime)
        {
            // Set Noise Effect parameters
            noiseEffect.CurrentTechnique = noiseEffect.Techniques["Noise"];
            noiseEffect.Parameters["World"].SetValue(Matrix.CreateScale(10000.0f) *
                Matrix.CreateTranslation(new Vector3(0, 0, -900)) *
                Matrix.CreateRotationX((float)Math.PI / 2.0f) *
                Matrix.CreateTranslation(CameraManager.GetActiveCamera().Position.X,
                                         CameraManager.GetActiveCamera().Position.Y,
                                         CameraManager.GetActiveCamera().Position.Z)
                                        );
            noiseEffect.Parameters["View"].SetValue(CameraManager.GetActiveCamera().ViewMatrix);
            noiseEffect.Parameters["Projection"].SetValue(CameraManager.GetActiveCamera().ProjectionMatrix);
            noiseEffect.Parameters["permTexture"].SetValue(permTex);
            noiseEffect.Parameters["time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds / inverseCloudVelocity);
            noiseEffect.Parameters["SunColor"].SetValue(sunColor);
            noiseEffect.Parameters["numTiles"].SetValue(numTiles);
            noiseEffect.Parameters["CloudCover"].SetValue(cloudCover);
            noiseEffect.Parameters["CloudSharpness"].SetValue(cloudSharpness);

            // Apply each pass
            foreach (EffectPass pass in noiseEffect.CurrentTechnique.Passes)
            {
                // Apply Effect
                pass.Apply();

                // Draw Primitives
                device.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, quadVerts, 0, 4, quadIb, 0, 2);
            }
        }
        
        /// <summary>
        /// Draws the glow.
        /// </summary>
        private void DrawGlow(GraphicsDevice device)
        {
            // Set the Effect parameters
            texturedEffect.CurrentTechnique = texturedEffect.Techniques["Textured"];
            texturedEffect.Parameters["World"].SetValue(
                Matrix.CreateRotationX(Theta + (float)Math.PI / 2.0f) *
                Matrix.CreateRotationY(-Phi + (float)Math.PI / 2.0f) *
                Matrix.CreateTranslation(parameters.LightDirection.X * 5,
                                         parameters.LightDirection.Y * 5,
                                         parameters.LightDirection.Z * 5) *
                Matrix.CreateTranslation(CameraManager.GetActiveCamera().Position.X,
                                         CameraManager.GetActiveCamera().Position.Y,
                                         CameraManager.GetActiveCamera().Position.Z));
            texturedEffect.Parameters["View"].SetValue(CameraManager.GetActiveCamera().ViewMatrix);
            texturedEffect.Parameters["Projection"].SetValue(CameraManager.GetActiveCamera().ProjectionMatrix);
            texturedEffect.Parameters["Texture"].SetValue(this.glowTex);
            if (theta < Math.PI / 2.0f || theta > 3.0f * Math.PI / 2.0f)
                texturedEffect.Parameters["alpha"].SetValue((float)Math.Abs(Math.Sin(Theta + (float)Math.PI / 2.0f)));
            else
                texturedEffect.Parameters["alpha"].SetValue(0.0f);

            // Apply each pass
            foreach (EffectPass pass in texturedEffect.CurrentTechnique.Passes)
            {
                // Apply Effect
                pass.Apply();

                // Draw Primitives
                device.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, quadVerts, 0, 4, quadIb, 0, 2);
            }
        }

        /// <summary>
        /// Draws the moon.
        /// </summary>
        private void DrawMoon(GraphicsDevice device)
        {
            // Set Textured Effect parameters
            texturedEffect.CurrentTechnique = texturedEffect.Techniques["Textured"];
            texturedEffect.Parameters["World"].SetValue(
                Matrix.CreateRotationX(Theta + (float)Math.PI / 2.0f) *
                Matrix.CreateRotationY(-Phi + (float)Math.PI / 2.0f) *
                Matrix.CreateTranslation(parameters.LightDirection.X * 15,
                                         parameters.LightDirection.Y * 15,
                                         parameters.LightDirection.Z * 15) *
                Matrix.CreateTranslation(CameraManager.GetActiveCamera().Position.X,
                                         CameraManager.GetActiveCamera().Position.Y,
                                         CameraManager.GetActiveCamera().Position.Z)
                                        );
            texturedEffect.Parameters["View"].SetValue(CameraManager.GetActiveCamera().ViewMatrix);
            texturedEffect.Parameters["Projection"].SetValue(CameraManager.GetActiveCamera().ProjectionMatrix);
            texturedEffect.Parameters["Texture"].SetValue(this.moonTex);
            if (theta < Math.PI / 2.0f || theta > 3.0f * Math.PI / 2.0f)
                texturedEffect.Parameters["alpha"].SetValue((float)Math.Abs(Math.Sin(Theta + (float)Math.PI / 2.0f)));
            else
                texturedEffect.Parameters["alpha"].SetValue(0.0f);

            // Apply each pass
            foreach (EffectPass pass in texturedEffect.CurrentTechnique.Passes)
            {
                // Apply Effect
                pass.Apply();

                // Draw Primitives
                device.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, quadVerts, 0, 4, quadIb, 0, 2);
            }
        }

        /// <summary>
        /// Generates the dome.
        /// </summary>
        private void GenerateDome()
        {
            int Latitude = DomeN / 2;
            int Longitude = DomeN;
            DVSize = Longitude * Latitude;
            DISize = (Longitude - 1) * (Latitude - 1) * 2;
            DVSize *= 2;
            DISize *= 2;
            
            domeVerts = new VertexPositionTexture[DVSize];

            // Fill Vertex Buffer
            int DomeIndex = 0;
            for (int i = 0; i < Longitude; i++)
            {
                double MoveXZ = 100.0f * (i / ((float)Longitude - 1.0f)) * MathHelper.Pi / 180.0;

                for (int j = 0; j < Latitude; j++)
                {
                    double MoveY = MathHelper.Pi * j / (Latitude - 1);

                    domeVerts[DomeIndex] = new VertexPositionTexture();
                    domeVerts[DomeIndex].Position.X = (float)(Math.Sin(MoveXZ) * Math.Cos(MoveY));
                    domeVerts[DomeIndex].Position.Y = (float)Math.Cos(MoveXZ);
                    domeVerts[DomeIndex].Position.Z = (float)(Math.Sin(MoveXZ) * Math.Sin(MoveY));

                    domeVerts[DomeIndex].Position *= 10.0f;

                    domeVerts[DomeIndex].TextureCoordinate.X = 0.5f / (float)Longitude + i / (float)Longitude;
                    domeVerts[DomeIndex].TextureCoordinate.Y = 0.5f / (float)Latitude + j / (float)Latitude;

                    DomeIndex++;
                }
            }
            for (int i = 0; i < Longitude; i++)
            {
                double MoveXZ = 100.0 * (i / (float)(Longitude - 1)) * MathHelper.Pi / 180.0;

                for (int j = 0; j < Latitude; j++)
                {
                    double MoveY = (MathHelper.Pi * 2.0) - (MathHelper.Pi * j / (Latitude - 1));

                    domeVerts[DomeIndex] = new VertexPositionTexture();
                    domeVerts[DomeIndex].Position.X = (float)(Math.Sin(MoveXZ) * Math.Cos(MoveY));
                    domeVerts[DomeIndex].Position.Y = (float)Math.Cos(MoveXZ);
                    domeVerts[DomeIndex].Position.Z = (float)(Math.Sin(MoveXZ) * Math.Sin(MoveY));

                    domeVerts[DomeIndex].Position *= 10.0f;

                    domeVerts[DomeIndex].TextureCoordinate.X = 0.5f / (float)Longitude + i / (float)Longitude;
                    domeVerts[DomeIndex].TextureCoordinate.Y = 0.5f / (float)Latitude + j / (float)Latitude;

                    DomeIndex++;
                }
            }

            // Fill index buffer
            ib = new short[DISize * 3];
            int index = 0;
            for (short i = 0; i < Longitude - 1; i++)
            {
                for (short j = 0; j < Latitude - 1; j++)
                {
                    ib[index++] = (short)(i * Latitude + j);
                    ib[index++] = (short)((i + 1) * Latitude + j);
                    ib[index++] = (short)((i + 1) * Latitude + j + 1);

                    ib[index++] = (short)((i + 1) * Latitude + j + 1);
                    ib[index++] = (short)(i * Latitude + j + 1);
                    ib[index++] = (short)(i * Latitude + j);
                }
            }
            short Offset = (short)(Latitude * Longitude);
            for (short i = 0; i < Longitude - 1; i++)
            {
                for (short j = 0; j < Latitude - 1; j++)
                {
                    ib[index++] = (short)(Offset + i * Latitude + j);
                    ib[index++] = (short)(Offset + (i + 1) * Latitude + j + 1);
                    ib[index++] = (short)(Offset + (i + 1) * Latitude + j);

                    ib[index++] = (short)(Offset + i * Latitude + j + 1);
                    ib[index++] = (short)(Offset + (i + 1) * Latitude + j + 1);
                    ib[index++] = (short)(Offset + i * Latitude + j);
                }
            }
        }

        /// <summary>
        /// Generates the moon.
        /// </summary>
        private void GenerateMoon()
        {
            quadVerts = new VertexPositionTexture[]
                        {
                            new VertexPositionTexture(
                                new Vector3(1,-1,0),
                                new Vector2(1,1)),
                            new VertexPositionTexture(
                                new Vector3(-1,-1,0),
                                new Vector2(0,1)),
                            new VertexPositionTexture(
                                new Vector3(-1,1,0),
                                new Vector2(0,0)),
                            new VertexPositionTexture(
                                new Vector3(1,1,0),
                                new Vector2(1,0))
                        };

            quadIb = new short[] { 0, 1, 2, 2, 3, 0 };
        }

        /// <summary>
        /// Generates the perm tex.
        /// </summary>
        private void GeneratePermTex(GraphicsDevice device)
        {
            int[] perm = { 151,160,137,91,90,15,
                           131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
                           190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
                           88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
                           77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
                           102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
                           135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
                           5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
                           223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
                           129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
                           251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
                           49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
                           138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
            };

            int[] gradValues = { 1,1,0,    
                -1,1,0, 1,-1,0, 
                -1,-1,0, 1,0,1,
                -1,0,1, 1,0,-1,
                -1,0,-1, 0,1,1,
                0,-1,1, 0,1,-1,
                0,-1,-1, 1,1,0,
                0,-1,1, -1,1,0, 
                0,-1,-1
            };

            permTex = new Texture2D(device, 256, 256, false, SurfaceFormat.Color);

            byte[] pixels;
            pixels = new byte[256 * 256 * 4];
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    int offset = (i * 256 + j) * 4;
                    byte value = (byte)perm[(j + perm[i]) & 0xFF];
                    pixels[offset + 1] = (byte)(gradValues[value & 0x0F] * 64 + 64);
                    pixels[offset + 2] = (byte)(gradValues[value & 0x0F + 1] * 64 + 64);
                    pixels[offset + 3] = (byte)(gradValues[value & 0x0F + 2] * 64 + 64);
                    pixels[offset] = value;
                }
            }

            permTex.SetData<byte>(pixels);
        }

        /// <summary>
        /// Gets the direction.
        /// </summary>
        /// <returns></returns>
        Vector4 GetDirection()
        {

            float y = (float)Math.Cos((double)theta);
            float x = (float)(Math.Sin((double)theta) * Math.Cos(phi));
            float z = (float)(Math.Sin((double)theta) * Math.Sin(phi));
            float w = 1.0f;

            return new Vector4(x, y, z, w);
        }

        /// <summary>
        /// Gets the color of the sun.
        /// </summary>
        /// <param name="fTheta">The f theta.</param>
        /// <param name="nTurbidity">The n turbidity.</param>
        /// <returns></returns>
        Vector4 GetSunColor(float fTheta, int nTurbidity)
        {
            float fBeta = 0.04608365822050f * nTurbidity - 0.04586025928522f;
            float fTauR, fTauA;
            float[] fTau = new float[3];

            float coseno = (float)Math.Cos((double)fTheta + Math.PI);
            double factor = (double)fTheta / Math.PI * 180.0;
            double jarl = Math.Pow(93.885 - factor, -1.253);
            float potencia = (float)jarl;
            float m = 1.0f / (coseno + 0.15f * potencia);

            int i;
            float[] fLambda = new float[3];
            fLambda[0] = parameters.WaveLengths.X;
            fLambda[1] = parameters.WaveLengths.Y;
            fLambda[2] = parameters.WaveLengths.Z;


            for (i = 0; i < 3; i++)
            {
                potencia = (float)Math.Pow((double)fLambda[i], 4.0);
                fTauR = (float)Math.Exp((double)(-m * 0.008735f * potencia));

                const float fAlpha = 1.3f;
                potencia = (float)Math.Pow((double)fLambda[i], (double)-fAlpha);
                if (m < 0.0f)
                    fTau[i] = 0.0f;
                else
                {
                    fTauA = (float)Math.Exp((double)(-m * fBeta * potencia));
                    fTau[i] = fTauR * fTauA;
                }

            }

            Vector4 vAttenuation = new Vector4(fTau[0], fTau[1], fTau[2], 1.0f);
            return vAttenuation;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            if (realTime)
            {
                int minutos = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
                theta = (float)minutos * (float)(Math.PI) / 12.0f / 60.0f;
            }

            parameters.LightDirection = this.GetDirection();
            parameters.LightDirection.Normalize();
        }

        /// <summary>
        /// Updates the mie rayleigh textures.
        /// </summary>
        void UpdateMieRayleighTextures(GraphicsDevice device)
        {
            device.SetRenderTargets(rayleighRT, mieRT);

            device.Clear(Color.CornflowerBlue);

            scatterEffect.CurrentTechnique = scatterEffect.Techniques["Update"];
            scatterEffect.Parameters["InvWavelength"].SetValue(parameters.InverseWaveLengths);
            scatterEffect.Parameters["WavelengthMie"].SetValue(parameters.WaveLengthsMie);
            scatterEffect.Parameters["v3SunDir"].SetValue(new Vector3(-parameters.LightDirection.X, -parameters.LightDirection.Y, -parameters.LightDirection.Z));
            EffectPass pass = scatterEffect.CurrentTechnique.Passes[0];

            pass.Apply();
            quad.Draw();
            
            device.SetRenderTargets(null);
        }
        #endregion
    }
}
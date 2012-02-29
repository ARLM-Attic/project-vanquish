/*
 * Skydome Parameters Class
 * 
 * Alex Urbano Álvarez
 * XNA Community Coordinator
 * 
 * goefuika@gmail.com
 * 
 * http://elgoe.blogspot.com
 * http://www.codeplex.com/XNACommunity
 *
 * Modified by Neil Knight to work with Project Vanquish
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectVanquish.Sky
{
    public class SkyDomeParameters
    {
        #region Fields
        /// <summary>
        /// Light Direction
        /// </summary>
        private Vector4 lightDirection = new Vector4(100.0f, 100.0f, 100.0f, 1.0f);

        /// <summary>
        /// Light Color
        /// </summary>
        private Vector4 lightColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

        /// <summary>
        /// Ambient Light Color
        /// </summary>
        private Vector4 lightColorAmbient = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);

        /// <summary>
        /// Fog Density
        /// </summary>
        private float density = 0.0003f;

        /// <summary>
        /// Wave Lengths
        /// </summary>
        private Vector3 waveLengths = new Vector3(0.65f, 0.57f, 0.475f);

        /// <summary>
        /// Inverse Wave Lengths
        /// </summary>
        private Vector3 invWaveLengths;

        /// <summary>
        /// Wave Lengths Mie
        /// </summary>
        private Vector3 waveLengthsMie;

        /// <summary>
        /// Number of Samples
        /// </summary>
        private int numSamples = 10;

        /// <summary>
        /// Exposure
        /// </summary>
        private float exposure = -0.2f;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the exposure.
        /// </summary>
        /// <value>The exposure.</value>
        public float Exposure
        {
            get { return exposure; }
            set { exposure = value; }
        }

        /// <summary>
        /// Gets or sets the fog density.
        /// </summary>
        /// <value>The fog density.</value>
        public float FogDensity 
        { 
            get { return density; } 
            set { density = value; } 
        }

        /// <summary>
        /// Gets the inverse wave lengths.
        /// </summary>
        /// <value>The inverse wave lengths.</value>
        public Vector3 InverseWaveLengths
        {
            get { return invWaveLengths; }
        }
        
        /// <summary>
        /// Gets or sets the color of the light.
        /// </summary>
        /// <value>The color of the light.</value>
        public Vector4 LightColor 
        { 
            get { return lightColor; } 
            set { lightColor = value; } 
        }

        /// <summary>
        /// Gets or sets the light color ambient.
        /// </summary>
        /// <value>The light color ambient.</value>
        public Vector4 LightColorAmbient 
        { 
            get { return lightColorAmbient; } 
            set { lightColorAmbient = value; } 
        }

        /// <summary>
        /// Gets or sets the light direction.
        /// </summary>
        /// <value>The light direction.</value>
        public Vector4 LightDirection
        {
            get { return lightDirection; }
            set { lightDirection = value; }
        }

        /// <summary>
        /// Gets or sets the number of samples.
        /// </summary>
        /// <value>The number of samples.</value>
        public int NumberOfSamples
        {
            get { return numSamples; }
            set { numSamples = value; }
        }

        /// <summary>
        /// Gets the wave lengths mie.
        /// </summary>
        /// <value>The wave lengths mie.</value>
        public Vector3 WaveLengthsMie
        {
            get { return waveLengthsMie; }
        }

        /// <summary>
        /// Gets or sets the wave lengths.
        /// </summary>
        /// <value>The wave lengths.</value>
        public Vector3 WaveLengths
        {
            get { return waveLengths; }
            set
            {
                waveLengths = value;
                SetLengths();
            }
        } 
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SkyDomeParameters"/> class.
        /// </summary>
        public SkyDomeParameters()
        {
            SetLengths();
        }
        #endregion

        #region Members
        /// <summary>
        /// Sets the lengths.
        /// </summary>
        private void SetLengths()
        {
            invWaveLengths.X = 1.0f / (float)Math.Pow((double)waveLengths.X, 4.0);
            invWaveLengths.Y = 1.0f / (float)Math.Pow((double)waveLengths.Y, 4.0);
            invWaveLengths.Z = 1.0f / (float)Math.Pow((double)waveLengths.Z, 4.0);

            waveLengthsMie.X = (float)Math.Pow((double)waveLengths.X, -0.84);
            waveLengthsMie.Y = (float)Math.Pow((double)waveLengths.Y, -0.84);
            waveLengthsMie.Z = (float)Math.Pow((double)waveLengths.Z, -0.84);
        }
        #endregion
    }
}
using Microsoft.Xna.Framework;

namespace ProjectVanquish
{
    /// <summary>
    /// SkyDomeParameters class
    /// </summary>
    public class SkyDomeParameters
    {
        #region Fields
        private Vector4 lightDirection = new Vector4(100.0f, 100.0f, 100.0f, 1.0f);
        private Vector4 lightColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private Vector4 lightColorAmbient = new Vector4(0.1f, 0.1f, 0.1f, 1.0f);
        private Vector4 fogColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private float density = 0.000028f;
        private float sunLightness = 0.2f;
        private float sunRadiusAttenuation = 256.0f;
        private float largeSunLightness = 0.2f;
        private float largeSunRadiusAttenuation = 1.0f;
        private float dayToSunsetSharpness = 1.5f;
        private float hazeTopAltitude = 100.0f;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the day to sunset sharpness.
        /// </summary>
        /// <value>The day to sunset sharpness.</value>
        public float DayToSunsetSharpness
        {
            get { return this.dayToSunsetSharpness; }
            set { this.dayToSunsetSharpness = value; }
        }

        /// <summary>
        /// Gets or sets the color of the fog.
        /// </summary>
        /// <value>The color of the fog.</value>
        public Vector4 FogColor
        {
            get { return this.fogColor; }
            set { this.fogColor = value; }
        }

        /// <summary>
        /// Gets or sets the fog density.
        /// </summary>
        /// <value>The fog density.</value>
        public float FogDensity
        {
            get { return this.density; }
            set { this.density = value; }
        }

        /// <summary>
        /// Gets or sets the haze top altitude.
        /// </summary>
        /// <value>The haze top altitude.</value>
        public float HazeTopAltitude
        {
            get { return this.hazeTopAltitude; }
            set { this.hazeTopAltitude = value; }
        }

        /// <summary>
        /// Gets or sets the large sun lightness.
        /// </summary>
        /// <value>The large sun lightness.</value>
        public float LargeSunLightness
        {
            get { return this.largeSunLightness; }
            set { this.largeSunLightness = value; }
        }

        /// <summary>
        /// Gets or sets the large sun radius attenuation.
        /// </summary>
        /// <value>The large sun radius attenuation.</value>
        public float LargeSunRadiusAttenuation
        {
            get { return this.largeSunRadiusAttenuation; }
            set { this.largeSunRadiusAttenuation = value; }
        }

        /// <summary>
        /// Gets or sets the color of the light.
        /// </summary>
        /// <value>The color of the light.</value>
        public Vector4 LightColor
        {
            get { return this.lightColor; }
            set { this.lightColor = value; }
        }

        /// <summary>
        /// Gets or sets the light color ambient.
        /// </summary>
        /// <value>The light color ambient.</value>
        public Vector4 LightColorAmbient
        {
            get { return this.lightColorAmbient; }
            set { this.lightColorAmbient = value; }
        }

        /// <summary>
        /// Gets or sets the light direction.
        /// </summary>
        /// <value>The light direction.</value>
        public Vector4 LightDirection
        {
            get { return this.lightDirection; }
            set { this.lightDirection = value; }
        }

        /// <summary>
        /// Gets or sets the sun lightness.
        /// </summary>
        /// <value>The sun lightness.</value>
        public float SunLightness
        {
            get { return this.sunLightness; }
            set { this.sunLightness = value; }
        }

        /// <summary>
        /// Gets or sets the sun radius attenuation.
        /// </summary>
        /// <value>The sun radius attenuation.</value>
        public float SunRadiusAttenuation
        {
            get { return this.sunRadiusAttenuation; }
            set { this.sunRadiusAttenuation = value; }
        }
        #endregion
    }
}

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectVanquish.Models;
using ProjectVanquish.Cameras;
using ProjectVanquish.Core;

namespace ProjectVanquish
{
    /// <summary>
    /// SkyDome class
    /// </summary>
    public class SkyDome
    {
        #region Fields
        Matrix[] boneTransforms;

        /// <summary>
        /// Camera
        /// </summary>
        private BaseCamera camera;

        /// <summary>
        /// Dome model
        /// </summary>
        private Actor domeModel;

        /// <summary>
        /// Dome effect
        /// </summary>
        private Effect domeEffect;

        /// <summary>
        /// Graphics Device
        /// </summary>
        private GraphicsDevice device;

        /// <summary>
        /// Theta
        /// </summary>
        private static float fTheta = 0.0f;

        /// <summary>
        /// Phi
        /// </summary>
        private static float fPhi = 0.0f;

        /// <summary>
        /// Real time?
        /// </summary>
        private bool realTime;

        /// <summary>
        /// Textures
        /// </summary>
        Texture2D day, sunset, night;

        /// <summary>
        /// SkyDome Parameters
        /// </summary>
        SkyDomeParameters parameters;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SkyDome"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="content">The content.</param>
        /// <param name="camera">The camera.</param>
        /// <param name="useRealTime">if set to <c>true</c> [use real time].</param>
        public SkyDome(GraphicsDevice device, ContentManager content, BaseCamera camera, bool useRealTime)
        {
            // Set up the camera
            camera = camera;

            // Set the Graphics Device
            device = device;

            // Load the SkyDome Shader
            domeEffect = content.Load<Effect>("Shaders/Sky/Sky");

            // Load the SkyDome model
            domeModel = new Actor(content.Load<Model>("Models/Skydome"));

            // Scale the SkyDome
            domeModel.Scale = new Vector3(999f);
            domeModel.Position = new Vector3(0, -10, 0);

            // Set up the SkyDome parameters
            realTime = useRealTime;
            parameters = new SkyDomeParameters();

            // Load the Textures
            day = content.Load<Texture2D>("Textures/SkyDay");
            sunset = content.Load<Texture2D>("Textures/Sunset");
            night = content.Load<Texture2D>("Textures/SkyNight");

            // Set the Current Technique
            domeEffect.CurrentTechnique = domeEffect.Techniques["SkyDomeTechnique"];
            RemapModel(domeModel.Model, domeEffect);

            // Create the Bone Matrices
            boneTransforms = new Matrix[this.domeModel.Model.Bones.Count];
            domeModel.Model.CopyAbsoluteBoneTransformsTo(boneTransforms);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public SkyDomeParameters Parameters
        {
            get { return this.parameters; }
            set { this.parameters = value; }
        }

        /// <summary>
        /// Gets or sets the phi.
        /// </summary>
        /// <value>The phi.</value>
        public float Phi
        {
            get { return fPhi; }
            set { fPhi = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [real time].
        /// </summary>
        /// <value><c>true</c> if [real time]; otherwise, <c>false</c>.</value>
        public bool RealTime
        {
            get { return this.realTime; }
            set { this.realTime = value; }
        }

        /// <summary>
        /// Gets or sets the theta.
        /// </summary>
        /// <value>The theta.</value>
        public float Theta
        {
            get { return fTheta; }
            set { fTheta = value; }
        }
        #endregion

        #region Members
        /// <summary>
        /// Draws the specified device.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="camera">The camera.</param>
        public void Draw(GraphicsDevice device, BaseCamera camera)
        {            
            Matrix View = camera.ViewMatrix;
            Matrix Projection = camera.ProjectionMatrix;
            device.Clear(ClearOptions.Target, new Vector4(1.0f), 1.0f, 0);
            device.Clear(ClearOptions.DepthBuffer, new Vector4(1.0f), 1.0f, 0);
            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.DepthStencilState = DepthStencilState.Default;

            foreach (ModelMesh mesh in domeModel.Model.Meshes)
            {
                Matrix World = boneTransforms[mesh.ParentBone.Index] *
                               Matrix.CreateTranslation(camera.Position.X, camera.Position.Y - 50.0f, camera.Position.Z);
                Matrix WorldIT = Matrix.Invert(World);
                WorldIT = Matrix.Transpose(WorldIT);

                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["WorldIT"].SetValue(WorldIT);
                    effect.Parameters["WorldViewProj"].SetValue(World * View * Projection);
                    effect.Parameters["ViewInv"].SetValue(Matrix.Invert(View));
                    effect.Parameters["World"].SetValue(World);
                    effect.Parameters["SkyTextureNight"].SetValue(night);
                    effect.Parameters["SkyTextureSunset"].SetValue(sunset);
                    effect.Parameters["SkyTextureDay"].SetValue(day);
                    effect.Parameters["isSkydome"].SetValue(true);
                    effect.Parameters["LightDirection"].SetValue(parameters.LightDirection);
                    effect.Parameters["LightColor"].SetValue(parameters.LightColor);
                    effect.Parameters["LightColorAmbient"].SetValue(parameters.LightColorAmbient);
                    effect.Parameters["FogColor"].SetValue(parameters.FogColor);
                    effect.Parameters["fDensity"].SetValue(parameters.FogDensity);
                    effect.Parameters["SunLightness"].SetValue(parameters.SunLightness);
                    effect.Parameters["sunRadiusAttenuation"].SetValue(parameters.SunRadiusAttenuation);
                    effect.Parameters["largeSunLightness"].SetValue(parameters.LargeSunLightness);
                    effect.Parameters["largeSunRadiusAttenuation"].SetValue(parameters.LargeSunRadiusAttenuation);
                    effect.Parameters["dayToSunsetSharpness"].SetValue(parameters.DayToSunsetSharpness);
                    effect.Parameters["hazeTopAltitude"].SetValue(parameters.HazeTopAltitude);
                    mesh.Draw();
                }
            }
        }

        /// <summary>
        /// Gets the light direction.
        /// </summary>
        /// <returns></returns>
        public static Vector4 GetLightDirection()
        {
            float y = (float)Math.Cos((double)fTheta);
            float x = (float)(Math.Sin((double)fTheta) * Math.Cos(fPhi));
            float z = (float)(Math.Sin((double)fTheta) * Math.Sin(fPhi));
            float w = 1.0f;
            return new Vector4(x, y, z, w);
        }

        /// <summary>
        /// Remaps the model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="effect">The effect.</param>
        public static void RemapModel(Model model, Effect effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                }
            }
        }

        /// <summary>
        /// Updates the specified lensflare.
        /// </summary>
        public void Update()//LensFlare lensFlare)
        {
            if (this.realTime)
            {
                int minutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
                fTheta = (float)minutes * (float)(Math.PI) / 12.0f / 30.0f;
            }

            this.parameters.LightDirection = SkyDome.GetLightDirection();
            this.parameters.LightDirection.Normalize();
            //if (lensFlare != null)
            //{
            //    lensFlare.LightDirection = new Vector3(this.parameters.LightDirection.X, this.parameters.LightDirection.Y, this.parameters.LightDirection.Z);
            //}
        }
        #endregion
    }
}

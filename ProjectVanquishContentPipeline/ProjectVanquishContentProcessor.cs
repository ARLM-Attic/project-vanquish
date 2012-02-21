using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace ProjectVanquishContentPipeline
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// TODO: change the ContentProcessor attribute to specify the correct
    /// display name for this processor.
    /// </summary>
    [ContentProcessor(DisplayName = "ProjectVanquishContentPipeline")]
    public class ProjectVanquishContentProcessor : ModelProcessor
    {
        #region Fields
        string directory;
        // Normal and Specular Map textures
        string normalMapTexture, specularMapTexture;

        // Normal Map Key
        string normalMapKey = "NormalMap";
        // Specular Map Key
        string specularMapKey = "SpecularMap";

        // Create a List of Acceptable Vertex Channel Names
        static IList acceptableVertexChannelNames = new string[]
        {
            VertexChannelNames.TextureCoordinate(0),
            VertexChannelNames.Normal(0),
            VertexChannelNames.Binormal(0),
            VertexChannelNames.Tangent(0),
        }; 
        #endregion

        #region Properties
        [Browsable(false)]
        public override bool GenerateTangentFrames
        {
            get { return true; }
            set { }
        }

        [DisplayName("Normal Map Key")]
        [Description("This will be the key that will be used to search the Normal Map in the Opaque data of the model")]
        [DefaultValue("NormalMap")]
        public string NormalMapKey
        {
            get { return normalMapKey; }
            set { normalMapKey = value; }
        }

        [DisplayName("Specular Map Key")]
        [Description("This will be the key that will be used to search the Specular Map in the Opaque data of the model")]
        [DefaultValue("SpecularMap")]
        public string SpecularMapKey
        {
            get { return specularMapKey; }
            set { specularMapKey = value; }
        }

        [DisplayName("Normal Map Texture")]
        [Description("If set, this file will be used as the Normal Map on the model, overriding anything found in the Opaque data.")]
        [DefaultValue("")]
        public string NormalMapTexture
        {
            get { return normalMapTexture; }
            set { normalMapTexture = value; }
        }

        [DisplayName("Specular Map Texture")]
        [Description("If set, this file will be used as the Specular Map on the model, overriding anything found in the Opaque data.")]
        [DefaultValue("")]
        public string SpecularMapTexture
        {
            get { return specularMapTexture; }
            set { specularMapTexture = value; }
        } 
        #endregion

        #region Members
        /// <summary>
        /// Converts mesh content to model content.
        /// </summary>
        /// <param name="input">The root node content.</param>
        /// <param name="context">Context for the specified processor.</param>
        /// <returns></returns>
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            directory = Path.GetDirectoryName(input.Identity.SourceFilename);
            LookUpTextures(input);
            return base.Process(input, context);
        }

        /// <summary>
        /// Processes geometry content vertex channels at the specified index.
        /// </summary>
        /// <param name="geometry">The geometry content to process.</param>
        /// <param name="vertexChannelIndex">Index of the vertex channel to process.</param>
        /// <param name="context">Context for the specified processor.</param>
        protected override void ProcessVertexChannel(GeometryContent geometry, int vertexChannelIndex, ContentProcessorContext context)
        {
            string vertexChannelName = geometry.Vertices.Channels[vertexChannelIndex].Name;

            // If this vertex channel has an acceptable names, process it as normal.
            if (acceptableVertexChannelNames.Contains(vertexChannelName))
                base.ProcessVertexChannel(geometry, vertexChannelIndex, context);
            // Otherwise, remove it from the vertex channels; it's just extra data
            // we don't need.
            else
                geometry.Vertices.Channels.Remove(vertexChannelName);
        }

        /// <summary>
        /// Called by the framework when the MaterialContent property of a GeometryContent object is encountered in the input node collection.
        /// </summary>
        /// <param name="material">The input material content.</param>
        /// <param name="context">Context for the specified processor.</param>
        /// <returns></returns>
        protected override MaterialContent ConvertMaterial(MaterialContent material, ContentProcessorContext context)
        {
            EffectMaterialContent deferredShadingMaterial = new EffectMaterialContent();
            deferredShadingMaterial.Effect = new ExternalReference<EffectContent>("Shaders/GBuffer/RenderGBuffer.fx");

            // Copy the textures in the original material to the new normal mapping
            // material, if they are relevant to our renderer. The
            // LookUpTextures function has added the normal map and specular map
            // textures to the Textures collection, so that will be copied as well.
            foreach (KeyValuePair<String, ExternalReference<TextureContent>> texture in material.Textures)
            {
                if ((texture.Key == "Texture") ||
                    (texture.Key == "NormalMap") ||
                    (texture.Key == "SpecularMap"))
                    deferredShadingMaterial.Textures.Add(texture.Key, texture.Value);
            }

            return context.Convert<MaterialContent, MaterialContent>(deferredShadingMaterial, typeof(MaterialProcessor).Name);
        }

        /// <summary>
        /// Looks up textures.
        /// </summary>
        /// <param name="node">The node.</param>
        void LookUpTextures(NodeContent node)
        {
            MeshContent mesh = node as MeshContent;
            if (mesh != null)
            {
                // This will contatin the path to the normal map texture
                string normalMapPath;

                // If the NormalMapTexture property is set, we use that normal map for all meshes in the model.
                // This overrides anything else
                if (!String.IsNullOrEmpty(NormalMapTexture))
                    normalMapPath = NormalMapTexture;
                else
                    // If NormalMapTexture is not set, we look into the opaque data of the model,
                    // and search for a texture with the key equal to NormalMapKey
                    normalMapPath = mesh.OpaqueData.GetValue<string>(NormalMapKey, null);

                // If the NormalMapTexture Property was not used, and the key was not found in the model, than normalMapPath would have the value null.
                if (normalMapPath == null)
                {
                    // If a key with the required name is not found, we make a final attempt,
                    // and search, in the same directory as the model, for a texture named
                    // meshname_n.tga, where meshname is the name of a mesh inside the model.
                    normalMapPath = Path.Combine(directory, mesh.Name + "_n.tga");
                    if (!File.Exists(normalMapPath))
                        // If this fails also (that texture does not exist),
                        // then we use a default texture, named null_normal.tga
                        normalMapPath = "null_normal.tga";
                }
                else
                    normalMapPath = Path.Combine(directory, normalMapPath);

                string specularMapPath;

                // If the SpecularMapTexture property is set, we use it
                if (!String.IsNullOrEmpty(SpecularMapTexture))
                    specularMapPath = SpecularMapTexture;
                else
                    // If SpecularMapTexture is not set, we look into the opaque data of the model,
                    // and search for a texture with the key equal to specularMapKey
                    specularMapPath = mesh.OpaqueData.GetValue<string>(SpecularMapKey, null);

                if (specularMapPath == null)
                {
                    // We search, in the same directory as the model, for a texture named
                    // meshname_s.tga
                    specularMapPath = Path.Combine(directory, mesh.Name + "_s.tga");
                    if (!File.Exists(specularMapPath))
                        // If this fails also (that texture does not exist),
                        // then we use a default texture, named null_specular.tga
                        specularMapPath = "null_specular.tga";
                }
                else
                    specularMapPath = Path.Combine(directory, specularMapPath);

                // Add the keys to the material, so they can be used by the shader
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    // In some .fbx files, the key might be found in the textures collection, but not
                    // in the mesh, as we checked above. If this is the case, we need to get it out, and
                    // add it with the "NormalMap" key
                    if (geometry.Material.Textures.ContainsKey(normalMapKey))
                    {
                        ExternalReference<TextureContent> texRef = geometry.Material.Textures[normalMapKey];
                        geometry.Material.Textures.Remove(normalMapKey);
                        geometry.Material.Textures.Add("NormalMap", texRef);
                    }
                    else
                        geometry.Material.Textures.Add("NormalMap", new ExternalReference<TextureContent>(normalMapPath));

                    if (geometry.Material.Textures.ContainsKey(specularMapKey))
                    {
                        ExternalReference<TextureContent> texRef = geometry.Material.Textures[specularMapKey];
                        geometry.Material.Textures.Remove(specularMapKey);
                        geometry.Material.Textures.Add("SpecularMap", texRef);
                    }
                    else
                        geometry.Material.Textures.Add("SpecularMap", new ExternalReference<TextureContent>(specularMapPath));
                }
            }

            // go through all children and apply LookUpTextures recursively
            foreach (NodeContent child in node.Children)
                LookUpTextures(child);
        } 
        #endregion
    }
}
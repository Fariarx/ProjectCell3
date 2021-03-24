#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Griffin;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Reflection;

namespace Pinwheel.Griffin.URP
{
    public static class GGriffinUrpInstaller
    {
        [DidReloadScripts]
        private static void HandleAutomaticInstallAndUpgrade()
        {
            string key = GEditorCommon.GetProjectRelatedEditorPrefsKey("polaris-urp-package-imported");
            bool isFirstImport = !EditorPrefs.HasKey(key);

            if (isFirstImport)
            {
                Install();
                UpgradeTerrainMaterialInProject();
            }

            EditorPrefs.SetBool(key, true);
        }

        public static void Install()
        {
            GGriffinUrpResources resources = GGriffinUrpResources.Instance;
            if (resources == null)
            {
                Debug.Log("Unable to load Griffin URP Resources.");
            }

            List<GWizardMaterialTemplate> terrainMaterialTemplates = new List<GWizardMaterialTemplate>();
            terrainMaterialTemplates.Add(new GWizardMaterialTemplate()
            {
                Pipeline = GRenderPipelineType.Universal,
                LightingModel = GLightingModel.PBR,
                TexturingModel = GTexturingModel.Splat,
                SplatsModel = GSplatsModel.Splats4,
                Material = resources.Terrain4SplatsMaterial
            });

            terrainMaterialTemplates.Add(new GWizardMaterialTemplate()
            {
                Pipeline = GRenderPipelineType.Universal,
                LightingModel = GLightingModel.PBR,
                TexturingModel = GTexturingModel.Splat,
                SplatsModel = GSplatsModel.Splats4Normals4,
                Material = resources.Terrain4Splats4NormalsMaterial
            });

            terrainMaterialTemplates.Add(new GWizardMaterialTemplate()
            {
                Pipeline = GRenderPipelineType.Universal,
                LightingModel = GLightingModel.PBR,
                TexturingModel = GTexturingModel.Splat,
                SplatsModel = GSplatsModel.Splats8,
                Material = resources.Terrain8SplatsMaterial
            });

            terrainMaterialTemplates.Add(new GWizardMaterialTemplate()
            {
                Pipeline = GRenderPipelineType.Universal,
                LightingModel = GLightingModel.PBR,
                TexturingModel = GTexturingModel.GradientLookup,
                Material = resources.TerrainGradientLookupMaterial
            });

            terrainMaterialTemplates.Add(new GWizardMaterialTemplate()
            {
                Pipeline = GRenderPipelineType.Universal,
                LightingModel = GLightingModel.PBR,
                TexturingModel = GTexturingModel.VertexColor,
                Material = resources.TerrainVertexColorMaterial
            });

            terrainMaterialTemplates.Add(new GWizardMaterialTemplate()
            {
                Pipeline = GRenderPipelineType.Universal,
                LightingModel = GLightingModel.PBR,
                TexturingModel = GTexturingModel.ColorMap,
                Material = resources.TerrainColorMapMaterial
            });

            GCreateTerrainWizardSettings wizardSetting = GGriffinSettings.Instance.WizardSettings;
            wizardSetting.UniversalRPMaterials = terrainMaterialTemplates;
            GGriffinSettings.Instance.WizardSettings = wizardSetting;

            GTerrainDataDefault terrainDataDefault = GGriffinSettings.Instance.TerrainDataDefault;
            GFoliageDefault foliageDefault = terrainDataDefault.Foliage;
            SetMaterial(ref foliageDefault, "grassMaterialURP", resources.GrassMaterial);
            SetMaterial(ref foliageDefault, "grassBillboardMaterialURP", resources.GrassBillboardMaterial);
            SetMaterial(ref foliageDefault, "grassInteractiveMaterialURP", resources.GrassInteractiveMaterial);
            SetMaterial(ref foliageDefault, "treeBillboardMaterialURP", resources.TreeBillboardMaterial);
            terrainDataDefault.Foliage = foliageDefault;
            GGriffinSettings.Instance.TerrainDataDefault = terrainDataDefault;

            string[] pathToDelete = new string[]
            {
                "Assets/Griffin - PolarisV2/Internal/Materials/TerrainMaterials/UniversalRP",
                "Assets/Griffin - PolarisV2/Internal/Materials/GrassMaterialURP.mat",
                "Assets/Griffin - PolarisV2/Internal/Materials/GrassInteractiveMaterialURP.mat",
                "Assets/Griffin - PolarisV2/Internal/Materials/TreeBillboardDefaultMaterialURP.mat",
                "Assets/Griffin - PolarisV2/Shaders/UniversalRP"
            };

            for (int i = 0; i < pathToDelete.Length; ++i)
            {
                FileUtil.DeleteFileOrDirectory(pathToDelete[i]);
            }

            EditorUtility.SetDirty(GGriffinSettings.Instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Completed", "Successfully installed Polaris V2 Universal Render Pipeline support.", "OK");
        }

        public static void UpgradeTerrainMaterialInProject()
        {
            if (GCommon.CurrentRenderPipeline != GRenderPipelineType.Universal)
            {
                return;
            }
            string[] guid = AssetDatabase.FindAssets("t:GTerrainData");
            for (int i = 0; i < guid.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid[i]);
                GTerrainData data = AssetDatabase.LoadAssetAtPath<GTerrainData>(path);
                Material mat = data.Shading.CustomMaterial;
                if (mat != null)
                {
                    if (UpgradeMaterial(mat))
                    {
                        data.Shading.UpdateMaterials();
                    }
                }
            }
        }

        public static bool UpgradeMaterial(Material mat)
        {
            if (GCommon.CurrentRenderPipeline != GRenderPipelineType.Universal)
                return false;

            Shader currentShader = mat.shader;
            GWizardMaterialTemplate template;
            bool found = GGriffinSettings.Instance.WizardSettings.FindMaterialTemplate(
                currentShader,
                GRenderPipelineType.Builtin,
                out template);
            if (!found)
                return false;

            Material urpMat = GGriffinSettings.Instance.WizardSettings.GetClonedMaterial(
                GRenderPipelineType.Universal,
                GLightingModel.PBR,
                template.TexturingModel,
                template.SplatsModel);
            if (urpMat != null)
            {
                mat.shader = urpMat.shader;
                GUtilities.DestroyObject(urpMat);
                Debug.Log(string.Format("Polaris Auto Upgrader: Upgrade material {0} to URP succeeded.", mat.name));
                return true;
            }
            return false;
        }

        private static void SetMaterial(ref GFoliageDefault foliage, string fieldName, Material value)
        {
            FieldInfo f = foliage.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            object foliageRef = foliage;
            if (f != null && f.FieldType == typeof(Material))
            {
                f.SetValue(foliageRef, value);
            }
            foliage = (GFoliageDefault)foliageRef;
        }
    }
}
#endif

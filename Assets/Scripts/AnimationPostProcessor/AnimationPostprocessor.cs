using System.IO;
using UnityEditor;
using UnityEngine;

public class AnimationPostprocessor : AssetPostprocessor 
{
    static AnimationPostProcessorSettings settings;
    static Avatar referenceAvatar;
    static GameObject referenceFBX;
    static ModelImporter referenceImporter;

    void OnPreprocessModel() 
    {

        LoadSettings();
        if (!settings.enabled) return;
        
        ModelImporter importer = assetImporter as ModelImporter;
        AssetDatabase.ImportAsset(importer.assetPath);
        
        // Extract avatar from the reference FBX if not already specified
        if (referenceAvatar == null) 
        {
            referenceAvatar = referenceImporter.sourceAvatar;
        }
        
        // Set the avatar and rig type of the imported model
        importer.sourceAvatar = referenceAvatar;
        importer.animationType = settings.animationType;
        
        // Set the animation to Generic if some issue with the avatar
        if (referenceAvatar == null || !referenceAvatar.isValid) 
        {
            importer.animationType = ModelImporterAnimationType.Generic;
        }
        
        // Use serialization to set the avatar correctly
        SerializedObject serializedObject = new SerializedObject((UnityEngine.Object) importer.sourceAvatar);
        using (SerializedObject sourceObject = new SerializedObject((UnityEngine.Object) referenceAvatar))
            CopyHumanDescriptionToDestination(sourceObject, serializedObject);
        serializedObject.ApplyModifiedProperties();
        importer.sourceAvatar = serializedObject.targetObject as Avatar;
        serializedObject.Dispose();
        
        // Translation DoF
        if (settings.enableTranslationDoF) 
        {
            var importerHumanDescription = importer.humanDescription;
            importerHumanDescription.hasTranslationDoF = true;
            importer.humanDescription = importerHumanDescription;
        }

        Debug.Log("Model processed!");
    }

    void OnPreprocessAnimation() 
    {
        
        LoadSettings();
        if (!settings.enabled) return;
        
        ModelImporter modelImporter = CopyModelImporterSettings(assetImporter as ModelImporter);
        
        AssetDatabase.ImportAsset(modelImporter.assetPath, ImportAssetOptions.ForceUpdate);

        Debug.Log("Animation processed!");
    }

    ModelImporter CopyModelImporterSettings(ModelImporter importer) 
    {

        #region Model Tab
        /*importer.globalScale = referenceImporter.globalScale;
        importer.useFileScale = referenceImporter.useFileScale;
        importer.meshCompression = referenceImporter.meshCompression;
        importer.isReadable = referenceImporter.isReadable;
        importer.optimizeMeshPolygons = referenceImporter.optimizeMeshPolygons;
        importer.optimizeMeshVertices = referenceImporter.optimizeMeshVertices;
        importer.importBlendShapes = referenceImporter.importBlendShapes;
        importer.keepQuads = referenceImporter.keepQuads;
        importer.indexFormat = referenceImporter.indexFormat;
        importer.weldVertices = referenceImporter.weldVertices;
        importer.importVisibility = referenceImporter.importVisibility;
        importer.importCameras = referenceImporter.importCameras;
        importer.importLights = referenceImporter.importLights;
        importer.preserveHierarchy = referenceImporter.preserveHierarchy;
        importer.swapUVChannels = referenceImporter.swapUVChannels;
        importer.generateSecondaryUV = referenceImporter.generateSecondaryUV;
        importer.importNormals = referenceImporter.importNormals;
        importer.normalCalculationMode = referenceImporter.normalCalculationMode;
        importer.normalSmoothingAngle = referenceImporter.normalSmoothingAngle;
        importer.importTangents = referenceImporter.importTangents;*/
        #endregion

        #region Rig Tab
        importer.animationType = referenceImporter.animationType;
        importer.optimizeGameObjects = referenceImporter.optimizeGameObjects;
        #endregion

        #region Animation Tab

        // naming conventions
        // get the filename of the FBX in case we want to use it for the animation name
        var fileName = Path.GetFileNameWithoutExtension(importer.assetPath);

        // return if there are no clips to copy on the reference importer
        if (referenceImporter.defaultClipAnimations.Length == 0) return importer;

        // Copy the first reference clip settings to all imported clips
        var referenceClip = referenceImporter.defaultClipAnimations[0];
        var referenceClipAnimations = referenceImporter.defaultClipAnimations;
        
        var defaultClipAnimations = importer.defaultClipAnimations;

        foreach (var clipAnimation in defaultClipAnimations) 
        {
            clipAnimation.hasAdditiveReferencePose = referenceClip.hasAdditiveReferencePose;
            if (referenceClip.hasAdditiveReferencePose) 
            {
                clipAnimation.additiveReferencePoseFrame = referenceClip.additiveReferencePoseFrame;
            }
            
            // Rename if needed
            if (settings.renameClips) {
                if (referenceClipAnimations.Length == 1) {
                    clipAnimation.name = fileName;
                } else {
                    clipAnimation.name = fileName + "" + clipAnimation.name;
                }
            }

            // Set loop time
            clipAnimation.loopTime = settings.loopTime;
            clipAnimation.loopPose = settings.loopPose;

            clipAnimation.maskType = referenceClip.maskType;
            clipAnimation.maskSource = referenceClip.maskSource;

            clipAnimation.keepOriginalOrientation = referenceClip.keepOriginalOrientation;
            clipAnimation.keepOriginalPositionXZ = referenceClip.keepOriginalPositionXZ;
            clipAnimation.keepOriginalPositionY = settings.keepOriginalPositionY;

            clipAnimation.lockRootRotation = settings.lockRootRotation;
            clipAnimation.lockRootPositionXZ = referenceClip.lockRootPositionXZ;
            clipAnimation.lockRootHeightY = settings.lockRootHeightY;
            clipAnimation.heightFromFeet = settings.heightFromFeet;

            clipAnimation.mirror = referenceClip.mirror;
            clipAnimation.wrapMode = referenceClip.wrapMode;
        }

        importer.clipAnimations = defaultClipAnimations;

        #endregion

        #region Materials Tab
        /*importer.materialImportMode = referenceImporter.materialImportMode;
        importer.materialLocation = referenceImporter.materialLocation;
        importer.materialName = referenceImporter.materialName;*/
        #endregion

        return importer;
    }

    void CopyHumanDescriptionToDestination(SerializedObject sourceObject, SerializedObject serializedObject) 
    {
        serializedObject.CopyFromSerializedProperty(sourceObject.FindProperty("m_HumanDescription"));
    }

    static void LoadSettings() 
    {
        // Load the animation settings specified in the setting asset file (settings.asset)
        var guids = AssetDatabase.FindAssets("t:AnimationPostProcessorSettings");
        if (guids.Length > 0) {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            settings = AssetDatabase.LoadAssetAtPath<AnimationPostProcessorSettings>(path);
            
            referenceAvatar = settings.referenceAvatar; // UMA Avatar
            referenceFBX = settings.referenceFBX; // Mixamo T-Pose
            referenceImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(referenceFBX)) as ModelImporter;
        }
    }
}
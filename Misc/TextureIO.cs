using System;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TATools.ImageProcessingTools.Misc
{
    public static class TextureIO
    {
        #if UNITY_EDITOR
        public static void WriteBackToPng(this Texture2D texture2D)
        {
            var path = AssetDatabase.GetAssetPath(texture2D);
            if (path == null)
            {
                throw new Exception("Saving a texture that isn't already present on disk is not supported");
            }

            var absPath = Path.Combine(Application.dataPath, "../", path);
            
            File.WriteAllBytes(absPath, texture2D.EncodeToPNG());
        }
        #endif
    }
    
    #if UNITY_EDITOR

    public class TextureEditScope: IDisposable
    {
        private bool _isPreviouslyEditable;
        private TextureImporterFormat _previousFormat;
        private bool _isPreviouslyOverriden;
        public string path;
        public TextureEditScope(Texture2D texture)
        {
            path = AssetDatabase.GetAssetPath(texture);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            _isPreviouslyEditable = importer.isReadable;
            importer.isReadable = true;
            var settings = importer.GetDefaultPlatformTextureSettings();
            _isPreviouslyOverriden = settings.overridden;
            _previousFormat = settings.format;
            settings.overridden = true;
            settings.format = TextureImporterFormat.RGBAFloat;
            importer.SetPlatformTextureSettings(settings);
            importer.SaveAndReimport();
        }

        public void Dispose()
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.isReadable = _isPreviouslyEditable;
            var settings = importer.GetDefaultPlatformTextureSettings();
            settings.overridden = _isPreviouslyOverriden;
            settings.format = _previousFormat;
            importer.SetPlatformTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }
    
    #endif
}
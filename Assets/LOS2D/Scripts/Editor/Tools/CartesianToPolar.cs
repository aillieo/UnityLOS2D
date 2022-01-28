using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AillieoUtils.LOS2D
{
    public static class CartesianToPolar
    {
        [MenuItem("Assets/AillieoUtils/LOS2D/CartesianToPolar", true)]
        public static bool ConvertTexture2DValidate()
        {
            Texture2D input = Selection.activeObject as Texture2D;
            return input != null;
        }

        [MenuItem("Assets/AillieoUtils/LOS2D/CartesianToPolar")]
        public static void ConvertTexture2D()
        {
            Texture2D texture = Selection.activeObject as Texture2D;
            if (texture == null)
            {
                return;
            }

            bool isReadable = texture.isReadable;
            string assetPath = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = default;
            if (!isReadable)
            {
                importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null)
                {
                    importer.isReadable = true;
                    AssetDatabase.ImportAsset(assetPath);
                    AssetDatabase.Refresh();
                }
            }

            try
            {
                int w = texture.width;
                int h = texture.height;

                Texture2D newTexture = new Texture2D(w, h, TextureFormat.RGBA32, false);

                float halfW = w / 2f;

                float r = h;
                float a = Mathf.PI / 2f;

                for (int i = 0; i < w; ++i)
                {
                    for (int j = 0; j < h; ++j)
                    {
                        // 输出纹理 标准化的 r 和 a
                        float rn = (float)j / h;
                        float an = (float)i / w;

                        // 对应的旧图中的位置
                        float angle = an * a;
                        int inX = Mathf.RoundToInt(Mathf.Cos(angle) * r * rn);
                        int inY = Mathf.RoundToInt(Mathf.Sin(angle) * r * rn);

                        Color c = texture.GetPixel(inX, inY);

                        newTexture.SetPixel(i, j, c);
                    }
                }

                newTexture.Apply(true);
                EditorUtility.SetDirty(newTexture);
                byte[] bytes = newTexture.EncodeToPNG();

                string fullpath = $"{Application.dataPath}/../{assetPath}";

                string directory = Path.GetDirectoryName(fullpath);
                string filename = Path.GetFileNameWithoutExtension(fullpath);
                string ext = Path.GetExtension(fullpath);

                string newPath = $"{directory}/{filename}_polar{ext}";

                File.WriteAllBytes(newPath, bytes);

                Debug.Log($"Created: {newPath}");

                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                if (!isReadable)
                {
                    if (importer == null)
                    {
                        importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    }

                    if (importer != null)
                    {
                        importer.isReadable = false;
                        AssetDatabase.ImportAsset(assetPath);
                        AssetDatabase.Refresh();
                    }
                }
            }
        }
    }
}

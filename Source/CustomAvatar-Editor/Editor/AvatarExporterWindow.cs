﻿//  Beat Saber Custom Avatars - Custom player models for body presence in Beat Saber.
//  Copyright © 2018-2024  Nicolas Gnyra and Beat Saber Custom Avatars Contributors
//
//  This library is free software: you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation, either
//  version 3 of the License, or (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using UnityEditor;
using UnityEngine;

namespace CustomAvatar.Editor
{
    public class AvatarExporterWindow : EditorWindow
    {
        private AvatarDescriptor[] _avatars;

        [MenuItem("Window/Avatar Exporter")]
        public static void ShowWindow()
        {
            GetWindow(typeof(AvatarExporterWindow), false, "Avatar Exporter");
        }

        #region Behaviour Lifecycle

        internal void OnFocus()
        {
            _avatars = FindObjectsOfType<AvatarDescriptor>();
            Repaint();
        }

        internal void OnGUI()
        {
            var titleLabelStyle = new GUIStyle(EditorStyles.largeLabel)
            {
                fontSize = 16
            };

            var textureStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.UpperRight
            };

            foreach (AvatarDescriptor avatar in _avatars)
            {
                if (!avatar || !avatar.gameObject) continue;

                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();

                GUILayout.Label(avatar.name, titleLabelStyle);

                GUILayout.Label("Properties", EditorStyles.largeLabel);

                EditorGUILayout.LabelField("Game Object: ", avatar.gameObject.name);
                EditorGUILayout.LabelField("Author: ", avatar.author);

                GUILayout.EndVertical();

                Texture2D texture = AssetPreview.GetAssetPreview(avatar.cover);
                GUILayout.Label(texture, textureStyle, GUILayout.MaxWidth(80), GUILayout.MaxHeight(80));

                GUILayout.EndHorizontal();

                if (GUILayout.Button("Export " + avatar.name))
                {
                    SaveAvatar(avatar);
                }

                GUILayout.Space(20);
            }
        }

        #endregion

        private void SaveAvatar(AvatarDescriptor avatar)
        {
            string destinationPath = EditorUtility.SaveFilePanel("Save avatar file", null, avatar.name + ".avatar", "avatar");

            if (string.IsNullOrEmpty(destinationPath)) return;

            string destinationFileName = Path.GetFileName(destinationPath);
            string tempFolder = Application.temporaryCachePath;
            string prefabPath = Path.Combine("Assets", "_CustomAvatar.prefab");

            PrefabUtility.SaveAsPrefabAsset(avatar.gameObject, prefabPath);
            AssetBundleManifest manifest;

            try
            {
                var assetBundleBuild = new AssetBundleBuild
                {
                    assetBundleName = destinationFileName,
                    assetNames = new[] { prefabPath }
                };

                BuildTargetGroup selectedBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;

                manifest = BuildPipeline.BuildAssetBundles(tempFolder, new[] { assetBundleBuild }, BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.StandaloneWindows64);

                // switch back to what it was before creating the asset bundle
                EditorUserBuildSettings.SwitchActiveBuildTarget(selectedBuildTargetGroup, activeBuildTarget);
            }
            finally
            {
                AssetDatabase.DeleteAsset(prefabPath);
                AssetDatabase.Refresh();
            }

            if (manifest == null)
            {
                EditorUtility.DisplayDialog("Export Failed", "Failed to create asset bundle! Please check the Unity console for more information.", "OK");
                return;
            }

            string[] assetBundleNames = manifest.GetAllAssetBundles();
            string tempAssetBundlePath = Path.Combine(tempFolder, assetBundleNames[0]);

            try
            {
                File.Copy(tempAssetBundlePath, destinationPath, true);

                EditorUtility.DisplayDialog("Export Successful!", $"{avatar.name} was exported successfully!", "OK");
            }
            catch (IOException ex)
            {
                Debug.LogError(ex);

                EditorUtility.DisplayDialog("Export Failed", $"Could not copy avatar to selected folder. Please check the Unity console for more information.", "OK");
            }
        }
    }
}

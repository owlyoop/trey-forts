using RealtimeCSG.Legacy;
using System;
using UnityEditor;
using UnityEngine;

namespace RealtimeCSG
{
    internal class CSGOptionsPreferenceWindow
    {
        [PreferenceItem("CSG Options")]
        static void PreferenceWindow()
        {
            EditorGUI.BeginChangeCheck();
            { 
                CSGSettings.ShowTooltips		= EditorGUILayout.ToggleLeft("Show Tool-Tips",						CSGSettings.ShowTooltips);
                CSGSettings.SnapNonCSGObjects	= EditorGUILayout.ToggleLeft("Snap Non-CSG Objects to the grid",	CSGSettings.SnapNonCSGObjects);
                EditorGUILayout.Space();
                CSGSettings.MaxCircleSides		= EditorGUILayout.IntField("Max Circle Sides",  CSGSettings.MaxCircleSides);
                CSGSettings.MaxSphereSplits		= EditorGUILayout.IntField("Max Sphere Splits", CSGSettings.MaxSphereSplits);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Surfaces");
                var beforeToggleWorldSpaceTexture = (CSGSettings.DefaultTexGenFlags & TexGenFlags.WorldSpaceTexture) != TexGenFlags.WorldSpaceTexture;
                var afterToggleWorldSpaceTexture = EditorGUILayout.ToggleLeft("Lock Texture To Object (Default)", beforeToggleWorldSpaceTexture);
                if (afterToggleWorldSpaceTexture != beforeToggleWorldSpaceTexture)
                {
                    if (afterToggleWorldSpaceTexture)
                        CSGSettings.DefaultTexGenFlags &= ~TexGenFlags.WorldSpaceTexture;
                    else
                        CSGSettings.DefaultTexGenFlags |= TexGenFlags.WorldSpaceTexture;
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                CSGSettings.Save();
            }
        }
    }
}

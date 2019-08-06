﻿using System;
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
			{ /*
				EditorGUILayout.LabelField("Hidden surfaces");
				EditorGUI.indentLevel++;
				{
					CSGSettings.HiddenSurfacesNotSelectable	 = EditorGUILayout.Toggle("Not selectable with marquee", CSGSettings.HiddenSurfacesNotSelectable);
					EditorGUI.indentLevel++;
					EditorGUI.BeginDisabledGroup(!CSGSettings.HiddenSurfacesNotSelectable);
					{
						EditorGUI.showMixedValue = !CSGSettings.HiddenSurfacesNotSelectable;
						CSGSettings.HiddenSurfacesOrthoSelectable = EditorGUILayout.Toggle("Except in ortho camera", CSGSettings.HiddenSurfacesOrthoSelectable);
						EditorGUI.showMixedValue = false;
					}
					EditorGUI.EndDisabledGroup();
					EditorGUI.indentLevel--;
				}
				EditorGUI.indentLevel--;
				EditorGUILayout.LabelField("Misc");*/
				//EditorGUI.indentLevel++;
				{
					CSGSettings.ShowTooltips				 = EditorGUILayout.Toggle("Show tooltips", CSGSettings.ShowTooltips);
				}
				//EditorGUI.indentLevel--;
			}
			if (EditorGUI.EndChangeCheck())
			{
				CSGSettings.Save();
			}
		}
	}
}

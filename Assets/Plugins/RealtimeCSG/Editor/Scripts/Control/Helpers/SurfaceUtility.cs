﻿using InternalRealtimeCSG;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using RealtimeCSG.Legacy;
using RealtimeCSG.Components;

namespace RealtimeCSG
{
	internal static class SurfaceUtility
	{
		public static bool CanUnSmooth(SelectedBrushSurface[] selectedSurfaces)
		{
			if (selectedSurfaces == null ||
				selectedSurfaces.Length == 0)
				return false;
			for (var s = 0; s < selectedSurfaces.Length; s++)
			{
				var brush			= selectedSurfaces[s].brush;
				var surfaceIndex	= selectedSurfaces[s].surfaceIndex;
				var shape			= brush.Shape;
				var texGens			= shape.TexGens;
				var surfaces		= shape.Surfaces;
				var texGenIndex		= surfaces[surfaceIndex].TexGenIndex;

				if (texGenIndex >= texGens.Length)
					continue;
				if (texGens[texGenIndex].SmoothingGroup > 0)
					return true;
			} 
			return false;
		}

		public static bool CanSmooth(SelectedBrushSurface[] selectedSurfaces)
		{
			if (selectedSurfaces == null ||
				selectedSurfaces.Length == 0)
				return false;
			long foundSmoothGroup = -1;
			for (var s = 0; s < selectedSurfaces.Length; s++)
			{
				var brush			= selectedSurfaces[s].brush;
				var surfaceIndex	= selectedSurfaces[s].surfaceIndex;
				var shape			= brush.Shape;
				var texGens			= shape.TexGens;
				var surfaces		= shape.Surfaces;

				var texGenIndex		= surfaces[surfaceIndex].TexGenIndex;
				if (texGens[texGenIndex].SmoothingGroup == 0)
					return true;

				if (foundSmoothGroup == -1)
				{
					foundSmoothGroup = texGens[texGenIndex].SmoothingGroup;
					continue;
				}

				if (texGens[texGenIndex].SmoothingGroup != foundSmoothGroup)
					return true;
			} 
			return false;
		}

		public static void UnSmooth(SelectedBrushSurface[] selectedSurfaces)
		{
			if (selectedSurfaces == null ||
				selectedSurfaces.Length == 0)
				return;

			var brushSurfaces = new Dictionary<CSGBrush, List<int>>();
			for (var s = 0; s < selectedSurfaces.Length; s++)
			{
				var brush			= selectedSurfaces[s].brush;
				var surfaceIndex	= selectedSurfaces[s].surfaceIndex;

				List<int> indices;
				if (!brushSurfaces.TryGetValue(brush, out indices))
				{
					indices = new List<int>();
					brushSurfaces.Add(brush, indices);
				}
				indices.Add(surfaceIndex);
			}

			if (brushSurfaces.Count == 0)
				return;

			Undo.RecordObjects(brushSurfaces.Keys.ToArray(), "Unsmoothing surfaces");
			foreach(var pair in brushSurfaces)
			{
				var brush			= pair.Key;
				var surfaceIndices	= pair.Value;

				var shape			= brush.Shape;
				var texGens			= shape.TexGens;
				var surfaces		= shape.Surfaces;

				for (var s = 0; s < surfaceIndices.Count; s++)
				{
					var texGenIndex = surfaces[surfaceIndices[s]].TexGenIndex;
					texGens[texGenIndex].SmoothingGroup = 0;
				}
				InternalCSGModelManager.CheckSurfaceModifications(brush);
			}
			InternalCSGModelManager.CheckForChanges();
		}
		
		public static void Smooth(SelectedBrushSurface[] selectedSurfaces)
		{
			if (selectedSurfaces == null ||
				selectedSurfaces.Length == 0)
				return;

			// Try to find a good smoothing group index
			var smoothingGroupIndex = FindUnusedSmoothingGroupIndex();
			var brushSurfaces		= new Dictionary<CSGBrush, List<int>>();
			for (var s = 0; s < selectedSurfaces.Length; s++)
			{
				var brush			= selectedSurfaces[s].brush;
				var surfaceIndex	= selectedSurfaces[s].surfaceIndex;

				List<int> indices;
				if (!brushSurfaces.TryGetValue(brush, out indices))
				{
					indices = new List<int>();
					brushSurfaces.Add(brush, indices);
				}
				indices.Add(surfaceIndex);
			}
			
			Undo.RecordObjects(brushSurfaces.Keys.ToArray(), "Smoothing surfaces");
			foreach(var pair in brushSurfaces)
			{
				var brush			= pair.Key;
				var surfaceIndices	= pair.Value;

				var shape			= brush.Shape;
				var texGens			= shape.TexGens;
				var surfaces		= shape.Surfaces;
				
				for (var s = 0; s < surfaceIndices.Count; s++)
				{
					var texGenIndex = surfaces[surfaceIndices[s]].TexGenIndex;
					texGens[texGenIndex].SmoothingGroup = smoothingGroupIndex;
				}
				InternalCSGModelManager.CheckSurfaceModifications(brush);
			}			
			InternalCSGModelManager.CheckForChanges();
		}
		
		public static void SetSurfaceTexGenFlags(SelectedBrushSurface[] selectedBrushSurfaces, TexGenFlags flag, bool set)
		{
			using (new UndoGroup(selectedBrushSurfaces, "discarding surface"))
			{ 
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex		= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					if (set) brush.Shape.TexGenFlags[texGenIndex] |= flag; 
					else	 brush.Shape.TexGenFlags[texGenIndex] &= ~flag;
				}
			}
		}
		
		public static void SetTextureLock(SelectedBrushSurface[] selectedBrushSurfaces, bool value)
		{
			using (new UndoGroup(selectedBrushSurfaces, "setting texture lock"))
			{ 
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex		= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;
					var oldFlags		= brush.Shape.TexGenFlags[texGenIndex];
					TexGenFlags newFlags;					
					if (value)	newFlags = oldFlags & ~TexGenFlags.WorldSpaceTexture;
					else		newFlags = oldFlags |  TexGenFlags.WorldSpaceTexture; 
					if (oldFlags == newFlags)
						continue;

					Vector2 oldTextureCoord = ConvertWorldToTextureCoord(brush, surfaceIndex, Vector3.zero);

					brush.Shape.TexGenFlags[texGenIndex] = newFlags;
					InternalCSGModelManager.SetBrushMeshSurfaces(brush);
						
					Vector2 newTextureCoord = ConvertWorldToTextureCoord(brush, surfaceIndex, Vector3.zero);
						
					brush.Shape.TexGens[texGenIndex].Translation -= (newTextureCoord - oldTextureCoord);
				}
			}
		}
				
		public static void SetMaterials(SelectedBrushSurface[] selectedBrushSurfaces, Material material)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Setting materials"))
			{
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex		= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					brush.Shape.TexGens[texGenIndex].RenderMaterial = material;
				}
			}
		}
		
		/*
		public static void SetColors(SelectedBrushSurface[] selectedBrushSurfaces, Color color)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Setting colors"))
			{ 
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					var shape			= brush.Shape;
					var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex		= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;
					
					shape.TexGens[texGenIndex].Color = color;
				}
			}
		}*/
		
		public static void MultiplyScale(SelectedBrushSurface[] selectedBrushSurfaces, float scalar)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Multiplying scale"))
			{ 
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					var shape			= brush.Shape;
					var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex		= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;
					
					shape.TexGens[texGenIndex].Scale.x *= scalar;
					shape.TexGens[texGenIndex].Scale.y *= scalar;
				}
			}
		}
		
		public static void FlipX(SelectedBrushSurface[] selectedBrushSurfaces)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Reversing texture coordinates of surface in X direction"))
			{ 
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex		= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					brush.Shape.TexGens[texGenIndex].Scale.x *= -1;
					brush.Shape.TexGens[texGenIndex].Translation.x *= -1;
				}
			}
		}
		
		public static void FlipY(SelectedBrushSurface[] selectedBrushSurfaces)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Reversing texture coordinates of surface in Y direction"))
			{ 
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex		= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					brush.Shape.TexGens[texGenIndex].Scale.y *= -1;
					brush.Shape.TexGens[texGenIndex].Translation.y *= -1;
				}
			}
		}
		
		public static void FlipXY(SelectedBrushSurface[] selectedBrushSurfaces)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Reversing texture coordinates of surface in both X and Y direction"))
			{ 
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex		= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					brush.Shape.TexGens[texGenIndex].Scale.x *= -1;
					brush.Shape.TexGens[texGenIndex].Scale.y *= -1;
					brush.Shape.TexGens[texGenIndex].Translation.x *= -1;
					brush.Shape.TexGens[texGenIndex].Translation.y *= -1;
				}
			}
		}

		public static void SetScaleX(SelectedBrushSurface[] selectedBrushSurfaces, float scaleX)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Setting X scale of surface"))
			{ 
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex		= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					brush.Shape.TexGens[texGenIndex].Scale.x = scaleX;
				}
			}
		}
		
		public static void SetScaleY(SelectedBrushSurface[] selectedBrushSurfaces, float scaleY)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Setting Y scale of surface"))
			{ 
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex		= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					brush.Shape.TexGens[texGenIndex].Scale.y = scaleY;
				}
			}
		}

		public static void SetScale(SelectedBrushSurface[] selectedBrushSurfaces, Vector2 scale)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Setting scale of surface"))
			{ 
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex		= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					brush.Shape.TexGens[texGenIndex].Scale = scale;
				}
			}
		}
		
		public static void SetTranslationX(SelectedBrushSurface[] selectedBrushSurfaces, float translationX)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Setting X translation of surface"))
			{ 
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex		= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					brush.Shape.TexGens[texGenIndex].Translation.x = translationX;
				}
			}
		}
		
		public static void SetTranslationY(SelectedBrushSurface[] selectedBrushSurfaces, float translationY)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Setting Y translation of surface"))
			{ 
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex		= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					brush.Shape.TexGens[texGenIndex].Translation.y = translationY;
				}
			}
		}

		private const float MaxAngle = 360 * 10.0f;

		public static void SetRotationAngle(SelectedBrushSurface[] selectedBrushSurfaces, float angle)
		{
			angle = angle % MaxAngle;
			using (new UndoGroup(selectedBrushSurfaces, "Setting rotation angle of surface"))
			{ 
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex		= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					brush.Shape.TexGens[texGenIndex].RotationAngle = angle;
				}
			}
		}

		public static void AddRotationAngle(SelectedBrushSurface[] selectedBrushSurfaces, float angle)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Adding to rotation angle of surface"))
			{ 
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex		= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					brush.Shape.TexGens[texGenIndex].RotationAngle = (brush.Shape.TexGens[texGenIndex].RotationAngle + angle) % MaxAngle;
				}
			}
		}
		
		public static void JustifyLayout(SelectedBrushSurface[] selectedBrushSurfaces, int justifyX, int justifyY)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Justifying surface in XY direction"))
			{ 
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					if (brush.ChildData == null ||
						brush.ChildData.ModelTransform == null)
						continue;
			
					var modelTransform			= brush.ChildData.ModelTransform;
					var modelLocalToWorldMatrix	= modelTransform.localToWorldMatrix;
					var shape					= brush.Shape;
					var surfaceIndex			= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex				= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					Vector2 minCoordinate;
					Vector2 maxCoordinate;
					GetSurfaceMinMaxTexCoords(brush.brushNodeID, surfaceIndex, modelLocalToWorldMatrix, out minCoordinate, out maxCoordinate);

					switch (justifyX)
					{
						case  1: { shape.TexGens[texGenIndex].Translation.x -= maxCoordinate.x - 1; break; }
						case  0: { shape.TexGens[texGenIndex].Translation.x -= ((maxCoordinate.x + minCoordinate.x) * 0.5f) + 0.5f; break; }
						case -1: { shape.TexGens[texGenIndex].Translation.x -= minCoordinate.x; break; }
					}
					
					switch (justifyY)
					{
						case -1: { shape.TexGens[texGenIndex].Translation.y -= maxCoordinate.y - 1; break; }
						case  0: { shape.TexGens[texGenIndex].Translation.y -= ((maxCoordinate.y + minCoordinate.y) * 0.5f) + 0.5f; break; }
						case  1: { shape.TexGens[texGenIndex].Translation.y -= minCoordinate.y; break; }
					}
				}
			}
		}
		
		public static void JustifyLayoutX(SelectedBrushSurface[] selectedBrushSurfaces, int justifyX)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Justifying surface in X direction"))
			{ 
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					if (brush.ChildData == null ||
						brush.ChildData.ModelTransform == null)
						continue;
			
					var modelTransform			= brush.ChildData.ModelTransform;
					var modelLocalToWorldMatrix	= modelTransform.localToWorldMatrix;
					var shape					= brush.Shape;
					var surfaceIndex			= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex				= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					Vector2 minCoordinate;
					Vector2 maxCoordinate;
					GetSurfaceMinMaxTexCoords(brush.brushNodeID, surfaceIndex, modelLocalToWorldMatrix, out minCoordinate, out maxCoordinate);

					switch (justifyX)
					{
						case  1: { shape.TexGens[texGenIndex].Translation.x -= maxCoordinate.x - 1; break; }
						case  0: { shape.TexGens[texGenIndex].Translation.x -= ((maxCoordinate.x + minCoordinate.x) * 0.5f) + 0.5f; break; }
						case -1: { shape.TexGens[texGenIndex].Translation.x -= minCoordinate.x; break; }
					}
				}
			}
		}
		
		public static void JustifyLayoutY(SelectedBrushSurface[] selectedBrushSurfaces, int justifyY)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Justifying surface in Y direction"))
			{ 
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					if (brush.ChildData == null ||
						brush.ChildData.ModelTransform == null)
						continue;
			
					var modelTransform			= brush.ChildData.ModelTransform;
					var modelLocalToWorldMatrix	= modelTransform.localToWorldMatrix;
					var shape					= brush.Shape;
					var surfaceIndex			= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex				= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					Vector2 minCoordinate;
					Vector2 maxCoordinate;
					GetSurfaceMinMaxTexCoords(brush.brushNodeID, surfaceIndex, modelLocalToWorldMatrix, out minCoordinate, out maxCoordinate);
					
					switch (justifyY)
					{
						case -1: { shape.TexGens[texGenIndex].Translation.y -= maxCoordinate.y - 1; break; }
						case  0: { shape.TexGens[texGenIndex].Translation.y -= ((maxCoordinate.y + minCoordinate.y) * 0.5f) + 0.5f; break; }
						case  1: { shape.TexGens[texGenIndex].Translation.y -= minCoordinate.y; break; }
					}
				}
			}
		}

		public static void FitSurface(SelectedBrushSurface[] selectedBrushSurfaces)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Fitting surface texture coordinates"))
			{
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					if (brush.ChildData == null ||
						brush.ChildData.ModelTransform == null)
						continue;
			
					var modelTransform			= brush.ChildData.ModelTransform;
					var modelLocalToWorldMatrix	= modelTransform.localToWorldMatrix;
					var shape					= brush.Shape;
					var surfaceIndex			= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex				= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					FitSurface(brush.brushNodeID, surfaceIndex, modelLocalToWorldMatrix, ref shape.TexGens[texGenIndex]);
				}
			}
		}
		
		public static void FitSurfaceX(SelectedBrushSurface[] selectedBrushSurfaces)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Fitting surface texture coordinates in X direction"))
			{
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					if (brush.ChildData == null ||
						brush.ChildData.ModelTransform == null)
						continue;
			
					var modelTransform			= brush.ChildData.ModelTransform;
					var modelLocalToWorldMatrix	= modelTransform.localToWorldMatrix;
					var shape					= brush.Shape;
					var surfaceIndex			= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex				= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					FitSurfaceX(brush.brushNodeID, surfaceIndex, modelLocalToWorldMatrix, ref shape.TexGens[texGenIndex]);
				}
			}
		}
		
		public static void FitSurfaceY(SelectedBrushSurface[] selectedBrushSurfaces)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Fitting surface texture coordinates in Y direction"))
			{
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					if (brush.ChildData == null ||
						brush.ChildData.ModelTransform == null)
						continue;
			
					var modelTransform			= brush.ChildData.ModelTransform;
					var modelLocalToWorldMatrix	= modelTransform.localToWorldMatrix;
					var shape					= brush.Shape;
					var surfaceIndex			= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex				= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					FitSurfaceY(brush.brushNodeID, surfaceIndex, modelLocalToWorldMatrix, ref shape.TexGens[texGenIndex]);
				}
			}
		}
		
		public static void ResetSurface(SelectedBrushSurface[] selectedBrushSurfaces)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Resetting surface texture coordinates"))
			{
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					var shape			= brush.Shape;
					var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex		= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					shape.TexGens[texGenIndex].RotationAngle	= 0;
					shape.TexGens[texGenIndex].Translation		= MathConstants.zeroVector2;
					shape.TexGens[texGenIndex].Scale			= MathConstants.oneVector2;
				}
			}
		}
		
		public static void ResetSurfaceX(SelectedBrushSurface[] selectedBrushSurfaces)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Resetting surface texture coordinates in X direction"))
			{
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					var shape			= brush.Shape;
					var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex		= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					shape.TexGens[texGenIndex].Translation.x	= 0;
					shape.TexGens[texGenIndex].Scale.x			= 1;
				}
			}
		}
		
		public static void ResetSurfaceY(SelectedBrushSurface[] selectedBrushSurfaces)
		{
			using (new UndoGroup(selectedBrushSurfaces, "Resetting surface texture coordinates in Y direction"))
			{
				for (var i = 0; i < selectedBrushSurfaces.Length; i++)
				{
					var brush			= selectedBrushSurfaces[i].brush;
					var shape			= brush.Shape;
					var surfaceIndex	= selectedBrushSurfaces[i].surfaceIndex;
					var texGenIndex		= brush.Shape.Surfaces[surfaceIndex].TexGenIndex;

					shape.TexGens[texGenIndex].Translation.y	= 0;
					shape.TexGens[texGenIndex].Scale.y		= 1;
				}
			}
		}

		public static bool CopyLastMaterial(CSGBrush dstBrush, int dstSurfaceIndex, CSGPlane dstSurfacePlane,
											CSGBrush srcBrush, int srcSurfaceIndex, CSGPlane srcSurfacePlane,
											bool registerUndo = true)
		{
			if (srcSurfaceIndex < 0 || srcSurfaceIndex >= srcBrush.Shape.Surfaces.Length ||
				!srcBrush)
				return false;

			if (srcBrush == dstBrush &&
				srcSurfaceIndex == dstSurfaceIndex)
				return false;
			
			var srcShape		= srcBrush.Shape;
			var dstShape		= dstBrush.Shape;

			var dstLocalPlane	= dstShape.Surfaces[dstSurfaceIndex].Plane;
			var srcLocalPlane	= srcShape.Surfaces[srcSurfaceIndex].Plane;


			var srcSurfaceInverted = Vector3.Dot(srcSurfacePlane.normal, srcLocalPlane.normal) > 0;
			var dstSurfaceInverted = Vector3.Dot(dstSurfacePlane.normal, dstLocalPlane.normal) > 0;

			return CopyLastMaterial(dstBrush, dstSurfaceIndex, dstSurfaceInverted,
									srcBrush, srcSurfaceIndex, srcSurfaceInverted,
									registerUndo);
		}
		
		public static bool CopyLastMaterial(CSGBrush dstBrush, int dstSurfaceIndex, bool dstSurfaceInverted,
											CSGBrush srcBrush, int srcSurfaceIndex, bool srcSurfaceInverted,
											bool registerUndo = true)
		{
			if (srcSurfaceIndex < 0 || srcSurfaceIndex >= srcBrush.Shape.Surfaces.Length ||
				!srcBrush)
				return false;

			if (srcBrush == dstBrush &&
				srcSurfaceIndex == dstSurfaceIndex)
				return false;


			// TODO: rewrite
			
			var srcShape		= srcBrush.Shape;
			var dstShape		= dstBrush.Shape;
			/*
			GeometryUtility.CalculateTangents(dstShape.Surfaces[dstSurfaceIndex].Plane.normal, 
												out dstShape.Surfaces[dstSurfaceIndex].Tangent, 
												out dstShape.Surfaces[dstSurfaceIndex].BiNormal);
					
			var srcBiNormal = dstShape.Surfaces[dstSurfaceIndex].BiNormal;
			var dstBiNormal = srcShape.Surfaces[srcSurfaceIndex].BiNormal;
					
			if (Vector3.Dot(srcBiNormal, dstBiNormal) > 0)
			{
				dstShape.Surfaces[dstSurfaceIndex].BiNormal = -dstShape.Surfaces[dstSurfaceIndex].BiNormal; 
				dstShape.Surfaces[dstSurfaceIndex].Tangent  = -dstShape.Surfaces[dstSurfaceIndex].Tangent;
			}
			*/

			var dstLocalPlane	= dstShape.Surfaces[dstSurfaceIndex].Plane;
			var srcLocalPlane	= srcShape.Surfaces[srcSurfaceIndex].Plane;
			

			// convert planes into worldspace
			dstLocalPlane	= GeometryUtility.InverseTransformPlane(dstBrush.transform.worldToLocalMatrix, dstLocalPlane);
			srcLocalPlane	= GeometryUtility.InverseTransformPlane(srcBrush.transform.worldToLocalMatrix, srcLocalPlane);
			
			var dstNormal	= dstLocalPlane.normal;
			var srcNormal	= srcLocalPlane.normal;
				
			var srcTexGenIndex = srcBrush.Shape.Surfaces[srcSurfaceIndex].TexGenIndex;
			var dstTexGenIndex = dstBrush.Shape.Surfaces[dstSurfaceIndex].TexGenIndex;
			
			if (srcTexGenIndex < 0 || srcTexGenIndex >= srcBrush.Shape.TexGens.Length)
				return false;
	
			var dstBrushSurface	= new [] { new SelectedBrushSurface(dstBrush, dstSurfaceIndex) };
			
			var srcTexGens		= srcShape.TexGens;
			var srcTexGenFlags	= srcShape.TexGenFlags;
			var srcSurfaces		= srcShape.Surfaces;
			var dstTexGens		= dstShape.TexGens;
			var dstTexGenFlags	= dstShape.TexGenFlags;
			var dstSurfaces		= dstShape.Surfaces;

			int groupIndex = -1;
			if (registerUndo)
			{
				Undo.IncrementCurrentGroup();
				groupIndex = Undo.GetCurrentGroup();
				using (new UndoGroup(dstBrushSurface, "Setting materials"))
				{
					dstBrush.Shape.TexGens[dstTexGenIndex].RenderMaterial = srcShape.TexGens[srcTexGenIndex].RenderMaterial;
				}
			} else
			{
				dstBrush.Shape.TexGens[dstTexGenIndex].RenderMaterial = srcShape.TexGens[srcTexGenIndex].RenderMaterial;
			}

			if (registerUndo)
			{
				using (new UndoGroup(dstBrushSurface, "Copy materials"))
				{

					// Reset destination shape to simplify calculations
					dstTexGens[dstTexGenIndex].Scale = srcShape.TexGens[srcTexGenIndex].Scale;
					dstTexGens[dstTexGenIndex].Translation = MathConstants.zeroVector2;
					dstTexGens[dstTexGenIndex].RotationAngle = 0;


					Vector3 srcPoint1, srcPoint2;
					Vector3 dstPoint1, dstPoint2;
					var edgeDirection = Vector3.Cross(dstNormal, srcNormal);
					var det = edgeDirection.sqrMagnitude;
					if (det < MathConstants.AlignmentTestEpsilon)
					{/*
						if (Vector3.Dot(dstNormal, srcNormal) < 0)
						{
							// WHY???
							if (!GetWorldSpaceTextureEdge(srcBrush.transform, srcTexGens[srcTexGenIndex], srcSurfaces[srcSurfaceIndex], out srcPoint1, out srcPoint2) ||
								!GetWorldSpaceTextureEdge(dstBrush.transform, dstTexGens[dstTexGenIndex], dstSurfaces[dstSurfaceIndex], out dstPoint1, out dstPoint2)
								)
							{
								return false;
							}
							//var dstWorldPlane	= dstBrush.Shape.Surfaces[dstSurfaceIndex].Plane.Translated(dstBrush.transform.position);
							//dstPoint1 = dstBrush.transform.TransformPoint( GeometryUtility.ProjectPointOnPlane(dstWorldPlane, srcPoint1));
							//dstPoint2 = dstBrush.transform.TransformPoint( GeometryUtility.ProjectPointOnPlane(dstWorldPlane, srcPoint2));
						} else*/
						{
							// Find 2 points on intersection of 2 planes
							srcPoint1 = srcLocalPlane.pointOnPlane;
							srcPoint2 = GeometryUtility.ProjectPointOnPlane(srcLocalPlane, srcPoint1 + MathConstants.oneVector3);

							dstPoint1 = GeometryUtility.ProjectPointOnPlane(dstLocalPlane, srcPoint1);
							dstPoint2 = GeometryUtility.ProjectPointOnPlane(dstLocalPlane, srcPoint2);
						}
					} else
					{
						// Find 2 points on intersection of 2 planes
						srcPoint1 = ((Vector3.Cross(edgeDirection, srcNormal) * -dstLocalPlane.d) +
									 (Vector3.Cross(dstNormal, edgeDirection) * -srcLocalPlane.d)) / det;
						srcPoint2 = srcPoint1 + edgeDirection;
						dstPoint1 = srcPoint1;
						dstPoint2 = srcPoint2;
					}

					if (AlignTextureSpaces(srcBrush.transform, ref srcTexGens[srcTexGenIndex], srcTexGenFlags[srcTexGenIndex], ref srcSurfaces[srcSurfaceIndex],
														   srcPoint1, srcPoint2, srcSurfaceInverted,
														   dstBrush.transform, ref dstTexGens[dstTexGenIndex], dstTexGenFlags[dstTexGenIndex], ref dstSurfaces[dstSurfaceIndex],
														   dstPoint1, dstPoint2, dstSurfaceInverted))
					{
						if (dstTexGens.Length != dstTexGenFlags.Length)
						{
							Debug.LogWarning("brush.Shape.TexGens.Length != brush.Shape.TexGenFlags.Length");
						} else
						{
							dstShape.TexGens	 = dstTexGens;
							dstShape.TexGenFlags = dstTexGenFlags;
							dstShape.Surfaces	 = dstSurfaces;
							InternalCSGModelManager.SetBrushMeshSurfaces(dstBrush);
						}
					}
				}
				Undo.CollapseUndoOperations(groupIndex);
			}
			else
			{
				// Reset destination shape to simplify calculations
				dstTexGens[dstTexGenIndex].Scale = srcShape.TexGens[srcTexGenIndex].Scale;
				dstTexGens[dstTexGenIndex].Translation = MathConstants.zeroVector2;
				dstTexGens[dstTexGenIndex].RotationAngle = 0;


				Vector3 srcPoint1, srcPoint2;
				Vector3 dstPoint1, dstPoint2;
				var edgeDirection = Vector3.Cross(dstNormal, srcNormal);
				var det = edgeDirection.sqrMagnitude;
				if (det < MathConstants.AlignmentTestEpsilon)
				{/*
					if (Vector3.Dot(dstNormal, srcNormal) < 0)
					{

						// WHY???
						if (!GetWorldSpaceTextureEdge(srcBrush.transform, srcTexGens[srcTexGenIndex], srcSurfaces[srcSurfaceIndex], out srcPoint1, out srcPoint2) ||
							!GetWorldSpaceTextureEdge(dstBrush.transform, dstTexGens[dstTexGenIndex], dstSurfaces[dstSurfaceIndex], out dstPoint1, out dstPoint2)
							)
						{
							return false;
						}
						//var dstWorldPlane	= dstBrush.Shape.Surfaces[dstSurfaceIndex].Plane.Translated(dstBrush.transform.position);
						//dstPoint1 = dstBrush.transform.TransformPoint( GeometryUtility.ProjectPointOnPlane(dstWorldPlane, srcPoint1));
						//dstPoint2 = dstBrush.transform.TransformPoint( GeometryUtility.ProjectPointOnPlane(dstWorldPlane, srcPoint2));
					} else*/
					{
						// Find 2 points on intersection of 2 planes
						srcPoint1 = srcLocalPlane.pointOnPlane;
						srcPoint2 = GeometryUtility.ProjectPointOnPlane(srcLocalPlane, srcPoint1 + MathConstants.oneVector3);

						dstPoint1 = GeometryUtility.ProjectPointOnPlane(dstLocalPlane, srcPoint1);
						dstPoint2 = GeometryUtility.ProjectPointOnPlane(dstLocalPlane, srcPoint2);
					}
				} else
				{
					// Find 2 points on intersection of 2 planes
					srcPoint1 = ((Vector3.Cross(edgeDirection, srcNormal) * -dstLocalPlane.d) +
									(Vector3.Cross(dstNormal, edgeDirection) * -srcLocalPlane.d)) / det;
					srcPoint2 = srcPoint1 + edgeDirection;
					dstPoint1 = srcPoint1;
					dstPoint2 = srcPoint2;
				}

				if (AlignTextureSpaces(srcBrush.transform, ref srcTexGens[srcTexGenIndex], srcTexGenFlags[srcTexGenIndex], ref srcSurfaces[srcSurfaceIndex],
															   srcPoint1, srcPoint2, srcSurfaceInverted,
															   dstBrush.transform, ref dstTexGens[dstTexGenIndex], dstTexGenFlags[dstTexGenIndex], ref dstSurfaces[dstSurfaceIndex],
															   dstPoint1, dstPoint2, dstSurfaceInverted))
				{
					if (dstTexGens.Length != dstTexGenFlags.Length)
					{
						Debug.LogWarning("brush.Shape.TexGens.Length != brush.Shape.TexGenFlags.Length");
					} else
					{
						dstShape.TexGens	 = dstTexGens;
						dstShape.TexGenFlags = dstTexGenFlags;
						dstShape.Surfaces	 = dstSurfaces;
						InternalCSGModelManager.SetBrushMeshSurfaces(dstBrush);
					}
				}
			}
			return true;
		}

		public static bool RotateSurfaces(SelectedBrushSurface[] selectedSurfaces, RotationCircle rotationCircle)
		{
			if (selectedSurfaces.Length == 0)
				return false;

			var prevFlags		= new TexGenFlags[selectedSurfaces.Length];
			var brushSurfaces	= new Dictionary<CSGBrush, List<int>>();
			for (var s = 0; s < selectedSurfaces.Length; s++)
			{
				var brush			= selectedSurfaces[s].brush;
				var surfaceIndex	= selectedSurfaces[s].surfaceIndex;

				List<int> indices;
				if (!brushSurfaces.TryGetValue(brush, out indices))
				{
					indices = new List<int>();
					brushSurfaces.Add(brush, indices);
				}
				indices.Add(surfaceIndex);
				
				var texGenIndex = brush.Shape.Surfaces[surfaceIndex].TexGenIndex;
				prevFlags[s] = brush.Shape.TexGenFlags[texGenIndex];
			}

			bool modified = false;
			foreach(var pair in brushSurfaces)
			{
				var brush			= pair.Key;
				var surfaceIndices	= pair.Value;

				if (brush.ChildData == null ||
					brush.ChildData.ModelTransform == null)
					continue;

				//var brushPosition		= brush.hierarchyItem.transform.position;
				//var brushPosition		= brush.hierarchyItem.transform.InverseTransformPoint(Vector3.zero);
				var brushLocalNormal	= brush.hierarchyItem.Transform.InverseTransformVector(rotationCircle.RotateSurfaceNormal);
				var brushLocalCenter	= brush.hierarchyItem.Transform.InverseTransformPoint(rotationCircle.RotateCenterPoint);
				var shape				= brush.Shape;
				for (var s = 0; s < surfaceIndices.Count; s++)
				{
					var surfaceIndex = surfaceIndices[s];
					if (Mathf.Abs(Vector3.Dot(brushLocalNormal, shape.Surfaces[surfaceIndex].Plane.normal)) > MathConstants.AngleEpsilon)
					{
						var texGenIndex		= shape.Surfaces[surfaceIndex].TexGenIndex;
						var localCenter		= brushLocalCenter;
						//if ((shape.TexGenFlags[texGenIndex] & TexGenFlags.WorldSpaceTexture) == TexGenFlags.WorldSpaceTexture)
						//	localCenter		-= brushPosition;

						RotateTextureCoordAroundLocalPoint(brush, surfaceIndex, localCenter, 
														   rotationCircle.RotateCurrentSnappedAngle);

						shape.TexGens[texGenIndex].RotationAngle = shape.TexGens[texGenIndex].RotationAngle % 360.0f;
						modified = true;
					}
				}
			}
			return modified;
		}
		
		static readonly Dictionary<CSGBrush, List<int>> __brushSurfaces = new Dictionary<CSGBrush, List<int>>();
		public static bool TranslateSurfaces(SelectedBrushSurface[] selectedSurfaces, Transform modelTransform, Vector3 oldWorldPosition, Vector3 newWorldPosition)
		{
			if (selectedSurfaces == null ||
				selectedSurfaces.Length == 0)
				return false;

			__brushSurfaces.Clear();
			for (var s = 0; s < selectedSurfaces.Length; s++)
			{
				var brush			= selectedSurfaces[s].brush;
				var surfaceIndex	= selectedSurfaces[s].surfaceIndex;

				List<int> indices;
				if (!__brushSurfaces.TryGetValue(brush, out indices))
				{
					indices = new List<int>(brush.Shape.Surfaces.Length);
					__brushSurfaces.Add(brush, indices);
				}
				indices.Add(surfaceIndex);
			}	

			var worldModified = false;
			foreach(var pair in __brushSurfaces)
			{
				var brush			= pair.Key;
				var surfaceIndices	= pair.Value;
				
				if (brush.ChildData == null ||
					brush.ChildData.ModelTransform == null)
					continue;

				var point1				= brush.hierarchyItem.Transform.InverseTransformPoint(oldWorldPosition);
				var point2				= brush.hierarchyItem.Transform.InverseTransformPoint(newWorldPosition);
				var targetControlMesh	= brush.ControlMesh;
				var targetShape			= brush.Shape;

				var brushModified		= false;
				for (var s = 0; s < surfaceIndices.Count; s++)
				{
					var surfaceIndex	= surfaceIndices[s];
					var texGenIndex		= targetShape.Surfaces[surfaceIndex].TexGenIndex;
					brushModified = TranslateTextureCoordInLocalSpace(
						ref targetShape.TexGens[texGenIndex], targetShape.TexGenFlags[texGenIndex], ref targetShape.Surfaces[surfaceIndex],
						point1, point2) || brushModified;
				}
				if (brushModified)
				{
					targetControlMesh.SetDirty();
					//InternalCSGModelManager.CheckSurfaceModifications(brush);
					//ControlMeshUtility.RebuildShape(brush);
					worldModified = true;
				}
			}
			return worldModified;
		}

		public static bool TranslateSurfacesInWorldSpace(CSGBrush[] brushes, Vector3 offset)
		{
			if (brushes == null ||
				brushes.Length == 0)
				return false;
			
			var worldModified = false;
			for (var b = 0; b < brushes.Length; b++)
			{
				var brush			= brushes[b];
				
				if (brush.ChildData == null ||
					brush.ChildData.ModelTransform == null)
					continue;
				
				var point1			= brush.hierarchyItem.Transform.InverseTransformPoint(MathConstants.zeroVector3);
				var point2			= brush.hierarchyItem.Transform.InverseTransformPoint(offset);
				var controlMesh		= brush.ControlMesh;
				var shape			= brush.Shape;

				var brushModified		= false;
				for (var s = 0; s < shape.Surfaces.Length; s++)
				{
					var surfaceIndex	= s;
					var texGenIndex		= shape.Surfaces[surfaceIndex].TexGenIndex;
					//if ((shape.TexGenFlags[texGenIndex] & TexGenFlags.WorldSpaceTexture) != TexGenFlags.WorldSpaceTexture)
						brushModified = TranslateTextureCoordInLocalSpace(
											ref shape.TexGens[texGenIndex], shape.TexGenFlags[texGenIndex], ref shape.Surfaces[surfaceIndex],
											point1, point2) || brushModified;
				}
				if (brushModified)
				{
					controlMesh.SetDirty();
					//InternalCSGModelManager.CheckSurfaceModifications(brush);
					//ControlMeshUtility.RebuildShape(brush);
					worldModified = true;
				}
			}
			return worldModified;
		}

		public static bool TranslateSurfacesInWorldSpace(CSGBrush brush, Vector3 offset)
		{
			if (!brush)
				return false;

			var worldModified = false;
			if (brush.ChildData == null ||
				brush.ChildData.ModelTransform == null)
				return false;

			var point1		= brush.hierarchyItem.Transform.InverseTransformPoint(MathConstants.zeroVector3);
			var point2		= brush.hierarchyItem.Transform.InverseTransformPoint(offset);
			var controlMesh = brush.ControlMesh;
			var shape		= brush.Shape;

			var brushModified = false;
			for (var s = 0; s < shape.Surfaces.Length; s++)
			{
				var surfaceIndex = s;
				var texGenIndex = shape.Surfaces[surfaceIndex].TexGenIndex;
				//if ((shape.TexGenFlags[texGenIndex] & TexGenFlags.WorldSpaceTexture) != TexGenFlags.WorldSpaceTexture)
				brushModified = TranslateTextureCoordInLocalSpace(
									ref shape.TexGens[texGenIndex], shape.TexGenFlags[texGenIndex], ref shape.Surfaces[surfaceIndex],
									point1, point2) || brushModified;
			}
			if (brushModified)
			{
				controlMesh.SetDirty();
				//InternalCSGModelManager.CheckSurfaceModifications(brush);
				//ControlMeshUtility.RebuildShape(brush);
				worldModified = true;
			}
			return worldModified;
		}




		#region FindUnusedSmoothingGroupIndex
		// TODO: optimize this
		internal static HashSet<uint> GetUsedSmoothingGroupIndices()
		{
			var foundGroupIndices = new HashSet<uint>();
			for (var i = 0; i < InternalCSGModelManager.Brushes.Count; i++)
			{
				var brush	= InternalCSGModelManager.Brushes[i];
				var texGens = brush.Shape.TexGens;
				for (var t = 0; t < texGens.Length; t++)
				{
					if (texGens[t].SmoothingGroup == 0)
						continue;

					foundGroupIndices.Add(texGens[t].SmoothingGroup);
				}
			}

			return foundGroupIndices;
		}

		// TODO: optimize this
		internal static uint FindUnusedSmoothingGroupIndex(HashSet<uint> foundGroupIndices)
		{
			if (foundGroupIndices.Count == 0)
				return 1;

			var list = new List<uint>(foundGroupIndices);
			list.Sort();

			if (list[0] != 1)
				return 1;

			if (list.Count == list[list.Count - 1])
				return list[list.Count - 1] + 1;

			uint prevIndex = 1;
			for (var i = 1; i < list.Count; i++)
			{
				var diff = list[i] - prevIndex;
				if (diff > 1)
					return prevIndex + 1;

				prevIndex = list[i];
			}
			return list[list.Count - 1] + 1;
		}

		public static uint FindUnusedSmoothingGroupIndex()
		{
			var foundGroupIndices = GetUsedSmoothingGroupIndices();
			return FindUnusedSmoothingGroupIndex(foundGroupIndices);
		}
		#endregion

		#region Convert..xx..Coord
		public static bool FitSurface(int brushNodeId, int surfaceIndex, Matrix4x4 modelLocalToWorldMatrix, ref TexGen surfaceTexGen)
		{
			Vector2 min;
			Vector2 max;
			if (!GetSurfaceMinMaxTexCoords(brushNodeId, surfaceIndex, modelLocalToWorldMatrix, out min, out max))
				return false;
			
			float size_x = (max.x - min.x); if (size_x < MathConstants.EqualityEpsilon) size_x = 1.0f;
			float size_y = (max.y - min.y); if (size_y < MathConstants.EqualityEpsilon) size_y = 1.0f;

			var prevScale		= surfaceTexGen.Scale;
			var prevTranslation = surfaceTexGen.Translation;

			var scale		= new Vector2(prevScale.x / size_x, prevScale.y / size_y);
			var translation = new Vector2((prevTranslation.x - min.x) / size_x, (prevTranslation.y - min.y) / size_y);
			
			surfaceTexGen.RotationAngle = 0;
			surfaceTexGen.Scale			= scale;
			surfaceTexGen.Translation	= translation;
			return true;
		}

		public static bool FitSurfaceX(int brushNodeId, int surfaceIndex, Matrix4x4 modelLocalToWorldMatrix, ref TexGen surfaceTexGen)
		{
			Vector2 min;
			Vector2 max;
			if (!GetSurfaceMinMaxTexCoords(brushNodeId, surfaceIndex, modelLocalToWorldMatrix, out min, out max))
				return false;
			
			float size_x = (max.x - min.x); if (size_x < MathConstants.EqualityEpsilon) size_x = 1.0f;

			var prevScale		= surfaceTexGen.Scale;
			var prevTranslation = surfaceTexGen.Translation;
			
			surfaceTexGen.Scale.x		= prevScale.x / size_x;
			surfaceTexGen.Translation.x	= (prevTranslation.x - min.x) / size_x;
			return true;
		}

		public static bool FitSurfaceY(int brushNodeId, int surfaceIndex, Matrix4x4 modelLocalToWorldMatrix, ref TexGen surfaceTexGen)
		{
			Vector2 min;
			Vector2 max;
			if (!GetSurfaceMinMaxTexCoords(brushNodeId, surfaceIndex, modelLocalToWorldMatrix, out min, out max))
				return false;
			
			float size_y = (max.y - min.y); if (size_y < MathConstants.EqualityEpsilon) size_y = 1.0f;
			
			var prevScale		= surfaceTexGen.Scale;
			var prevTranslation = surfaceTexGen.Translation;
			
			surfaceTexGen.Scale.y		= prevScale.y / size_y;
			surfaceTexGen.Translation.y = (prevTranslation.y - min.y) / size_y;
			return true;
		}

		public static bool GetSurfaceMinMaxTexCoords(int brushNodeId, int surfaceIndex, Matrix4x4 modelLocalToWorldMatrix, out Vector2 minTextureCoordinate, out Vector2 maxTextureCoordinate)
		{
			if (brushNodeId != CSGNode.InvalidNodeID && surfaceIndex >= 0 && 
				InternalCSGModelManager.External.GetSurfaceMinMaxTexCoords != null &&
				InternalCSGModelManager.External.GetSurfaceMinMaxTexCoords(brushNodeId, surfaceIndex, modelLocalToWorldMatrix, out minTextureCoordinate, out maxTextureCoordinate))
				return true;

			minTextureCoordinate = MathConstants.zeroVector2;
			maxTextureCoordinate = MathConstants.zeroVector2;
			return false;
		}

		public static Vector2 ConvertWorldToTextureCoord(CSGBrush brush, int surfaceIndex, Vector3 worldCoordinate)
		{
			if (brush.brushNodeID == CSGNode.InvalidNodeID || brush.Shape == null || 
				surfaceIndex < 0 || surfaceIndex >= brush.Shape.Surfaces.Length ||
				InternalCSGModelManager.External.ConvertWorldToTextureCoord == null)
				return MathConstants.zeroVector2;

			if (brush.ChildData == null ||
				brush.ChildData.ModelTransform == null)
				return MathConstants.zeroVector2;
							
			var modelTransform		= brush.ChildData.ModelTransform;
			var modelToWorldSpace	= modelTransform.localToWorldMatrix;
			
			Vector2 texcoordCoordinate;
			return InternalCSGModelManager.External.ConvertWorldToTextureCoord(brush.brushNodeID, surfaceIndex, modelToWorldSpace, worldCoordinate, out texcoordCoordinate) ? 
				texcoordCoordinate : MathConstants.zeroVector2;
		}

		public static Vector3 ConvertTextureToWorldCoord(CSGBrush brush, int surfaceIndex, Vector2 texcoordCoordinate)
		{
			if (brush == null || brush.brushNodeID == CSGNode.InvalidNodeID || brush.Shape == null || 
				surfaceIndex < 0 || surfaceIndex >= brush.Shape.Surfaces.Length ||
				InternalCSGModelManager.External.ConvertTextureToWorldCoord == null)
				return MathConstants.zeroVector3;

			if (brush.ChildData == null ||
				brush.ChildData.ModelTransform == null)
				return MathConstants.zeroVector2;
							
			var modelTransform		= brush.ChildData.ModelTransform;
			var modelToWorldSpace	= modelTransform.localToWorldMatrix;

			Vector3 worldCoordinate = MathConstants.zeroVector3;
			if (!InternalCSGModelManager.External.ConvertTextureToWorldCoord(brush.brushNodeID, 
																			 surfaceIndex, 
																			 texcoordCoordinate.x, 
																			 texcoordCoordinate.y,
																			 ref modelToWorldSpace,
																			 ref worldCoordinate.x,
																			 ref worldCoordinate.y,
																			 ref worldCoordinate.z))
				return MathConstants.zeroVector3;
			
			return worldCoordinate;
		}
		
		public static Matrix4x4 GetLocalToTextureSpaceMatrix(TexGen texGen, Surface surface)
		{
			var planeFromLocalSpace		= surface.GenerateLocalBrushSpaceToPlaneSpaceMatrix();
			var textureFromPlaneSpace	= texGen.GeneratePlaneSpaceToTextureSpaceMatrix();
			var textureFromLocalSpace	= textureFromPlaneSpace * planeFromLocalSpace;
			return textureFromLocalSpace;
		}
		
		public static bool ConvertLocalToTextureCoord(ref TexGen texGen, TexGenFlags texGenFlags, ref Surface surface, Vector3 localPoint, out Vector2 textureSpacePoint)
		{
			Matrix4x4 srcLocalSpaceToTextureSpace = GetLocalToTextureSpaceMatrix(texGen, surface);
			
			var textureSpacePoint3D = srcLocalSpaceToTextureSpace.MultiplyPoint(localPoint);
			textureSpacePoint = new Vector2(textureSpacePoint3D.x, textureSpacePoint3D.y);
			return true;
		}

		public static bool RotateTextureCoordAroundLocalPoint(CSGBrush brush, int surfaceIndex, 
															  Vector3 center, float angle)
		{
			var shape		= brush.Shape;
			var texgenIndex	= shape.Surfaces[surfaceIndex].TexGenIndex;

			Vector2 originalTextureCoordinate;
			if (!ConvertLocalToTextureCoord(ref shape.TexGens[texgenIndex], 
											shape.TexGenFlags[texgenIndex], 
											ref shape.Surfaces[surfaceIndex],
											center, out originalTextureCoordinate))
				return false;
			
			Vector3 tangent;
			Vector3 binormal;
			GeometryUtility.CalculateTangents(shape.Surfaces[surfaceIndex].Plane.normal, out tangent, out binormal);
			
			if (Vector3.Dot(shape.Surfaces[surfaceIndex].Tangent, tangent) < 0)
				shape.TexGens[texgenIndex].RotationAngle += angle;
			else
				shape.TexGens[texgenIndex].RotationAngle -= angle;
			
			Vector2 newTextureCoordinate;
			if (!ConvertLocalToTextureCoord(ref shape.TexGens[texgenIndex], 
											shape.TexGenFlags[texgenIndex], 
											ref shape.Surfaces[surfaceIndex],
											center, out newTextureCoordinate))
				return false;
			
			shape.TexGens[texgenIndex].Translation += originalTextureCoordinate - newTextureCoordinate;			
			return true;
		}
			
		public static bool TranslateTextureCoordInLocalSpace(ref TexGen texGen, TexGenFlags texGenFlags, ref Surface surface, 
															 Vector3 from, Vector3 to)
		{
			Matrix4x4 srcLocalSpaceToTextureSpace = GetLocalToTextureSpaceMatrix(texGen, surface);

			var textureSpaceFrom	= srcLocalSpaceToTextureSpace.MultiplyPoint(from);
			var textureSpaceTo		= srcLocalSpaceToTextureSpace.MultiplyPoint(to);

			Vector2 delta;
			delta.x = textureSpaceFrom.x - textureSpaceTo.x;
			delta.y = textureSpaceFrom.y - textureSpaceTo.y;

			texGen.Translation.x += delta.x;
			texGen.Translation.y += delta.y;
			return true;
		}

		public static bool AlignTextureSpaces(Transform srcTransform, ref TexGen srcTexGen, TexGenFlags srcTexGenFlags, ref Surface srcSurface, Vector3 srcWorldPoint1, Vector3 srcWorldPoint2, bool srcSurfaceInverted,
											  Transform dstTransform, ref TexGen dstTexGen, TexGenFlags dstTexGenFlags, ref Surface dstSurface, Vector3 dstWorldPoint1, Vector3 dstWorldPoint2, bool dstSurfaceInverted)
		{
			var srcLocalPoint1 = srcTransform.InverseTransformPoint(srcWorldPoint1);
			var srcLocalPoint2 = srcTransform.InverseTransformPoint(srcWorldPoint2);
			
			Matrix4x4 srcLocalSpaceToTextureSpace = GetLocalToTextureSpaceMatrix(srcTexGen, srcSurface);

			var srcTexcoord1	= srcLocalSpaceToTextureSpace.MultiplyPoint(srcLocalPoint1); srcTexcoord1.z = 0;
			var srcTexcoord2	= srcLocalSpaceToTextureSpace.MultiplyPoint(srcLocalPoint2); srcTexcoord2.z = 0;


			var dstLocalPoint1	= dstTransform.InverseTransformPoint(dstWorldPoint1);
			var dstLocalPoint2	= dstTransform.InverseTransformPoint(dstWorldPoint2);

			
			if (srcSurfaceInverted != dstSurfaceInverted)
				dstTexGen.Scale.x = -dstTexGen.Scale.x;
			
			{
				var dstLocalSpaceToTextureSpace = GetLocalToTextureSpaceMatrix(dstTexGen, dstSurface);
				var dstTexcoord1	= dstLocalSpaceToTextureSpace.MultiplyPoint(dstLocalPoint1); dstTexcoord1.z = 0;
				var translation		= new Vector3(dstTexcoord1.x - srcTexcoord1.x, dstTexcoord1.y - srcTexcoord1.y, 0);				
				dstTexGen.Translation.x -= translation.x;
				dstTexGen.Translation.y -= translation.y;
			}
			
			{
				var dstTextureSpaceToLocalSpace = GetLocalToTextureSpaceMatrix(dstTexGen, dstSurface).inverse;

				var dstLocalPoint3	= dstTextureSpaceToLocalSpace.MultiplyPoint(srcTexcoord2);
				var dstPlane		= dstSurface.Plane;
				var vec1		= (dstLocalPoint2 - dstLocalPoint1).normalized;
				var vec2		= (dstLocalPoint3 - dstLocalPoint1).normalized;
				var dstNormal	= dstPlane.normal;
				var angle		= GeometryUtility.SignedAngle(vec2, vec1, dstNormal);
				

				Vector3 tangent, binormal;
				GeometryUtility.CalculateTangents(dstPlane.normal, out tangent, out binormal);
				if (Vector3.Dot(dstSurface.Tangent, tangent) > 0)
					angle = -angle;

				
				dstTexGen.RotationAngle += angle;
			}

			{
				var dstLocalSpaceToTextureSpace = GetLocalToTextureSpaceMatrix(dstTexGen, dstSurface);
				
				var dstTexcoord1	= dstLocalSpaceToTextureSpace.MultiplyPoint(dstLocalPoint1); dstTexcoord1.z = 0;				
				var translation		= new Vector3(dstTexcoord1.x - srcTexcoord1.x, dstTexcoord1.y - srcTexcoord1.y, 0);
				dstTexGen.Translation.x -= translation.x;
				dstTexGen.Translation.y -= translation.y;
			}

			return true;
		}

		public static bool AlignTextureSpaces(Matrix4x4 srcTextureMatrix,
											  bool textureMatrixInPlaneSpace,
											  ref TexGen dstTexGen, 
											  ref TexGenFlags dstTexGenFlags,
											  ref Surface dstSurface)
		{
			if (InternalCSGModelManager.External == null)
				return false;
			

			dstTexGen.Translation	= MathConstants.zeroVector2;
			dstTexGen.Scale			= new Vector2(-1,1);
			dstTexGen.RotationAngle = 0;
			
			var dstTextureSpaceToLocalSpace = GetLocalToTextureSpaceMatrix(dstTexGen, dstSurface).inverse;

			if (!textureMatrixInPlaneSpace)
				srcTextureMatrix *= dstTextureSpaceToLocalSpace;

			var tangent		= MathConstants.rightVector3;
			var binormal	= MathConstants.upVector3;
			var normal		= MathConstants.forwardVector3;
			
			var textureSpacePlane = new CSGPlane(normal, MathConstants.zeroVector3);
			
			Quaternion invRotation = MathConstants.identityQuaternion;
			{
				var uVector				= textureSpacePlane.Project((Vector3)srcTextureMatrix.GetRow(1)).normalized; 
				var rotationAngle		= GeometryUtility.SignedAngle(uVector, binormal, normal);
				dstTexGen.RotationAngle = rotationAngle;

				invRotation = Quaternion.AngleAxis(-dstTexGen.RotationAngle, normal);
				var rotationMatrix		= Matrix4x4.TRS(MathConstants.zeroVector3, invRotation, MathConstants.oneVector3);
				srcTextureMatrix *= rotationMatrix;
			}

			var invScale = MathConstants.oneVector3;
			{
				var uVector			= textureSpacePlane.Project((Vector3)srcTextureMatrix.GetRow(0));
				var vVector			= textureSpacePlane.Project((Vector3)srcTextureMatrix.GetRow(1));
				var uVectorMagnitude = Vector3.Dot(uVector, tangent);
				var vVectorMagnitude = Vector3.Dot(vVector, binormal);
				
				dstTexGen.Scale = new Vector2(uVectorMagnitude, vVectorMagnitude);

				invScale = new Vector3(1.0f / dstTexGen.Scale.x, 1.0f / dstTexGen.Scale.y, 1.0f);				
				var scaleMatrix = Matrix4x4.TRS(MathConstants.zeroVector3, MathConstants.identityQuaternion, invScale);
				srcTextureMatrix *= scaleMatrix;
			}

			{
				var translation = srcTextureMatrix.GetColumn(3);
				translation.x /= translation.w;
				translation.y /= translation.w;				

				dstTexGen.Translation = translation;
			}

			return true;
		}
		#endregion
	}
}

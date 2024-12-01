using System;
using UnityEngine;

namespace MachineProject.CustomScripts.VehicleControls
{
    // This Class was created based on the blog post here: https://discussions.unity.com/t/simple-runtime-terrain-editor/681462 All props go to @WinterboltGames for the amazing TerrainTool which was used to implement this excavator focussed Ground Deformation Script 
    // The Major changes are mostly in the Update Function; some unneeded methods were removed
    public class CustomTerrainTerraform : MonoBehaviour
    {
        /// <summary>
        /// The width of the brush.
        /// </summary>
        [SerializeField] [Tooltip("The width of the brush.")]
        private int brushWidth;

        /// <summary>
        /// The height of the brush.
        /// </summary>
        [SerializeField] [Tooltip("The height of the brush.")]
        private int brushHeight;

        /// <summary>
        /// The strength of the brush.
        /// </summary>
        [SerializeField] [Tooltip("The strength of the brush.")]
        private float strength = 0.05f;

        /// <summary>
        /// The terrain to modify.
        /// </summary>
        private Terrain _targetTerrain;

        /// <summary>
        /// The terrain data of the terrain to modify.
        /// </summary>
        private TerrainData _targetTerrainData;

        /// <summary>
        /// The actions that can be performed when modifying the terrain.
        /// </summary>
        private enum TerrainModificationAction
        {
            Raise,
            Lower
        }

        private void Start()
        {
        }

        private void Update()
        {
            Transform shovelPos = this.transform;
            if (Physics.Raycast(shovelPos.position, -shovelPos.forward, out RaycastHit hitInfo))
            {
                // Only Handle Terrain hits
                if (hitInfo.collider.gameObject.name != "Terrain"
                    || hitInfo.distance > 5)
                {
                    return;
                }
                Debug.DrawRay(shovelPos.position, -shovelPos.forward, Color.green);

                TerrainModificationAction action = TerrainModificationAction.Lower;

                if (hitInfo.distance > 0.5f)
                {
                    if (shovelPos.rotation.eulerAngles.x < 290
                        && shovelPos.rotation.eulerAngles.x > 260)
                    {
                        action = TerrainModificationAction.Raise;
                    }
                    else
                    {
                        return;
                    }
                }
                
                
                if (hitInfo.transform.TryGetComponent(out Terrain terrain))
                {
                    _targetTerrain = terrain;

                    _targetTerrainData = _targetTerrain.terrainData;
                }
                else
                {
                    return;
                }
                
                switch (action)
                {
                    case TerrainModificationAction.Raise:
                    {
                       RaiseTerrain(hitInfo.point);

                        break;
                    }

                    case TerrainModificationAction.Lower:
                    {
                        LowerTerrain(hitInfo.point);

                        break;
                    }

                    default:
                    {
                        Debug.LogError("Invalid terrain modification action.", this);

                        break;
                    }
                }
            }
        }

        private Vector3 WorldToTerrainPosition(Vector3 worldPosition)
        {
            Vector3 terrainPosition = worldPosition - _targetTerrain.GetPosition();

            Vector3 terrainSize = _targetTerrainData.size;

            int heightmapResolution = _targetTerrainData.heightmapResolution;

            terrainPosition = new Vector3(terrainPosition.x / terrainSize.x, terrainPosition.y / terrainSize.y,
                terrainPosition.z / terrainSize.z);

            return new Vector3(terrainPosition.x * heightmapResolution, 0.0f, terrainPosition.z * heightmapResolution);
        }

        private (int, int) ClampBrushPosition(Vector3 brushWorldPosition)
        {
            Vector3 terrainPosition = WorldToTerrainPosition(brushWorldPosition);

            int heightmapResolution = _targetTerrainData.heightmapResolution;

            int clampedBrushX =
                (int)Math.Min(Math.Max((terrainPosition.x - brushWidth * 0.5f), 0), heightmapResolution);
            int clampedBrushY =
                (int)Math.Min(Math.Max((terrainPosition.z - brushHeight * 0.5f), 0), heightmapResolution);

            return (clampedBrushX, clampedBrushY);
        }

        private (int, int) ClampBrushSize(int brushX, int brushY)
        {
            int heightmapResolution = _targetTerrainData.heightmapResolution;

            int clampedBrushWidth = Math.Min(brushWidth, heightmapResolution - brushX);
            int clampedBrushHeight = Math.Min(brushHeight, heightmapResolution - brushY);

            return (clampedBrushWidth, clampedBrushHeight);
        }

        private void RaiseTerrain(Vector3 brushWorldPosition)
        {
            (int clampedBrushX, int clampedBrushY) = ClampBrushPosition(brushWorldPosition);

            (int clampedBrushWidth, int clampedBrushHeight) = ClampBrushSize(clampedBrushX, clampedBrushY);

            float[,] heights =
                _targetTerrainData.GetHeights(clampedBrushX, clampedBrushY, clampedBrushWidth, clampedBrushHeight);

            float increment = strength * Time.deltaTime;

            for (int y = 0; y < clampedBrushHeight; y++)
            {
                for (int x = 0; x < clampedBrushWidth; x++)
                {
                    heights[y, x] += increment;
                }
            }

            _targetTerrainData.SetHeights(clampedBrushX, clampedBrushY, heights);
            UpdateTerrainTexture(clampedBrushX, clampedBrushY,  2);
        }

        private void LowerTerrain(Vector3 brushWorldPosition)
        {
            (int clampedBrushX, int clampedBrushY) = ClampBrushPosition(brushWorldPosition);

            (int clampedBrushWidth, int clampedBrushHeight) = ClampBrushSize(clampedBrushX, clampedBrushY);

            float[,] heights =
                _targetTerrainData.GetHeights(clampedBrushX, clampedBrushY, clampedBrushWidth, clampedBrushHeight);

            float decrement = strength * Time.deltaTime;

            for (int y = 0; y < clampedBrushHeight; y++)
            {
                for (int x = 0; x < clampedBrushWidth; x++)
                {
                    heights[y, x] -= decrement;
                }
            }

            _targetTerrainData.SetHeights(clampedBrushX, clampedBrushY, heights);
            UpdateTerrainTexture(clampedBrushX, clampedBrushY,  2);
        }
        
        private void UpdateTerrainTexture(int posX, int posY, int textureIndex)
        {
            // Get the number of textures on the terrain
            int texturesCount = _targetTerrainData.alphamapLayers;
  
            // Create a 3D array to hold the new alpha values for each texture on the terrain
            float[,,] textureAlphas = new float[posY, posX, texturesCount];
  
            // Loop through each pixel in the brush area and set the alpha value for the specified texture index to 1, and 0 for all others
            for (var y = 0; y < posY; y++)
            {
                for (var x = 0; x < posX; x++)
                {
                    for (var i = 0; i < texturesCount; i++)
                    {
                        Debug.Log("CurrentTexture: " + i);
                        Debug.Log("CurrentTexture: " + textureIndex);
                        // If the current texture index matches the specified texture index, set its alpha value to 1
                        // Otherwise, set its alpha value to 0
                        textureAlphas[y, x, i] = (i == textureIndex) ? 1.0f : 0.0f;
                    }
                }
            }
  
            // Set the alpha map at the specified position to the updated texture alphas
            _targetTerrainData.SetAlphamaps(posX, posY, textureAlphas);
        }
    }
}
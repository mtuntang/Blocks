using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Vector2 mapSize;
    public int seed = 10;
    private Coordinate mapCenter;
    

    [Range(0,1)]
    public float outlinePercent;
    [Range(0, 1)]
    public float obstaclePercent;
    List<Coordinate> tileCoordinates;
    Queue<Coordinate> shuffledTileCoordinates;

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        tileCoordinates = new List<Coordinate>((int)(mapSize.x * mapSize.y));

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                tileCoordinates.Add(new Coordinate(x, y));
            }
        }

        shuffledTileCoordinates = new Queue<Coordinate>(Utility.ShuffleArray(tileCoordinates.ToArray(), seed));
        mapCenter = new Coordinate((int)mapSize.x / 2, (int)mapSize.y / 2);

        string holderName = "Generated Map";
        Transform mapHolder = transform.Find(holderName);
        if (mapHolder != null)
        {
            DestroyImmediate(mapHolder.gameObject);
        }

        mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                // Think Lorenz force sign for vectors, in vector3 z axis is the y of z
                // Generate each tile with its edge at the point, need to shift
                Vector3 tilePos = new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
                Transform newTile = Instantiate(tilePrefab, tilePos, Quaternion.Euler(Vector3.right * 90), mapHolder);
                newTile.localScale = Vector3.one * (1 - outlinePercent);
            }
        }

        bool[,] obstacleMap = new bool[(int)mapSize.x, (int)mapSize.y];
        int obstacleCount = (int)(mapSize.x * mapSize.y * obstaclePercent);
        int currentObstacleCount = 0;

        for (int i = 0; i < obstacleCount; i++)
        {
            Coordinate randomCoordinate = GetRandomCoordinate();
            obstacleMap[randomCoordinate.x, randomCoordinate.y] = true;
            currentObstacleCount++;

            if (randomCoordinate != mapCenter && IsMapFullyAccessible(obstacleMap, currentObstacleCount))
            {
                Vector3 obstaclePos = CoordinateToPos(randomCoordinate.x, randomCoordinate.y);
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePos + Vector3.up * 0.5f, Quaternion.identity, mapHolder);
            }
            else
            {
                obstacleMap[randomCoordinate.x, randomCoordinate.y] = false;
                currentObstacleCount--;
            }
        }
    }


    /* Floodfill, start from mapCenter since there won't be an obstacle there. Search all tiles in an expanding outwards radius (BFS). 
     * Store tiles searched in a queue and keep dequeing until no tiles are left, marking visited tiles.
     * After the traversal, the accessibleTileCount is compared to the target accessible tile count, calculated based on the map size (mapSize)
     * and current obstacle count. 
     * If the two counts match, the method returns true, indicating that the map is fully accessible. Returns false otherwise.
     */
    bool IsMapFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        int width = obstacleMap.GetLength(0);
        int height = obstacleMap.GetLength(1);
        bool[,] visitedTiles = new bool[width, height];
        Queue<Coordinate> queue = new Queue<Coordinate>();
        Coordinate mapCentre = new Coordinate(width / 2, height / 2); // Assuming mapCentre is initialized correctly

        queue.Enqueue(mapCentre);
        visitedTiles[mapCentre.x, mapCentre.y] = true;

        int accessibleTileCount = 1;

        while (queue.Count > 0)
        {
            Coordinate tile = queue.Dequeue();

            // Define the relative positions of neighboring tiles (left, up, right, down).
            int[] dx = { -1, 0, 1, 0 };
            int[] dy = { 0, -1, 0, 1 };

            for (int i = 0; i < 4; i++)
            {
                int neighbourXCoord = tile.x + dx[i];
                int neighbourYCoord = tile.y + dy[i];

                if (neighbourXCoord >= 0 && neighbourXCoord < width && neighbourYCoord >= 0 && neighbourYCoord < height)
                {
                    if (!visitedTiles[neighbourXCoord, neighbourYCoord] && !obstacleMap[neighbourXCoord, neighbourYCoord])
                    {
                        visitedTiles[neighbourXCoord, neighbourYCoord] = true;
                        queue.Enqueue(new Coordinate(neighbourXCoord, neighbourYCoord));
                        accessibleTileCount++;
                    }
                }
            }
        }

        int targetAccessibleTileCount = (width * height) - currentObstacleCount;
        return targetAccessibleTileCount == accessibleTileCount;
    }

    Vector3 CoordinateToPos(int x, int y)
    {
        return new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
    }

    public Coordinate GetRandomCoordinate()
    {
        Coordinate randomCoord = shuffledTileCoordinates.Dequeue();
        shuffledTileCoordinates.Enqueue(randomCoord);
        return randomCoord;
    }

    public struct Coordinate
    {
        public int x, y;

        public Coordinate(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coordinate c1, Coordinate c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coordinate c1, Coordinate c2)
        {
            return !(c1 == c2);
        }
    }
}

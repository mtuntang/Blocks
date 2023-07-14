using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;
    public Vector2 mapSize;
    public Vector2 maxMapSize;

    [Range(0, 1)]
    public float outlinePercent;
    [Range(0, 1)]
    public float obstaclePercent;

    public float tileSize;

    List<Coordinate> tileCoordinates;
    Queue<Coordinate> shuffledTileCoordinates;

    public int seed = 10;
    Coordinate mapCentre;

    void Start()
    {
        GenerateMap();
    }

    // TODO: Split up generate map into several helper methods, this is painful to read
    public void GenerateMap()
    {

        tileCoordinates = new List<Coordinate>();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                tileCoordinates.Add(new Coordinate(x, y));
            }
        }
        shuffledTileCoordinates = new Queue<Coordinate>(Utility.ShuffleArray(tileCoordinates.ToArray(), seed));
        mapCentre = new Coordinate((int)mapSize.x / 2, (int)mapSize.y / 2);

        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                // Think Lorenz force sign for vectors, in vector3 z axis is the y of z
                // Generate each tile with its edge at the point, need to shift
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = mapHolder;
            }
        }

        bool[,] obstacleMap = new bool[(int)mapSize.x, (int)mapSize.y];

        int obstacleCount = (int)(mapSize.x * mapSize.y * obstaclePercent);
        int currentObstacleCount = 0;

        for (int i = 0; i < obstacleCount; i++)
        {
            Coordinate randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            if (randomCoord != mapCentre && IsMapFullyAccessible(obstacleMap, currentObstacleCount))
            {
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * .5f, Quaternion.identity) as Transform;
                newObstacle.parent = mapHolder;
                newObstacle.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }

        // Generate map boundaries:
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (mapSize.x + maxMapSize.x) / 4 * tileSize, Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - mapSize.x) / 2, 1, mapSize.y) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (mapSize.x + maxMapSize.x) / 4 * tileSize, Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - mapSize.x) / 2, 1, mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (mapSize.y + maxMapSize.y) / 4 * tileSize, Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - mapSize.y) / 2) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (mapSize.y + maxMapSize.y) / 4 * tileSize, Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - mapSize.y) / 2) * tileSize;

        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;

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

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y) * tileSize;
    }

    public Coordinate GetRandomCoord()
    {
        Coordinate randomCoord = shuffledTileCoordinates.Dequeue();
        shuffledTileCoordinates.Enqueue(randomCoord);
        return randomCoord;
    }

    public struct Coordinate
    {
        public int x;
        public int y;

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
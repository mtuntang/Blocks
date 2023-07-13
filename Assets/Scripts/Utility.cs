using System.Collections;

public static class Utility
{
    /* Fisher-Yates Shuffling Algo Video: https://www.youtube.com/watch?v=TdOUjGfv1Gs&ab_channel=ygongcode
     * Read: https://exceptionnotfound.net/understanding-the-fisher-yates-card-shuffling-algorithm/
     * In summary: allows the perfect shuffling of the array and not allow 
    */
    public static T[] ShuffleArray<T>(T[] array, int seed)
    {
        System.Random prng = new System.Random(seed);

        for (int i = 0; i < array.Length - 1; i++)
        {
            int randomIndex = prng.Next(i, array.Length);
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }

        return array;
    }

    //TODO: placement of obstacles
    /* put coordinates of tiles in an array, we use fisher-yates shuffle algo so we avoid 
     * placing the same obstacles in the same location twice
     */

}
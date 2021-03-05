using System;
public static class IntHelpers 
{
    public static int[] Shuffle(this int[] array)
    {
        if (array.Length < 1) return array;
        var randorm = new Random();
        for(int i = 0; i < array.Length; i++)
        {
            int tmp = array[i];
            int rndIndex = randorm.Next(i, array.Length);
            array[i] = array[rndIndex];
            array[rndIndex] = tmp;
        }
        return array;
    }
}

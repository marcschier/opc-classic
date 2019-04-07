//----------------------------------------------------------------------------------------
//	Copyright © 2007 - 2014 Tangible Software Solutions Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class provides the logic to simulate Java rectangular arrays, which are jagged
//	arrays with inner arrays of the same length. A size of -1 indicates unknown length.
//----------------------------------------------------------------------------------------
internal static partial class RectangularArrays
{
    internal static float[][] ReturnRectangularFloatArray(int size1, int size2)
    {
        float[][] newArray;
        if (size1 > -1)
        {
            newArray = new float[size1][];
            if (size2 > -1)
            {
                for (int array1 = 0; array1 < size1; array1++)
                {
                    newArray[array1] = new float[size2];
                }
            }
        }
        else
            newArray = null;

        return newArray;
    }

    internal static int[][] ReturnRectangularIntArray(int size1, int size2)
    {
        int[][] newArray;
        if (size1 > -1)
        {
            newArray = new int[size1][];
            if (size2 > -1)
            {
                for (int array1 = 0; array1 < size1; array1++)
                {
                    newArray[array1] = new int[size2];
                }
            }
        }
        else
            newArray = null;

        return newArray;
    }

    internal static double[][] ReturnRectangularDoubleArray(int size1, int size2)
    {
        double[][] newArray;
        if (size1 > -1)
        {
            newArray = new double[size1][];
            if (size2 > -1)
            {
                for (int array1 = 0; array1 < size1; array1++)
                {
                    newArray[array1] = new double[size2];
                }
            }
        }
        else
            newArray = null;

        return newArray;
    }

    internal static char[][] ReturnRectangularCharArray(int size1, int size2)
    {
        char[][] newArray;
        if (size1 > -1)
        {
            newArray = new char[size1][];
            if (size2 > -1)
            {
                for (int array1 = 0; array1 < size1; array1++)
                {
                    newArray[array1] = new char[size2];
                }
            }
        }
        else
            newArray = null;

        return newArray;
    }

    internal static bool[][] ReturnRectangularBoolArray(int size1, int size2)
    {
        bool[][] newArray;
        if (size1 > -1)
        {
            newArray = new bool[size1][];
            if (size2 > -1)
            {
                for (int array1 = 0; array1 < size1; array1++)
                {
                    newArray[array1] = new bool[size2];
                }
            }
        }
        else
            newArray = null;

        return newArray;
    }

    internal static sbyte[][] ReturnRectangularSbyteArray(int size1, int size2)
    {
        sbyte[][] newArray;
        if (size1 > -1)
        {
            newArray = new sbyte[size1][];
            if (size2 > -1)
            {
                for (int array1 = 0; array1 < size1; array1++)
                {
                    newArray[array1] = new sbyte[size2];
                }
            }
        }
        else
            newArray = null;

        return newArray;
    }

    internal static short[][] ReturnRectangularShortArray(int size1, int size2)
    {
        short[][] newArray;
        if (size1 > -1)
        {
            newArray = new short[size1][];
            if (size2 > -1)
            {
                for (int array1 = 0; array1 < size1; array1++)
                {
                    newArray[array1] = new short[size2];
                }
            }
        }
        else
            newArray = null;

        return newArray;
    }

    internal static object[][] ReturnRectangularObjectArray(int size1, int size2)
    {
        object[][] newArray;
        if (size1 > -1)
        {
            newArray = new object[size1][];
            if (size2 > -1)
            {
                for (int array1 = 0; array1 < size1; array1++)
                {
                    newArray[array1] = new object[size2];
                }
            }
        }
        else
            newArray = null;

        return newArray;
    }

    internal static JIVariant[][] ReturnRectangularJIVariantArray(int size1, int size2)
    {
        JIVariant[][] newArray;
        if (size1 > -1)
        {
            newArray = new JIVariant[size1][];
            if (size2 > -1)
            {
                for (int array1 = 0; array1 < size1; array1++)
                {
                    newArray[array1] = new JIVariant[size2];
                }
            }
        }
        else
            newArray = null;

        return newArray;
    }
}
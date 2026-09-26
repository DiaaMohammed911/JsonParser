using System;
using System.Collections.Generic;
using System.Text;
/*
 * This class tracks the nesting depth of objects and arrays.
*/
static class DepthTracker{

    private const int MAX_DEPTH = 5;
    private static int depth = 0;

    public static void enter()
    {
        depth++;
        if (depth > MAX_DEPTH)
        {
            throw new InvalidOperationException("depth exceeded");
        }
    }
    public static void leave()
    {
        depth--;
    }


}


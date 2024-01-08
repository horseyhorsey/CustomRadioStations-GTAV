using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public class PathHelper
{
    private static int MaxPathLength { get; set; }

    static PathHelper()
    {
        // reflection
        FieldInfo maxPathField = typeof(Path).GetField("MaxPath",
            BindingFlags.Static |
            BindingFlags.GetField |
            BindingFlags.NonPublic);

        // invoke the field gettor, which returns 260
        MaxPathLength = (int)maxPathField.GetValue(null);
        //the NUL terminator is part of MAX_PATH https://msdn.microsoft.com/en-us/library/aa365247.aspx#maxpath
        MaxPathLength--; //So decrease by 1

    }


    public static bool IsPathWithinLimits(string fullPathAndFilename)
    {
        return fullPathAndFilename.Length <= MaxPathLength;
    }

    public static string MakeValidFileName(string original, char replacementChar = '_')
    {
        var invalidChars = new HashSet<char>(GetInvalidFileNameChars());
        return new string(original.Select(c => invalidChars.Contains(c) ? replacementChar : c).ToArray());
    }

    static char[] GetInvalidFileNameChars() => new char[] { '"', '<', '>', '|' };

    // Note: Some characters may not be displayable on the console.
    // The output will look something like:
    //
    // The following characters are invalid in a path:
    // Char    Hex Value
    // ",      0022
    // <,      003C
    // >,      003E
    // |,      007C
    // ...
    //
    // The following characters are invalid in a filename:
    // Char    Hex Value
    // ",      0022
    // <,      003C
    // >,      003E
    // |,      007C
    // ...
}

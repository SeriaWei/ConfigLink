using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ConfigLink.Extensions
{
    public static class JsonElementExtensions
    {
        /// <summary>
        /// Gets a JsonElement based on a path string with dot and bracket notation.
        /// Supports paths like "property.subProperty", "array[0].property", or "object['key']".
        /// </summary>
        /// <param name="root">The root JsonElement to search from</param>
        /// <param name="path">The path to the desired element, e.g. "user.profile.name" or "items[0].id"</param>
        /// <returns>The JsonElement at the specified path, or null if not found</returns>
        public static JsonElement? GetByPath(this JsonElement root, string path)
        {
            if (string.IsNullOrEmpty(path))
                return root;

            var parts = ParsePath(path);
            JsonElement currentElement = root;

            foreach (var part in parts)
            {
                if (currentElement.ValueKind == JsonValueKind.Object)
                {
                    if (!currentElement.TryGetProperty(part, out currentElement))
                        return null;
                }
                else if (currentElement.ValueKind == JsonValueKind.Array)
                {
                    // Check if the part is a numeric string that represents an array index
                    if (int.TryParse(part, out int index))
                    {
                        if (index < 0 || index >= currentElement.GetArrayLength())
                            return null;
                        
                        currentElement = currentElement[index];
                    }
                    else
                    {
                        // If trying to access an array with a non-numeric key, return null
                        return null;
                    }
                }
                else
                {
                    // If the current element is neither an object nor array, return null
                    return null;
                }
            }

            return currentElement;
        }

        /// <summary>
        /// Gets a string value from a path in the JsonElement.
        /// </summary>
        /// <param name="root">The root JsonElement to search from</param>
        /// <param name="path">The path to the desired element, e.g. "user.profile.name" or "items[0].id"</param>
        /// <returns>The string value at the specified path, or null if not found</returns>
        public static string? GetValueByPath(this JsonElement root, string path)
        {
            var element = GetByPath(root, path);
            
            if (!element.HasValue)
                return null;

            var currentElement = element.Value;
            
            return currentElement.ValueKind switch
            {
                JsonValueKind.String => currentElement.GetString(),
                JsonValueKind.Number => currentElement.TryGetInt64(out long l) ? l.ToString() : 
                                        currentElement.TryGetDouble(out double d) ? d.ToString() : 
                                        currentElement.GetDouble().ToString(),
                JsonValueKind.True => true.ToString(),
                JsonValueKind.False => false.ToString(),
                JsonValueKind.Null => null,
                JsonValueKind.Array => currentElement.GetArrayLength().ToString(),
                _ => currentElement.ToString()
            };
        }

        /// <summary>
        /// Parses a path string into individual path components, supporting both dot and bracket notation.
        /// Supports paths like "property.subProperty", "array[0].property", or "object['key']".
        /// </summary>
        /// <param name="path">The path string to parse</param>
        /// <returns>A list of path components</returns>
        private static List<string> ParsePath(string path)
        {
            var parts = new List<string>();
            var currentPart = "";
            
            for (int i = 0; i < path.Length; i++)
            {
                char c = path[i];
                
                if (c == '[')
                {
                    // Add the current part before the bracket
                    if (!string.IsNullOrEmpty(currentPart))
                    {
                        parts.Add(currentPart);
                        currentPart = "";
                    }
                    
                    // Find the closing bracket
                    int closingBracketIndex = path.IndexOf(']', i);
                    if (closingBracketIndex > i)
                    {
                        // Extract the array index/content inside brackets
                        string bracketContent = path.Substring(i + 1, closingBracketIndex - i - 1);
                        // Remove quotes if they exist (for cases like ['key'])
                        if (bracketContent.StartsWith("'") && bracketContent.EndsWith("'") && bracketContent.Length > 1)
                        {
                            bracketContent = bracketContent.Substring(1, bracketContent.Length - 2);
                        }
                        else if (bracketContent.StartsWith("\"") && bracketContent.EndsWith("\"") && bracketContent.Length > 1)
                        {
                            bracketContent = bracketContent.Substring(1, bracketContent.Length - 2);
                        }
                        parts.Add(bracketContent);
                        i = closingBracketIndex; // Skip to after the closing bracket
                    }
                    else
                    {
                        // If no closing bracket found, treat as part of the current part
                        currentPart += c;
                    }
                }
                else if (c == '.')
                {
                    // Add the current part and reset for the next
                    if (!string.IsNullOrEmpty(currentPart))
                    {
                        parts.Add(currentPart);
                        currentPart = "";
                    }
                }
                else
                {
                    currentPart += c;
                }
            }
            
            // Add the last part if it exists
            if (!string.IsNullOrEmpty(currentPart))
            {
                parts.Add(currentPart);
            }
            
            return parts;
        }
    }
}
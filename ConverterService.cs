using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VueConverter
{
    public class ConverterService
    {
        public static void ConvertFiles()
        {

            ConvertFile("");
        }

        private static void ConvertFile(string fileName)
        {
            string[] lines = System.IO.File.ReadAllLines(fileName);
            string templateSection = string.Empty;
            List<PropertyInfo> properties = new List<PropertyInfo>();
            List<GetterInfo> getters = new List<GetterInfo>();

            for (var i = 0; i < lines.Length; i++)
            {
                try
                {
                    var currentLine = lines[i];
                    if (IsProperty(currentLine))
                    {
                        properties.Add(GetProperty(currentLine, lines[i + 1]));
                    }
                    else if (IsGetter(currentLine))
                    {
                        getters.Add(GetGetter(i, lines));
                    }
                    else
                    {
                        //throw new Exception("Unable to match line");
                    }
                }
                catch (Exception ex)
                {
                    Console.Write($"Error: {ex}{Environment.NewLine}File: {new FileInfo(fileName).Name}{Environment.NewLine}Line: {i}");
                }
            }
        }

        private static bool IsFunction(string line)
        {
            return !IsGetter(line) && Regex.Match(line, ".+\\(").Success;
        }

        private static FunctionInfo GetFunction(int currentLineIndex, string[] fileLines)
        {
            return new FunctionInfo
            {
                Name = GetFunctionName(fileLines[currentLineIndex]),
                IsAsync = GetFunctionIsAsync(fileLines[currentLineIndex]),
                Body = GetBody(currentLineIndex, fileLines)
            };
        }

        private static string GetFunctionName(string functionDeclarationLine)
        {
            string[] splitArray = functionDeclarationLine.Split(" ");

            foreach (var token in splitArray)
            {
                if (token.EndsWith("()") ||
                    token.EndsWith("():"))
                {
                    return token.Replace("()", string.Empty).Replace(":", string.Empty);
                }
            }

            throw new Exception("Unable to get function name.");
        }

        private static bool GetFunctionIsAsync(string functionDeclarationLine)
        {
            return Regex.Match(functionDeclarationLine, "async").Success;
        }

        private static bool IsProperty(string line)
        {
            return line.Contains("@Prop(");
        }

        private static PropertyInfo GetProperty(string propertyDecoratorLine, string propertyLine)
        {
            return new PropertyInfo
            {
                Name = GetPropertyName(propertyLine),
                Type = GetPropertyTypes(propertyLine)
            };
        }

        private static string GetPropertyName(string propertyLine)
        {
            return propertyLine.Trim().Substring(0, propertyLine.Trim().IndexOf("!"));
        }

        private static string GetPropertyTypes(string propertyLine)
        {
            var types = propertyLine.Trim().Substring(propertyLine.Trim().IndexOf(":") + 1);
            if (types.EndsWith(";"))
            {
                types = types.Substring(0, types.Length - 1);
            }

            return types;
        }

        public static bool IsGetter(string getterLine)
        {
            const string GetterRegex = "get .+()";
            return Regex.Match(getterLine, GetterRegex).Success;
        }

        public static GetterInfo GetGetter(int currentLineIndex, string[] fileLines)
        {
            return new GetterInfo
            {
                Name = GetGetterName(fileLines[currentLineIndex]),
                Type = GetGetterType(fileLines[currentLineIndex]),
                Body = GetBody(currentLineIndex, fileLines)
            };
        }

        private static string GetGetterName(string getterLine)
        {
            string[] splitArray = getterLine.Split(" ");

            foreach (var token in splitArray)
            {
                if (token.EndsWith("():") ||
                    token.EndsWith("()"))
                {
                    return token.Replace("()", "").Replace(":", "");
                }
            }

            throw new Exception("Unable to get Getter name.");
        }

        private static string GetGetterType(string getterLine)
        {
            string[] splitArray = getterLine.Split(" ");

            for (int i = 0; i < splitArray.Length; i++)
            {
                if (splitArray[i].EndsWith("():"))
                {
                    return splitArray[i + 1];
                }
            }

            throw new Exception("Unable to get Getter type.");
        }

        private static List<string> GetBody(int currentLineIndex, string[] fileLines)
        {
            if (!fileLines[currentLineIndex].EndsWith("{"))
            {
                throw new Exception("Cannot get body for line that does not end with '{'");
            }

            var openBraceCount = 1;
            var body = new List<string>();

            while (openBraceCount > 0)
            {
                currentLineIndex++;

                var splitArray = fileLines[currentLineIndex].Split(" ");

                foreach (var token in splitArray)
                {
                    if (string.Compare(token, "{") == 0)
                    {

                        openBraceCount++;
                    }
                    else if (string.Compare(token, "}") == 0)
                    {
                        openBraceCount--;
                    }
                }

                if (openBraceCount > 0)
                {
                    body.Add(fileLines[currentLineIndex]);
                }
            }

            if (body.Count == 0)
            {
                throw new Exception("Unable to get body.");
            }

            return body;
        }

        private static List<string> GetParameters(int currentLineIndex, string[] fileLines)
        {
            var parameters = new List<string>();

            var currentLine = fileLines[currentLineIndex];
            if (currentLine.IndexOf("()") >= 0)
            {
                return parameters;
            }
            else if (currentLine.IndexOf(")") >= 0)
            {
                //return Regex.Match(currentLine, "\\(.+\\)").Result.Split(",").ToList();
            }

            parameters.Add(fileLines[currentLineIndex].Substring(fileLines[currentLineIndex].IndexOf("(")));

            currentLineIndex++;

            return parameters;
        }

    }
}

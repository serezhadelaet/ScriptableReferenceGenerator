using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Scriptables.References;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Editor
{
    [InitializeOnLoad]
    public class ReferenceGeneratorEditor
    {
        private const string ScriptsFilePathFormat = "{0}/{1}.cs";
        private const string AssetsFilePathFormat = "{0}/{1}.asset";
        private const string ScriptsDirPath = "Assets/Scripts/Scriptables/References/Generated/";
        private const string AssetsDirPath = "Assets/Data/References/";
        private static string ScriptsPath(string fileName) => string.Format(ScriptsFilePathFormat, ScriptsDirPath, fileName);
        private static string AssetsPath(string fileName) => string.Format(AssetsFilePathFormat, AssetsDirPath, fileName);

        private static ReferenceGeneratorEditor _instance;

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            _instance = new ReferenceGeneratorEditor();
            Update();
        }
        
        private static void Update()
        {
            var scripts = (MonoScript[])Resources.FindObjectsOfTypeAll(typeof(MonoScript));
            var shouldRefresh = false;
            var assets = GetRefAttributedScripts(scripts);
            foreach (var asset in assets)
                shouldRefresh |= GenerateRefSO(asset.name, asset);

            if (shouldRefresh)
                AssetDatabase.Refresh();
            
            assets = GetRefsSO(scripts);
            foreach (var asset in assets)
            {
                shouldRefresh |= GenerateRefSetter(asset);
                _instance.CreateAsset(asset);
            }

            if (shouldRefresh)
                AssetDatabase.Refresh();
        }

        private static bool GenerateRefSetter(MonoScript script)
        {
            var fileName = script.name;
            var originalScriptName = fileName.Replace("RefSO", "");
            var newFileName = $"{originalScriptName}RefSetter";
            var path = ScriptsPath(newFileName);
            if (File.Exists(path))
                return false;
            var lines = script.text.Split("\n");
            var usings = new List<string>();
            foreach (var line in lines)
            { 
               if (line.StartsWith("using") && line.EndsWith(";"))
                   usings.Add(line);
            }
            if (!usings.Contains("using UnityEngine;"))
                usings.Add("using UnityEngine;");
            WriteCodeFile(ScriptsPath(newFileName), builder =>
            {
                builder.AppendLine("//Auto generated");
                foreach (var u in usings)
                    builder.AppendLine(u);
                builder.AppendLine("\nnamespace Scriptables.References");
                builder.AppendLine("{");
                builder.AppendLine($"    [RequireComponent(typeof({originalScriptName}))]");
                builder.Append($"    public class {newFileName} : BaseRefSetter<{originalScriptName}, {fileName}>");
                builder.AppendLine(" { }");
                builder.AppendLine("}");
            });
            
            Debug.Log("Created reference setter for " + fileName);
            
            return true;
        }

        private static bool GenerateRefSO(string fileName, MonoScript script)
        {
            var newFileName = $"{fileName}RefSO";
            var path = ScriptsPath(newFileName);
            if (File.Exists(path))
                return false;
            var lines = script.text.Split("\n");
            var usings = new List<string>();
            foreach (var line in lines)
            {
                if (line.StartsWith("namespace"))
                    usings.Add("using" + line.Replace("namespace", "") + ";");
            }

            if (!usings.Contains("using UnityEngine;"))
                usings.Add("using UnityEngine;");
            WriteCodeFile(path, builder =>
            {
                builder.AppendLine("//Auto generated");
                foreach (var u in usings)
                    builder.AppendLine(u);
                builder.AppendLine("\nnamespace Scriptables.References");
                builder.AppendLine("{");
                builder.AppendLine($"    [CreateAssetMenu(menuName = \"References/{fileName}\", fileName = \"{fileName}Ref\")]");
                builder.Append($"    public class {fileName}RefSO : BaseRefSO<{fileName}>");
                builder.Append(" { }\n}");
            });

            Debug.Log("Created reference SO for " + fileName);
            
            return true;
        }

        private void CreateAsset(MonoScript script)
        {
            if (File.Exists(AssetsPath(script.name)))
                return;
            
            Type type = null;
            string typeName = $"Scriptables.References.{script.name}";
            var path = AssetsPath(script.name);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                type = assembly.GetType(typeName);
                if (type != null)
                    break;
            }  
            MethodInfo method = _instance.GetType().GetMethod("CreateSO").MakeGenericMethod(type);
            method.Invoke(_instance, new object[] { script.name });
        }

        public void CreateSO<T>(string fileName) where T : ScriptableObject
        {
            var instance = ScriptableObject.CreateInstance<T>();
            Directory.CreateDirectory(AssetsDirPath);
            AssetDatabase.CreateAsset(instance, AssetsPath(fileName));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private static List<MonoScript> GetRefAttributedScripts(MonoScript[] scripts)
        {
            var result = new List<MonoScript>();
            foreach (var m in scripts)
            {
                var cl = m.GetClass(); 
                if (cl?.GetCustomAttribute<ReferenceAutoGeneration>() != null)
                    result.Add(m);
            }
            return result;
        }
        
        private static List<MonoScript> GetRefsSO(MonoScript[] scripts)
        {
            var result = new List<MonoScript>();
            foreach (var m in scripts)
            {
                var cl = m.GetClass();
                if (cl != null && cl.FullName != null && cl.FullName.Contains("RefSO"))
                    result.Add(m);
            }
            return result;
        }
        
        private static void WriteCodeFile(string path, Action<StringBuilder> callback)
        {
            if (!Directory.Exists(ScriptsDirPath))
                Directory.CreateDirectory(ScriptsDirPath);
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
                using FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write);
                using StreamWriter writer = new StreamWriter(stream);
                var builder = new StringBuilder();
                callback(builder);
                writer.Write(builder.ToString());
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}
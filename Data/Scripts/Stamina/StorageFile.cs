using Sandbox.ModAPI;
using System;
using VRage.Utils;

namespace Keyspace.Stamina
{
    public class StorageFile
    {
        public int Test { get; set; }

        public StorageFile()
        {
            Test = 0;
        }

        public static StorageFile LoadFromFile(string fileName)
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, typeof(StorageFile)))
            {
                try
                {
                    StorageFile config = null;

                    string contents;
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(fileName, typeof(StorageFile)))
                    {
                        contents = reader.ReadToEnd();
                    }
                    
                    config = MyAPIGateway.Utilities.SerializeFromXML<StorageFile>(contents);
                    config.Test++; // DEBUG: hello world

                    MyLog.Default.WriteLineAndConsole($"Loaded {fileName}.");

                    return config;
                }
                catch (Exception e)
                {
                    MyLog.Default.WriteLineAndConsole($"ERROR: Could not load {fileName}. Will use defaults. Exception:");
                    MyLog.Default.WriteLineAndConsole(e.ToString());
                }
            }
            else
            {
                MyLog.Default.WriteLineAndConsole($"{fileName} not found. Will use defaults.");
            }

            return new StorageFile();
        }

        public void SaveToFile(string fileName)
        {
            try
            {
                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(fileName, typeof(StorageFile)))
                {
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML(this));
                }

                MyLog.Default.WriteLineAndConsole($"Config saved to {fileName}. Setting test: {Test}.");
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"ERROR: Could not save {fileName}. Exception:");
                MyLog.Default.WriteLineAndConsole(e.ToString());
            }
        }
    }
}

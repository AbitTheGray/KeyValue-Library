using System;
using System.IO;
using System.Text;

namespace Kv
{
    public static class KvFile
    {
        #region Save

        public static void Save(this KvRecord kv, string path, bool excludeFirst = true)
        {
            using(StreamWriter writer = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None)))
                Save(kv, writer, excludeFirst);
        }

        public static void Save(this KvRecord kv, TextWriter writer, bool excludeFirst = true)
        {
            if (excludeFirst)
            {
                foreach (KvRecord record in kv)
                    record.Write(writer, 0);
            }
            else
                kv.Write(writer, 0);
        }
        
        private static void Write(this KvRecord kv, TextWriter writer, int depth)
        {
            // Depth prefix by TABs
            for(int i = 0; i < depth; i++)
                writer.Write('\t');

            if (kv.HasChild)
            {
                writer.Write('"');
                writer.Write(kv.EscapedKey);
                writer.WriteLine('"');
                
                // Opening { with right depth
                for(int i = 0; i < depth; i++)
                    writer.Write('\t');
                writer.WriteLine('{');
                
                // Child Data
                foreach(KvRecord record in kv)
                    record.Write(writer, depth+1);
                
                // Closing } with right depth
                for(int i = 0; i < depth; i++)
                    writer.Write('\t');
                writer.WriteLine('}');
            }
            else
            {
                writer.Write('"');
                writer.Write(kv.EscapedKey);
                writer.Write('"');
                
                writer.Write('\t');
                
                writer.Write('"');
                writer.Write(kv.EscapedValue);
                writer.WriteLine('"');
            }
        }
        
        #endregion
        
        #region Load

        public static KvRecord Load(string path)
        {
            if (!File.Exists(path))
                return null;
            using(StreamReader reader = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
                return Load(reader, Path.GetFileNameWithoutExtension(path));
        }

        public static KvRecord Load(StreamReader reader, string filename = "File")
        {
            StringBuilder sb = new StringBuilder();
            int lineNum = 0;

            int keyStart = -1;
            int keyEnd = -1;
            int valueStart = -1;
            int valueEnd = -1;
            
            KvRecord record_current = new KvRecord(filename);
            KvRecord record_top = record_current;

            while (true)
            {
                // Key
                {
                    // Start of key
                    if (keyStart == -1)
                    {
                        bool foundEnd = false;
                        for (int i = 0; i < sb.Length; i++)
                        {
                            if (sb[i] == '"')
                            {
                                keyStart = i + 1;
                                break;
                            }

                            if (sb[i] == '}')
                            {
                                record_current = record_current.Parent;
                                if(record_current == null)
                                    throw new Exception($"Line {lineNum} ended the file too soon.");
                                sb.Remove(0, i + 1);
                                foundEnd = true;
                                break;
                            }

                            // Comment (checking for keyStart = outside "" block)
                            if (sb[i] == '/' && i < sb.Length-1 && sb[i+1] == '/')
                            {
                                if (reader.EndOfStream)
                                    break;

                                sb.Remove(i, sb.Length - i);
                                
                                lineNum++;
                                sb.Append('\n');
                                sb.Append(reader.ReadLine());

                                // Redo current index instead of going whole loop again
                                i--;
                            }
                        }
                        if (foundEnd)
                            continue;

                        if (keyStart == -1)
                        {
                            if (reader.EndOfStream)
                                break;
                            lineNum++;
                            sb.Append('\n');
                            sb.Append(reader.ReadLine());
                            continue;
                        }
                    }

                    // End of key
                    if (keyEnd == -1)
                    {
                        for (int i = keyStart; i < sb.Length; i++)
                        {
                            if (sb[i] == '"' && i < sb.Length-1 && sb[i + 1] != '\\')
                            {
                                keyEnd = i;
                                break;
                            }
                        }

                        if (keyEnd == -1)
                        {
                            if (reader.EndOfStream)
                                break;
                            lineNum++;
                            sb.Append('\n');
                            sb.Append(reader.ReadLine());
                            continue;
                        }
                    }
                }

                // Value
                {
                    // Start of value
                    if (valueStart == -1)
                    {
                        bool foundStart = false;
                        for (int i = keyEnd + 1; i < sb.Length; i++)
                        {
                            if (sb[i] == '"')
                            {
                                valueStart = i + 1;
                                break;
                            }

                            if (sb[i] == '{')
                            {
                                record_current = record_current.Add(KvRecord.Unescape(sb.ToString(keyStart, keyEnd - keyStart)));
                                sb.Remove(0, i + 1);
                                
                                keyStart = -1;
                                keyEnd = -1;
                                
                                foundStart = true;
                                break;
                            }

                            // Comment (checking for valueStart = outside "" block)
                            if (sb[i] == '/' && i < sb.Length-1 && sb[i-1] == '/')
                            {
                                if (reader.EndOfStream)
                                    break;

                                sb.Remove(i, sb.Length - i);
                                
                                lineNum++;
                                sb.Append('\n');
                                sb.Append(reader.ReadLine());

                                // Redo current index instead of going whole loop again
                                i--;
                            }
                        }
                        if (foundStart)
                            continue;

                        if (valueStart == -1)
                        {
                            if (reader.EndOfStream)
                                break;
                            lineNum++;
                            sb.Append('\n');
                            sb.Append(reader.ReadLine());
                            continue;
                        }
                    }

                    // End of value
                    if (valueEnd == -1)
                    {
                        for (int i = valueStart; i < sb.Length; i++)
                        {
                            if (sb[i] == '"' && sb[i - 1] != '\\')
                            {
                                valueEnd = i;
                                break;
                            }
                        }

                        if (valueEnd == -1)
                        {
                            if (reader.EndOfStream)
                                break;
                            lineNum++;
                            sb.Append('\n');
                            sb.Append(reader.ReadLine());
                            continue;
                        }
                    }
                }

                record_current.Add(KvRecord.Unescape(sb.ToString(keyStart, keyEnd - keyStart)), KvRecord.Unescape(sb.ToString(valueStart, valueEnd - valueStart)));
                sb.Remove(0, valueEnd + 1);
                
                keyStart = -1;
                keyEnd = -1;
                
                valueStart = -1;
                valueEnd = -1;
            }
            
            //Console.WriteLine("Lines: "+lineNum);

            return record_top;
        }
        
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using MessagePack;

namespace Raccoon
{
    public static class SaveTool
    {
        /// <summary>
        /// Saves an object to a file using MessagePack serialization without compression.
        /// Automatically saves to Application.persistentDataPath.
        /// </summary>
        /// <typeparam name="T">The type of the object to save. Must be a class.</typeparam>
        /// <param name="filename">The name of the file (e.g., "mySave.dat").</param>
        /// <param name="data">The object instance to save.</param>
        public static void SaveFile<T>(string filename, T data) where T : class
        {
            // Validate the filename to prevent illegal characters.
            if (!IsValidFilename(filename))
            {
                Debug.LogError($"[SaveTool] Invalid filename provided: '{filename}'. Save operation aborted.");
                return;
            }

            string fullpath = Application.persistentDataPath + "/" + filename;
            try
            {
                // Serialize data using MessagePack.
                byte[] bytes = MessagePackSerializer.Serialize(data);

                // Write the serialized bytes to the file.
                File.WriteAllBytes(fullpath, bytes);
                Debug.Log($"[SaveTool] Saved: '{filename}' ({bytes.Length} bytes)");
            }
            catch (Exception e)
            {
                // Log any errors that occur during the save process.
                Debug.LogError("[SaveTool] Error Saving: " + e.Message);
            }
        }

        /// <summary>
        /// Loads an object from a file using MessagePack deserialization.
        /// Includes a fallback to BinaryFormatter for loading legacy save files,
        /// and automatically upgrades them to MessagePack format.
        /// </summary>
        /// <typeparam name="T">The type of the object to load. Must be a class.</typeparam>
        /// <param name="filename">The name of the file (e.g., "mySave.dat").</param>
        /// <returns>The loaded object instance, or null if the file does not exist or loading fails.</returns>
        public static T LoadFile<T>(string filename) where T : class
        {
            string fullpath = Application.persistentDataPath + "/" + filename;
            // Check for valid filename and if the file exists before attempting to load.
            if (!IsValidFilename(filename) || !File.Exists(fullpath))
            {
                Debug.LogWarning($"[SaveTool] File '{filename}' not found or invalid filename. Returning null.");
                return null;
            }

            T data = null;
            byte[] bytes = null; // This variable is not strictly necessary but kept from original structure.

            try
            {
                // Read all bytes from the file.
                bytes = File.ReadAllBytes(fullpath);
                // Deserialize data using MessagePack.
                data = MessagePackSerializer.Deserialize<T>(bytes);
                Debug.Log("[SaveTool] Loaded MessagePack save");
            }
            catch (Exception mpEx)
            {
                // If MessagePack deserialization fails, attempt to load using BinaryFormatter for legacy saves.
                Debug.LogWarning("[SaveTool] MessagePack load failed, trying BinaryFormatter fallback: " + mpEx.Message);
                try
                {
                    using (FileStream file = File.Open(fullpath, FileMode.Open))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
#pragma warning disable SYSLIB0011 // Suppress warning about BinaryFormatter being obsolete.
                        data = (T)bf.Deserialize(file);
#pragma warning restore SYSLIB0011
                    }
                    Debug.Log("[SaveTool] Loaded legacy BinaryFormatter save");

                    // If a legacy save is loaded, re-save it in the new MessagePack format.
                    SaveFile(filename, data);
                    Debug.Log("[SaveTool] Re-saved in MessagePack format");
                }
                catch (Exception bfEx)
                {
                    // Log error if BinaryFormatter also fails.
                    Debug.LogError("[SaveTool] BinaryFormatter load failed: " + bfEx.Message);
                    return null;
                }
            }
            return data;
        }

        /// <summary>
        /// Checks if a save file exists at the persistent data path.
        /// </summary>
        /// <param name="filename">The name of the file to check.</param>
        /// <returns>True if the file exists and the filename is valid, false otherwise.</returns>
        public static bool DoesFileExist(string filename)
        {
            string fullpath = Application.persistentDataPath + "/" + filename;
            return IsValidFilename(filename) && File.Exists(fullpath);
        }

        /// <summary>
        /// Deletes a specified file from the persistent data path.
        /// </summary>
        /// <param name="filename">The name of the file to delete.</param>
        public static void DeleteFile(string filename)
        {
            string fullpath = Application.persistentDataPath + "/" + filename;
            if (File.Exists(fullpath))
            {
                File.Delete(fullpath);
                Debug.Log($"[SaveTool] Deleted '{filename}'.");
            }
            else
            {
                Debug.LogWarning($"[SaveTool] Attempted to delete '{filename}', but it does not exist.");
            }
        }

        /// <summary>
        /// Retrieves a list of all save file names in the persistent data path,
        /// optionally filtered by a specific file extension.
        /// </summary>
        /// <param name="extension">The file extension to filter by (e.g., ".dat"). Leave empty to get all files.</param>
        /// <returns>A list of filenames.</returns>
        public static List<string> GetAllSave(string extension = "")
        {
            List<string> saves = new List<string>();
            string[] files = Directory.GetFiles(Application.persistentDataPath);
            foreach (string file in files)
            {
                // Ensure the extension check is case-insensitive for broader compatibility.
                if (string.IsNullOrEmpty(extension) || file.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    string filename = Path.GetFileName(file);
                    // Add filename if not already present (prevents duplicates if case sensitivity differs).
                    if (!saves.Contains(filename))
                        saves.Add(filename);
                }
            }
            Debug.Log($"[SaveTool] Found {saves.Count} save files with extension '{extension}'.");
            return saves;
        }

        /// <summary>
        /// Validates if a given string is suitable as a filename, checking for null/whitespace
        /// and invalid characters.
        /// </summary>
        /// <param name="filename">The string to validate as a filename.</param>
        /// <returns>True if the filename is valid, false otherwise.</returns>
        public static bool IsValidFilename(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename)) return false;
            foreach (char c in Path.GetInvalidFileNameChars())
                if (filename.Contains(c.ToString()))
                    return false;
            return true;
        }
    }
}

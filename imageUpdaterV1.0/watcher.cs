﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.IO;

namespace imageUpdaterV1._0
{
    public partial class watcher
    {
 
        private static db database;
        
       
        //give full access to directory
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        
        public static void Main()
        {

            database = new db();

            //using GetCurrentDirectory() get directory where executable is located
            string path = System.IO.Directory.GetCurrentDirectory();

            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();

            watcher.IncludeSubdirectories = true; //allow to watch for changes in subdirectories

            watcher.Path = path;
            /* Watch for changes in LastAccess and LastWrite times, and the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch jpeg files.
            watcher.Filter = "*.jp*";// "*.jpeg";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
           
            // Begin watching.
            watcher.EnableRaisingEvents = true;

            // Wait for the user to quit the program.
            Console.WriteLine("Press \'q\' to quit.");
            while (Console.Read() != 'q') ;

        }//end of Main method

        
        
        public static void OnChanged(object source, FileSystemEventArgs e)
        {

                string filename = e.Name.ToString();//turn e.Name to string
                filename = filename.Remove(filename.LastIndexOf(".")); //remove the file extention
                filename = filename.Substring(filename.IndexOf("10.0.")); //remove any directory names or anything else before the image name to leave only ip_address and reg_no: 10.0.14.17_AY15USN
                
                string reg_no = filename.Remove(0,11);

                string cam_no = null;
                string ip_address;
                ip_address = filename.Remove(filename.LastIndexOf("_"));

                if (ip_address == "***.***.***.***")
                {
                    cam_no = ip_address.Replace(ip_address, "1");
                }
                // Specify what is done when a file is changed, created, or deleted.
                Console.WriteLine(e.FullPath + " " + e.ChangeType + " " + filename);

                //just for the test purpose write the name without extension again
                Console.WriteLine(filename);
                Console.WriteLine(ip_address);
                Console.WriteLine(reg_no);
               

           
                //06-10-2017 when file changed , created, or deleted create text file and make record
                using (StreamWriter write = new StreamWriter("onChange.txt", true))
                {
                   
                    write.WriteLine(e.FullPath + " " + e.ChangeType + " " + filename + " " + DateTime.Now);
                    //just for the test purpose write the name without extension again
                    write.WriteLine(filename);
                    write.WriteLine(ip_address);
                    write.Close();


                }
                string path = e.FullPath;
                path = path.Replace(@"\", @"\\").Replace("'", @"\'");

                if (e.ChangeType == WatcherChangeTypes.Created)
                {
                    string query = "INSERT into `images` VALUES (null, current_timestamp(), '" + reg_no + "','" + path + "','" + cam_no + "','" + e.FullPath + "');";
                    database.Insert(query);
                }

                if (e.ChangeType == WatcherChangeTypes.Changed)
                {
                    string query = "UPDATE images SET `reg_no` = '"+ reg_no +"' WHERE `reg_no` = '"+ reg_no +"'";
                    //string query = "INSERT into `images` VALUES (null, current_timestamp(), '" + reg_no + "','" + n_path + "','" + cam_no + "','" + e.FullPath + "');";
                    database.Update(query);
                }

                if (e.ChangeType == WatcherChangeTypes.Deleted)
                {
                    string query = "DELETE FROM `images` WHERE reg_no = '" + reg_no + "');";
                    database.Delete(query);
                }



        }//end of onChange 

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);

            //06-10-2017 when file is renamed create 
            using (StreamWriter write = new StreamWriter("onChange.txt", true))
            {
                write.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath + " " + DateTime.Now);
                write.Close();

            }

            
        }
    }
}

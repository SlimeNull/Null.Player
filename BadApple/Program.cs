using CHO.Json;
using ForMinecraft;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Media;
using System.Runtime.InteropServices;

namespace NullPlayer
{
    class Program
    {
        public static uint SND_ASYNC = 0x0001;
        public static uint SND_FILENAME = 0x00020000;
        [DllImport("winmm.dll")]
        public static extern uint mciSendString(string lpstrCommand, string lpstrReturnString, uint uReturnLength, uint hWndCallback);
        [DllImport("winmm.dll")]
        public static extern uint mciExecute(string lpstrCommand);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName([MarshalAs(UnmanagedType.LPTStr)] string path, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath, int shortPathLength);

        static void Main(string[] args)
        {
            ConsArgs consArgs = new ConsArgs(args);


            string filename;
            int rate = 30;
            bool audio = false;
            if (consArgs.Content.Length > 0)
            {
                filename = consArgs.Content[0];
                if (File.Exists(filename))
                {
                    List<string> frames = null;
                    try
                    {
                        frames = JsonData.Deserialize<List<string>>(JsonData.Parse(File.ReadAllText(filename)));

                        if (consArgs.Properties.ContainsKey("FRAMERATE"))
                        {
                            if (int.TryParse(consArgs.Properties["FRAMERATE"], out int _rate))
                            {
                                rate = _rate;
                            }
                            else
                            {
                                Console.WriteLine("Frame rate is not a number");
                                Environment.Exit(-1);
                            }
                        }
                        if (consArgs.Properties.ContainsKey("AUDIO"))
                        {
                            string audioFilename = consArgs.Properties["AUDIO"];
                            if (File.Exists(audioFilename))
                            {
                                StringBuilder sb = new StringBuilder(255);
                                int audioShortnameCount = GetShortPathName(audioFilename, sb, sb.Capacity);
                                if (audioShortnameCount != 0)
                                {
                                    mciSendString($"close {sb}", null, 0, 0);
                                    mciSendString($"open {sb} alias frameaudio", null, 0, 0);
                                    //mciExecute($"open {sb} alias frameaudio");
                                    audio = true;
                                }
                                else
                                {
                                    Console.WriteLine("Get shortname of audio file -failed");
                                    Environment.Exit(-1);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Audio file not found");
                            }
                        }

                        Console.CursorVisible = false;

                        int left = Console.CursorLeft;
                        int top = Console.CursorTop;

                        int playIndex = 0;
                        DateTime timeTarget = DateTime.Now;
                        TimeSpan per = new TimeSpan(TimeSpan.TicksPerSecond / rate);

                        if (audio)
                        { 
                            mciSendString("play frameaudio", null, 0, 0);
                        }

                        while(playIndex < frames.Count)
                        {
                            while(DateTime.Now < timeTarget)
                            { }
                            timeTarget += per;

                            Console.CursorLeft = 0;
                            Console.CursorTop = 0;
                            Console.WriteLine(frames[playIndex]);
                            Console.Title = $"NullPlayer: {filename} 播放进度:{playIndex}/{frames.Count}";

                            playIndex++;
                        }
                        Console.CursorVisible = true;

                        Console.CursorLeft = left;
                        Console.CursorTop = top;

                        Console.WriteLine();
                    }
                    catch
                    {
                        Console.WriteLine("CharVideo format error");
                        Environment.Exit(-1);
                    }
                }
                else
                {
                    Console.WriteLine("File not found");
                    Environment.Exit(-1);
                }
            }
            else
            {
                Console.WriteLine("NullPlayer : Official program for playing charframes");
                Console.WriteLine("    NullPlayer [-Audio filename] [-FrameRate number] -source");
                Console.WriteLine("    | Audio: Background music of charframes, no default audio");
                Console.WriteLine("    | FrameRate: Frame rate of charframes, default value is 30");
                Console.WriteLine();
                Console.WriteLine("    Copyright (C) Null 2019, All rights reserved.");
                Environment.Exit(-1);
            }
        }
    }
}

/*
 *  Author: Luke Watson
 *  Date:   11/27/2013
 */
using SpencerGifts.Common.Logging;
using System;
using TLogUpdater.Library;
using System.Configuration;

namespace CON_TlogUpdater
{
    /// <summary>
    /// This is just a simple console app to call the main library
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main program
        /// </summary>
        /// <param name="args">The library expects 3 parameters, soure, destination and backup locations as strings</param>
        static void Main(string[] args)
        {
            Logger log = Logger.Create(); //create an instance of the logger

            if (!Convert.ToBoolean(ConfigurationManager.AppSettings["enabled"].ToString()))
            {
                log.LogMessage("TLog Updater invoked, but program is not enabled in config. Exiting.");
                Environment.ExitCode = 0;
            }
            else
            {
                try
                {
                    if (args.Length > 0) //if there are arguments, use them
                    {
                        UpdateTLog updater = new UpdateTLog(args[0], args[1], args[2]);
                    }
                    else //use default constructor if no arguments are passed.
                    {
                        log.LogMessage("no arguments passed, using default constructor.");
                        UpdateTLog updater = new UpdateTLog();
                    }
                }
                catch (Exception ex)
                {
                    if (log == null) { Console.WriteLine(ex.Message.ToString()); } //just output the error to screen if no logger is present
                    else { log.LogMessage("Unhandled exception during TLog Update " + ex.Message + "\r\n" + ex.StackTrace); }

                    Environment.ExitCode = 1; //exit console app with non 0 exit code
                }
                Environment.ExitCode = 0;
            }
        }
    }
}

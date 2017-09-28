using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using SpencerGifts.Common.Logging;
using System.Diagnostics.Contracts;

namespace TLogUpdater.Library
{
    public class UpdateTLog
    {
        private string _inputLocation;
        private string _outputLocation;
        private string _backupLocation;
        private string _fileName;

        private Logger log;

        public UpdateTLog()
            : this(ConfigurationManager.AppSettings["sourceLocation"].ToString(), ConfigurationManager.AppSettings["destinationLocation"].ToString(), ConfigurationManager.AppSettings["backupLocation"].ToString())
        {
        }

        public UpdateTLog(string source, string destination, string backup)
        {
            Contract.Requires(source != null, "source file location cannot be null");
            Contract.Requires(destination != null, "desination file location cannot be null");
            Contract.Requires(backup != null, "backup file location cannot be null");

            _inputLocation = source;
            _outputLocation = destination;
            _backupLocation = backup;

            log = Logger.Create(); //create an instance of the logger

            if (!_inputLocation.EndsWith(@"\")) _inputLocation = _inputLocation + @"\";
            if (!_outputLocation.EndsWith(@"\")) _outputLocation = _outputLocation + @"\";
            if (!_backupLocation.EndsWith(@"\")) _backupLocation = _backupLocation + @"\";

            try
            {
                foreach (FileInfo TLog in new DirectoryInfo(_inputLocation).GetFiles())
                {
                    _fileName = TLog.Name;

                    try
                    {
                        ReadTLog();
                        moveFile(_outputLocation + "TEMP_" + _fileName, _outputLocation + _fileName, 1);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    BackupLog();
                }
            }
            catch (Exception ex) { throw ex; }
        }

        /// <summary>
        /// Read the TLog in and check each line meets the conditions for processing, otherwise simply output the line.
        /// </summary>
        public void ReadTLog()
        {
            Contract.Requires(_inputLocation != null, "input location must be specified");
            Contract.Requires(_fileName != null, "there must be a file to process");

            log.LogMessage(String.Format("begining processing of {0}", _fileName));

            string currentLine = String.Empty;
            string curTran = "";
            bool shouldProcess = true;
            var config = TLogUpdaterSection.GetConfig();
            List<Definition> definitions = new List<Definition>();

            try
            {
                using (FileStream stream = File.OpenRead(_inputLocation + _fileName))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        while (reader.Peek() != -1)
                        {
                            try
                            {
                                currentLine = reader.ReadLine();
                                if (currentLine.Trim().Length != 0)
                                {
                                    string[] splitLine = currentLine.Split('\t');

                                    if (curTran != splitLine[3].ToString()) //should only change on a 10 record?
                                    {
                                        definitions = new List<Definition>();
                                        curTran = splitLine[3].ToString();
                                        //log.LogMessage(String.Format("Transaction #{0}, begining checks", curTran));

                                        foreach (Definition definition in config.Definitions)
                                        {
                                            shouldProcess = true;

                                            foreach (ConditionDefinition condition in definition.ConditionDefinitions)
                                            {
                                                try
                                                {
                                                    if (processTest(splitLine[condition.Position].ToString(), condition.Value, condition.Logic, Type.GetType(condition.Type, true, true)) && shouldProcess)
                                                    {
                                                        shouldProcess = condition.ShouldProcess;
                                                    }
                                                    else
                                                    {
                                                        shouldProcess = !condition.ShouldProcess;
                                                    }
                                                }
                                                catch (Exception) { }
                                            }
                                            if (shouldProcess)
                                            {
                                                definitions.Add(definition);
                                            }
                                        }
                                    }
                                    if (definitions.Count > 0)
                                    {
                                        //log.LogMessage(String.Format("Transaction #{0} needs updating", curTran));
                                        splitLine = ProcessLine(splitLine, definitions);
                                    }

                                    WriteTLog(String.Join("\t", splitLine));

                                }
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Process a single line entry and update the values based on the definitions. Pass the output to WriteLog
        /// </summary>
        /// <param name="line">an array representing the fields in one TLog line entry</param>
        public string[] ProcessLine(string[] line, List<Definition> definitions)
        {
            try
            {
                foreach (Definition definition in definitions)
                {
                    foreach (UpdateDefinition updateDefinition in definition.UpdateDefinitions)
                    {
                        if (updateDefinition.LineType == -1 || updateDefinition.LineType == Convert.ToInt32(line[0].ToString()))
                        {
                            if (line[updateDefinition.Position] != String.Empty)
                            {
                                //log.LogMessage(String.Format("Transaction #{0}, changing {1} to {2}", line[3].ToString(), line[updateDefinition.Position].ToString(), updateDefinition.Value.ToString()));
                                line[updateDefinition.Position] = updateDefinition.Value;
                            }
                        }
                    }
                }

                return line;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Write the line to the new TLog.
        /// </summary>
        /// <param name="line">a string representing a tlog line entry</param>
        public void WriteTLog(string line)
        {
            Contract.Requires(_outputLocation != null, "an output location must be specified");
            try
            {
                //log.LogMessage(String.Format("updates complete, saving temp file TEMP_{0}", _fileName));
                using (StreamWriter file = new StreamWriter(_outputLocation + "TEMP_" + _fileName, true))
                {
                    file.WriteLine(line);
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Each time this runs we need to save the file to a backup location by date.        
        /// </summary>
        public void BackupLog()
        {
            try
            {
                moveFile(_inputLocation + _fileName, _backupLocation + @"\" + _fileName, 1);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void moveFile(string inputFile, string outputFile, int attempt)
        {
            try
            {
                if (File.Exists(outputFile))
                {
                    if (outputFile.EndsWith("." + (attempt - 1).ToString().PadLeft(3, '0')))
                    {
                        outputFile = outputFile.Replace((attempt - 1).ToString().PadLeft(3, '0'), attempt.ToString().PadLeft(3, '0'));
                    }
                    else
                    {
                        outputFile = outputFile + "." + attempt.ToString().PadLeft(3, '0');
                    }

                    attempt++;
                    moveFile(inputFile, outputFile, attempt);
                }
                else
                {
                    log.LogMessage(String.Format("moving file {0} to {1}", inputFile, outputFile));
                    File.Move(inputFile, outputFile);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool processTest(object Object, object Test, LogicOperator logic, Type type)
        {
            switch (logic)
            {
                case LogicOperator.Eq:
                    if (type == typeof(DateTime))
                    {
                        if (Convert.ToDateTime(Object) == Convert.ToDateTime(Test))
                            return true;
                    }
                    if (type == typeof(int))
                    {
                        if ((int)Object == (int)Test)
                            return true;
                    }
                    if (type == typeof(String))
                    {
                        if ((String)Object == (String)Test)
                            return true;
                    }
                    break;
                case LogicOperator.Geq:
                    if (type == typeof(DateTime))
                    {
                        if (Convert.ToDateTime(Object) >= Convert.ToDateTime(Test))
                            return true;
                    }
                    if (type == typeof(int))
                    {
                        if ((int)Object >= (int)Test)
                            return true;
                    }
                    break;
                case LogicOperator.Gt:
                    if (type == typeof(DateTime))
                    {
                        if (Convert.ToDateTime(Object) > Convert.ToDateTime(Test))
                            return true;
                    }
                    if (type == typeof(int))
                    {
                        if ((int)Object > (int)Test)
                            return true;
                    }
                    break;
                case LogicOperator.Leq:
                    if (type == typeof(DateTime))
                    {
                        if (Convert.ToDateTime(Object) <= Convert.ToDateTime(Test))
                            return true;
                    }
                    if (type == typeof(int))
                    {
                        if ((int)Object <= (int)Test)
                            return true;
                    }
                    break;
                case LogicOperator.Lt:
                    if (type == typeof(DateTime))
                    {
                        if (Convert.ToDateTime(Object) < Convert.ToDateTime(Test))
                            return true;
                    }
                    if (type == typeof(int))
                    {
                        if ((int)Object < (int)Test)
                            return true;
                    }
                    break;
                case LogicOperator.Neq:
                    if (type == typeof(DateTime))
                    {
                        if (Convert.ToDateTime(Object) != Convert.ToDateTime(Test))
                            return true;
                    }
                    if (type == typeof(int))
                    {
                        if ((int)Object != (int)Test)
                            return true;
                    }
                    if (type == typeof(String))
                    {
                        if ((String)Object != (String)Test)
                            return true;
                    }
                    break;
                default:
                    return false;
            }
            return false;
        }
    }
}

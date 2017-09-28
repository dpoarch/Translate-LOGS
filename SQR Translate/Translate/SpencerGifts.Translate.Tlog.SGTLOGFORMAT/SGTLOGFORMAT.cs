using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpencerGifts.Translate.Plugin.Tlog.SGTLOGFORMAT
{
    public class SGTLOGFORMAT : TranslateItem
    {
        public string INPUTDIR = "\\sgawapp\\Retail_apps\\ValueLink\\PAYMENTECH\\TLOG\\";
        public string OUTPUTDIR = "\\sgicorp.spencergifts.com\\spencergifts\\POS\\Polling\\Data\\RecoverT\\";
        public string PROGDIR = "\\sgawapp\\Retail_apps\\GIFTCARD\\PROGRAMS\\";
        public string WORKDIR = "\\sgawapp\\Retail_apps\\ValueLink\\PAYMENTECH\\TLOG\\WORK\\";

        public void begin_report()
        {
            delete_tlog_go_file();
            program1();
            Delay_10_Seconds();
            delete_filea();
            Delay_10_Seconds();
            //Reset();
            return;
        }

        /// <summary>
        /// This is called from the Begin-Report procedure.  
        /// </summary>
        public void program1()
        {

            Console.WriteLine("I am in Program1");

            var prog1 = "GFTLOGSN.EXE";
            var inputfile = "tlog.txt";
            var workfile = "tlog";

            var dos_string0 = "cmd /c ";
            var dos_string1 = base.PluginConfig.GetValue("PROGDIR");
            var dos_string2 = prog1;
            var dos_string3 = base.PluginConfig.GetValue("INPUTDIR");
            var dos_string4 = inputfile;
            var dos_string5 = base.PluginConfig.GetValue("OUTPUTDIR");

            var dos_string = dos_string0 + dos_string1 + dos_string2 + " " + dos_string3 + dos_string4 + " " + dos_string5;

            Console.WriteLine("***** FILE XX ****");
            Console.WriteLine(" ");
            Console.WriteLine(dos_string);
            Console.WriteLine(" ");

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = dos_string;
            process.StartInfo = startInfo;
            process.Start();

            if (process.ExitCode == 0)
            {
                Console.WriteLine(" ");
                Console.WriteLine("* Program1 completed Successfully  *");
                Console.WriteLine(" ");
                var copy_flag = 'Y';
                Console.WriteLine(prog1);
                Console.WriteLine(" ");
            }
            else
            {
                Console.WriteLine(" ");
                Console.WriteLine("***** Program1 failed");
                var sgi_err_msg = "Stop at Program1 Execution";
                //SGI_Stop_Job();
                Console.WriteLine(" ");
            }
            Console.WriteLine("---------------------");
            return;
        }

        /// <summary>
        /// This is called from the Begin-Report procedure.
        /// </summary>
        public void program2()
        {
            Console.WriteLine("I am in Program2");

            var prog2 = "SPLTLOGS.EXE";
            var inputfile = "tlog";

            var dos_string0 = "cmd /c ";
            var dos_string1 = base.PluginConfig.GetValue("PROGDIR");
            var dos_string2 = prog2;
            var dos_string3 = base.PluginConfig.GetValue("WORKDIR");
            var dos_string4 = inputfile;
            var dos_string5 = base.PluginConfig.GetValue("OUTPUTDIR");

            var dos_string = dos_string0 + dos_string1 + dos_string2 + " " + dos_string3 + " " + dos_string4 + " " + dos_string5;

            Console.WriteLine("***** FILE XX ****");
            Console.WriteLine(" ");
            Console.WriteLine(dos_string);
            Console.WriteLine(" ");

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = dos_string;
            process.StartInfo = startInfo;
            process.Start();

            if (process.ExitCode == 0)
            {
                Console.WriteLine(" ");
                Console.WriteLine("* Program2 completed Successfully  *");
                Console.WriteLine(" ");
                var copy_flag = 'Y';
                Console.WriteLine("SPLTLOGS.EXE");
                Console.WriteLine(" ");
            }
            else
            {
                Console.WriteLine(" ");
                Console.WriteLine("***** Program2 failed");
                var sgi_err_msg = "Stop at Program2 Execution";
                //SGI_Stop_Job();
                Console.WriteLine(" ");
            }
            Console.WriteLine("---------------------");
            return;
        }

        public void Delay_10_Seconds()
        {
            //#debuga display 'Delay-10-Seconds  '

            System.Threading.Thread.Sleep(10000);

            var Rpt_Msg = "Delay 10 Seconds  COMPLETED";
            Console.WriteLine(" ");
            Console.WriteLine(Rpt_Msg);
            Console.WriteLine(" ");

            return;
        }

        /// <summary>
        /// This is called from the Begin-Report procedure. 
        /// </summary>
        public void delete_tlog_go_file()
        {

            var dos_string = "cmd /c del " + base.PluginConfig.GetValue("INPUTDIR") + "pt_tlog.GO";
            Console.WriteLine("***** FILE XX ****");
            Console.WriteLine(" ");
            Console.WriteLine(dos_string);
            Console.WriteLine(" ");

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = dos_string;
            process.StartInfo = startInfo;
            process.Start();

            if (process.ExitCode >= 32)
            {
                Console.WriteLine(" ");
                Console.WriteLine("***** deleted ****");
                Console.WriteLine(" ");
                Console.WriteLine("pt_tlog.GO");
                Console.WriteLine(" ");
            }
            Console.WriteLine("---------------------");
            return;
        }

        public void delete_filea()
        {

            var dos_string = "cmd /c del " + base.PluginConfig.GetValue("INPUTDIR") + "tlog.txt";
            Console.WriteLine("***** FILE XX ****");
            Console.WriteLine(" ");
            Console.WriteLine(dos_string);
            Console.WriteLine(" ");
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = dos_string;
            process.StartInfo = startInfo;
            process.Start();
            if (process.ExitCode >= 32)
            {
                Console.WriteLine(" ");
                Console.WriteLine("***** deleted ****");
                Console.WriteLine(" ");
                Console.WriteLine("tlog.txt");
                Console.WriteLine(" ");
            }
            Console.WriteLine("---------------------");

            return;
        }

        public void delete_fileb()
        {
            var dos_string1 = "cmd /c del " + base.PluginConfig.GetValue("WORKDIR") + "tlog";
            Console.WriteLine("***** FILE XX ****");
            Console.WriteLine(" ");
            Console.WriteLine(dos_string1);
            Console.WriteLine(" ");
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = dos_string1;
            process.StartInfo = startInfo;
            process.Start();
            if (process.ExitCode >= 32)
            {
                Console.WriteLine(" ");
                Console.WriteLine("***** deleted ****");
                Console.WriteLine(" ");
                Console.WriteLine("tlog");
                Console.WriteLine(" ");
            }
            Console.WriteLine("---------------------");
            return;
        }

        /// <summary>
        /// Reports SQL Errors Called by various procedures.
        /// </summary>
        public void SQL_Error()
        {
            //evaluate #sql-status
            //#ifdef DB2
            //when = 6100    !DB2 error for empty-table result set
            //break
            //#end-if

            //#ifdef DB2UNIX
            //when = 6100    !DB2 error for empty-table result set
            //break
            //#end-if

            //when = -99999  !Token "when" clause for non-DB2 environments
            //when-other
            //display $sqr-program noline
            //display ': ' noline
            //display $ReportID noline
            //display ' - SQL Statement = '
            //display $SQL-STATEMENT
            //display 'SQL Status =' noline
            //display #sql-status 99999 noline
            //Console.WriteLine(" "); noline
            //display 'SQL Error  = ' noline
            //display $sql-error
            //display $Sql-Msg
            //SHOW  $loadrecord
            //Rollback_Transaction();
            var sgi_err_msg = "Stop at SQL Processing";
            //SGI_Stop_Job();
            return;
        }

        //!----------------------------------------------------------------------!
        //! Called SQC Procedures                                                !
        //!----------------------------------------------------------------------!
        // #include 'reset.sqc'     ! Reset printer procedure
        // #include 'tranctrl.sqc'  ! Tools transaction control module
        // #include 'sgerror.sqc'   ! SGI Error Handling procedure
    }
}

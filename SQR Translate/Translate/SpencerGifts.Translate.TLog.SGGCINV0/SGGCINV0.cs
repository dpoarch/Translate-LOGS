﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpencerGifts.Translate.Plugin.TLog.SGGCINV0
{
    public class SGGCINV0 : TranslateItem
    {
        public datadbDataContext db = new datadbDataContext();
        //#include 'setenv.sqc'   !Set environment procedure
        //#include 'setup02.sqc'  ! Landscape printer/page size initialization
        public string filestring3 = "D" + DateTime.Now.ToString("yyyy-MM-dd") + "." + "T" + DateTime.Now.ToString("HH:mm:ss");

        /// <summary>
        /// Begin-Report
        /// </summary>
        public void begin_report()
        {
            process_main();
            create_GCINV_go_file();
            create_AS400_files();
            //reset();
            return;
        }

        /// <summary>
        /// This is highest level driving procedure called from Begin-Report.  
        /// </summary>
        public void process_main()
        {
            open_file();
            char recordfound = select_gcinv();

            if (recordfound == 'Y')
            {
                update_gcinv();
            }

            //Added code by Murali Kaithi on 06/11/2001 to close the opened files

            //close 1
            //close 2

            //backup_file();
            //#debuga show 'Total Records selected from GIFT_CARD_INV ' #recordcount
            return;
        }

        /// <summary>
        /// File Opening Procedure   
        /// </summary>
        public void open_file()
        {
            var ok = false;
            var filename = base.PluginConfig.GetValue("filestring1") + base.PluginConfig.GetValue("filestring2") + base.PluginConfig.GetValue("filetype");

            FileStream openLog = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

            if (!File.Exists(filename))
            {
                File.Create(base.PluginConfig.GetValue("filestring1") + base.PluginConfig.GetValue("filestring2") + base.PluginConfig.GetValue("filetype"));
            }
            //open $filename as 1 for-writing record=40:vary status=#filestat
            if (openLog.CanRead == false)
            {
                Console.WriteLine("Could not open GCINV file");
                Console.WriteLine(filename);
                var sgi_err_msg = "Stop at GCINV file opening";
                Console.WriteLine(sgi_err_msg);
                //SGI_Stop_Job();
            }
            else
            {

                //open $tlogwdir as 20 for_writing record=100:vary status=#filestat
                var count = 0;
                while (ok != true)
                {
                    var lineCount = File.ReadAllLines(filename).Length;
                    if (lineCount >= 2000)
                    {
                        count++;
                        if (File.Exists(base.PluginConfig.GetValue("filstring1") + base.PluginConfig.GetValue("filestring2") + count + base.PluginConfig.GetValue("filetype")))
                        {
                            filename = base.PluginConfig.GetValue("filstring1") + base.PluginConfig.GetValue("filestring2") + count + base.PluginConfig.GetValue("filetype");
                        }
                        else
                        {
                            File.Create(base.PluginConfig.GetValue("filstring1") + base.PluginConfig.GetValue("filestring2") + count + base.PluginConfig.GetValue("filetype"));
                        }
                    }
                }
            }



            var BKfilename = base.PluginConfig.GetValue("filestringbu") + base.PluginConfig.GetValue("filestring2") + filestring3 + base.PluginConfig.GetValue("filetype");

            if (!File.Exists(BKfilename))
            {
                File.Create(base.PluginConfig.GetValue("filestringbu") + base.PluginConfig.GetValue("filestring2") + filestring3 + base.PluginConfig.GetValue("filetype"));
            }
            FileStream openBKfilename = File.Open(BKfilename, FileMode.Open, FileAccess.Read, FileShare.Read);
            //open $filename as 1 for-writing record=40:vary status=#filestat
            if (openLog.CanRead == false)
            {
                Console.WriteLine("Could not open GCINV file");
                Console.WriteLine(BKfilename);
                var sgi_err_msg = "Stop at GCINV file opening";
                Console.WriteLine(sgi_err_msg);
                //SGI_Stop_Job();
            }
            else
            {

                //open $tlogwdir as 20 for_writing record=100:vary status=#filestat
                var count = 0;
                while (ok != true)
                {
                    var lineCount = File.ReadAllLines(BKfilename).Length;
                    if (lineCount >= 2000)
                    {
                        count++;
                        if (File.Exists(base.PluginConfig.GetValue("filestringbu") + base.PluginConfig.GetValue("filestring2") + filestring3 + count + base.PluginConfig.GetValue("filetype")))
                        {
                            BKfilename = base.PluginConfig.GetValue("filestringbu") + base.PluginConfig.GetValue("filestring2") + filestring3 + count + base.PluginConfig.GetValue("filetype");
                        }
                        else
                        {
                            File.Create(base.PluginConfig.GetValue("filestringbu") + base.PluginConfig.GetValue("filestring2") + filestring3 + count + base.PluginConfig.GetValue("filetype"));
                        }
                    }
                }
            }
            return;
        }

        /// <summary>
        /// selects the current date and time.  
        /// </summary>
        public DateTime select_date_time()
        {
            //#ifdef debuga
            //   show 'FLOW: select-date_time'
            //#endif

            DateTime current = DateTime.Now;

            return current;
        }

        /// <summary>
        /// The Procedure which selects the records from GIFT_CARD_INV                             
        /// </summary>
        public char select_gcinv()
        {
            //#ifdef debuga
            //   show 'FLOW: select-gcinv'
            //#endif

            var recordfound = 'N';

            var GCINV = db.GIFT_CARD_INV_PR10_TEMPs.Join(db.PROMOs, g => g.id, p => p.id, (giftcard, promo) => new { GiftCard = giftcard, Promo = promo }).FirstOrDefault(i => i.GiftCard.VL_PROMOTION_CODE == i.Promo.PROMO_CODE && i.GiftCard.SENT_DATE == null);


            string PRO_CD = GCINV.GiftCard.VL_PROMOTION_CODE;
            int ST_NO = Convert.ToInt32(GCINV.GiftCard.STORE_NO);
            string TRANS_DT = GCINV.GiftCard.TRANSACTION_DATE.ToString();
            int CARD_USED = Convert.ToInt32(GCINV.GiftCard.CARDS_USED_QTY);
            string SKU_ID = GCINV.Promo.ID_ITM_SKU;

            string pro_code = PRO_CD;
            int store_no = ST_NO;
            string trans_date = TRANS_DT;
            int card_qty = CARD_USED;
            string sign;
            if (card_qty < 0)
            {
                sign = "-";
                card_qty = card_qty * -1;
            }
            else
            {
                sign = "+";
            }
            var sku_id_item = SKU_ID;

            recordfound = 'Y';
            //var recordcount = recordcount + 1;

            write_gcinv(store_no, trans_date, pro_code, sku_id_item, sign, card_qty);

            return recordfound;
        }

        /// <summary>
        /// This procedure writes the GCINV Header Record 
        /// </summary>
        public void write_gcinv(int store_no, string trans_date, string pro_code, string sku_id_item, string sign, int card_qty)
        {
            var rec_format = "030";
            var card_used_qty = card_qty;

            File.AppendAllText("1.txt", Environment.NewLine + rec_format + store_no + trans_date + pro_code + sku_id_item + sign + card_used_qty);
            File.AppendAllText("2.txt", Environment.NewLine + rec_format + store_no + trans_date + pro_code + sku_id_item + sign + card_used_qty);

            return;
        }

        /// <summary>
        /// This procedure will update GIFT_CARD_INV  
        /// </summary>
        public void update_gcinv()
        {
            GIFT_CARD_INV_PR10_TEMP giftcard = db.GIFT_CARD_INV_PR10_TEMPs.FirstOrDefault(i => i.SENT_DATE == null);
            giftcard.SENT_DATE = DateTime.Now;
            db.SubmitChanges();

            return;
        }

        /// <summary>
        /// This is called from the Begin-Report procedure. 
        /// </summary>
        public void create_GCINV_go_file()
        {
            var ok = false;
            var GoFileName = base.PluginConfig.GetValue("filstring1") + "GCINV.GO";

            if (!File.Exists(GoFileName))
            {
                File.Create(base.PluginConfig.GetValue("filstring1") + "GCINV.GO");
            }
            FileStream openGoFileName = File.Open(GoFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            //open $filename as 1 for-writing record=40:vary status=#filestat
            if (openGoFileName.CanRead == false)
            {
                Console.WriteLine("Could not open GCINV file");
                Console.WriteLine(GoFileName);
                var sgi_err_msg = "Stop at GCINV file opening";
                Console.WriteLine(sgi_err_msg);
                //SGI_Stop_Job();
            }
            else
            {

                //open $tlogwdir as 20 for_writing record=100:vary status=#filestat
                var count = 0;
                while (ok != true)
                {
                    var lineCount = File.ReadAllLines(GoFileName).Length;
                    if (lineCount >= 2000)
                    {
                        count++;
                        if (File.Exists(base.PluginConfig.GetValue("filstring1") + "GCINV_" + count + ".GO" + base.PluginConfig.GetValue("filetype")))
                        {
                            GoFileName = base.PluginConfig.GetValue("filstring1") + "GCINV_" + count + ".GO" + base.PluginConfig.GetValue("filetype");
                        }
                        else
                        {
                            File.Create(base.PluginConfig.GetValue("filstring1") + "GCINV_" + count + ".GO" + base.PluginConfig.GetValue("filetype"));
                        }
                    }
                }
            }

            //var dummy = "GCINV.GO";

            //write 100 from
            //$dummy:8

            return;
        }

        public void create_AS400_files()
        {
            string filestring4 = "GCINV1.";
            string GoFileName2 = "GCINV.GO";
            string GoFileName3 = "GCINV1.GO";

            string dos_stringd = "cmd /c del ";
            string dos_stringd1 = base.PluginConfig.GetValue("filstring1") + base.PluginConfig.GetValue("filestring2") + base.PluginConfig.GetValue("filetype");
            string dos_stringd2 = base.PluginConfig.GetValue("filstring1") + GoFileName2;
            string dos_string1 = "cmd /c copy ";
            string dos_string2 = base.PluginConfig.GetValue("filstring1") + base.PluginConfig.GetValue("filestring2") + base.PluginConfig.GetValue("filetype");
            string dos_string21 = base.PluginConfig.GetValue("filstring1") + GoFileName2;
            string dos_string3 = base.PluginConfig.GetValue("filstring1") + filestring4 + base.PluginConfig.GetValue("filetype");
            string dos_string31 = base.PluginConfig.GetValue("filstring1") + GoFileName3;
            string dos_string = dos_string1 + dos_string2 + " " + dos_string3;
            string dos_stringx = dos_string1 + dos_string21 + " " + dos_string31;
            string dos_string_del1 = dos_stringd + dos_stringd1;
            string dos_string_del2 = dos_stringd + dos_stringd2;
            Console.WriteLine("***** FILE XX ****");
            Console.WriteLine(" ");
            Console.WriteLine(dos_string);
            Console.WriteLine(dos_stringx);
            Console.WriteLine(dos_string_del1);
            Console.WriteLine(dos_string_del2);
            Console.WriteLine(" ");

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = dos_string;
            process.StartInfo = startInfo;
            process.Start();

            System.Diagnostics.ProcessStartInfo startInfo2 = new System.Diagnostics.ProcessStartInfo();
            startInfo2.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo2.FileName = "cmd.exe";
            startInfo2.Arguments = dos_stringx;
            process.StartInfo = startInfo2;
            process.Start();

            System.Diagnostics.ProcessStartInfo startInfo3 = new System.Diagnostics.ProcessStartInfo();
            startInfo3.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo3.FileName = "cmd.exe";
            startInfo3.Arguments = dos_string_del1;
            process.StartInfo = startInfo3;
            process.Start();

            System.Diagnostics.ProcessStartInfo startInfo4 = new System.Diagnostics.ProcessStartInfo();
            startInfo4.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo4.FileName = "cmd.exe";
            startInfo4.Arguments = dos_string_del2;
            process.StartInfo = startInfo4;
            process.Start();

            //#DEBUGA SHOW '#dos_status is ' #dos_status
            if (process.ExitCode < 32)
            {
                Console.WriteLine(" ");
                Console.WriteLine("* Copy Failed  *");
                Console.WriteLine(" ");
            }
            else
            {
                Console.WriteLine(" ");
                Console.WriteLine("***** copy success");
                Console.WriteLine(" ");
            }
            Console.WriteLine("---------------------");
            return;
        }

        public void SQL_Error()
        {
            //  evaluate #sql-status
            //#ifdef DB2
            //    when = 6100    !DB2 error for empty-table result set
            //      break
            //#end-if

            //#ifdef DB2UNIX
            //    when = 6100    !DB2 error for empty-table result set
            //      break
            //#end-if

            //   when = -99999  !Token "when" clause for non-DB2 environments
            //   when-other
            //     display $sqr-program noline
            //     display ': ' noline
            //     display $ReportID noline
            //     display ' - SQL Statement = '
            //     display $SQL-STATEMENT
            //     display 'SQL Status =' noline
            //     display #sql-status 99999 noline
            //     display ' ' noline
            //     display 'SQL Error  = ' noline
            //     display $sql-error
            //     display $Sql-Msg
            //     Do Rollback-Transaction
            //let $sgi_err_msg   = 'Stop at SQL Processing'
            //SGI_Stop_Job();
            // end-evaluate
        }
        //#include 'reset.sqc'     ! Reset printer procedure
        //#include 'tranctrl.sqc'  ! Tools transaction control module
        //#include 'sgerror.sqc'   ! SGI Error Handling procedure
    }
}

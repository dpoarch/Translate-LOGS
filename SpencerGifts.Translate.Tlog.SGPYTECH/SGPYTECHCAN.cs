using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SpencerGifts.Translate.Tlog.SGPYTECH;

namespace SpencerGifts.Translate.Tlog.SGPYTECH
{
    class SGPYTECHCAN
    {
        public datadbDataContext db = new datadbDataContext();
        //public string COPYFROM = "\\sgawapp\\Retail_apps\\ValueLink\\PAYMENTECH\\PTD\\CANPTD\\";
        //public string COPYTO = "\\sgawapp\\Retail_apps\\ValueLink\\PAYMENTECH\\PTD\\Work\\";
        //public string PTDWDIR = "\\sgawapp\\Retail_apps\\ValueLink\\PAYMENTECH\\PTD\\CANPTD\\";
        //public string PTDBKUP = "\\sgawapp\\Retail_apps\\ValueLink\\PAYMENTECH\\PTD\\CANPTD\\BACKUP\\";
        //public string TLOGWDIR = "\\sgawapp\\Retail_apps\\ValueLink\\PAYMENTECH\\TLOG\\";
        //public string TLOGBKUP = "\\sgawapp\\Retail_apps\\ValueLink\\PAYMENTECH\\TLOG\\BKUP\\";
        public string COPYFROM = "F:\\Test\\SGPYTECH\\copyfrom\\";
        public string COPYTO = "F:\\Test\\SGPYTECH\\copyto\\";
        public string PTDWDIR = "F:\\Test\\SGPYTECH\\ptdwdir\\";
        public string PTDBKUP = "F:\\Test\\SGPYTECH\\ptdbkup\\";
        public string TLOGWDIR = "F:\\Test\\SGPYTECH\\tlogwdir\\";
        public string TLOGBKUP = "F:\\Test\\SGPYTECH\\tlogbkup\\";
        public string inactivity_charge_tlog = "F";
        public string promo_cd_cre_tlog = "F";
        public string sub_promo_cd = "F";
        public string sub_promo_cd_cre_tlog = "F";
        public string source_cd_cre_tlog = "F";
        public string req_cd_cre_tlog = "F";
        public string tlog_created = "F";
        public string spc_promo_code_sub = " ";
        public string bin_range_cre_tlog = "F";
        public Int64 tlog_count = 0;

        public void begin_report()
        {
            Init_Report();
            Get_DateTime();
            delete_ptd_go_file();
            Truncate_Tables();
            Files_copy();
            //Delay_1_Minute();
            open_main_file();
            create_tlog_go_file();
            //Reset();
            return;
        }

        /// <summary>
        /// Init_Report
        /// Report initialization procedure.  Set titles, parameters.
        /// This is called from the Begin_Report procedure.
        /// </summary>
        public void Init_Report()
        {
            string ReportID = "SGPYTECHCAN";
            string ReportTitle = "SGPYTECHCAN";
            Console.WriteLine(" ");
            Console.WriteLine(ReportTitle);
            Console.WriteLine(" ");
            return;
        }

        /// <summary>
        /// delete_ptd_go_file
        /// This is called from the Begin_Report procedure.
        /// </summary>
        public void delete_ptd_go_file()
        {
            string dos_string = "cmd /c del " + PTDWDIR + "PT_PTD.GO";
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
            process.WaitForExit();
            if (process.ExitCode >= 32)
            {
                Console.WriteLine(" ");
                Console.WriteLine("***** deleted ****");
                Console.WriteLine(" ");
                //Console.WriteLine(file);
                Console.WriteLine(" ");
            }
            Console.WriteLine("_____________________");
            return;
        }

        /// <summary>
        /// Truncate_Tables 
        /// Truncate Tables use in Program.
        /// </summary>
        public void Truncate_Tables()
        {
            //move 'Truncating Tables  _ PROBLEM' to Sql_Msg
            //move 'Truncating Tables           ' to trunc_msg
            //#debugb Console.WriteLine(' '
            //#debugb Console.WriteLine(trunc_msg
            //#debugb Console.WriteLine(' '

            db.ExecuteCommand("TRUNCATE TABLE GIFT_CARD_INV_PR10_TEMP");
            db.SubmitChanges();

            //move 'Truncating Tables    OK     ' to trunc_msg
            //#debugb Console.WriteLine(' '
            //#debugb Console.WriteLine(trunc_msg
            //#debugb Console.WriteLine(' '
            return;
        }

        /// <summary>
        /// Files_copy 
        /// This is called from the Begin_Report procedure.
        /// </summary>
        public void Files_copy()
        {
            //#debuga Console.WriteLine('Files_copy  '
            string dos1 = "cmd /c dir ";
            string dos2 = COPYFROM;
            string dos3 = "SGI_PT_PTD.* /ON/B > ";
            string dos4 = COPYTO + "PTD.DAT";
            string dos_string = dos1 + dos2 + dos3 + dos4;
            //#debuga Console.WriteLine('dos4  '
            //#debuga Console.WriteLine( dos4

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
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                Console.WriteLine(" ");
                Console.WriteLine("* Processing Files Now  *");
                Console.WriteLine(" ");
                var copy_flag = 'Y';
                //Console.WriteLine(file);
                Console.WriteLine(" ");
            }
            else
            {
                Console.WriteLine(" ");
                Console.WriteLine("***** processing failed");
                Console.WriteLine(" ");
            }
            Console.WriteLine("_____________________");
            return;
        }

        /// <summary>
        /// open_main_file 
        /// File Opening Procedure 
        /// </summary>
        public void open_main_file()
        {
            Console.WriteLine("open_main_file   X  ");

            var ptdmainfile = COPYTO + "PTD.DAT";

            if (File.Exists(ptdmainfile))
            {

                var logFile = File.ReadAllLines(ptdmainfile);
                List<string> LogList = new List<string>(logFile);
                var file_count = 0;
                foreach (var item in LogList)
                {
                    file_count++;
                    var ptdfile = item.Trim();
                    process_main(ptdfile);
                    var record_count = 0;

                    Get_DateTime();
                    Console.WriteLine("End of the PTD Process: ");
                }
            }
            else
            {
                Console.WriteLine("Could not open PTD Main file");
                Console.WriteLine(ptdmainfile);
            }

        }

        /// <summary>
        /// Process_Main
        /// This is highest level driving procedure called from open_main_file. 
        /// </summary>
        public void process_main(string ptdfile)
        {

            Console.WriteLine("Begin of the PTD Process: " + DateTime.Now);

            open_ptd_file(ptdfile);
            GIFT_CARD_INV_PR10_TEMP gft = Process_Input(ptdfile);
            if (gft != null)
            {
                Process_Gift_Card_Tbl(gft);
            }
            backup_file(ptdfile);
            delete_file(ptdfile);
            //Reset();

            return;
        }

        /// <summary>
        ///  open_ptd_file
        /// This is called from the process_main procedure.
        /// </summary>
        public void open_ptd_file(string ptdfile)
        {

            var FullFileName = PTDWDIR + ptdfile;
            if (!File.Exists(FullFileName))
            {
                Console.WriteLine("Open for file 15 failed");
                //var sgi_err_msg   = "Stop at PTD file opening";

                //SGI_Stop_Job();
            }
            return;
        }

        /// <summary>
        /// Process Input  
        /// </summary>
        public GIFT_CARD_INV_PR10_TEMP Process_Input(string path)
        {
            GIFT_CARD_INV_PR10_TEMP gft = new GIFT_CARD_INV_PR10_TEMP();
            //#debuga Console.WriteLine(" I am in Process_Input "
            if (File.Exists(PTDWDIR + path))
            {
                var logFile = File.ReadAllLines(PTDWDIR + path);
                List<string> LogList = new List<string>(logFile);

                foreach (var item in LogList)
                {
                    char sep = '|';

                    string[] splitdata = item.Split(sep);

                    if (item != "")
                    {
                        gft = Process_OLTP_Record(splitdata);
                    }
                }
            }

            return gft;
        }

        /// <summary>
        /// Process_OLTP_Record 
        /// </summary>
        public GIFT_CARD_INV_PR10_TEMP Process_OLTP_Record(string[] splitdata)
        {
            var record_type = splitdata[0];
            var card_program = splitdata[1];
            var bank_merchant_number = splitdata[2];
            var pns_merchant_number = splitdata[3];
            var merchant_name = splitdata[4];
            var transaction_type = splitdata[5];
            var terminal_id = splitdata[6];
            var transaction_datetime = splitdata[7];
            var card_number = splitdata[8];
            var auth_number = splitdata[9];
            var employee_number = splitdata[10];
            var transaction_reference = splitdata[11];
            var dummy = splitdata[12];
            var transaction_amount = splitdata[13];
            var mcc = Convert.ToInt64(splitdata[14].Trim());
            var store_no = merchant_name.ToUpper();
            var store_length = store_no.Length;

            var start_valid_pos = 0;
            var start_pos = 1;
            var i = 1;
            while (i <= store_length)
            {
                var value = store_no.Substring(16, 2);
                if (Convert.ToInt64(value) > 0)
                {
                    start_valid_pos = start_pos;
                    break;
                }
                else
                {
                    start_pos = start_pos + 1;
                }
                i++;
            }

            if (start_valid_pos == 0)
            {
                store_no = "3000";
            }
            else
            {
                store_no = store_no.Substring(16, 2);
                store_no = store_no.Trim();
            }

            PS_SG_STORE_INFO selectedstore = select_store_state(store_no);
            if (selectedstore == null)
            {
                GIFT_CARD_INV_PR10_TEMP nodata = new GIFT_CARD_INV_PR10_TEMP();
                return nodata;
            }
            var transaction_type_st = selectedstore.TRANSACTION_TYPE.Trim();
            var terminal_id_st = Convert.ToInt64(selectedstore.TERMINAL_ID);
            var transaction_datetime_st = selectedstore.TRANSACTION_DATETIME;
            Int64 card_number_st = Convert.ToInt64(selectedstore.CARD_NUMBER.Trim());
            string i1_promotion_code_st = "00" + selectedstore.CARD_NUMBER.Substring(7, 6);
            var bin_range_st = selectedstore.CARD_NUMBER.Substring(9, 4);
            var auth_number_st = selectedstore.AUTH_NUMBER.Trim();
            var tlog_authcode_st = auth_number;
            var employee_number_st = Convert.ToInt64(selectedstore.EMPLOYEE_NUMBER.Trim());
            var transaction_reference_st = Convert.ToInt64(selectedstore.TRANSACTION_REFERENCE.Trim());
            var transaction_amount_st = Convert.ToInt64(selectedstore.TRANSACTION_AMOUNT.Trim());
            var i1_transaction_amount_st = transaction_amount_st;
            var trans_amt_sign = "";
            if (transaction_amount_st >= 0)
            {
                trans_amt_sign = "+";
            }
            else
            {
                trans_amt_sign = "-";
            }
            if (transaction_amount_st < 0)
            {
                transaction_amount_st = -1 * transaction_amount_st;
            }
            var transaction_amt = transaction_amount_st * 100;
            //#debuga Console.WriteLine("#transaction_amt is " #transaction_amount
            // move #transaction_amt to transaction_amt 099999999
            //#debuga Console.WriteLine("transaction_amt is " transaction_amt
            GIFTCARD_TRAN gftrans = new GIFTCARD_TRAN();
            gftrans.RECORD_TYPE = record_type;
            gftrans.CARD_PROGRAM = card_program;
            gftrans.BANK_MERCHANT_ID = bank_merchant_number;
            gftrans.PNS_MERCHANT_ID = pns_merchant_number;
            gftrans.MERCHANT_NAME = merchant_name;
            gftrans.STORE_NO = Convert.ToInt64(store_no);
            gftrans.STATE = store_no;
            gftrans.TRANS_TYPE = transaction_type_st;
            gftrans.TERMINAL_ID = selectedstore.TERMINAL_ID;
            gftrans.TRANS_DATETIME = transaction_datetime_st;
            gftrans.GIFT_CARD_NO = card_number_st;
            gftrans.AUTH_NO = auth_number_st;
            gftrans.EMPLOYEE_NO = employee_number_st;
            gftrans.TRANS_REF = transaction_reference_st;
            gftrans.TRANS_AMOUNT = transaction_amount_st;
            gftrans.MCC = mcc;

            Insert_PT_PTD_Record(gftrans);
            reset_flags();
            string promo_cd_cre_tlog = check_promo_code(i1_promotion_code_st);
            string subreturn = check_subpromo_code(i1_promotion_code_st, card_number_st);
            string[] cre = subreturn.Split(',');
            string sub_promo_cd = cre[1];
            string sub_promo_cd_cre_tlog = cre[0];
            PROMO_CONTROL promo = select_promo_tbl(i1_promotion_code_st);
            SPECIAL_PROMO_CONTROL SPC = select_subpromo_tbl(i1_promotion_code_st, card_number_st);
            //#debugc Console.WriteLine("sub_promo_cd is       "sub_promo_cd
            //#debugc Console.WriteLine("sub_promo_cd_cre_tlog is "sub_promo_cd_cre_tlog
            //#debugc Console.WriteLine("promo_cd_cre_tlog is  "promo_cd_cre_tlog

            if (bin_range_st != "0235")
            {
                if (bin_range_st != "1144")
                {
                    if (transaction_type.ToUpper() != "REDEMPTION")
                    {
                        if (transaction_type.ToUpper() != "VOICE REDEMPTION")
                        {
                            if (transaction_type.ToUpper() != "REV_REDEMPTION")
                            {
                                if (transaction_type.ToUpper() != "BALANCE INQUIRY")
                                {
                                    if (transaction_type.ToUpper() != "ISSUANCE/ADD VALUE")
                                    {
                                        if (transaction_type.ToUpper() == "INACTIVITY CHARGE")
                                        {
                                            var inactivity_charge_tlog = 'Y';
                                            create_tlog(card_number_st, promo, SPC, gftrans, selectedstore, trans_amt_sign);
                                        }
                                        else
                                        {
                                            if (sub_promo_cd == "T" && sub_promo_cd_cre_tlog == "T")
                                            {
                                                create_tlog(card_number_st, promo, SPC, gftrans, selectedstore, trans_amt_sign);
                                            }
                                            if (sub_promo_cd == "F" && promo_cd_cre_tlog == "T")
                                            {
                                                create_tlog(card_number_st, promo, SPC, gftrans, selectedstore, trans_amt_sign);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (tlog_created != "T")
            {
                if (pns_merchant_number == "800000025247" || pns_merchant_number == "30000013257")
                {
                    var inactivity_charge_tlog = "Y";
                    create_tlog(card_number_st, promo, SPC, gftrans, selectedstore, trans_amt_sign);
                }
            }

            if (bin_range_st == "1144" || bin_range_st == "0235")
            {
                var bin_range_cre_tlog = "T";
                var tlog_store_no = "03200";
                create_tlog(card_number_st, promo, SPC, gftrans, selectedstore, trans_amt_sign);
            }
            string check_store_no = "";
            string check_card_no = "";
            string check_amount = "";

            if (i1_promotion_code_st != "00011518")
            {
                if (i1_promotion_code_st != "00011519")
                {
                    if (transaction_type.ToUpper() == "REV_ACTIVATION/ISSUANCE (NEW)")
                    {
                        check_store_no = store_no;
                        check_card_no = card_number;
                        check_amount = transaction_amount;
                    }

                    if (transaction_type.ToUpper() == "ACTIVATION/ISSUANCE (NEW)" || transaction_type.ToUpper() == "VOICE ACTIVATION/ISSUANCE (NEW)" || transaction_type.ToUpper() == "BLOCK ACTIVATION TRANSACTION")
                    {
                        if (store_no != check_store_no && card_number != check_card_no && Convert.ToInt64(transaction_amount) != -1 * Convert.ToInt64(check_amount))
                        {
                            Insert_GC_Inv_Temp(promo.PROMO_CODE, store_no);
                        }
                    }
                }
            }
            GIFT_CARD_INV_PR10_TEMP gft = new GIFT_CARD_INV_PR10_TEMP();
            gft.VL_PROMOTION_CODE = i1_promotion_code_st;
            gft.STORE_NO = gftrans.STORE_NO;
            gft.TRANSACTION_DATE = gftrans.TRANS_DATETIME;
            gft.TRANS_POST_DATE = gftrans.TRANS_DATETIME;
            gft.CARDS_USED_QTY = 1;

            return gft;
        }

        /// <summary>
        /// select_store_state   
        /// The Procedure which gets the state code.
        /// </summary>
        public PS_SG_STORE_INFO select_store_state(string store_no)
        {
            //#ifdef debuga
            //   Console.WriteLine("FLOW: select_store_state"
            //#endif
            string store_st_exists = "N";
            var state_cd = "";
            PS_SG_STORE_INFO pssg = db.PS_SG_STORE_INFOs.FirstOrDefault(i => i.DEPTID == store_no);

            state_cd = pssg.SG_STATE;
            store_st_exists = "Y";

            return pssg;
        }

        /// <summary>
        /// Insert_PT_PTD_Record  
        /// This procedure inserts a row in the GIFTCARD_TRAN Record.
        /// </summary>
        /// <param name="data"></param>
        public void Insert_PT_PTD_Record(GIFTCARD_TRAN data)
        {
            GIFTCARD_TRAN gftrans = new GIFTCARD_TRAN();
            gftrans.RECORD_TYPE = data.RECORD_TYPE;
            gftrans.CARD_PROGRAM = data.CARD_PROGRAM;
            gftrans.BANK_MERCHANT_ID = data.BANK_MERCHANT_ID;
            gftrans.PNS_MERCHANT_ID = data.PNS_MERCHANT_ID;
            gftrans.MERCHANT_NAME = data.MERCHANT_NAME;
            gftrans.STORE_NO = data.STORE_NO;
            gftrans.STATE = data.STATE;
            gftrans.TRANS_TYPE = data.TRANS_TYPE;
            gftrans.TERMINAL_ID = data.TERMINAL_ID;
            gftrans.TRANS_DATETIME = data.TRANS_DATETIME;
            gftrans.GIFT_CARD_NO = data.GIFT_CARD_NO;
            gftrans.AUTH_NO = data.AUTH_NO;
            gftrans.EMPLOYEE_NO = data.EMPLOYEE_NO;
            gftrans.TRANS_REF = data.TRANS_REF;
            gftrans.TRANS_AMOUNT = data.TRANS_AMOUNT;
            gftrans.MCC = data.MCC;
            db.GIFTCARD_TRANs.InsertOnSubmit(gftrans);
            db.SubmitChanges();

            return;
        }

        /// <summary>
        /// check_promo_code
        /// This is called from the Process_OLTP_Record procedure. 
        /// </summary>
        public string check_promo_code(string i1_promotion_code_st)
        {
            var promo_cd_cre_tlog = "F";
            Int64 promocount = db.PROMO_CONTROLs.Where(i => i.PROMO_CODE == i1_promotion_code_st).Count();
            if (promocount != 0)
            {
                PROMO_CONTROL promo = select_promo_tbl(i1_promotion_code_st);
                if (promo.VL_CREATE_TLOG_FLAG.ToUpper() == "Y")
                {
                    promo_cd_cre_tlog = "T";
                }
            }
            else
            {
                //#debugb Console.WriteLine("i1_promotion_code    "  noline
                //#debugb Console.WriteLine(i1_promotion_code
                //var sgi_err_msg   = "Stop at check_promo_code";
                //SGI_Stop_Job();
            }


            return promo_cd_cre_tlog;
        }

        public PROMO_CONTROL select_promo_tbl(string i1_promotion_code)
        {

            PROMO_CONTROL PROMO = db.PROMO_CONTROLs.FirstOrDefault(i => i.PROMO_CODE == i1_promotion_code && i.ID_COUNTRY == "can");
            return PROMO;
        }


        /// <summary>
        /// check_subpromo_code 
        ///  This is called from the Process_OLTP_Record procedure.
        /// </summary>
        public string check_subpromo_code(string i1_promotion_code_st, Int64 card_no)
        {
            Int64 promo_code_sub = Convert.ToInt64(card_no.ToString().Substring(7, 12));

            var subpromo_rec_found = "N";
            var sub_promo_cd_cre_tlog = "F";
            var sub_promo_cd = "F";
            Int64 found = db.SPECIAL_PROMO_CONTROLs.Where(i => i.PROMO_CODE == promo_code_sub && i.GC_BEGIN_RANGE >= promo_code_sub && i.GC_END_RANGE <= promo_code_sub).Count();
            if (found != 0)
            {
                SPECIAL_PROMO_CONTROL SPC = select_subpromo_tbl(i1_promotion_code_st, card_no);
                subpromo_rec_found = "Y";

                sub_promo_cd = "T";
                if (SPC.VL_CREATE_TLOG_FLAG.ToUpper() == "Y")
                {
                    sub_promo_cd_cre_tlog = "T";
                }
            }
            return sub_promo_cd_cre_tlog + "," + sub_promo_cd;
        }

        /// <summary>
        /// select_subpromo_tbl
        /// This is called from the check_subpromo_code procedure.
        /// </summary>
        public SPECIAL_PROMO_CONTROL select_subpromo_tbl(string i1_promotion_code, Int64 card_number)
        {
            Int64 promo_code_sub = Convert.ToInt64(card_number.ToString().Substring(7, 12));
            SPECIAL_PROMO_CONTROL SPC = db.SPECIAL_PROMO_CONTROLs.FirstOrDefault(i => i.PROMO_CODE == Convert.ToInt64(i1_promotion_code) && i.GC_BEGIN_RANGE < promo_code_sub || i.GC_END_RANGE > promo_code_sub);

            return SPC;
        }

        /// <summary>
        /// create_tlog 
        /// This is called from the Process_OLTP_Record procedure. 
        /// </summary>
        public void create_tlog(Int64 cardno, PROMO_CONTROL promo, SPECIAL_PROMO_CONTROL SPC, GIFTCARD_TRAN gftrans, PS_SG_STORE_INFO selectedstore, string trans_amt_sign)
        {
            Int64 ucxl_card_number = card_lookup(cardno);

            string[] opendata = Open_tlog_Files().Split(',');
            string tlog_file_opened = opendata[0];
            string tlogdir = opendata[1];
            string backupdir = opendata[2];

            var tlog_created = "T";
            var tlog_trans_date = DateTime.Now;
            Int64 tlog_store_no = 0;
            string tlog_obj_cd0 = "";
            string tlog_obj_cd1 = "";
            string tlog_promo_cd_sub = "";
            if (bin_range_cre_tlog == "T")
            {
                tlog_obj_cd0 = promo.OBJECT_CODE;
                tlog_obj_cd1 = promo.OBJECT_CODE_EXP;
                tlog_promo_cd_sub = " ";
            }
            else
            {
                if (sub_promo_cd == "T")
                {
                    tlog_store_no = Convert.ToInt64(SPC.STORE_NO);
                    tlog_obj_cd0 = SPC.OBJECT_CODE;
                    tlog_obj_cd1 = SPC.OBJECT_CODE_EXP;
                    tlog_promo_cd_sub = SPC.PROMO_CODE_SUB;
                }
                else
                {
                    tlog_store_no = Convert.ToInt64(promo.STORE_NO);
                    //#debugc Console.WriteLine("promo_store_no is " promo_store_no
                    tlog_obj_cd0 = promo.OBJECT_CODE;
                    tlog_obj_cd1 = promo.OBJECT_CODE_EXP;
                    tlog_promo_cd_sub = " ";
                }

                if (inactivity_charge_tlog == "Y")
                {
                    tlog_store_no = Convert.ToInt64(promo.STORE_NO);
                }

            }
            var register_found = "N";
            STORE_TRANS_NO getregister = get_register(tlog_store_no, DateTime.Now.ToString());
            Int64 register_no = Convert.ToInt64(getregister.REGISTER_ID);

            tlog_count++;
            if (tlog_count >= 9999)
            {
                update_register1(tlog_store_no, register_no);
                tlog_count = 0;
            }

            string trans_nso_found = checktransfound(tlog_store_no, register_no);
            STORE_TRANS_NO transno = get_trans_no(tlog_store_no, register_no);
            Int64 tlog_trans_no = Convert.ToInt64(transno.TRANSACTION_NO) + 1;

            string tlog_acct_no = ucxl_card_number.ToString();
            string filecreated = "";
            string req_pos_card_auth_type = "0";
            write_10_record(transno, promo.ID_COUNTRY, tlogdir, backupdir);
            write_30_record(tlogdir, backupdir);
            req_pos_card_auth_type = write_47_record(tlog_obj_cd0, tlog_obj_cd1, gftrans.TRANS_TYPE, tlog_acct_no, trans_amt_sign, Convert.ToString(gftrans.TRANS_AMOUNT), gftrans.AUTH_NO, tlogdir, backupdir);
            write_99_record(trans_amt_sign, Convert.ToInt64(gftrans.TRANS_AMOUNT), tlogdir, backupdir);
            TLOG_HIST tlog = new TLOG_HIST();
            tlog.STORE_NO = tlog_store_no;
            tlog.TRANSACTION_DATE = gftrans.TRANS_DATETIME;
            tlog.TRANSACTION_NO = tlog_trans_no;
            tlog.GIFT_CARD_NUMBER = gftrans.GIFT_CARD_NO.ToString();
            tlog.AUTHORIZATION_CODE = gftrans.AUTH_NO;
            tlog.REGISTER_NO = register_no.ToString();
            tlog.POS_CARD_AUTH_TYPE = req_pos_card_auth_type;
            tlog.POS_GIFT_CARD_FLAG = 6;
            tlog.GROSS_LINE_AMOUNT = selectedstore.TRANSACTION_AMOUNT;
            tlog.REMAING_CARD_BAL = "0";
            tlog.LINE_OBJECT_TYPE = tlog_obj_cd1;
            tlog.PROMO_CODE = promo.PROMO_CODE;
            tlog.PROMO_CODE_SUB = tlog_promo_cd_sub;
            tlog.DA_TIMESTMP_CRE = DateTime.Now;
            create_tlog_history(tlog);
            update_trans_no(tlog_store_no, register_no, tlog_trans_no);
            return;
        }

        /// <summary>
        /// card_lookup  
        ///  This is called from the create_tlog procedure.
        /// </summary>
        public Int64 card_lookup(Int64 card_number)
        {
            var card_no_found = "N";
            USA_CANADA_XREF_LOOKUP UCXL = db.USA_CANADA_XREF_LOOKUPs.FirstOrDefault(i => i.PAYMENTTECH == card_number.ToString());
            card_no_found = "Y";

            Int64 ucxl_card_number = Convert.ToInt64(UCXL.VALUELINK);
            if (card_no_found == "N")
            {
                ucxl_card_number = card_number;
            }
            return ucxl_card_number;
        }

        /// <summary>
        /// get_trans_no 
        /// This is called from the create_tlog procedure.
        /// </summary>
        public STORE_TRANS_NO get_trans_no(Int64 tlog_store_no, Int64 register_no)
        {

            STORE_TRANS_NO STN = db.STORE_TRANS_NOs.FirstOrDefault(i => i.STORE_NO == tlog_store_no && i.REGISTER_ID == register_no);


            return STN;
        }

        /// <summary>
        /// Checks if trans is  found
        /// </summary>
        /// <param name="tlog_store_no"></param>
        /// <param name="register_no"></param>
        /// <returns></returns>
        public string checktransfound(Int64 tlog_store_no, Int64 register_no)
        {
            string trans_no_found = "N";

            STORE_TRANS_NO STN = db.STORE_TRANS_NOs.FirstOrDefault(i => i.STORE_NO == tlog_store_no && i.REGISTER_ID == register_no);

            var stn_trans_no = STN.TRANSACTION_NO;

            if (stn_trans_no == 9999)
            {
                stn_trans_no = 0;
            }
            trans_no_found = "Y";

            if (trans_no_found == "N")
            {
                create_trans_no(STN);
            }
            return trans_no_found;
        }


        /// <summary>
        /// create_trans_no  
        /// This is called from the get_trans_no procedure.  
        /// </summary>
        public void create_trans_no(STORE_TRANS_NO data)
        {
            STORE_TRANS_NO store = new STORE_TRANS_NO();
            store.STORE_NO = data.STORE_NO;
            store.REGISTER_ID = data.REGISTER_ID;
            store.TRANSACTION_NO = 0;
            db.STORE_TRANS_NOs.InsertOnSubmit(store);
            db.SubmitChanges();

            return;
        }

        /// <summary>
        /// get_register 
        /// This is called from the Process_Input procedure.
        /// </summary>
        public STORE_TRANS_NO get_register(Int64 tlog_store_no, string currentdate0)
        {
            STORE_TRANS_NO REG = db.STORE_TRANS_NOs.FirstOrDefault(i => i.STORE_NO == tlog_store_no && i.DA_TIMESTMP_MOD == null || i.DA_TIMESTMP_MOD != currentdate0);
            return REG;
        }


        /// <summary>
        /// write_10_record   
        /// This procedure writes the TLOG Header Record  
        /// </summary>
        public void write_10_record(STORE_TRANS_NO transno, string promo_id_country, string tlogdir, string backupdir)
        {

            var trans_code = "10";
            var trans_qualifier = "00";
            var trans_type = "00";
            var flags = "00200";
            var prefix_store_no = "0";
            var cashier = "000000000";
            var record_count = "04";
            var time = "1201";
            var orig_store_no = "000000";
            var orig_register = "00";
            var orig_trans = "00000";
            var orig_date = "000000";
            var reason = "00";

            Int64 country = 0;
            if (promo_id_country == "USA")
            {
                country = 30;
            }
            if (promo_id_country == "CAN")
            {
                country = 31;
            }
            if (promo_id_country == "GBR")
            {
                country = 32;
            }

            string data = trans_code + " " + trans_qualifier + " " + trans_type + " " + flags + " " + prefix_store_no + " " + transno.STORE_NO + " " + transno.REGISTER_ID + " " + transno.STORE_NO + " " + cashier + " " + record_count + " " + transno.DA_TIMESTMP_MOD + " " + time + " " + orig_store_no + " " + orig_register + " " + orig_trans + " " + orig_date + " " + reason + " " + country;

            File.AppendAllText(tlogdir, data + Environment.NewLine);
            File.AppendAllText(backupdir, data + Environment.NewLine);

            var file_created = "Y";
            return;
        }

        /// <summary>
        /// write_30_record 
        /// This procedure writes the line item Record  
        /// </summary>
        public void write_30_record(string tlogdir, string backupdir)
        {

            var trans_code = "30";
            var trans_type = "20";
            var flags = "00028";
            var item = "000000000001";
            var dept = "00032";
            var jrnl_key = "00";
            var for_qty = "00001";
            var quantity = "00001";
            var reason = "00";
            var return_reason = "00";
            var orig_price = "000000000";
            var orig_for_qty = "00";

            string data = trans_code + " " + " " + trans_type + " " + flags + " " + item + " " + dept + " " + jrnl_key + " " + for_qty + " " + quantity + " " + reason + " " + return_reason + " " + orig_price + " " + orig_for_qty;

            File.AppendAllText(tlogdir, data + Environment.NewLine);
            File.AppendAllText(backupdir, data + Environment.NewLine);
            var file_created = "Y";
            return;
        }

        /// <summary>
        /// 
        /// </summary>
        public string write_47_record(string tlog_obj_cd0, string tlog_obj_cd1, string transaction_type, string tlog_acct_no, string trans_amt_sign, string transaction_amt, string tlog_authcode, string tlogdir, string backupdir)
        {

            var trans_code = "47";
            var flags = "00006";
            var req_pos_card_auth_type = 8;
            var trans_type = "00";
            var tlog_obj_cd = tlog_obj_cd0;
            if (inactivity_charge_tlog == "Y")
            {
                req_pos_card_auth_type = 9;
                trans_type = "02";
                tlog_obj_cd = "20511";
            }
            if (transaction_type.ToUpper() == "ACCOUNT EXPIRATION")
            {
                req_pos_card_auth_type = 9;
                trans_type = "02";
                tlog_obj_cd = tlog_obj_cd1;
            }

            // move #tlog_obj_cd to tlog_obj_cd 099999

            var balance_sign = "+";
            var remain_balance = 0;
            //move #remain_balance to remain_balance 099999999

            string data = trans_code + " " + trans_type + " " + flags + " " + tlog_acct_no + " " + trans_amt_sign + " " + transaction_amt + " " + balance_sign + " " + remain_balance + " " + tlog_authcode + " " + tlog_obj_cd;

            File.AppendAllText(tlogdir, data + Environment.NewLine);
            File.AppendAllText(backupdir, data + Environment.NewLine);
            var file_created = "Y";
            return req_pos_card_auth_type.ToString();
        }

        /// <summary>
        /// write_99_record
        /// This procedure writes the Gift card Record
        /// </summary>
        public void write_99_record(string trans_amt_sign, Int64 transaction_amt, string tlogdir, string backupdir)
        {

            var trans_code = "99";
            var trans_type = "00";
            var flags = "00000";
            var foreign_curr = "000000000";
            var usa_cash = "000000000";
            var taxes = "000000000";
            var sales_person = "000000000";
            var orig_sales_person = "000000000";

            string data = trans_code + " " + trans_type + " " + flags + " " + trans_amt_sign + " " + transaction_amt + " " + foreign_curr + " " + usa_cash + " " + taxes + " " + trans_amt_sign + " " + sales_person + orig_sales_person;

            File.AppendAllText(tlogdir, data + Environment.NewLine);
            File.AppendAllText(backupdir, data + Environment.NewLine);
            var file_created = "Y";

            return;
        }


        /// <summary>
        /// create_tlog_history  
        /// This procedure inserts a row in the TLOG_HIST Record. 
        /// </summary>
        public void create_tlog_history(TLOG_HIST data)
        {
            //#debugc Console.WriteLine("#tlog_obj_cd is " #tlog_obj_cd
            TLOG_HIST tlog = new TLOG_HIST();
            tlog.STORE_NO = data.STORE_NO;
            tlog.TRANSACTION_DATE = data.TRANSACTION_DATE;
            tlog.TRANSACTION_NO = data.TRANSACTION_NO;
            tlog.GIFT_CARD_NUMBER = data.GIFT_CARD_NUMBER;
            tlog.AUTHORIZATION_CODE = data.AUTHORIZATION_CODE;
            tlog.REGISTER_NO = data.REGISTER_NO;
            tlog.POS_CARD_AUTH_TYPE = data.POS_CARD_AUTH_TYPE;
            tlog.POS_GIFT_CARD_FLAG = 6;
            tlog.GROSS_LINE_AMOUNT = data.GROSS_LINE_AMOUNT;
            tlog.REMAING_CARD_BAL = data.REMAING_CARD_BAL;
            tlog.LINE_OBJECT_TYPE = data.LINE_OBJECT_TYPE;
            tlog.PROMO_CODE = data.PROMO_CODE;
            tlog.PROMO_CODE_SUB = data.PROMO_CODE_SUB;
            tlog.DA_TIMESTMP_CRE = data.DA_TIMESTMP_CRE;
            db.TLOG_HISTs.InsertOnSubmit(tlog);
            db.SubmitChanges();

            return;
        }

        /// <summary>
        /// update_trans_no 
        /// The Procedure which Updates the STORE_TRANS_NO Record.
        /// </summary>
        public void update_trans_no(Int64 tlog_store_no, Int64 register_no, Int64 tlog_trans_no)
        {

            //#ifdef debuga
            // #debugb  Console.WriteLine("FLOW: update_trans_no"
            //#endif

            STORE_TRANS_NO strno = db.STORE_TRANS_NOs.FirstOrDefault(i => i.STORE_NO == tlog_store_no && i.REGISTER_ID == register_no);
            strno.TRANSACTION_NO = tlog_trans_no;
            db.SubmitChanges();

            return;
        }

        /// <summary>
        /// select_register  
        /// The Procedure which selects the Register from TLOG_HIST to Update with the Current Date 
        /// </summary>
        //public void select_register()
        //{
        //    //#ifdef debuga
        //    // #debugb  Console.WriteLine("FLOW: select_register"
        //    //#endif

        //    TLOG_HIST TLOGHIST = db.TLOG_HISTs.FirstOrDefault(i => i.DA_TIMESTMP_CRE == currentdate0).OrderBy(i => i.STORE_NO, i.REGISTER_NO).OrderByDescending();

        //    Int64 tloghist_store_no = Convert.ToInt64(TLOGHIST.STORE_NO.Trim());
        //    var tloghist_register_no = TLOGHIST.REGISTER_NO.Trim();

        //    //update_register(tloghist_store_no, tloghist_register_no);

        //    return;
        //}

        /// <summary>
        /// update_register
        /// The Procedure which Updates the STORE_TRANS_NO Record.
        /// </summary>
        public void update_register(Int64 tloghist_store_no, Int64 tloghist_register_no)
        {
            //#ifdef debuga
            // #debugb  Console.WriteLine("FLOW: update_register"
            //#endif

            STORE_TRANS_NO str_no = db.STORE_TRANS_NOs.FirstOrDefault(r => r.STORE_NO == tloghist_store_no && r.REGISTER_ID == tloghist_register_no);
            str_no.DA_TIMESTMP_MOD = DateTime.Now.ToString();

            return;
        }

        /// <summary>
        /// update_register1 
        /// The Procedure which Updates the STORE_TRANS_NO Record.
        /// </summary>
        public void update_register1(Int64 tlog_store_no, Int64 register_no)
        {
            //#ifdef debuga
            // #debugb  Console.WriteLine("FLOW: update_register1"
            //#endif

            STORE_TRANS_NO udpate = db.STORE_TRANS_NOs.FirstOrDefault(i => i.STORE_NO == tlog_store_no && i.REGISTER_ID == register_no);
            udpate.DA_TIMESTMP_MOD = DateTime.Now.ToString();
            db.SubmitChanges();
            return;
        }

        /// <summary>
        /// Insert_GC_Inv_Temp
        /// This is called from the Process_OLTP_Record procedure.
        /// </summary>
        public void Insert_GC_Inv_Temp(string VL_PROMOTION_CODE, string STORE_NO)
        {
            var Sql_Msg = "Insert_GC_Inv_Temp    PROBLEM";
            var RJP_WORK = "Insert_GC_Inv_Temp         ";
            //#debuga Console.WriteLine(" "
            //#debuga Console.WriteLine(RJP_WORK
            //#debuga Console.WriteLine(" "

            var inv_trans_date = DateTime.Now;

            //#debuga Console.WriteLine("i1_promotion_code        is " i1_promotion_code
            //#debugb Console.WriteLine("store_no                 is " store_no
            //#debuga Console.WriteLine("transaction_datetime     is " transaction_datetime
            //#debuga Console.WriteLine("ws_timestmp_cre          is " ws_timestmp_cre


            GIFT_CARD_INV_PR10_TEMP gft = new GIFT_CARD_INV_PR10_TEMP();
            gft.VL_PROMOTION_CODE = VL_PROMOTION_CODE;
            gft.STORE_NO = Convert.ToInt64(STORE_NO);
            gft.TRANSACTION_DATE = DateTime.Now; //to_date($inv_trans_date,'MMDDYY'),
            gft.TRANS_POST_DATE = DateTime.Now; //to_date($ws_timestmp_cre,'YYYY-MM-DDHH24MISS'),
            gft.CARDS_USED_QTY = 1;
            db.GIFT_CARD_INV_PR10_TEMPs.InsertOnSubmit(gft);
            db.SubmitChanges();


            RJP_WORK = "Insert_GC_Inv_Temp   OK     ";
            //#debuga Console.WriteLine(" "
            //#debuga Console.WriteLine(RJP_WORK
            //#debuga Console.WriteLine(" "
            return;
        }

        /// <summary>
        /// Process_Gift_Card_Tbl
        /// This is called from the Begin_Report procedure. 
        /// </summary>
        public void Process_Gift_Card_Tbl(GIFT_CARD_INV_PR10_TEMP gft)
        {
            Insert_Gift_Card_Rec(gft);

            GIFT_CARD_INV_PR10_TEMP PR10GCI = db.GIFT_CARD_INV_PR10_TEMPs.FirstOrDefault();
            //GROUP BY
            //VL_PROMOTION_CODE,
            //STORE_NO,
            //TRANSACTION_DATE,
            //TRANS_POST_DATE


            return;
        }

        /// <summary>
        /// Insert_Gift_Card_Rec
        /// This is called from the Process_Gift_Card_Tbl  procedure.
        /// </summary>
        public void Insert_Gift_Card_Rec(GIFT_CARD_INV_PR10_TEMP data)
        {
            var Sql_Msg = "Insert_Gift_Card_Rec   PROBLEM";
            var RJP_WORK = "Insert_Gift_Card_Rec        ";
            //#debugb Console.WriteLine(" "
            //#debugb Console.WriteLine(RJP_WORK
            //#debugb Console.WriteLine(" "

            GIFT_CARD_INV_PR10_TEMP gft = new GIFT_CARD_INV_PR10_TEMP();
            gft.VL_PROMOTION_CODE = data.VL_PROMOTION_CODE;
            gft.STORE_NO = data.STORE_NO;
            gft.TRANSACTION_DATE = data.TRANSACTION_DATE;
            gft.TRANS_POST_DATE = data.TRANS_POST_DATE;
            gft.CARDS_USED_QTY = data.CARDS_USED_QTY;
            db.GIFT_CARD_INV_PR10_TEMPs.InsertOnSubmit(gft);
            db.SubmitChanges();

            RJP_WORK = "Insert_Gift_Card_Rec    OK     ";
            //#debugb Console.WriteLine(" "
            //#debugb Console.WriteLine(RJP_WORK
            //#debugb Console.WriteLine(" "
            return;
        }

        /// <summary>
        /// Open_tlog_Files   
        /// This is called from the create_tlog procedure.
        /// </summary>
        public string Open_tlog_Files()
        {
            var tlogwdir = TLOGWDIR + "tlog.txt";
            var ok = false;
            if (!File.Exists(tlogwdir))
            {
                File.Create(TLOGWDIR + "tlog.txt");
            }

            //open $tlogwdir as 20 for_writing record=100:vary status=#filestat
            var count = 0;
            while (ok != true)
            {
                var lineCount = File.ReadAllLines(tlogwdir).Length;
                if (lineCount >= 2000)
                {
                    count++;
                    if (File.Exists(TLOGWDIR + "tlog" + count + ".txt"))
                    {
                        tlogwdir = TLOGWDIR + "tlog" + count + ".txt";
                    }
                    else
                    {
                        File.Create(TLOGWDIR + "tlog" + count + ".txt");
                    }
                }
            }

            var tlogbkup = TLOGBKUP + "tlog" + DateTime.Now + "(Can)" + ".txt";
            //open tlogbkup as 60 for_writing record=100:vary status=#filestat
            if (!File.Exists(tlogbkup))
            {
                File.Create(TLOGBKUP + "tlog" + DateTime.Now + ".txt");
            }
            var backupok = false;
            //open $tlogwdir as 20 for_writing record=100:vary status=#filestat
            count = 0;
            while (backupok != true)
            {
                var lineCount = File.ReadAllLines(tlogbkup).Length;
                if (lineCount >= 2000)
                {
                    count++;
                    if (File.Exists(TLOGWDIR + "tlog" + count + ".txt"))
                    {
                        tlogwdir = TLOGBKUP + "tlog" + DateTime.Now + count + ".txt";
                    }
                    else
                    {
                        File.Create(TLOGBKUP + "tlog" + DateTime.Now + count + ".txt");
                    }
                }
            }

            var tlog_file_opened = "Y";
            return tlog_file_opened + "," + tlogwdir + "," + tlogbkup;
        }

        /// <summary>
        /// Get_DateTime
        /// This is called from the Begin_Report procedure.
        /// </summary>
        public void Get_DateTime()
        {
            var TODAY0 = DateTime.Now; // DD_MON_YYYY
            var ws_current_dttm = DateTime.Now; //YYYY_MM_DDHH24MISS
            var currentdate0 = TODAY0;
            var ws_timestmp_cre = ws_current_dttm;
            var ws_currdttm = DateTime.Now;

            return;
        }

        /// <summary>
        /// reset_flags 
        /// The procedure to Reset all the Flags.  
        /// </summary>
        public void reset_flags()
        {
            inactivity_charge_tlog = "F";
            promo_cd_cre_tlog = "F";
            sub_promo_cd = "F";
            sub_promo_cd_cre_tlog = "F";
            source_cd_cre_tlog = "F";
            req_cd_cre_tlog = "F";
            tlog_created = "F";
            spc_promo_code_sub = " ";
            bin_range_cre_tlog = "F";
            return;
        }

        /// <summary>
        /// create_tlog_go_file  
        /// This is called from the Begin_Report procedure. 
        /// </summary>
        public void create_tlog_go_file()
        {
            var ok = false;
            var TlogGoFileName = TLOGWDIR + "pt_tlog.GO";
            //open TlogGoFileName as 200 for_writing record=100:vary status=#filestat
            if (!File.Exists(TlogGoFileName))
            {
                File.Create(TLOGWDIR + "pt_tlog.GO");
            }
            //open $tlogwdir as 20 for_writing record=100:vary status=#filestat
            var count = 0;
            while (ok != true)
            {
                var lineCount = File.ReadAllLines(TlogGoFileName).Length;
                if (lineCount >= 2000)
                {
                    count++;
                    if (File.Exists(TLOGWDIR + "pt_tlog.GO"))
                    {
                        TlogGoFileName = TLOGWDIR + "pt_tlog" + count + ".GO";
                    }
                    else
                    {
                        File.Create(TLOGWDIR + "pt_tlog" + count + ".GO");
                    }
                }
            }

            var Tlogdummy = "VL_TLOG.GO";

            File.AppendAllText(TlogGoFileName, Tlogdummy + Environment.NewLine);
            return;
        }

        /// <summary>
        /// backup_file   
        /// This is called from the Begin_Report procedure.
        /// </summary>
        public void backup_file(string ptdfile)
        {
            string extension = ".txt";
            string dos_string1 = "cmd /c copy ";
            string dos_string2 = PTDWDIR + ptdfile + " ";
            string dos_string3 = PTDWDIR + ptdfile + extension;
            string dos_string4 = "." + DateTime.Now + extension;
            string dos_string = dos_string1 + dos_string2 + dos_string3 + dos_string4;
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
            process.WaitForExit();
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
            Console.WriteLine("_____________________");
            return;
        }

        /// <summary>
        /// delete_file  
        /// This is called from the Begin_Report procedure.
        /// </summary>
        public void delete_file(string ptdfile)
        {
            var dos_string = "cmd /c del " + PTDWDIR + ptdfile;
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
            process.WaitForExit();
            if (process.ExitCode >= 32)
            {
                Console.WriteLine(" ");
                Console.WriteLine("***** deleted ****");
                Console.WriteLine(" ");
                //Console.WriteLine(file);
                Console.WriteLine(" ");
            }
            Console.WriteLine("_____________________");
            return;
        }

        public void Delay_1_Minute()
        {
            //#debuga Console.WriteLine("Delay_1_Minute  "

            var Rpt_Msg = "Delay 1 Minute  COMPLETED";
            System.Threading.Thread.Sleep(60000);
            Console.WriteLine(" ");
            Console.WriteLine(Rpt_Msg);
            Console.WriteLine(" ");
            return;
        }

        ///// <summary>
        ///// SQL_Error 
        ///// Reports SQL Errors Called by various procedures.
        ///// </summary>
        //public void SQL_Error()
        //{
        //    //#ifdef DB2
        //    //    when = 6100    !DB2 error for empty_table result set
        //    //      break
        //    //#end_if

        //    //#ifdef DB2UNIX
        //    //    when = 6100    !DB2 error for empty_table result set
        //    //      break
        //    //#end_if

        //    //when = _99999  !Token "when" clause for non_DB2 environments
        //    //when_other
        //    Console.Write(sqr_program);
        //    Console.WriteLine(": ");
        //    Console.WriteLine(ReportID);
        //    Console.WriteLine(" _ SQL Statement = ");
        //    Console.WriteLine(SQL_STATEMENT);
        //    Console.WriteLine("SQL Status =");
        //    Console.WriteLine(sql_status);
        //    Console.WriteLine(" ");
        //    Console.WriteLine("SQL Error  = ");
        //    Console.WriteLine(sql_error);
        //    Console.WriteLine(Sql_Msg);
        //    Rollback_Transaction();
        //    var sgi_err_msg = "Stop at SQL Processing";
        //    SGI_Stop_Job();

        //    return;
        //}

        /// <summary>
        /// SQL_Error1 
        /// Reports SQL Errors Called by various procedures.  
        /// </summary>
        //public void SQL_Error1()
        //{

        //    if (i1_request_code == "00500")
        //    {

        //        //  evaluate #sql_status
        //        //#ifdef DB2
        //        //    when = 6100    !DB2 error for empty_table result set
        //        //      break
        //        //#end_if

        //        //#ifdef DB2UNIX
        //        //    when = 6100    !DB2 error for empty_table result set
        //        //      break
        //        //#end_if

        //        //when = _99999  !Token "when" clause for non_DB2 environments
        //        //when_other
        //        Console.Write(sqr_program);
        //        Console.WriteLine(": ");
        //        Console.WriteLine(ReportID);
        //        Console.WriteLine(" _ SQL Statement = ");
        //        Console.WriteLine(SQL_STATEMENT);
        //        Console.WriteLine("SQL Status =");
        //        Console.WriteLine(sql_status);
        //        Console.WriteLine(" ");
        //        Console.WriteLine("SQL Error  = ");
        //        Console.WriteLine(sql_error);
        //        Console.WriteLine(Sql_Msg);
        //        //Rollback_Transaction();
        //        var sgi_err_msg = "Stop at SQL Processing";
        //        //SGI_Stop_Job();
        //    }
        //    return;
        //}


        ////!______________________________________________________________________!
        ////! Called SQC Procedures                                                !
        ////!______________________________________________________________________!
        //// #include "reset.sqc"     ! Reset printer procedure
        //// #include "tranctrl.sqc"  ! Tools transaction control module
        //// #include "sgerror.sqc"   ! SGI Error H&&ling procedure

    }
}

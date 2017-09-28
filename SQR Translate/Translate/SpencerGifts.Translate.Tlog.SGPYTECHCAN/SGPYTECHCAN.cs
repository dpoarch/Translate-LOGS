using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpencerGifts.Translate.Plugin.Tlog.SGPYTECHCAN
{
    public class SGPYTECHCAN : TranslateItem
    {
        public datadbDataContext db = new datadbDataContext();

        public string TLOGBKUP = "\\sgawapp\\Retail_apps\\ValueLink\\PAYMENTECH\\TLOG\\BKUP\\";

        public void begin_report()
        {
            Init_Report();
            Get_DateTime();
            delete_ptd_go_file();
            Truncate_Tables();
            Files_copy();
            Delay_1_Minute();
            open_main_file();
            if (tlog_file_opened == 'Y')
            {
                create_tlog_go_file();
            }
            Reset();
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
            string dos_string = "cmd /c del " + base.PluginConfig.GetValue("PTDWDIR") + "PT_PTD.GO";
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
                Console.WriteLine(file);
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
            string dos2 = base.PluginConfig.GetValue("COPYFROM");
            string dos3 = "SGI_PT_PTD.* /ON/B > ";
            string dos4 = base.PluginConfig.GetValue("COPYTO") + "PTD.DAT";
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
            if (dos_status == 0)
            {
                Console.WriteLine(" ");
                Console.WriteLine("* Processing Files Now  *");
                Console.WriteLine(" ");
                var copy_flag = 'Y';
                Console.WriteLine(file);
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

            var ptdmainfile = base.PluginConfig.GetValue("COPYTO") + "PTD.DAT";

            if (File.Exists(ptdmainfile))
            {

                var logFile = File.ReadAllLines(ptdmainfile);
                List<string> LogList = new List<string>(logFile);
                var file_count = 0;
                foreach (var item in LogList)
                {
                    file_count++;
                    var ptdfile = item.Trim();
                    Process_Main();
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
        public void process_main()
        {

            Console.WriteLine("Begin of the PTD Process: " + AsOfToday + " " + AsOfNow);

            open_ptd_file();
            Process_Input();
            Process_Gift_Card_Tbl();
            backup_file();
            delete_file();
            Reset();

            return;
        }

        /// <summary>
        ///  open_ptd_file
        /// This is called from the process_main procedure.
        /// </summary>
        public void open_ptd_file()
        {

            var FullFileName = base.PluginConfig.GetValue("PTDWDIR") + ptdfile;
            if (!File.Exists(FullFileName))
            {
                //#debugb    Console.WriteLine("Open for file 15 failed"
                var sgi_err_msg = "Stop at PTD file opening";

                SGI_Stop_Job();
            }
            return;
        }

        /// <summary>
        /// Process Input  
        /// </summary>
        public void Process_Input(string path)
        {

            //#debuga Console.WriteLine(" I am in Process_Input "
            if (File.Exists(path))
            {

                var logFile = File.ReadAllLines(path);
                List<string> LogList = new List<string>(logFile);

                foreach (var item in LogList)
                {
                    char sep = '|';

                    string[] splitdata = item.Split(sep);
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
                    var mcc = splitdata[14];

                    record_type = record_type.Trim();
                    if (record_type != "")
                    {
                        Process_OLTP_Record();
                    }
                }
            }

            return;
        }

        /// <summary>
        /// Process_OLTP_Record 
        /// </summary>
        public void Process_OLTP_Record()
        {
            var record_type = record_type.Trim();
            var card_program = card_program.Trim();
            var bank_merchant_number = bank_merchant_number.Trim();
            bank_merchant_number = Convert.ToInt32(bank_merchant_number);
            var pns_merchant_number = pns_merchant_number.Trim();
            pns_merchant_number = Convert.ToInt32(pns_merchant_number);
            var merchant_name = merchant_name.Trim();
            var store_no = merchant_name.ToUpper();
            var store_length = store_no.length;
            var start_valid_pos = 0;
            var start_pos = 1;
            var i = 1;
            while (i <= store_length)
            {
                var value = store_no.Substring(start_pos, 1);
                if (Convert.ToInt32(value) > 0)
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
                store_no = store_no.Substring(start_valid_pos, store_length - start_valid_pos + 1);
                store_no = store_no.Trim();
            }

            select_store_state();

            var transaction_type = transaction_type.Trim();
            var terminal_id = terminal_id.Trim();
            terminal_id = Convert.ToInt32(terminal_id);
            var transaction_datetime = transaction_datetime.Trim();
            var card_number = card_number.Trim();
            var i1_promotion_code = card_number.Substring(7, 6);
            i1_promotion_code = "00" + i1_promotion_code;
            var bin_range = card_number.Substring(9, 4);
            card_number = Convert.ToInt32(card_number);
            var auth_number = auth_number.Trim();
            var tlog_authcode = auth_number;
            var employee_number = employee_number.Trim();
            employee_number = Convert.ToInt32(employee_number);
            var transaction_reference = transaction_reference.Trim();
            transaction_reference = Convert.ToInt32(transaction_reference);
            var transaction_amount = transaction_amount.Trim();
            var i1_transaction_amount = Convert.ToInt32(transaction_amount);
            transaction_amount = Convert.ToInt32(transaction_amount);

            if (transaction_amount >= 0)
            {
                var trans_amt_sign = "+";
            }
            else
            {
                var trans_amt_sign = "_";
            }
            if (transaction_amount < 0)
            {
                transaction_amount = -1 * transaction_amount;
            }
            var transaction_amt = transaction_amount * 100;
            //#debuga Console.WriteLine("#transaction_amt is " #transaction_amount
            // move #transaction_amt to transaction_amt 099999999
            //#debuga Console.WriteLine("transaction_amt is " transaction_amt
            var mcc = mcc.Trim();
            mcc = Convert.ToInt32(mcc);

            Insert_PT_PTD_Record();
            reset_flags();
            check_promo_code();
            check_subpromo_code();

            //#debugc Console.WriteLine("sub_promo_cd is       "sub_promo_cd
            //#debugc Console.WriteLine("sub_promo_cd_cre_tlog is "sub_promo_cd_cre_tlog
            //#debugc Console.WriteLine("promo_cd_cre_tlog is  "promo_cd_cre_tlog

            if (bin_range != "0235")
            {
                if (bin_range != "1144")
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
                                        if (transaction_type.ToUpper() = "INACTIVITY CHARGE")
                                        {
                                            var inactivity_charge_tlog = 'Y';
                                            create_tlog();
                                        }
                                        else
                                        {
                                            if (sub_promo_cd = "T" && sub_promo_cd_cre_tlog = "T")
                                            {
                                                create_tlog();
                                            }
                                            if (sub_promo_cd = "F" && promo_cd_cre_tlog = "T")
                                            {
                                                create_tlog();
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
                if (pns_merchant_number == "800000025247" || pns_merchant_number = "30000013257")
                {
                    var inactivity_charge_tlog = "Y";
                    create_tlog();
                }
            }

            if (bin_range == "1144" || bin_range == "0235")
            {
                var bin_range_cre_tlog = "T";
                var tlog_store_no = "03200";
                create_tlog();
            }

            if (i1_promotion_code != "00011518")
            {
                if (i1_promotion_code != "00011519")
                {
                    if (transaction_type.ToUpper() == "REV_ACTIVATION/ISSUANCE (NEW)")
                    {
                        var check_store_no = store_no;
                        var check_card_no = card_number;
                        var check_amount = transaction_amount;
                    }

                    if (transaction_type.ToUpper() == "ACTIVATION/ISSUANCE (NEW)" || transaction_type.ToUpper() == "VOICE ACTIVATION/ISSUANCE (NEW)" || transaction_type.ToUpper() == "BLOCK ACTIVATION TRANSACTION")
                    {
                        if (store_no != check_store_no && card_number != check_card_no && transaction_amount != -1 * check_amount)
                        {
                            Insert_GC_Inv_Temp();
                        }
                    }
                }
            }

            return;
        }

        /// <summary>
        /// select_store_state   
        /// The Procedure which gets the state code.
        /// </summary>
        public void select_store_state()
        {

            //#ifdef debuga
            //   Console.WriteLine("FLOW: select_store_state"
            //#endif
            var store_st_exists = "N";
            var state_cd = "";
            PS_SG_STORE_INFO pssg = db.PS_SG_STORE_INFOs.FirstOrDefault(i => i.DEPTID == store_no);

            state_cd = pssg.SG_STATE;
            store_st_exists = "Y";



            return;
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
            db.GIFTCARD_TRAN.InsertOnSubmit(gftrans);
            db.SubmitChanges();

            return;
        }

        /// <summary>
        /// check_promo_code
        /// This is called from the Process_OLTP_Record procedure. 
        /// </summary>
        public void check_promo_code()
        {

            var promo_rec_found = "N";

            select_promo_tbl();

            if (promo_rec_found == "N")
            {
                //#debugb Console.WriteLine("i1_promotion_code    "  noline
                //#debugb Console.WriteLine(i1_promotion_code
                var sgi_err_msg = "Stop at check_promo_code";
                SGI_Stop_Job();
            }
            if (promo_vl_cre_tlog_flag.ToUpper() == "Y")
            {
                var promo_cd_cre_tlog = "T";
            }
            return;
        }

        /// <summary>
        /// This is called from the check_promo_code procedure.  
        /// </summary>
        public void select_promo_tbl(int i1_promotion_code)
        {

            PROMO_CONTROL PROMO = db.PROMO_CONTROLs.FirstOrDefault(i => i.PROMO_CODE == i1_promotion_code);

            var promo_rec_found = "Y";

            return;
        }

        /// <summary>
        /// check_subpromo_code 
        ///  This is called from the Process_OLTP_Record procedure.
        /// </summary>
        public void check_subpromo_code()
        {
            var promo_code_sub = card_number.Substring(7, 12);

            var subpromo_rec_found = "N";

            select_subpromo_tbl();

            if (subpromo_rec_found == "Y")
            {
                var sub_promo_cd = "T";
                if (spc_vl_create_tlog_flag.ToUpper() == "Y")
                {
                    var sub_promo_cd_cre_tlog = "T";
                }
            }
            return;
        }


        /// <summary>
        /// select_subpromo_tbl
        /// This is called from the check_subpromo_code procedure.
        /// </summary>
        public void select_subpromo_tbl(int i1_promotion_code)
        {
            SPECIAL_PROMO_CONTROL SPC = db.SPECIAL_PROMO_CONTROLs.FirstOrDefault(i => i.PROMO_CODE == i1_promotion_code).Between(s => s.GC_BEGIN_RANGE && s.GC_END_RANGE);

            var subpromo_rec_found = "Y";


            return;
        }

        /// <summary>
        /// create_tlog 
        /// This is called from the Process_OLTP_Record procedure. 
        /// </summary>
        public void create_tlog()
        {
            card_lookup();

            if (tlog_file_opened != "Y")
            {
                Open_tlog_Files();
            }
            var tlog_created = "T";
            var tlog_trans_date = transaction_datetime.Substring(1, 2) + transaction_datetime.Substring(4, 2) + transaction_datetime(9, 2);

            if (bin_range_cre_tlog = "T")
            {
                var tlog_obj_cd0 = promo_object_code;
                var tlog_obj_cd1 = promo_object_code_exp;
                var tlog_promo_cd_sub = " ";
            }
            else
            {
                if (sub_promo_cd == "T")
                {
                    var tlog_store_no = spc_store_no;
                    var tlog_obj_cd0 = spc_object_code;
                    var tlog_obj_cd1 = spc_object_code_exp;
                    var tlog_promo_cd_sub = spc_promo_code_sub;
                }
                else
                {
                    var tlog_store_no = promo_store_no;
                    //#debugc Console.WriteLine("promo_store_no is " promo_store_no
                    tlog_store_no = promo_store_no;
                    var tlog_obj_cd0 = promo_object_code;
                    var tlog_obj_cd1 = promo_object_code_exp;
                    var tlog_promo_cd_sub = " ";
                }

                if (inactivity_charge_tlog == "Y")
                {
                    var tlog_store_no = promo_store_no;
                }

            }

            get_register();

            var register_no = to_char(register_no);

            var tlog_count = tlog_count + 1;
            if (tlog_count >= 9999)
            {
                update_register1();
                get_register();
                tlog_count = 0;
            }

            get_trans_no();

            var tlog_trans_no = stn_trans_no + 1;

            var tlog_acct_no = ucxl_card_number;
            write_10_record();
            write_30_record();
            write_47_record();
            write_99_record();
            create_tlog_history();
            update_trans_no();
            return;
        }

        /// <summary>
        /// card_lookup  
        ///  This is called from the create_tlog procedure.
        /// </summary>
        public void card_lookup(int card_number)
        {
            var card_no_found = "N";

            card_no_found = "Y";

            var ucxl_card_number = UCXL.VALUELINK;
            if (card_no_found == "N")
            {
                var ucxl_card_number = card_number;
            }
            return;
        }

        /// <summary>
        /// get_trans_no 
        /// This is called from the create_tlog procedure.
        /// </summary>
        public void get_trans_no(int tlog_store_no, int register_no)
        {

            var trans_nso_found = "N";


            STORE_TRANS_NO STN = db.STORE_TRANS_NOs.FirstOrDefault(i => i.STORE_NO == tlog_store_no && i.REGISTER_ID == register_no);

            var stn_trans_no = STN.TRANSACTION_NO;

            if (stn_trans_no == 9999)
            {
                stn_trans_no = 0;
            }
            var trans_no_found = "Y";


            if (trans_no_found == "N")
            {
                create_trans_no();
            }
            return;
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

            get_trans_no();

            return;
        }

        /// <summary>
        /// get_register 
        /// This is called from the Process_Input procedure.
        /// </summary>
        public void get_register(int tlog_store_no, string currentdate0)
        {
            var register_found = "N";

            STORE_TRANS_NO REG = db.STORE_TRANS_NOs.FirstOrDefault(i => i.STORE_NO == tlog_store_no && i.DA_TIMESTMP_MOD == null || i.DA_TIMESTMP_MOD != currentdate0);


            return;
        }


        /// <summary>
        /// write_10_record   
        /// This procedure writes the TLOG Header Record  
        /// </summary>
        public void write_10_record()
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

            if (promo_id_country == "USA")
            {
                var country = 30;
            }
            if (promo_id_country == "CAN")
            {
                var country = 31;
            }
            if (promo_id_country == "GBR")
            {
                var country = 32;
            }


            //write 20 from
            //trans_code:2
            //trans_qualifier:2
            //trans_type:2
            //flags:5
            //prefix_store_no:1
            //tlog_store_no:5
            //register_no:2
            //tlog_trans_no:5
            //cashier:9
            //record_count:2
            //tlog_trans_date:6
            //time:4
            //orig_store_no:6
            //orig_register:2
            //orig_trans:5
            //orig_date:6
            //reason:2
            //country:2

            //write 60 from
            //trans_code:2
            //trans_qualifier:2
            //trans_type:2
            //flags:5
            //prefix_store_no:1
            //tlog_store_no:5
            //register_no:2
            //tlog_trans_no:5
            //cashier:9
            //record_count:2
            //tlog_trans_date:6
            //time:4
            //orig_store_no:6
            //orig_register:2
            //orig_trans:5
            //orig_date:6
            //reason:2
            //country:2


            var file_created = "Y";
            return;
        }

        /// <summary>
        /// write_30_record 
        /// This procedure writes the line item Record  
        /// </summary>
        public void write_30_record()
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

            //write 20 from
            //trans_code:2
            //trans_type:2
            //flags:5
            //item:12
            //dept:5
            //jrnl_key:2
            //for_qty:5
            //quantity:5
            //reason:2
            //return_reason:2
            //orig_price:9
            //trans_amt_sign:1
            //transaction_amt:9
            //trans_amt_sign:1
            //transaction_amt:9
            //orig_for_qty:2


            //write 60 from
            //trans_code:2
            //trans_type:2
            //flags:5
            //item:12
            //dept:5
            //jrnl_key:2
            //for_qty:5
            //quantity:5
            //reason:2
            //return_reason:2
            //orig_price:9
            //trans_amt_sign:1
            //transaction_amt:9
            //trans_amt_sign:1
            //transaction_amt:9
            //orig_for_qty:2
            return;
        }

        /// <summary>
        /// 
        /// </summary>
        public void write_47_record()
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
                tlog_obj_cd = 20511;
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

            //write 20 from
            //trans_code:2
            //trans_type:2
            //flags:5
            //tlog_acct_no:20
            //trans_amt_sign:1
            //transaction_amt:9
            //balance_sign:1
            //remain_balance:9
            //tlog_authcode:6
            //tlog_obj_cd:6

            //write 60 from
            //trans_code:2
            //trans_type:2
            //flags:5
            //tlog_acct_no:20
            //trans_amt_sign:1
            //transaction_amt:9
            //balance_sign:1
            //remain_balance:9
            //tlog_authcode:6
            //tlog_obj_cd:6
            return;
        }

        /// <summary>
        /// write_99_record
        /// This procedure writes the Gift card Record
        /// </summary>
        public void write_99_record()
        {

            var trans_code = "99";
            var trans_type = "00";
            var flags = "00000";
            var foreign_curr = "000000000";
            var usa_cash = "000000000";
            var taxes = "000000000";
            var sales_person = "000000000";
            var orig_sales_person = "000000000";

            //write 20 from
            //trans_code:2
            //trans_type:2
            //flags:5
            //trans_amt_sign:1
            //transaction_amt:9
            //foreign_curr:9
            //usa_cash:9
            //taxes:9
            //trans_amt_sign:1
            //transaction_amt:9
            //sales_person:9
            //orig_sales_person:9


            //write 60 from
            //trans_code:2
            //trans_type:2
            //flags:5
            //trans_amt_sign:1
            //transaction_amt:9
            //foreign_curr:9
            //usa_cash:9
            //taxes:9
            //trans_amt_sign:1
            //transaction_amt:9
            //sales_person:9
            //orig_sales_person:9
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
            db.TLOG_HIST.InsertOnSubmit(tlog);
            db.SubmitChanges();

            return;
        }

        /// <summary>
        /// update_trans_no 
        /// The Procedure which Updates the STORE_TRANS_NO Record.
        /// </summary>
        public void update_trans_no(STORE_TRANS_NO data)
        {

            //#ifdef debuga
            // #debugb  Console.WriteLine("FLOW: update_trans_no"
            //#endif

            STORE_TRANS_NO strno = db.STORE_TRANS_NOs.FirstOrDefault(i => i.STORE_NO == tlog_store_no && i.REGISTER_ID == register_no);
            strno.TRANSACTION_NO = tlog_trans_no;


            return;
        }

        /// <summary>
        /// select_register  
        /// The Procedure which selects the Register from TLOG_HIST to Update with the Current Date 
        /// </summary>
        public void select_register()
        {
            //#ifdef debuga
            // #debugb  Console.WriteLine("FLOW: select_register"
            //#endif

            TLOG_HIST TLOGHIST = db.TLOG_HISTs.FirstOrDefault(i => i.DA_TIMESTMP_CRE == currentdate0).OrderBy(i => i.STORE_NO, i.REGISTER_NO).OrderByDescending();

            int tloghist_store_no = Convert.ToInt32(TLOGHIST.STORE_NO.Trim());
            var tloghist_register_no = TLOGHIST.REGISTER_NO.Trim();

            update_register();

            return;
        }

        /// <summary>
        /// update_register
        /// The Procedure which Updates the STORE_TRANS_NO Record.
        /// </summary>
        public void update_register()
        {
            //#ifdef debuga
            // #debugb  Console.WriteLine("FLOW: update_register"
            //#endif

            STORE_TRANS_NO str_no = db.STORE_TRANS_NOs.FirstOrDefault(r => r.STORE_NO == tloghist_store_no && r.REGISTER_ID == tloghist_register_no);
            str_no.DA_TIMESTMP_MOD = DateTime.Now;

            return;
        }

        /// <summary>
        /// update_register1 
        /// The Procedure which Updates the STORE_TRANS_NO Record.
        /// </summary>
        public void update_register1(int tlog_store_no, int register_no)
        {
            //#ifdef debuga
            // #debugb  Console.WriteLine("FLOW: update_register1"
            //#endif

            STORE_TRANS_NO udpate = db.STORE_TRANS_NOs.FirstOrDefault(i => i.STORE_NO == tlog_store_no && i.REGISTER_ID == register_no);
            udpate.DA_TIMESTMP_MOD = DateTime.Now;
            db.SubmitChanges();
            return;
        }

        /// <summary>
        /// Insert_GC_Inv_Temp
        /// This is called from the Process_OLTP_Record procedure.
        /// </summary>
        public void Insert_GC_Inv_Temp(GIFT_CARD_INV_PR10_TEMP data)
        {
            var Sql_Msg = "Insert_GC_Inv_Temp    PROBLEM";
            var RJP_WORK = "Insert_GC_Inv_Temp         ";
            //#debuga Console.WriteLine(" "
            //#debuga Console.WriteLine(RJP_WORK
            //#debuga Console.WriteLine(" "

            var inv_trans_date = transaction_datetime.Substring(1, 2) + transaction_datetime.Substring(4, 2) + transaction_datetime.Substring(9, 2);

            //#debuga Console.WriteLine("i1_promotion_code        is " i1_promotion_code
            //#debugb Console.WriteLine("store_no                 is " store_no
            //#debuga Console.WriteLine("transaction_datetime     is " transaction_datetime
            //#debuga Console.WriteLine("ws_timestmp_cre          is " ws_timestmp_cre


            GIFT_CARD_INV_PR10_TEMP gft = new GIFT_CARD_INV_PR10_TEMP();
            gft.VL_PROMOTION_CODE = data.VL_PROMOTION_CODE;
            gft.STORE_NO = data.STORE_NO;
            gft.TRANSACTION_DATE = data.TRANSACTION_DATE;
            gft.TRANS_POST_DATE = data.TRANS_POST_DATE;
            gft.CARDS_USED_QTY = 1;
            db.GIFT_CARD_INV_PR10_TEMPs.InsertOnSubmit(gft);
            db.SubmitChanges();

            var RJP_WORK = "Insert_GC_Inv_Temp   OK     ";
            //#debuga Console.WriteLine(" "
            //#debuga Console.WriteLine(RJP_WORK
            //#debuga Console.WriteLine(" "
            return;
        }

        /// <summary>
        /// Process_Gift_Card_Tbl
        /// This is called from the Begin_Report procedure. 
        /// </summary>
        public void Process_Gift_Card_Tbl()
        {
            Insert_Gift_Card_Rec();

            GIFT_CARD_INV_PR10_TEMP  PR10GCI = db.GIFT_CARD_INV_PR10_TEMPs,FirstOrDefault();
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

            var RJP_WORK = "Insert_Gift_Card_Rec    OK     ";
            //#debugb Console.WriteLine(" "
            //#debugb Console.WriteLine(RJP_WORK
            //#debugb Console.WriteLine(" "
            return;
        }

        /// <summary>
        /// Open_tlog_Files   
        /// This is called from the create_tlog procedure.
        /// </summary>
        public void Open_tlog_Files()
        {
            var tlogwdir = base.PluginConfig.GetValue("TLOGWDIR") + "tlog.txt";

            if (!File.Exists(tlogwdir))
            {
                File.Create(base.PluginConfig.GetValue("TLOGWDIR") + "tlog" + count + ".txt");
            }

            //open $tlogwdir as 20 for_writing record=100:vary status=#filestat
            var count = 0;
            while (ok != true)
            {
                var lineCount = File.ReadAllLines(tlogwdir).Length;
                if (lineCount >= 2000)
                {
                    count++;
                    if (File.Exists(base.PluginConfig.GetValue("TLOGWDIR") + "tlog" + count + ".txt"))
                    {
                        tlogwdir = base.PluginConfig.GetValue("TLOGWDIR") + "tlog" + count + ".txt";
                    }
                    else
                    {
                        File.Create(base.PluginConfig.GetValue("TLOGWDIR") + "tlog" + count + ".txt");
                    }
                }
            }

            var tlogbkup = base.PluginConfig.GetValue("TLOGBKUP") + "tlog" + ws_currdttm + "(Can)" + ".txt";
            //open tlogbkup as 60 for_writing record=100:vary status=#filestat
            if (!File.Exists(tlogbkup))
            {
                File.Create(base.PluginConfig.GetValue("TLOGBKUP") + "tlog" + ws_currdttm + ".txt");
            }
            var backupok = false;
            //open $tlogwdir as 20 for_writing record=100:vary status=#filestat
            var count = 0;
            while (backupok != true)
            {
                var lineCount = File.ReadAllLines(tlogbkup).Length;
                if (lineCount >= 2000)
                {
                    count++;
                    if (File.Exists(base.PluginConfig.GetValue("TLOGWDIR") + "tlog" + count + ".txt"))
                    {
                        tlogwdir = base.PluginConfig.GetValue("TLOGBKUP") + "tlog" + ws_currdttm + count + ".txt";
                    }
                    else
                    {
                        File.Create(base.PluginConfig.GetValue("TLOGBKUP") + "tlog" + ws_currdttm + count + ".txt");
                    }
                }
            }

            var tlog_file_opened = "Y";
            return;
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
            var ws_currdttm = ws_timestmp_cre.Substring(1, 4) + ws_timestmp_cre.Substring(6, 2) + ws_timestmp_cre.Substring(9, 2) + ws_timestmp_cre.Substring(11, 6);

            return;
        }

        /// <summary>
        /// reset_flags 
        /// The procedure to Reset all the Flags.  
        /// </summary>
        public void reset_flags()
        {
            var inactivity_charge_tlog = "F";
            var promo_cd_cre_tlog = "F";
            var sub_promo_cd = "F";
            var sub_promo_cd_cre_tlog = "F";
            var source_cd_cre_tlog = "F";
            var req_cd_cre_tlog = "F";
            var tlog_created = "F";
            var spc_promo_code_sub = " ";
            var bin_range_cre_tlog = "F";
            return;
        }

        /// <summary>
        /// create_tlog_go_file  
        /// This is called from the Begin_Report procedure. 
        /// </summary>
        public void create_tlog_go_file()
        {
            var ok = false;
            var TlogGoFileName = base.PluginConfig.GetValue("TLOGWDIR") + "pt_tlog.GO";
            //open TlogGoFileName as 200 for_writing record=100:vary status=#filestat
            if (!File.Exists(TlogGoFileName))
            {
                File.Create(base.PluginConfig.GetValue("TLOGWDIR") + "pt_tlog.GO");
            }
            //open $tlogwdir as 20 for_writing record=100:vary status=#filestat
            var count = 0;
            while (ok != true)
            {
                var lineCount = File.ReadAllLines(TlogGoFileName).Length;
                if (lineCount >= 2000)
                {
                    count++;
                    if (File.Exists(base.PluginConfig.GetValue("TLOGWDIR") + "pt_tlog.GO"))
                    {
                        tlogwdir = base.PluginConfig.GetValue("TLOGWDIR") + "pt_tlog" + count + ".GO";
                    }
                    else
                    {
                        File.Create(base.PluginConfig.GetValue("TLOGWDIR") + "pt_tlog" + count + ".GO");
                    }
                }
            }

            var Tlogdummy = "VL_TLOG.GO";

            //write 200 from
            //Tlogdummy:18
            return;
        }

        /// <summary>
        /// backup_file   
        /// This is called from the Begin_Report procedure.
        /// </summary>
        public void backup_file()
        {
            var dos_string1 = "cmd /c copy ";
            var dos_string2 = base.PluginConfig.GetValue("PTDWDIR") + ptdfile + " ";
            var dos_string3 = base.PluginConfig.GetValue("PTDBKUP") + RTRIM(ptdfile, ".txt");
            var dos_string4 = "." + ws_currdttm + ".txt";
            var dos_string = dos_string1 + dos_string2 + dos_string3 + dos_string4;
            Console.WriteLine("***** FILE XX ****");
            Console.WriteLine(" ");
            Console.WriteLine(dos_string);
            Console.WriteLine(" ");
            //call system using dos_string #dos_status wait
            if (dos_status < 32)
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
        public void delete_file()
        {
            var dos_string = "cmd /c del " + base.PluginConfig.GetValue("PTDWDIR") + ptdfile;
            Console.WriteLine("***** FILE XX ****");
            Console.WriteLine(" ");
            Console.WriteLine(dos_string);
            Console.WriteLine(" ");
            //call system using dos_string #dos_status
            if (dos_status >= 32)
            {
                Console.WriteLine(" ");
                Console.WriteLine("***** deleted ****");
                Console.WriteLine(" ");
                Console.WriteLine(file);
                Console.WriteLine(" ");
            }
            Console.WriteLine("_____________________");
            return;
        }

        public void Delay_1_Minute()
        {
            //#debuga Console.WriteLine("Delay_1_Minute  "

            var Rpt_Msg = "Delay 1 Minute  COMPLETED";
            System.Threading.Thread.Sleep(5000);
            Console.WriteLine(" ");
            Console.WriteLine(Rpt_Msg);
            Console.WriteLine(" ");
            return;
        }

        /// <summary>
        /// SQL_Error 
        /// Reports SQL Errors Called by various procedures.
        /// </summary>
        public void SQL_Error()
        {
            //#ifdef DB2
            //    when = 6100    !DB2 error for empty_table result set
            //      break
            //#end_if

            //#ifdef DB2UNIX
            //    when = 6100    !DB2 error for empty_table result set
            //      break
            //#end_if

            //when = _99999  !Token "when" clause for non_DB2 environments
            //when_other
            Console.Write(sqr_program);
            Console.WriteLine(": ");
            Console.WriteLine(ReportID);
            Console.WriteLine(" _ SQL Statement = ");
            Console.WriteLine(SQL_STATEMENT);
            Console.WriteLine("SQL Status =");
            Console.WriteLine(sql_status);
            Console.WriteLine(" ");
            Console.WriteLine("SQL Error  = ");
            Console.WriteLine(sql_error);
            Console.WriteLine(Sql_Msg);
            Rollback_Transaction();
            var sgi_err_msg = "Stop at SQL Processing";
            SGI_Stop_Job();

            return;
        }

        /// <summary>
        /// SQL_Error1 
        /// Reports SQL Errors Called by various procedures.  
        /// </summary>
        public void SQL_Error1()
        {

            if (i1_request_code == "00500")
            {

                //  evaluate #sql_status
                //#ifdef DB2
                //    when = 6100    !DB2 error for empty_table result set
                //      break
                //#end_if

                //#ifdef DB2UNIX
                //    when = 6100    !DB2 error for empty_table result set
                //      break
                //#end_if

                //when = _99999  !Token "when" clause for non_DB2 environments
                //when_other
                Console.Write(sqr_program);
                Console.WriteLine(": ");
                Console.WriteLine(ReportID);
                Console.WriteLine(" _ SQL Statement = ");
                Console.WriteLine(SQL_STATEMENT);
                Console.WriteLine("SQL Status =");
                Console.WriteLine(sql_status);
                Console.WriteLine(" ");
                Console.WriteLine("SQL Error  = ");
                Console.WriteLine(sql_error);
                Console.WriteLine(Sql_Msg);
                Rollback_Transaction();
                var sgi_err_msg = "Stop at SQL Processing";
                SGI_Stop_Job();
            }
            return;
        }


        //!______________________________________________________________________!
        //! Called SQC Procedures                                                !
        //!______________________________________________________________________!
        // #include "reset.sqc"     ! Reset printer procedure
        // #include "tranctrl.sqc"  ! Tools transaction control module
        // #include "sgerror.sqc"   ! SGI Error H&&ling procedure

    }
}

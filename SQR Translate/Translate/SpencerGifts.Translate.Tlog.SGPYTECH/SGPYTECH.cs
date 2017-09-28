using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpencerGifts.Translate.Plugin.Tlog.SGPYTECH
{
    public class SGPYTECH : TranslateItem
    {
        public datadbDataContext db = new datadbDataContext();

        public string EXPWDIR = "\\sgawapp\\Retail_apps\\ValueLink\\PAYMENTECH\\PTD\\Exception\\";


        public void begin_report()
        {
            Init_Report();
            Get_DateTime();
            delete_ptd_go_file();
            Truncate_Tables();
            Files_copy();
            Delay_1_Minute();
            open_main_file();
            Process_Gift_Card_Tbl();
            if (tlog_file_opened == 'Y')
            {
                create_tlog_go_file();
            }
            if (Exp_file_open == 'Y')
            {
                //close 101
                sendemail();
            }
            Reset();
            return;
        }

        /// <summary>
        /// Report initialization procedure.  Set titles, parameters. 
        /// This is called from the Begin_Report procedure. 
        /// </summary>
        public void Init_Report()
        {
            var ReportTitle = "SGPYTECH";
            Console.WriteLine(" ");
            Console.WriteLine(ReportTitle);
            Console.WriteLine(" ");
            return;
        }

        /// <summary>
        /// This is called from the Begin_Report procedure.
        /// </summary>
        public void delete_ptd_go_file()
        {
            var dos_string = "cmd /c del " + base.PluginConfig.GetValue("PTDWDIR") + "PT_PTD.GO";
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
        /// Truncate Tables use in Program.
        /// </summary>
        public void Truncate_Tables()
        {
            var Sql_Msg = "Truncating Tables  _ PROBLEM";
            var trunc_msg = "Truncating Tables";
            //#debugb display ' '
            //#debugb display $trunc_msg
            //#debugb display ' '

            db.ExecuteCommand("TRUNCATE TABLE GIFT_CARD_INV_PR10_TEMP");
            db.SubmitChanges();

            trunc_msg = "Truncating Tables    OK     ";
            //#debugb display ' '
            //#debugb display $trunc_msg
            //#debugb display ' '
            return;
        }

        /// <summary>
        /// This is called from the Begin_Report procedure.
        /// </summary>
        public void Files_copy()
        {
            //#debuga display 'Files_copy  '
            var dos1 = "cmd /c dir ";
            var dos2 = base.PluginConfig.GetValue("COPYFROM");
            var dos3 = "SGI_PT_PTD.* /O:N/B > ";
            var dos4 = base.PluginConfig.GetValue("COPYTO") + "PTD.DAT";
            var dos_string = dos1 + dos2 + dos3 + dos4;
            //#debuga display '$dos4  '
            //#debuga display  $dos4

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
                var copy_flag = "Y";
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
        /// File Opening Procedure 
        /// </summary>
        public void open_main_file(string ws_currdttm)
        {
            Console.WriteLine("open_main_file   X  ");

            var ptdmainfile = base.PluginConfig.GetValue("COPYTO") + "PTD.DAT";
            var logFile = File.ReadAllLines(ptdmainfile);
            List<string> readptdmainfile = new List<string>(logFile);
            if (!File.Exists(ptdmainfile))
            {
                Console.WriteLine("Could not open PTD Main file");
                Console.WriteLine(ptdmainfile);
                return;
            }
            var file_count = 0;
            foreach (var item in readptdmainfile)
            {

                file_count = file_count + 1;
                var ptdfile = item;

                process_main(item);
                var record_count = 0;

                Get_DateTime();
                Console.WriteLine("End of the PTD Process: " + ws_currdttm);
            }
            var total_files_processed = file_count + " file(s) processed.";
            Console.WriteLine(" ");
            Console.Write("End of input. ");
            Console.WriteLine(total_files_processed);
            return;
        }

        /// <summary>
        /// This is highest level driving procedure called from open_main_file.
        /// </summary>
        public void process_main(string filename)
        {
            Console.WriteLine("Begin of the PTD Process: " + AsOfToday + " " + AsOfNow);

            open_ptd_file(filename);
            Process_Input(filename);
            backup_file();
            delete_file();
            Reset();
            return;
        }

        /// <summary>
        /// This is called from the process_main procedure.
        /// </summary>
        public void open_ptd_file(string filename)
        {
            var FullFileName = base.PluginConfig.GetValue("PTDWDIR") + filename;
            //open $FullFileName as 15
            //for_reading
            //record=400:vary
            //status= #filestat

            FileStream fs = File.Open(FullFileName, FileMode.Open, FileAccess.Read, FileShare.Read)

            if(fs.CanRead == false)
            {
                //#debugb    show 'Open for file 15 failed'
                var sgi_err_msg   = "Stop at PTD file opening";
                //SGI_Stop_Job();
            }
            return;
        }

        /// <summary>
        /// Process Input  
        /// </summary>
        public void Process_Input(string path)
        {
            //#debuga show ' I am in Process_Input '
            var logFile = File.ReadAllLines(path);
            List<string> LogList = new List<string>(logFile);
            foreach (var item in LogList)
            {
                char sep = '|';

                string[] data = item.Split(sep);
                var record_type = data[0];
                var card_program = data[1];
                var bank_merchant_number = data[2];
                var pns_merchant_number = data[3];
                var merchant_name = data[4];
                var transaction_type = data[5];
                var terminal_id = data[6];
                var transaction_datetime = data[7];
                var card_number = data[8];
                var auth_number = data[9];
                var employee_number = data[10];
                var transaction_reference = data[10];
                var dummy = data[12];
                var transaction_amount = data[13];
                var mcc = data[14];

                record_type = record_type.Trim();
                if (record_type == null)
                {
                    Process_OLTP_Record(data);
                }
            }

            //CLOSE 15
            return;
        }

        /// <summary>
        /// Process_OLTP_Record   
        /// </summary>
        public void Process_OLTP_Record(string[] data)
        {
            var record_type = record_type.Trim();
            var card_program = card_program.Trim();
            var bank_merchant_number = bank_merchant_number.Trim();
            bank_merchant_number = Convert.ToInt32(bank_merchant_number);
            var pns_merchant_number = pns_merchant_number.Trim();
            pns_merchant_number = Convert.ToInt32(pns_merchant_number);
            var merchant_name = merchant_name.Trim();
            var store_no = merchant_name.ToUpper();
            var store_length = store_no.Length;
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
                    start_pos++;
                }
                i++;
            }


            if (start_valid_pos == 0)
            {
                store_no = "5";
            }
            else
            {
                store_no = store_no.Substring(start_valid_pos, store_length - start_valid_pos + 1);
                store_no = store_no.Trim();
            }
            //move $store_no to $store_no 09999

            PS_SG_STORE_INFO pssg = select_store_state(store_no);

            var transaction_type = pssg.TRANSACTION_TYPE.Trim();
            var terminal_id = pssg.TERMINAL_ID.Trim();
            terminal_id = Convert.ToInt32(pssg.TERMINAL_ID);
            var transaction_datetime = Convert.ToString(pssg.TRANSACTION_DATETIME).Trim();
            var i1_promotion_code = pssg.CARD_NUMBER.Substring(7, 6);
            i1_promotion_code = "00" + pssg.I1_PROMOTION_CODE;
            var bin_range = pssg.CARD_NUMBER.Substring(9, 4);
            int card_number = Convert.ToInt32(pssg.CARD_NUMBER.Trim());
            var auth_number = pssg.AUTH_NUMBER.Trim();
            var tlog_authcode = pssg.AUTH_NUMBER;
            var employee_number = pssg.EMPLOYEE_NUMBER.Trim();
            employee_number = Convert.ToInt32(pssg.EMPLOYEE_NUMBER);
            var transaction_reference = Convert.ToInt32(pssg.TRANSACTION_REFERENCE);
            var transaction_amount = pssg.transaction_amount.Trim();
            var i1_transaction_amount = Convert.ToString(pssg.TRANSACTION_AMOUNT);
            transaction_amount = Convert.ToInt32(pssg.TRANSACTION_AMOUNT);
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
            //#debuga show '#transaction_amt is ' #transaction_amount
            //move #transaction_amt to $transaction_amt 099999999
            //#debuga show '$transaction_amt is ' $transaction_amt
            var mcc = mcc.Trim();
            mcc = Convert.ToInt32(mcc);

            Insert_PT_PTD_Record();

            reset_flags();
            check_promo_code();
            check_subpromo_code();

            //#debugc show '$sub_promo_cd is       '$sub_promo_cd
            //#debugc show '$sub_promo_cd_cre_tlog is '$sub_promo_cd_cre_tlog
            //#debugc show '$promo_cd_cre_tlog is  '$promo_cd_cre_tlog


            if (bin_range == "0234" || bin_range == "3302" || bin_range == "3303" || bin_range == "3304" || bin_range == "1144" || bin_range == "0235")
            {
                // do nothing();
            }
            else
            {
                if (transaction_type.ToUpper() == "REDEMPTION" || transaction_type.ToUpper() == "VOICE REDEMPTION" || transaction_type.ToUpper() == "REV_REDEMPTION" || transaction_type.ToUpper() == "BALANCE INQUIRY" || transaction_type.ToUpper() == "ISSUANCE/ADD VALUE")
                {
                    //do nothing();
                }
                else
                {
                    if (transaction_type.ToUpper() == "INACTIVITY CHARGE" || transaction_type.ToUpper() == "REV_INACTIVITY CHARGE")
                    {
                        var inactivity_charge_tlog = 'Y';
                        create_tlog();
                    }
                    else
                    {
                        if (sub_promo_cd == "T" && sub_promo_cd_cre_tlog == "T")
                        {
                            if (transaction_type.ToUpper() == "ACTIVATION/ISSUANCE (NEW)" && bin_range == "2503" && store_no == "00005")
                            {
                                write_exception();
                            }
                            else
                            {
                                create_tlog();
                            }
                        }
                        if (sub_promo_cd == 'F' && promo_cd_cre_tlog == 'T')
                        {
                            if (transaction_type.ToUpper() == "ACTIVATION/ISSUANCE (NEW)" && bin_range == "2503" && store_no == "00005")           //! Added. Murali Kaithi. 03/25/2014. Requested by Kim Fedo.
                            {
                                write_exception();
                            }
                            else
                            {
                                create_tlog();
                            }
                        }
                    }
                }
            }


            if (tlog_created == 'T')
            {
                if (pns_merchant_number == "800000025247" || pns_merchant_number == "30000013257")
                {
                    create_tlog();
                }
            }

            if (bin_range == "3302" || bin_range == "3303" || bin_range == "3304" || bin_range == "0234")
            {
                if (transaction_type.ToUpper() == "BLOCK ACTIVATION TRANSACTION" || transaction_type.ToUpper() == "ACCOUNT EXPIRATION")
                {
                    var bin_range_cre_tlog = 'T';
                    tlog_store_no = "06500";
                    create_tlog();
                    tlog_store_no = "06999";
                    create_tlog();

                    if (transaction_type.ToUpper() == "REDEMPTION")
                    {
                        find_consig_store();
                        if (consig_store_found == 'Y')
                        {
                            if (lineofbuss == "SPIRIT CONSIGNMENT")
                            {
                                tlog_store_no = "06500";
                                create_tlog();
                            }
                            else
                            {
                                tlog_store_no = "06999";
                                create_tlog();
                            }
                        }
                    }
                }
            }


            if (bin_range == "1144" || bin_range == "0235")
            {
                if (transaction_type.ToUpper() == "BLOCK ACTIVATION TRANSACTION" || transaction_type.ToUpper() == "ACCOUNT EXPIRATION")
                {
                    var bin_range_cre_tlog = 'T';
                    var tlog_store_no = "03200";
                    create_tlog();
                }
            }


            if (bin_range == "0234" || bin_range == "3302" || bin_range == "3303" || bin_range == "3304" || bin_range == "1144" || bin_range == "0235")
            {
                //do nothing();
            }
            else
            {
                var bin_range_cre_tlog = 'T';
                if (transaction_type.ToUpper() == "REDEMPTION")
                {
                    find_consig_store();
                    if (consig_store_found == 'Y')
                    {
                        if (lineofbuss == "SPIRIT CONSIGNMENT")
                        {
                            var consig_tlog = 'T';
                            tlog_store_no = "00005";
                            create_tlog();
                            var create_act_uk = 'T';
                            tlog_store_no = "06999";
                            create_tlog();
                        }
                    }
                }
            }

            if (i1_promotion_code == "00011518")
            {
                if (i1_promotion_code == "00011519")
                {
                    if (transaction_type.ToUpper() == "REV_ACTIVATION/ISSUANCE (NEW)")
                    {
                        var check_store_no = store_no;
                        var check_card_no = card_number;
                        var check_amount = transaction_amount;
                    }

                    if (transaction_type.ToUpper() == "ACTIVATION/ISSUANCE (NEW)" || transaction_type.ToUpper() == "VOICE ACTIVATION/ISSUANCE (NEW)" || transaction_type.ToUpper() == "BLOCK ACTIVATION TRANSACTION")
                    {
                        if (bin_range == "2503" && store_no == "00005")
                        {
                            //do nothing
                        }
                        else
                        {
                            if (store_no == check_store_no && card_number == check_card_no && transaction_amount == _1 * check_amount)
                            {
                                Insert_GC_Inv_Temp();
                            }
                        }
                    }
                }
            }
            return;
        }

        /// <summary>
        /// The Procedure which gets the state code. 
        /// </summary>
        /// <param name="store_no"></param>
        public PS_SG_STORE_INFO select_store_state(int store_no)
        {

            //#ifdef debuga
            //   show 'FLOW: select_store_state'
            //#endif
            var store_st_exists = 'N';
            string state_cd = null;
            PS_SG_STORE_INFO pg_sg_info = db.PS_SG_STORE_INFOs.FirstOrDefault(i => i.DEPTID == store_no);

            state_cd = pg_sg_info.SG_STATE;
            store_st_exists = 'Y';

            return pg_sg_info;
        }

        /// <summary>
        /// This procedure inserts a row in the GIFTCARD_TRAN Record.
        /// </summary>
        /// <param name="data"></param>
        public void Insert_PT_PTD_Record(GIFTCARD_TRAN data)
        {
            GIFTCARD_TRAN gfcard = new GIFTCARD_TRAN();
            gfcard.RECORD_TYPE = data.RECORD_TYPE;
            gfcard.CARD_PROGRAM = data.CARD_PROGRAM;
            gfcard.BANK_MERCHANT_ID = data.BANK_MERCHANT_ID;
            gfcard.PNS_MERCHANT_ID = data.PNS_MERCHANT_ID;
            gfcard.MERCHANT_NAME = data.MERCHANT_NAME;
            gfcard.STORE_NO = data.STORE_NO;
            gfcard.STATE = data.STATE;
            gfcard.TRANS_TYPE = data.TRANS_TYPE;
            gfcard.TERMINAL_ID = data.TERMINAL_ID;
            gfcard.TRANS_DATETIME = data.TRANS_DATETIME;
            gfcard.GIFT_CARD_NO = data.GIFT_CARD_NO;
            gfcard.AUTH_NO = data.AUTH_NO;
            gfcard.EMPLOYEE_NO = data.EMPLOYEE_NO;
            gfcard.TRANS_REF = data.TRANS_REF;
            gfcard.TRANS_AMOUNT = data.TRANS_AMOUNT;
            gfcard.MCC = data.MCC;
            db.GIFTCARD_TRAN.InsertOnSubmit(gfcard);
            db.SubmitChanges();

            return;
        }

        /// <summary>
        ///  This is called from the Process_OLTP_Record procedure.  
        /// </summary>
        public void check_promo_code()
        {
            var promo_rec_found = 'N';

            select_promo_tbl();

            if (promo_rec_found == 'N')
            {
                //#debugb show 'i1_promotion_code    '  noline
                //#debugb show $i1_promotion_code
                var sgi_err_msg = "Stop at check_promo_code";
                SGI_Stop_Job();
            }

            if (promo_vl_cre_tlog_flag.ToUpper() == 'Y')
            {
                var promo_cd_cre_tlog = 'T';
            }
            return;
        }

        /// <summary>
        /// This is called from the check_promo_code procedure.
        /// </summary>
        public void select_promo_tbl()
        {
            PROMO_CONTROL PROMO = db.PROMO_CONTROLs.FirstOrDefault(i => i.PROMO_CODE == i1_promotion_code && i.ID_COUNTRY == "usa");

            return;
        }

        /// <summary>
        /// This is called from the Process_OLTP_Record procedure.
        /// </summary>
        public void check_subpromo_code()
        {
            var promo_code_sub = card_number.Substring(7, 12);

            var subpromo_rec_found = 'N';
            select_subpromo_tbl();

            if (subpromo_rec_found == 'Y')
            {
                var sub_promo_cd = 'T';
                if (spc_vl_create_tlog_flag.ToUpper() == 'Y')
                {
                    var sub_promo_cd_cre_tlog = 'T';
                }
            }
            return;
        }


        /// <summary>
        /// This is called from the check_subpromo_code procedure.
        /// </summary>
        /// <param name="i1_promotion_code"></param>
        /// <param name="promo_code_sub"></param>
        public void select_subpromo_tbl(int i1_promotion_code, int promo_code_sub)
        {

            SPECIAL_PROMO_CONTROL SPC = db.SPECIAL_PROMO_CONTROLs.FirstOrDefault(i => i.PROMO_CODE == i1_promotion_code && i.GC_BEGIN_RANGE < promo_code_sub && i.GC_END_RANGE > promo_code_sub);

            var spc_promo_code_sub = SPC.PROMO_CODE_SUB;
            var spc_vl_create_tlog_flag = SPC.VL_CREATE_TLOG_FLAG;
            var spc_store_no = SPC.STORE_NO;
            var sspc_object_code = SPC.OBJECT_CODE;
            var spc_object_code_exp = SPC.OBJECT_CODE_EXP;

            var subpromo_rec_found = 'Y';
            return;
        }

        /// <summary>
        /// This is called from the Process_OLTP_Record procedure.
        /// </summary>
        public void find_consig_store(int pns_merchant_number, int prof_id)
        {
            var store_no = store_no;
            var consig_store_found = 'N';

            vw_StoreProfile_Current p = db.vw_StoreProfile_Currents.FirstOrDefault(i => i.Profile_ID == prof_id && i.Authorizer_MID == pns_merchant_number);
            var LineOfBusiness = p.LineOfBusiness;
            var Store_No = p.Store_No;
            var Store_Name = p.Profile_ID;
            var Authorizer_MID = p.Authorizer_MID;

            var lineofbuss = p.LineOfBusiness.Trim().ToUpper();
            consig_store_found = 'Y';

            return;
        }

        /// <summary>
        /// This is called from the Process_OLTP_Record procedure.
        /// </summary>
        public void write_exception()
        {
            if (Exp_file_open == 'Y')
            {
                open_exp_file();
                write_exp_hdr();
                write_exp_det();
            }
            else
            {
                write_exp_det();
            }
            return;
        }

        /// <summary>
        /// This is called from the write_exception procedure.
        /// </summary>
        public void open_exp_file(string ws_currdttm)
        {

            var ExpFileName = "Paymentch_Exceptions_" + ws_currdttm + ".tab";
            ExpFileName = base.PluginConfig.GetValue("EXPWDIR") + ExpFileName;

            if (!File.Exists(ExpFileName))
            {
                //#debugb    show 'Open for Exp File failed'
                var sgi_err_msg = "Stop at Paymentech Exception file opening";
            }
            else
            {
                var read = File.ReadAllLines(ExpFileName);
            }

            var Exp_file_open = 'Y';
            return;
        }

        /// <summary>
        /// This is the called from the write_exception Procedure
        /// </summary>
        public void write_exp_hdr(string path)
        {
            var Trans_type_hdr = "Trans_type";
            var Trans_date_hdr = "Trans_date";
            var Store_no_hdr = "Store_no";
            var Giftcard_no_hdr = "Giftcard_no";
            var Amount_hdr = "Amount";

            File.WriteAllText(path, Trans_type_hdr + sep + Trans_date_hdr + sep + Store_no_hdr + sep + Giftcard_no_hdr + sep + Amount_hdr);

            return;
        }

        /// <summary>
        /// This is the called from the write_exception Procedure
        /// </summary>
        public void write_exp_det()
        {

            var trans_type_exp = transaction_type.ToUpper();
            var trans_date_exp = transaction_datetime.Substring(1, 10);

            File.WriteAllText(path, trans_type_exp + sep + trans_date_exp + sep + store_no + sep + card_number + sep + i1_transaction_amount);
            return;
        }

        /// <summary>
        /// This is called from the Process_OLTP_Record procedure. 
        /// </summary>
        public void create_tlog()
        {

            card_lookup();

            string[] dir = Open_tlog_Files().Split(',');

            string tlogdir = dir[0];
            string backupdir = dir[1];

            var tlog_created = 'T';
            var tlog_trans_date = transaction_datetime.Substring(1, 2) + transaction_datetime.Substring(4, 2) + transaction_datetime.Substring(9, 2);

            if (bin_range_cre_tlog == 'T')
            {
                var tlog_obj_cd0 = promo_object_code;
                var tlog_obj_cd1 = promo_object_code_exp;
                var tlog_promo_cd_sub = " ";
            }
            else
            {
                if (sub_promo_cd == 'T')
                {
                    var tlog_store_no = spc_store_no;
                    var tlog_obj_cd0 = spc_object_code;
                    var tlog_obj_cd1 = spc_object_code_exp;
                    var tlog_promo_cd_sub = spc_promo_code_sub;
                }
                else
                {
                    //#debugc show '$promo_store_no is ' $promo_store_no
                    var tlog_store_no = promo_store_no();
                    var tlog_obj_cd0 = promo_object_code();
                    var tlog_obj_cd1 = promo_object_code_exp();
                    var tlog_promo_cd_sub = " ";
                }

                if (inactivity_charge_tlog == 'Y')
                {
                    var tlog_store_no = promo_store_no;
                }

            }

            get_register();

            var register_no = Convert.ToString(register_no);

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


            //#debugc show '#tlog_store_no is  '#tlog_store_no
            //#debugc show '$register_no is  '$register_no

            write_10_record(tlogdir, backupdir);
            write_30_record(tlogdir, backupdir);
            write_47_record(tlogdir, backupdir);
            write_99_record(tlogdir, backupdir);
            create_tlog_history();
            update_trans_no();
            return;
        }

        /// <summary>
        /// This is called from the create_tlog procedure. 
        /// </summary>
        public void card_lookup(int card_number)
        {
            USA_CANADA_XREF_LOOKUP UCXL = db.USA_CANADA_XREF_LOOKUPs.FirstOrDefault(i => i.PAYMENTTECH == card_number);
            var card_no_found = 'N';

            //to_char(UCXL.VALUELINK)        &ucxl.card_number
            //move &ucxl.card_number      to $ucxl_card_number
            var card_no_found = 'Y';

            if (card_no_found == 'N')
            {
                var ucxl_card_number = card_number;
            }
            return;
        }

        /// <summary>
        ///  This is called from the create_tlog procedure.
        /// </summary>
        public void get_trans_no(int tlog_store_no, int register_no)
        {

            var tlog_store_no = Convert.ToString(tlog_store_no);
            var trans_no_found = 'N';


            STORE_TRANS_NO STN = db.STORE_TRANS_NOs.FirstOrDefault(i => i.STORE_NO == tlog_store_no && i.REGISTER_ID == register_no);
            var stn_trans_no = STN.TRANSACTION_NO;
            if (stn_trans_no = 9999)
            {
                stn_trans_no = 0;
            }
            var trans_no_found = 'Y';

            if (trans_no_found == 'N')
            {
                create_trans_no();
            }
            return;
        }

        /// <summary>
        /// This is called from the get_trans_no procedure. 
        /// </summary>
        public void create_trans_no(STORE_TRANS_NO data)
        {
            STORE_TRANS_NO storetrans = new STORE_TRANS_NO();
            storetrans.STORE_NO = data.STORE_NO;
            storetrans.REGISTER_ID = data.REGISTER_ID;
            storetrans.TRANSACTION_NO = 0;
            db.STORE_TRANS_NO.InsertOnSubmit(storetrans);
            db.SubmitChanges();

            get_trans_no();
            return;
        }

        /// <summary>
        /// This is called from the Process_Input procedure.
        /// </summary>
        public void get_register(string REGISTER_ID)
        {
            var register_found = 'N';
            var register_found = 'Y';
            var register_no = REGISTER_ID;

            return;

        }

        /// <summary>
        /// This procedure writes the TLOG Header Record 
        /// </summary>
        public void write_10_record(string file, string backupfile)
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


            if (tlog_store_no = "06999")
            {
                var country = 32;
            }

            if (tlog_store_no == "06500")
            {
                var country = 30;
            }

            if (create_act_uk = 'T')
            {
                var country = 32;
            }

            File.WriteAllText(file, trans_code + trans_qualifier + trans_type + flags + prefix_store_no + tlog_store_no + register_no + tlog_trans_no + cashier + record_count + tlog_trans_date + time + orig_store_no + orig_register + orig_trans + orig_date + reason + country);

            File.WriteAllText(backupfile, trans_code + trans_qualifier + trans_type + flags + prefix_store_no + tlog_store_no + register_no + tlog_trans_no + cashier + record_count + tlog_trans_date + time + orig_store_no + orig_register + orig_trans + orig_date + reason + country);


            var file_created = 'Y';
            return;
        }


        public void write_30_record(string file, string backupfile)
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

            File.WriteAllText(file, trans_code + trans_type + flags + item + dept + jrnl_key + for_qty + reason + return_reason + orig_price + trans_amt_sign + transaction_amt + trans_amt_sign + transaction_amt + orig_for_qty);
            File.WriteAllText(backupfile, trans_code + trans_type + flags + item + dept + jrnl_key + for_qty + reason + return_reason + orig_price + trans_amt_sign + transaction_amt + trans_amt_sign + transaction_amt + orig_for_qty);
            return;
        }

        /// <summary>
        /// write_47_record  
        /// This procedure writes the Gift card Record 
        /// </summary>
        public void write_47_record(string file, string backupfile)
        {
            var trans_code = "47";
            var flags = "00006";
            var req_pos_card_auth_type = 8;
            var trans_type = "00";
            var tlog_obj_cd = tlog_obj_cd0;
            if (bin_range == "3303" || bin_range == "3302")
            {
                if (transaction_type.ToUpper() == "REDEMPTION")
                {
                    var req_pos_card_auth_type = 9;
                    var trans_type = "02";
                    var tlog_obj_cd = 77516;
                }
            }
            if (pns_merchant_number == "800000025247" && transaction_type.ToUpper() == "REDEMPTION")
            {
                var req_pos_card_auth_type = 9;
                var trans_type = "02";
                var tlog_obj_cd = 70816;
            }
            if (inactivity_charge_tlog == 'Y')
            {
                if (transaction_type.ToUpper() == "REV_INACTIVITY CHARGE")
                {
                    var req_pos_card_auth_type = 9;
                    var trans_type = "07";
                    var tlog_obj_cd = 20312;
                }
                else
                {
                    var req_pos_card_auth_type = 9;
                    var trans_type = "02";
                    var tlog_obj_cd = 20311;
                }
            }
            if (transaction_type == "ACCOUNT EXPIRATION")
            {
                var req_pos_card_auth_type = 9;
                var trans_type = "02";

                var tlog_obj_cd = tlog_obj_cd1;
            }
            if (transaction_type.ToUpper() == "REV_BLOCK ACTIVATION TRANSACTION")
            {
                var req_pos_card_auth_type = 9;
                var trans_type = "02";
                var tlog_obj_cd = tlog_obj_cd1;
            }

            if (consig_tlog == 'T' && create_act_uk == 'T')
            {
                var req_pos_card_auth_type = 9;
                var trans_type = "02";
                var tlog_obj_cd = 73816;
            }
            if (consig_tlog == 'T' && create_act_uk == 'T')
            {
                var req_pos_card_auth_type = 8;
                var trans_type = "00";
                var tlog_obj_cd = 80815;
            }

            //move #tlog_obj_cd to $tlog_obj_cd 099999

            var balance_sign = '+';
            var remain_balance = 0;

            //move #remain_balance to $remain_balance 099999999

            File.WriteAllText(file, trans_code + trans_type + flags + tlog_acct_no + trans_amt_sign + transaction_amt + balance_sign + remain_balance + tlog_authcode + tlog_obj_cd);
            File.WriteAllText(backupfile, trans_code + trans_type + flags + tlog_acct_no + trans_amt_sign + transaction_amt + balance_sign + remain_balance + tlog_authcode + tlog_obj_cd);

            return;
        }

        /// <summary>
        /// write_99_record
        /// This procedure writes the Gift card Record
        /// </summary>
        public void write_99_record(string file, string backupfile)
        {

            var trans_code = "99";
            var trans_type = "00";
            var flags = "00000";
            var foreign_curr = "000000000";
            var usa_cash = "000000000";
            var taxes = "000000000";
            var sales_person = "000000000";
            var orig_sales_person = "000000000";

            File.WriteAllText(file, trans_code + trans_type + flags + tlog_acct_no + trans_amt_sign + transaction_amt + foreign_curr + usa_cash + taxes + trans_amt_sign + transaction_amt + sales_person + orig_sales_person);
            File.WriteAllText(backupfile, trans_code + trans_type + flags + tlog_acct_no + trans_amt_sign + transaction_amt + foreign_curr + usa_cash + taxes + trans_amt_sign + transaction_amt + sales_person + orig_sales_person);


            return;
        }




        /// <summary>
        /// create_tlog_history
        /// This procedure inserts a row in the TLOG_HIST Record.
        /// </summary>
        public void create_tlog_history(TLOG_HIST data)
        {
            //#debugc show '#tlog_obj_cd is ' #tlog_obj_cd

            TLOG_HIST tloghist = new TLOG_HIST();
            tloghist.STORE_NO = data.STORE_NO;
            tloghist.TRANSACTION_DATE = data.TRANSACTION_DATE;
            tloghist.TRANSACTION_NO = data.TRANSACTION_NO;
            tloghist.GIFT_CARD_NUMBER = data.GIFT_CARD_NUMBER;
            tloghist.AUTHORIZATION_CODE = data.AUTHORIZATION_CODE;
            tloghist.REGISTER_NO = data.REGISTER_NO;
            tloghist.POS_CARD_AUTH_TYPE = data.POS_CARD_AUTH_TYPE;
            tloghist.POS_GIFT_CARD_FLAG = data.POS_GIFT_CARD_FLAG;
            tloghist.GROSS_LINE_AMOUNT = data.GROSS_LINE_AMOUNT;
            tloghist.REMAING_CARD_BAL = data.REMAING_CARD_BAL;
            tloghist.LINE_OBJECT_TYPE = data.LINE_OBJECT_TYPE;
            tloghist.PROMO_CODE = data.PROMO_CODE;
            tloghist.PROMO_CODE_SUB = data.PROMO_CODE_SUB;
            tloghist.DA_TIMESTMP_CRE = data.DA_TIMESTMP_CRE;
            db.TLOG_HISTs.InsertOnSubmit(tloghist);
            db.SubmitChanges();
            return;
        }


        public void update_trans_no(int tlog_store_no, int register_no, int tlog_trans_no)
        {
            //#ifdef debuga
            // #debugb  show 'FLOW: update_trans_no'
            //#endif
            STORE_TRANS_NO storetrans = db.STORE_TRANS_NOs.FirstOrDefault(i => i.STORE_NO == tlog_store_no && i.REGISTER_ID == register_no);
            storetrans.TRANSACTION_NO = tlog_trans_no;
            db.SubmitChanges();
            return;
        }

        /// <summary>
        /// select_register
        /// The Procedure which selects the Register from TLOG_HIST to Update with the Current Date
        /// </summary>
        public void select_register()
        {

            //#ifdef debuga
            // #debugb  show 'FLOW: select_register'
            //#endif

            TLOG_HIST TLOGHIST = db.TLOG_HISTs.FirstOrDefault(i => i.DA_TIMESTMP_CRE == currentdate0).OrderByAscending(i => i.STORE_NO && i.REGISTER_NO);

            var TLOGHIST_STORE_NO = TLOGHIST.STORE_NO;
            var TLOGHIST_REGISTER_NO = TLOGHIST.REGISTER_NO;

            var tloghist_store_no = TLOGHIST_STORE_NO.Trim();
            var tloghist_store_no = Convert.ToInt32(tloghist_store_no);
            var tloghist_register_no = LOGHIST_REGISTER_NO.Trim();

            update_register();
            return;
        }

        /// <summary>
        /// update_register 
        /// The Procedure which Updates the STORE_TRANS_NO Record.
        /// </summary>
        /// <param name="tloghist_store_no"></param>
        /// <param name="tloghist_register_no"></param>
        public void update_register(int tloghist_store_no, int tloghist_register_no)
        {
            //#ifdef debuga
            // #debugb  show 'FLOW: update_register'
            //#endif
            STORE_TRANS_NO storetrans = db.STORE_TRANS_NOs.FirstOrDefault(i => i.STORE_NO == tloghist_store_no && i.REGISTER_ID == tloghist_register_no);
            storetrans.DA_TIMESTMP_MOD = DateTime.Now;
            db.SubmitChanges();

            return;
        }

        /// <summary>
        /// update_register1
        /// The Procedure which Updates the STORE_TRANS_NO Record.
        /// </summary>
        /// <param name="tlog_store_no"></param>
        /// <param name="register_no"></param>
        public void update_register1(int tlog_store_no, string register_no)
        {
            //#ifdef debuga
            // #debugb  show 'FLOW: update_register1'
            //#endif
            STORE_TRANS_NO storetrans = db.STORE_TRANS_NOs.FirstOrDefault(i => i.STORE_NO == tlog_store_no && i.REGISTER_ID == register_no);
            storetrans.DA_TIMESTMP_MOD = DateTime.Now;
            db.SubmitChanges();

            return;
        }

        /// <summary>
        /// Insert_GC_Inv_Temp 
        /// This is called from the Process_OLTP_Record procedure.
        /// </summary>
        /// <param name="data"></param>
        public void Insert_GC_Inv_Temp(GIFT_CARD_INV_PR10_TEMP data)
        {
            string Sql_Msg = "Insert_GC_Inv_Temp    PROBLEM";
            string RJP_WORK = "Insert_GC_Inv_Temp         ";
            //#debuga display ' '
            //#debuga display $RJP_WORK
            //#debuga display ' '

            var inv_trans_date = transaction_datetime.Substring(1, 2) + transaction_datetime.Substring(4, 2) + transaction_datetime.Substring(9, 2);

            //#debuga show '$i1_promotion_code        is ' $i1_promotion_code
            //#debugb show '$store_no                 is ' $store_no
            //#debuga show '$transaction_datetime     is ' $transaction_datetime
            //#debuga show '$ws_timestmp_cre          is ' $ws_timestmp_cre
            GIFT_CARD_INV_PR10_TEMP giftcard = new GIFT_CARD_INV_PR10_TEMP();
            giftcard.VL_PROMOTION_CODE = data.VL_PROMOTION_CODE;
            giftcard.STORE_NO = data.STORE_NO;
            giftcard.TRANSACTION_DATE = DateTime.ParseExact(data.TRANSACTION_DATE, "MMddyy", CultureInfo.InvariantCulture);
            giftcard.TRANS_POST_DATE = DateTime.ParseExact(data.TRANS_POST_DATE, "YYYY_MM_DDHH24MISS", CultureInfo.InvariantCulture);
            giftcard.CARDS_USED_QTY = 1;
            db.GIFT_CARD_INV_PR10_TEMPs.InsertOnSubmit(giftcard);
            db.SubmitChanges();

            string RJP_WORK = "Insert_GC_Inv_Temp   OK     ";
            //#debuga display ' '
            //#debuga display $RJP_WORK
            //#debuga display ' '
            return;
        }

        /// <summary>
        ///  This is called from the Begin_Report procedure.
        /// </summary>
        public void Process_Gift_Card_Tbl()
        {
            GIFT_CARD_INV_PR10_TEMP PR10GCI = db.GIFT_CARD_INV_PR10_TEMPs.Groupby(i => i.VL_PROMOTION_CODE, i.STORE_NO, i.TRANSACTION_DATE, i.TRANS_POST_DATE);

            string pr10gci_vl_promotion_code = PR10GCI.VL_PROMOTION_CODE;
            string pr10gci_store_no = PR10GCI.STORE_NO;
            string pr10gci_transaction_date = PR10GCI.TRANSACTION_DATE;
            string pr10gci_trans_post_date = PR10GCI.TRANS_POST_DATE;
            int pr10gci_cards_used_qty = PR10GCI.CARDS_USED_QTY;

            Insert_Gift_Card_Rec(PR10GCI);

            return;
        }

        /// <summary>
        /// Insert_Gift_Card_Rec
        /// This is called from the Process_Gift_Card_Tbl  procedure.
        /// </summary>
        /// <param name="data"></param>
        public void Insert_Gift_Card_Rec(GIFT_CARD_INV data)
        {
            string Sql_Msg = "Insert_Gift_Card_Rec   PROBLEM";
            string RJP_WORK = "Insert_Gift_Card_Rec        ";
            //#debugb display ' '
            //#debugb display $RJP_WORK
            //#debugb display ' '

            GIFT_CARD_INV_PR10_TEMP gftcard = new GIFT_CARD_INV_PR10_TEMP();

            gftcard.VL_PROMOTION_CODE = data.VL_PROMOTION_CODE;
            gftcard.STORE_NO = data.STORE_NO;
            gftcard.TRANSACTION_DATE = data.TRANSACTION_DATE;
            gftcard.TRANS_POST_DATE = data.TRANS_POST_DATE;
            gftcard.CARDS_USED_QTY = data.CARDS_USED_QTY;
            db.GIFT_CARD_INV_PR10_TEMPs.InsertOnSubmit(gftcard);
            db.SubmitChanges();
            string RJP_WORK = "Insert_Gift_Card_Rec    OK     ";
            //#debugb display ' '
            //#debugb display $RJP_WORK
            //#debugb display ' '
        }

        /// <summary>
        /// Open_tlog_Files  
        /// This is called from the create_tlog procedure. 
        /// </summary>
        public string Open_tlog_Files()
        {
            var ok = false;
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

            var tlogbkup = base.PluginConfig.GetValue("TLOGBKUP") + "tlog" + ws_currdttm + ".txt";
            //open $tlogbkup as 60 for_writing record=100:vary status=#filestat

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

            var tlog_file_opened = 'Y';
            string directories = tlogwdir + "," + tlogbkup;
            return directories;
        }

        /// <summary>
        /// Get_DateTime  
        /// This is called from the Begin_Report procedure.
        /// </summary>
        public string Get_DateTime()
        {
            DateTime time = DateTime.Now;
            string format1 = "dd_MM_yyyy";
            string format2 = "YYYY_MM_DDHH24MISS";
            string TODAY0 = time.ToString(format1);
            string ws_current_dttm = time.ToString(format2);
            string currentdate0 = TODAY0;
            string ws_timestmp_cre = ws_current_dttm;
            string ws_currdttm = ws_timestmp_cre.Substring(1, 4) + ws_timestmp_cre.Substring(6, 2) + ws_timestmp_cre.Substring(9, 2) + ws_timestmp_cre.Substring(11, 6);

            return ws_currdttm;
        }

        /// <summary>
        /// reset_flags
        /// The procedure to Reset all the Flags.
        /// </summary>
        public void reset_flags()
        {
            var inactivity_charge_tlog = 'F';
            var promo_cd_cre_tlog = 'F';
            var sub_promo_cd = 'F';
            var sub_promo_cd_cre_tlog = 'F';
            var source_cd_cre_tlog = 'F';
            var req_cd_cre_tlog = 'F';
            var tlog_created = 'F';
            var spc_promo_code_sub = " ";
            var bin_range_cre_tlog = 'F';
            var consig_tlog = 'F';
            var create_act_uk = 'F';
            return;
        }

        /// <summary>
        /// create_tlog_go_file    
        /// This is called from the Begin_Report procedure.
        /// </summary>
        public void create_tlog_go_file()
        {
            string TlogGoFileName = base.PluginConfig.GetValue("TLOGWDIR") + "pt_tlog.GO";
            //open $TlogGoFileName as 200 for_writing record=100:vary status=#filestat
            if (!File.Exists(tlogwdir))
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
                    if (File.Exists(base.PluginConfig.GetValue("TLOGWDIR") + "pt_tlog" + count + ".GO"))
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
            //$Tlogdummy:18
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
            char[] MyChar = { 't', 'x', 't', '.' };
            var dos_string3 = base.PluginConfig.GetValue("PTDBKUP") + ptdfile.TrimEnd(MyChar);
            var dos_string4 = '.' + ws_currdttm + ".txt";
            var dos_string = dos_string1 + dos_string2 + dos_string3 + dos_string4;
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
        public void delete_file()
        {
            string dos_string = "cmd /c del " + base.PluginConfig.GetValue("PTDWDIR") + ptdfile;
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
                display(file);
                Console.WriteLine(" ");
            }
            Console.WriteLine("_____________________");
            return;
        }

        /// <summary>
        /// send_email
        /// Mail file creating Procedure
        /// </summary>
        public void send_email()
        {

            string mailbody = "Find below the Paymentech Giftcard Activation Exceptions file. ";
            string mail_to = "kim.fedo@spencergifts.com,vickie.britton@spencergifts.com,Viktoryia.Durante@spencergifts.com";
            string cc_to = "murali.kaithi@spencergifts.com";

            string mailfiletext1 = "_ _body ";
            string mailfiletext2 = mailfiletext1 + '\"';
            string mailfiletext3 = mailfiletext2 + mailbody;
            string mailfiletext4 = mailfiletext3 + '|';
            string mailfiletext5 = mailfiletext4 + ExpFileName;
            string mailfiletext6 = mailfiletext5 + '\"';
            string mailfiletext7 = mailfiletext6 + " _subject ";
            string mailfiletext8 = mailfiletext7 + '\"';
            string mailfiletext9 = mailfiletext8 + "Paymentech Giftcard Activation Exceptions.";
            string mailfiletext10 = mailfiletext9 + '\"';
            string mailfiletext11 = mailfiletext10 + " _to ";
            string mailfiletext12 = mailfiletext11 + mail_to;
            string mailfiletext13 = mailfiletext12 + " _cc ";
            string mailfiletext14 = mailfiletext13 + cc_to;

            string mail_dos_string = "cmd /c blat " + mailfiletext14;
            Console.WriteLine(" ");
            Console.WriteLine(mail_dos_string);
            Console.WriteLine(" ");
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = mail_dos_string;
            process.StartInfo = startInfo;
            process.Start();
            if (mail_dos_status = 0)
            {
                Console.WriteLine(" ");
                Console.WriteLine("***** Paymentech Giftcard Activation Exceptions file Mailed ****");
                Console.WriteLine(" ");
            }
            Console.WriteLine("_____________________");

            return;
        }

        public void Delay_1_Minute()
        {
            //#debuga display 'Delay_1_Minute  '
            string Sql_Msg = "Delay_1_Minute  PROBLEM";

            System.Threading.Thread.Sleep(5000);

            string Rpt_Msg = "Delay 1 Minute  COMPLETED";
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
            //when = 6100    !DB2 error for empty_table result set
            //break
            //#end_if

            //#ifdef DB2UNIX
            //when = 6100    !DB2 error for empty_table result set
            //break
            //#end_if

            //when = _99999  !Token "when" clause for non_DB2 environments
            //when_other
            Console.Write(sqr_program);
            Console.WriteLine(": ");
            Console.Write(ReportID);
            Console.WriteLine(" _ SQL Statement = ");
            Console.WriteLine(SQL_STATEMENT);
            Console.Write("SQL Status =");
            Console.Write(sql_status + 99999);
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
            if (i1_request_code != "00500")
            {
                //#ifdef DB2
                //when = 6100    !DB2 error for empty_table result set
                //break
                //#end_if

                //#ifdef DB2UNIX
                //when = 6100    !DB2 error for empty_table result set
                //break
                //#end_if

                //when = _99999  !Token "when" clause for non_DB2 environments
                //when_other
                Console.WriteLine(sqr_program);
                Console.WriteLine(": ");
                Console.Write(ReportID);
                Console.WriteLine(" _ SQL Statement = ");
                Console.WriteLine(SQL_STATEMENT);
                Console.WriteLine("SQL Status =");
                Console.WriteLine(sql_status + 99999);
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
        // #include 'reset.sqc'     ! Reset printer procedure
        // #include 'tranctrl.sqc'  ! Tools transaction control module
        // #include 'sgerror.sqc'   ! SGI Error Handling procedure
    }
}

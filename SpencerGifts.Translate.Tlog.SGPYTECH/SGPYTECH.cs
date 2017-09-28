using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpencerGifts.Translate.Tlog.SGPYTECH
{
    class SGPYTECH
    {
        public datadbDataContext db = new datadbDataContext();
        //public string COPYFROM = "\\sgawapp\\Retail_apps\\ValueLink\\PAYMENTECH\\PTD\\";
        //public string COPYTO = "\\sgawapp\\Retail_apps\\ValueLink\\PAYMENTECH\\PTD\\Work\\";
        //public string PTDWDIR = "\\sgawapp\\Retail_apps\\ValueLink\\PAYMENTECH\\PTD\\";
        //public string PTDBKUP = "\\sgawapp\\Retail_apps\\ValueLink\\PAYMENTECH\\PTD\\BACKUP\\";
        //public string TLOGWDIR = "\\sgawapp\\Retail_apps\\ValueLink\\PAYMENTECH\\TLOG\\";
        //public string TLOGBKUP = "\\sgawapp\\Retail_apps\\ValueLink\\PAYMENTECH\\TLOG\\BKUP\\";
        //public string EXPWDIR = "\\sgawapp\\Retail_apps\\ValueLink\\PAYMENTECH\\PTD\\Exception\\";
        public string COPYFROM = "F:\\Test\\SGPYTECH\\copyfrom\\";
        public string COPYTO = "F:\\Test\\SGPYTECH\\copyto\\";
        public string PTDWDIR = "F:\\Test\\SGPYTECH\\ptdwdir\\";
        public string PTDBKUP = "F:\\Test\\SGPYTECH\\ptdbkup\\";
        public string TLOGWDIR = "F:\\Test\\SGPYTECH\\tlogwdir\\";
        public string TLOGBKUP = "F:\\Test\\SGPYTECH\\tlogbkup\\";
        public string EXPWDIR = "F:\\Test\\SGPYTECH\\expwdir\\";
        public string ws_currdttm = DateTime.Now.ToString("yyyyMMdd") + "." + "T" + DateTime.Now.ToString("HHmmss");
        public string inactivity_charge_tlog = "F";
        public string promo_cd_cre_tlog = "F";
        public string sub_promo_cd = "F";
        public string sub_promo_cd_cre_tlog = "F";
        public string source_cd_cre_tlog = "F";
        public string req_cd_cre_tlog = "F";
        public string tlog_created = "F";
        public string spc_promo_code_sub = " ";
        public string bin_range_cre_tlog = "F";
        public string consig_tlog = "F";
        public string create_act_uk = "F";
        public Int64 tlog_count = 0;



        public void begin_report()
        {
            Init_Report();
            //Get_DateTime();
            delete_ptd_go_file();
            Truncate_Tables();
            Files_copy();                             
            Delay_1_Minute();
            open_main_file();                      
            //create_tlog_go_file();
            //send_email();
            //Reset();
            return;
        } 

        /// <summary>
        /// Report initialization procedure.  Set titles, parameters. 
        /// This is called from the Begin-Report procedure. 
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
        /// This is called from the Begin-Report procedure.
        /// </summary>
        public void delete_ptd_go_file()
        {
            var dos_string = "cmd /c del " + PTDWDIR + "PT_PTD.GO";
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
            if(process.ExitCode >= 32)
            {
                Console.WriteLine(" ");
                Console.WriteLine("***** deleted ****");
                Console.WriteLine(" ");
                Console.WriteLine(PTDWDIR + "PT_PTD.GO");
                Console.WriteLine(" ");
            }
            Console.WriteLine("---------------------");
            return;
        }


        /// <summary>
        /// Truncate Tables use in Program.
        /// </summary>
        public void Truncate_Tables()
        {
            //var Sql_Msg = "Truncating Tables  - PROBLEM";
            var trunc_msg = "Truncating Tables";
             Console.WriteLine(" ");
            Console.WriteLine(trunc_msg);
             Console.WriteLine(" ");
            
            db.ExecuteCommand("TRUNCATE TABLE GIFT_CARD_INV_PR10_TEMP");
            db.SubmitChanges();
            
            trunc_msg = "Truncating Tables    OK     ";
            Console.WriteLine(" ");
            Console.WriteLine(trunc_msg);
             Console.WriteLine(" ");
            return;
        }

        /// <summary>
        /// This is called from the Begin-Report procedure.
        /// </summary>
        public void Files_copy()
        {
            //#debuga display 'Files-copy  '
            var dos1 = "cmd /c dir ";
            var dos2 = COPYFROM;
            var dos3 = "SGI_PT_PTD.* /ON/B > ";
            var dos4 = COPYTO + "PTD.DAT";
            var dos_string  = dos1 + dos2 + dos3 + dos4;
            //#debuga display 'dos4  '
            //#debuga display  dos4

            Console.WriteLine("***** FILsE XX ****");
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
            if(process.ExitCode == 0)
            {
                Console.WriteLine(" ");
                Console.WriteLine("* Processing Files Now  *");
                Console.WriteLine(" ");
                var copy_flag = "Y";
                Console.WriteLine(dos4);
                Console.WriteLine(" ");
            }   
            else
            {
                Console.WriteLine(" ");
                Console.WriteLine("***** processing failed");
                Console.WriteLine(" ");
            }   
            Console.WriteLine("---------------------");
            return;
        }

        /// <summary>
        /// File Opening Procedure 
        /// </summary>
        public void open_main_file()
        {
            Console.WriteLine("open-main-file   X  ");

            var ptdmainfile = COPYTO + "PTD.DAT";
            var logFile = File.ReadAllLines(ptdmainfile);
            List<string> readptdmainfile = new List<string>(logFile);
            if(!File.Exists(ptdmainfile))
            {
                Console.WriteLine("Could not open PTD Main file");
                Console.WriteLine(ptdmainfile);
                return;
            }
            var file_count = 0;
            foreach(var item in readptdmainfile)
            {

                file_count = file_count + 1;
                var ptdfile = item;

                process_main(ptdfile);
                var record_count = 0;

                //Get_DateTime();
                Console.WriteLine("End of the PTD Process: " + ws_currdttm);
            }
            var total_files_processed = file_count + " file(s) processed.";
            Console.WriteLine(" ");
            Console.Write("End of input. ");
            Console.WriteLine(total_files_processed);
            return;
        }

        /// <summary>
        /// This is highest level driving procedure called from open-main-file.
        /// </summary>
        public void process_main(string ptdfile)
        {
            Console.WriteLine("Begin of the PTD Process: " +  ws_currdttm);

            open_ptd_file(ptdfile);
            Process_Input(ptdfile);
            //Process_Gift_Card_Tbl(); 
            backup_file(ptdfile);
            delete_file(ptdfile);
            //Reset();
            return;
        }

         /// <summary>
         /// This is called from the process-main procedure.
         /// </summary>
        public void open_ptd_file(string ptdfile)
        {
            var FullFileName = PTDWDIR + ptdfile;
            //open FullFileName as 15
            //for-reading
            //record=400:vary
            //status= #filestat
            FileStream stream = File.Open(FullFileName, FileMode.Open, FileAccess.Read);

            if(stream.CanRead != true)
            {
                //#debugb    show 'Open for file 15s failed'
                var sgi_err_msg   = "Stop at PTD file opening";
                Console.WriteLine(sgi_err_msg);
                //SGI_Stop_Job();
            }
            stream.Close();
            return;
        }

        /// <summary>
        /// Process Input  
        /// </summary>
        public void Process_Input(string ptdfile)
        {
            int count = 0;
            //#debuga show ' I am in Process-Input '
            var logFile = File.ReadAllLines(PTDWDIR + ptdfile);
            List<string> LogList = new List<string>(logFile);
            foreach(var item in LogList)
            {
                count++;
                char sep = '|';

                string[] data = item.Split(sep);
                var record_type = data[0];
                record_type = record_type.Trim();
                if(record_type != null)
                {
                    Process_OLTP_Record(data, sep);
                }
                Console.WriteLine("Data Processed: " + count);
            }

            //CLOSE 15
            return;
        }

        /// <summary>
        /// Process-OLTP-Record   
        /// </summary>
        public void Process_OLTP_Record(string[] data, char sep)
        {
            string record_type = data[0].Trim();
            string card_program = data[1].Trim();
            Int64 bank_merchant_number = Convert.ToInt64(data[2].Trim());
            Int64 pns_merchant_number = Convert.ToInt64(data[3].Trim());
            string merchant_name = data[4].Trim();
            var store_no =  merchant_name.ToUpper();
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
                    start_pos++;
                }
                i++;
            }


            if(start_valid_pos == 0)
            {
                store_no = "5";
            }
            else
            {
                store_no = store_no.Substring(16, 2);
                store_no = store_no.Trim();
            }
            //move store_no to store_no 09999

            PS_SG_STORE_INFO pg_sg_info = select_store_state(store_no);
            if (pg_sg_info == null)
            {
                return;
            }
            string transaction_type =  pg_sg_info.TRANSACTION_TYPE.Trim();
            Int64 terminal_id =  Convert.ToInt64(pg_sg_info.TERMINAL_ID);
            string transaction_datetime = pg_sg_info.TRANSACTION_DATETIME.ToString().Trim();
            Int64 card_number = Convert.ToInt64(pg_sg_info.CARD_NUMBER);
            string i1_promotion_code = "00" +  card_number.ToString().Trim().Substring(7,6);
            string bin_range = card_number.ToString().Trim().Substring(9,4);
            string auth_number = pg_sg_info.AUTH_NUMBER.Trim();
            string tlog_authcode = auth_number;
            Int64 employee_number = Convert.ToInt64(pg_sg_info.EMPLOYEE_NUMBER.Trim());
            Int64 transaction_reference  =  Convert.ToInt64(pg_sg_info.TRANSACTION_REFERENCE.Trim());
            Int64 transaction_amount     =  Convert.ToInt64(pg_sg_info.TRANSACTION_AMOUNT.Trim());
            string i1_transaction_amount = Convert.ToString(transaction_amount);
            string trans_amt_sign;
            if(transaction_amount >= 0)
            {
                 trans_amt_sign = "+";
            }
            else
            {
                 trans_amt_sign = "-";
            }

            if(transaction_amount < 0)
            {
                transaction_amount = -1 * transaction_amount;
            }
            transaction_amount = transaction_amount * 100;
            //#debuga show '#transaction_amt is ' #transaction_amount
            //move #transaction_amt to transaction_amt 099999999
            //#debuga show 'transaction_amt is ' transaction_amt
            Int64 mcc = Convert.ToInt64(data[14]);

            Insert_PT_PTD_Record(data, pg_sg_info);

            reset_flags();
            string[] promocd = check_promo_code(i1_promotion_code).Split(',');
            string promo_rec_found = promocd[0];
            string promo_cd_cre_tlog = promocd[1];
            string[] promosub = check_subpromo_code(i1_promotion_code, card_number).Split(',');
            string subpromo_rec_found =  promosub[0];
            string sub_promo_cd = promosub[1];
            PROMO_CONTROL promo = select_promo_tbl(i1_promotion_code);
            SPECIAL_PROMO_CONTROL SPC = select_subpromo_tbl(i1_promotion_code, card_number);
            //#debugc show 'sub_promo_cd is       'sub_promo_cd
            //#debugc show 'sub_promo_cd_cre_tlog is 'sub_promo_cd_cre_tlog
            //#debugc show 'promo_cd_cre_tlog is  'promo_cd_cre_tlog


            if(bin_range == "0234" || bin_range == "3302" || bin_range == "3303" || bin_range == "3304" || bin_range == "1144" || bin_range == "0235")
            {
                // do nothing();
            }
            else
            {
                if(transaction_type.ToUpper() == "REDEMPTION" || transaction_type.ToUpper() == "VOICE REDEMPTION" || transaction_type.ToUpper() == "REV-REDEMPTION" || transaction_type.ToUpper() == "BALANCE INQUIRY" || transaction_type.ToUpper() == "ISSUANCE/ADD VALUE")
                {
                    //do nothing();
                }
                else
                {
                    if(transaction_type.ToUpper() == "INACTIVITY CHARGE" || transaction_type.ToUpper() == "REV-INACTIVITY CHARGE")
                    {
                        string inactivity_charge_tlog = "Y";
                        create_tlog(card_number, SPC, promo, pg_sg_info, sub_promo_cd, trans_amt_sign, transaction_amount.ToString(), pns_merchant_number, bin_range);
                    }
                    else
                    {
                        if(sub_promo_cd == "T" && sub_promo_cd_cre_tlog == "T")
                        {
                            if(transaction_type.ToUpper() == "ACTIVATION/ISSUANCE (NEW)" && bin_range == "2503" && store_no == "00005")           //! Added. Murali Kaithi. 03/25/2014. Requested by Kim Fedo.
                            {
                                write_exception(sep, transaction_type, transaction_datetime, store_no, card_number.ToString(), transaction_amount.ToString());
                            }
                            else
                            {
                                create_tlog(card_number, SPC, promo, pg_sg_info, sub_promo_cd, trans_amt_sign, transaction_amount.ToString(), pns_merchant_number, bin_range);
                            }
                        }
                        if(sub_promo_cd == "F" && promo_cd_cre_tlog == "T")
                        {
                            if (transaction_type.ToUpper() == "ACTIVATION/ISSUANCE (NEW)" && bin_range == "2503" && store_no == "00005")           //! Added. Murali Kaithi. 03/25/2014. Requested by Kim Fedo.
                            {
                                write_exception(sep, transaction_type, transaction_datetime, store_no, card_number.ToString(), transaction_amount.ToString());
                            }
                            else
                            {
                                create_tlog(card_number, SPC, promo, pg_sg_info, sub_promo_cd, trans_amt_sign, transaction_amount.ToString(), pns_merchant_number, bin_range);
                            }
                        }
                    }
                }
            }

            string consig_store_found  = "N";
            vw_StoreProfile_Current vsc = new vw_StoreProfile_Current();
            if(tlog_created == "T")
            {
                if(pns_merchant_number.ToString() == "800000025247" || pns_merchant_number.ToString() == "30000013257")
                {
                    create_tlog(card_number, SPC, promo, pg_sg_info, sub_promo_cd, trans_amt_sign, transaction_amount.ToString(), pns_merchant_number, bin_range);
                }
            }

            if(bin_range == "3302" || bin_range == "3303" || bin_range == "3304" || bin_range == "0234")
            {
                if(transaction_type.ToUpper() == "BLOCK ACTIVATION TRANSACTION" || transaction_type.ToUpper() == "ACCOUNT EXPIRATION")
                {
                    var bin_range_cre_tlog = "T";
                    var tlog_store_no = "06500";
                    create_tlog(card_number, SPC, promo, pg_sg_info, sub_promo_cd, trans_amt_sign, transaction_amount.ToString(), pns_merchant_number, bin_range);
                    tlog_store_no = "06999";
                    create_tlog(card_number, SPC, promo, pg_sg_info, sub_promo_cd, trans_amt_sign, transaction_amount.ToString(), pns_merchant_number, bin_range);
    
                    if(transaction_type.ToUpper() == "REDEMPTION")
                    {
                        
                        bin_range_cre_tlog = "T";
                        vsc = find_consig_store(pns_merchant_number, Convert.ToInt64(pg_sg_info.TERMINAL_ID), store_no);
                        consig_store_found  = "Y";
                        if(consig_store_found  == "Y")
                        {
                            if(vsc.LineOfBusiness == "SPIRIT CONSIGNMENT")
                            {
                                tlog_store_no = "06500";
                                create_tlog(card_number, SPC, promo, pg_sg_info, sub_promo_cd, trans_amt_sign, transaction_amount.ToString(), pns_merchant_number, bin_range);
                            }
                            else
                            {
                                 tlog_store_no = "06999";
                                 create_tlog(card_number, SPC, promo, pg_sg_info, sub_promo_cd, trans_amt_sign, transaction_amount.ToString(), pns_merchant_number, bin_range);
                            }
                        }
                    }
                }
            }


            if(bin_range == "1144" || bin_range == "0235")
            {
                if(transaction_type.ToUpper() == "BLOCK ACTIVATION TRANSACTION" || transaction_type.ToUpper() == "ACCOUNT EXPIRATION")
                {
                    var bin_range_cre_tlog = 'T';
                    var tlog_store_no = "03200";
                    create_tlog(card_number, SPC, promo, pg_sg_info, sub_promo_cd, trans_amt_sign, transaction_amount.ToString(), pns_merchant_number, bin_range);
                }
            }


            if(bin_range == "0234" || bin_range == "3302" || bin_range == "3303" || bin_range == "3304" || bin_range == "1144" || bin_range == "0235")
            {
                //do nothing();
            }
            else
            {
                var bin_range_cre_tlog = 'T';
                if(transaction_type.ToUpper() == "REDEMPTION")
                {
                    vsc = find_consig_store(pns_merchant_number, Convert.ToInt64(pg_sg_info.TERMINAL_ID), store_no);
                    consig_store_found  = "Y";
                    if(consig_store_found  == "Y")
                    {
                        if (vsc.LineOfBusiness == "SPIRIT CONSIGNMENT")
                        {
                            var consig_tlog = 'T';
                            var tlog_store_no = "00005";
                            create_tlog(card_number, SPC, promo, pg_sg_info, sub_promo_cd, trans_amt_sign, transaction_amount.ToString(), pns_merchant_number, bin_range);
                            var create_act_uk = 'T';
                             tlog_store_no = "06999";
                            create_tlog(card_number, SPC, promo, pg_sg_info, sub_promo_cd, trans_amt_sign, transaction_amount.ToString(), pns_merchant_number, bin_range);
                        }
                    }
                }
            }
            string check_store_no = store_no;
            Int64 check_card_no  = card_number;
            Int64 check_amount   = transaction_amount;
            if(i1_promotion_code == "00011518")
            {
                if(i1_promotion_code == "00011519")
                {
                    if(transaction_type.ToUpper() == "REV-ACTIVATION/ISSUANCE (NEW)")
                    {
                         check_store_no = store_no;
                         check_card_no  = card_number;
                         check_amount   = transaction_amount;
                    }

                    if(transaction_type.ToUpper() == "ACTIVATION/ISSUANCE (NEW)" || transaction_type.ToUpper() == "VOICE ACTIVATION/ISSUANCE (NEW)" || transaction_type.ToUpper() == "BLOCK ACTIVATION TRANSACTION")
                    {
                        if (bin_range == "2503" && store_no == "00005")
                        {
                            //do nothing
                        }
                        else
                        {
                            if(store_no == check_store_no && card_number == check_card_no && transaction_amount == -1 * check_amount)
                            {
                                Insert_GC_Inv_Temp(promo.PROMO_CODE, store_no);
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
        public PS_SG_STORE_INFO select_store_state(string store_no)
        {

            //#ifdef debuga
            //   show 'FLOW: select-store-state'
            //#endif
            string store_st_exists = "N";
            string state_cd = null;
            PS_SG_STORE_INFO pg_sg_info = db.PS_SG_STORE_INFOs.FirstOrDefault(i => i.DEPTID == store_no);

            state_cd = pg_sg_info.SG_STATE;
            store_st_exists = "Y";

            return pg_sg_info;
        }

        /// <summary>
        /// This procedure inserts a row in the GIFTCARD_TRANS Record.
        /// </summary>
        /// <param name="data"></param>
        public void Insert_PT_PTD_Record(string[] data, PS_SG_STORE_INFO pg_sg_info)
        {
            GIFTCARD_TRAN gfcard = new GIFTCARD_TRAN();
            gfcard.RECORD_TYPE = data[0];
            gfcard.CARD_PROGRAM = data[1];
            gfcard.BANK_MERCHANT_ID = data[2];
            gfcard.PNS_MERCHANT_ID = data[3];
            gfcard.MERCHANT_NAME = data[4];
            gfcard.STORE_NO = Convert.ToInt64(pg_sg_info.DEPTID);
            gfcard.STATE = pg_sg_info.SG_STATE;
            gfcard.TRANS_TYPE = pg_sg_info.TRANSACTION_TYPE;
            gfcard.TERMINAL_ID = pg_sg_info.TERMINAL_ID;
            gfcard.TRANS_DATETIME = pg_sg_info.TRANSACTION_DATETIME;
            gfcard.GIFT_CARD_NO = Convert.ToInt64(pg_sg_info.CARD_NUMBER);
            gfcard.AUTH_NO = pg_sg_info.AUTH_NUMBER;
            gfcard.EMPLOYEE_NO = Convert.ToInt64(pg_sg_info.EMPLOYEE_NUMBER);
            gfcard.TRANS_REF = Convert.ToInt64(pg_sg_info.TRANSACTION_REFERENCE);
            gfcard.TRANS_AMOUNT = Convert.ToInt64(pg_sg_info.TRANSACTION_AMOUNT);
            gfcard.MCC = Convert.ToInt64(data[14]);
            db.GIFTCARD_TRANs.InsertOnSubmit(gfcard);
            db.SubmitChanges();

            return;
        }

        /// <summary>
        ///  This is called from the Process-OLTP-Record procedure.  
        /// </summary>
        public string check_promo_code(string i1_promotion_code)
        {
            var promo_rec_found  = "N";
            string promo_cd_cre_tlog = "F";
            PROMO_CONTROL promo = select_promo_tbl(i1_promotion_code);
            promo_rec_found  = "Y";
            if(promo.VL_CREATE_TLOG_FLAG.ToUpper() == "Y")
            {
                promo_cd_cre_tlog = "T";
            }
            return promo_rec_found + "," + promo_cd_cre_tlog;
        }

        /// <summary>
        /// This is called from the check_promo_code procedure.
        /// </summary>
        public PROMO_CONTROL select_promo_tbl(string i1_promotion_code)
        {
            PROMO_CONTROL PROMO = db.PROMO_CONTROLs.FirstOrDefault(i => i.PROMO_CODE == i1_promotion_code && i.ID_COUNTRY == "usa" );
            return PROMO;
        }

        /// <summary>
        /// This is called from the Process-OLTP-Record procedure.
        /// </summary>
        public string check_subpromo_code(string i1_promotion_code, Int64 card_number)
        {
            Int64 promo_code_sub = Convert.ToInt64(card_number.ToString().Substring(7,12));
            string subpromo_rec_found  = "N";
            SPECIAL_PROMO_CONTROL SPC = select_subpromo_tbl(i1_promotion_code, card_number);
            string sub_promo_cd = "F";
            subpromo_rec_found  = "Y";
            if(subpromo_rec_found  == "Y")
            {
                sub_promo_cd = "T";
                if(SPC.VL_CREATE_TLOG_FLAG.ToUpper() == "Y")
                {
                    sub_promo_cd_cre_tlog = "T";
                }
            }
            return subpromo_rec_found + "," + sub_promo_cd;
        }

        /// <summary>
        /// This is called from the check_subpromo_code procedure.
        /// </summary>
        public SPECIAL_PROMO_CONTROL select_subpromo_tbl(string i1_promotion_code, Int64 card_number)
        {
            Int64 promo_code_sub = Convert.ToInt64(card_number.ToString().Substring(7,12));
            SPECIAL_PROMO_CONTROL SPC = db.SPECIAL_PROMO_CONTROLs.FirstOrDefault(i => i.PROMO_CODE == Convert.ToInt64(i1_promotion_code) && i.GC_BEGIN_RANGE < promo_code_sub ||  i.GC_END_RANGE > promo_code_sub);


            return SPC;
        }
        public vw_StoreProfile_Current find_consig_store(Int64 pns_merchant_number, Int64 prof_id, string store_no)
        {


            vw_StoreProfile_Current p = db.vw_StoreProfile_Currents.FirstOrDefault(i => i.Profile_ID == prof_id.ToString() && i.Authorizer_MID == pns_merchant_number.ToString());
            var LineOfBusiness = p.LineOfBusiness;
            var Store_No = p.Store_No;
            var Store_Name = p.Profile_ID;
            var Authorizer_MID = p.Authorizer_MID;

            var lineofbuss = p.LineOfBusiness.Trim().ToUpper();

            return p;
        }

        /// <summary>
        /// This is called from the Process_OLTP_Record procedure.
        /// </summary>
        public void write_exception(char sep, string Trans_type_hdr, string Trans_date_hdr, string Store_no_hdr, string Giftcard_no_hdr, string Amount_hdr)
        {   string path = EXPWDIR + "Paymentch_Exceptions_" + ws_currdttm + ".tab";
            string[] allLines = open_exp_file(ws_currdttm.ToString());
            string Exp_file_open = "Y";
            if(Exp_file_open == "Y")
            {
                write_exp_hdr(sep, path, Trans_type_hdr, Trans_date_hdr, Store_no_hdr, Giftcard_no_hdr, Amount_hdr);
                write_exp_det(path, sep, Trans_type_hdr, Trans_date_hdr, Store_no_hdr, Giftcard_no_hdr, Amount_hdr);
            }
            else
            {
                write_exp_det(path, sep, Trans_type_hdr, Trans_date_hdr, Store_no_hdr, Giftcard_no_hdr, Amount_hdr);
            }
            return;
        }

        /// <summary>
        /// This is called from the write_exception procedure.
        /// </summary>
        public string[] open_exp_file(string ws_currdttm)
        {
            string[] allLines = new string[] {};
            var ExpFileName = "Paymentch_Exceptions_" + ws_currdttm + ".tab";
            ExpFileName = EXPWDIR + ExpFileName;
            if(!File.Exists(ExpFileName))
            {
                //#debugb    show 'Open for Exp File failed'
                var sgi_err_msg = "Stop at Paymentech Exception file opening";
            }
            else
            {
                FileStream stream = File.Open(ExpFileName, FileMode.Open, FileAccess.Read);
                if(stream.CanRead == true)
                {
                    allLines = File.ReadAllLines(ExpFileName);
                }
                stream.Close();
            }

            string Exp_file_open = "Y";
            return allLines;
        }

        /// <summary>
        /// This is the called from the write_exception Procedure
        /// </summary>
        public void  write_exp_hdr(char sep, string path, string Trans_type_hdr, string Trans_date_hdr, string Store_no_hdr, string Giftcard_no_hdr, string Amount_hdr)
        {
            File.WriteAllText(path, Trans_type_hdr + sep + Trans_date_hdr + sep + Store_no_hdr + sep + Giftcard_no_hdr + sep + Amount_hdr + Environment.NewLine);

            return;
        }

        /// <summary>
        /// This is the called from the write_exception Procedure
        /// </summary>
        public void  write_exp_det(string path, char sep, string transaction_type, string transaction_datetime, string store_no, string card_number, string i1_transaction_amount)
        {

            var trans_type_exp = transaction_type.ToUpper();
            var trans_date_exp = transaction_datetime.Substring(1,10);

            File.WriteAllText(path, trans_type_exp + sep + trans_date_exp + sep + store_no + sep + card_number + sep + i1_transaction_amount);
            return;
        }

        /// <summary>
        /// This is called from the Process_OLTP_Record procedure. 
        /// </summary>
        public void create_tlog(Int64 card_number, SPECIAL_PROMO_CONTROL SPC, PROMO_CONTROL promo, PS_SG_STORE_INFO pg_sg_info, string sub_promo_cd, string trans_amount_sign, string transaction_amount, Int64 pns_merchant_number, string bin_range)
        {

            string ucxl_card_number = card_lookup(card_number);
            string[] dir = Open_tlog_Files().Split(',');
            string tlogwdir = dir[0];
            string tlogbkup = dir[1];
            string tlog_created = "T";
            string tlog_trans_date = DateTime.Now.ToString();
            Int64 tlog_store_no = 0;
            string tlog_obj_cd0 = "";
            string tlog_obj_cd1 = "";
            string tlog_promo_cd_sub = "";
            if(bin_range_cre_tlog == "T")
            {
                 tlog_obj_cd0 = promo.OBJECT_CODE;
                 tlog_obj_cd1 = promo.OBJECT_CODE_EXP;
                 tlog_promo_cd_sub = " ";
            }
            else
            {
                if(sub_promo_cd == "T")
                {
                    tlog_store_no     = Convert.ToInt64(SPC.STORE_NO);
                     tlog_obj_cd0      = SPC.OBJECT_CODE;
                     tlog_obj_cd1      = SPC.OBJECT_CODE_EXP;
                     tlog_promo_cd_sub = SPC.PROMO_CODE_SUB;
                }
                else
                {
                    //#debugc show '$promo_store_no is ' $promo_store_no
                    tlog_store_no     = Convert.ToInt64(promo.STORE_NO);
                     tlog_obj_cd0 = promo.OBJECT_CODE;
                     tlog_obj_cd1 = promo.OBJECT_CODE_EXP;
                     tlog_promo_cd_sub = " ";
                }

                if(inactivity_charge_tlog == "Y")
                {
                    tlog_store_no = Convert.ToInt64(promo.STORE_NO);
                }

            }

            STORE_TRANS_NO get_reg = get_register(tlog_store_no, DateTime.Now.ToString("yyyyMMdd"));

            string register_no = Convert.ToString(get_reg.REGISTER_ID);

            tlog_count = tlog_count + 1;
            if(tlog_count >= 9999)
            {
                update_register1(tlog_store_no, register_no);
                tlog_count = 0;
            }

            string[] stn = get_trans_no(tlog_store_no, Convert.ToInt64(register_no)).Split(',');
            string trans_no_found = stn[0];
            string stn_trans_no = stn[1];
            Int64 tlog_trans_no = Convert.ToInt64(stn_trans_no) + 1;
            var tlog_acct_no = ucxl_card_number;
            string req_pos_card_auth_type = "0";

            //#debugc show '#tlog_store_no is  '#tlog_store_no
            //#debugc show '$register_no is  '$register_no

             write_10_record(get_reg, tlog_store_no.ToString(), promo.ID_COUNTRY, tlogwdir, tlogbkup, register_no, tlog_trans_no, tlog_trans_date);
             write_30_record(tlogwdir, tlogbkup, trans_amount_sign, transaction_amount);
             write_47_record(pns_merchant_number.ToString(), bin_range, Convert.ToInt64(tlog_obj_cd0), Convert.ToInt64(tlog_obj_cd1), pg_sg_info.TRANSACTION_TYPE, tlog_acct_no, trans_amount_sign, transaction_amount, pg_sg_info.AUTH_NUMBER, tlogwdir, tlogbkup);
             write_99_record(tlog_acct_no, trans_amount_sign, transaction_amount, tlogwdir, tlogbkup);
            TLOG_HIST tloghist = new TLOG_HIST();
            tloghist.STORE_NO = SPC.STORE_NO;
            tloghist.TRANSACTION_DATE = pg_sg_info.TRANSACTION_DATETIME;
            tloghist.TRANSACTION_NO = tlog_trans_no;
            tloghist.GIFT_CARD_NUMBER = card_number.ToString();
            tloghist.AUTHORIZATION_CODE = pg_sg_info.AUTH_NUMBER;
            tloghist.REGISTER_NO = register_no;
            tloghist.POS_CARD_AUTH_TYPE = req_pos_card_auth_type;
            tloghist.POS_GIFT_CARD_FLAG = 6;
            tloghist.GROSS_LINE_AMOUNT = pg_sg_info.TRANSACTION_AMOUNT;
            tloghist.REMAING_CARD_BAL = "0";
            tloghist.LINE_OBJECT_TYPE = tlog_obj_cd1;
            tloghist.PROMO_CODE = promo.PROMO_CODE;
            tloghist.PROMO_CODE_SUB = tlog_promo_cd_sub;
            tloghist.DA_TIMESTMP_CRE = DateTime.Now;
             create_tlog_history(tloghist);
             update_trans_no(tlog_store_no, Convert.ToInt64(register_no), Convert.ToInt64(tlog_trans_no));
             return;
        }

        /// <summary>
        /// This is called from the create_tlog procedure. 
        /// </summary>
        public string card_lookup(Int64 card_number)
        {
            string  card_no_found = "N";
            USA_CANADA_XREF_LOOKUP UCXL = db.USA_CANADA_XREF_LOOKUPs.FirstOrDefault(i => i.PAYMENTTECH == Convert.ToString(card_number));
            //to_char(UCXL.VALUELINK)        &ucxl.card_number
            //move &ucxl.card_number      to $ucxl_card_number
            card_no_found = "Y";
            string ucxl_card_number = UCXL.PAYMENTTECH;
            if(card_no_found == "N")
            {
               ucxl_card_number = card_number.ToString();
            }
            return ucxl_card_number;
        }

        /// <summary>
        ///  This is called from the create_tlog procedure.
        /// </summary>
        public string get_trans_no(Int64 tlog_store_no, Int64 register_no)
        {
            string trans_no_found = "N";

            STORE_TRANS_NO STN = db.STORE_TRANS_NOs.FirstOrDefault(i => i.STORE_NO == tlog_store_no && i.REGISTER_ID == register_no);
            Int64 stn_trans_no = Convert.ToInt64(STN.TRANSACTION_NO);
            if(stn_trans_no == 9999)
            {
                stn_trans_no = 0;
            }
            trans_no_found = "Y";

            if(trans_no_found == "N")
            {
                create_trans_no(tlog_store_no, register_no);
            }
            return trans_no_found + "," + stn_trans_no;
        }

        /// <summary>
        /// This is called from the get_trans_no procedure. 
        /// </summary>
        public void create_trans_no(Int64 tlog_store_no, Int64 register_no)
        {
            STORE_TRANS_NO storetrans = new STORE_TRANS_NO();
            storetrans.STORE_NO = tlog_store_no;
            storetrans.REGISTER_ID = register_no;
            storetrans.TRANSACTION_NO = 0;
            db.STORE_TRANS_NOs.InsertOnSubmit(storetrans);
            db.SubmitChanges();
            return;
        }

        /// <summary>
        /// This is called from the Process_Input procedure.
        /// </summary>
        /// <summary>
        /// get_register 
        /// This is called from the Process_Input procedure.
        /// </summary>
        public STORE_TRANS_NO get_register(Int64 tlog_store_no, string currentdate0)
        {
            STORE_TRANS_NO REG = db.STORE_TRANS_NOs.FirstOrDefault(i => i.STORE_NO == tlog_store_no && i.DA_TIMESTMP_MOD != currentdate0);
            return REG;
        }

        /// <summary>
        /// This procedure writes the TLOG Header Record 
        /// </summary>
        public void write_10_record(STORE_TRANS_NO transno, string tlog_store_no, string promo_id_country, string tlogdir, string backupdir, string register_no, Int64 tlog_trans_no, string tlog_trans_date)
        {

            var trans_code      = "10";
            var trans_qualifier = "00";
            var trans_type      = "00";
            var flags           = "00200";
            var prefix_store_no = "0";
            var cashier         = "000000000";
            var record_count    = "04";
            var time            = "1201";
            var orig_store_no   = "000000";
            var orig_register   = "00";
            var orig_trans      = "00000";
            var orig_date       = "000000";
            var reason          = "00";
            Int64 country = 0;
            if(promo_id_country == "USA")
            {
                 country = 30;
            }
            if(promo_id_country == "CAN")
            {
                 country = 31;
            }
            if(promo_id_country == "GBR")
            {
                 country = 32;
            }


            if(tlog_store_no == "06999")
            {
                 country = 32;
            }

            if(tlog_store_no == "06500") 
            {
                 country = 30;
            }

            //if(create_act_uk = "T")
            //{
            //    var country = 32;
            //}

            File.WriteAllText(tlogdir, trans_code + trans_qualifier + trans_type + flags + prefix_store_no + tlog_store_no + register_no + tlog_trans_no + cashier + record_count + tlog_trans_date + time + orig_store_no + orig_register + orig_trans + orig_date + reason + country);

            File.WriteAllText(backupdir, trans_code + trans_qualifier + trans_type + flags + prefix_store_no + tlog_store_no + register_no + tlog_trans_no + cashier + record_count + tlog_trans_date + time + orig_store_no + orig_register + orig_trans + orig_date + reason + country);


            var file_created = 'Y';
            return;
        }

                
        public void write_30_record(string path1, string path2, string trans_amt_sign, string transaction_amt)
        {
            var trans_code      = "30";
            var trans_type      = "20";
            var flags           = "00028";
            var item            = "000000000001";
            var dept            = "00032";
            var jrnl_key        = "00";
            var for_qty         = "00001";
            var quantity        = "00001";
            var reason          = "00";
            var return_reason   = "00";
            var orig_price      = "000000000";
            var orig_for_qty    = "00";

            File.WriteAllText(path1, trans_code + trans_type + flags + item + dept + jrnl_key + for_qty + reason + return_reason + orig_price + trans_amt_sign + transaction_amt + trans_amt_sign + transaction_amt + orig_for_qty);
            File.WriteAllText(path2, trans_code + trans_type + flags + item + dept + jrnl_key + for_qty + reason + return_reason + orig_price + trans_amt_sign + transaction_amt + trans_amt_sign + transaction_amt + orig_for_qty);
            return;
        }

        /// <summary>
        /// write_47_record  
        /// This procedure writes the Gift card Record 
        /// </summary>
        public void write_47_record(string pns_merchant_number, string bin_range, Int64 tlog_obj_cd0, Int64 tlog_obj_cd1, string transaction_type, string tlog_acct_no, string trans_amt_sign, string transaction_amt, string tlog_authcode, string tlogdir, string backupdir)
        {
            string trans_code = "47";
            string flags = "00006";
            Int64 req_pos_card_auth_type = 8;
            string trans_type  = "00";
            Int64 tlog_obj_cd = tlog_obj_cd0;
            if(bin_range == "3303" || bin_range == "3302")
            {
                if(transaction_type.ToUpper() == "REDEMPTION")
                {
                 req_pos_card_auth_type = 9;
                 trans_type  = "02";
                 tlog_obj_cd = 77516;
                }
            }
            if(pns_merchant_number == "800000025247" && transaction_type.ToUpper() == "REDEMPTION")
            {
             req_pos_card_auth_type = 9;
             trans_type  = "02";
             tlog_obj_cd = 70816;
            }
            if(inactivity_charge_tlog == "Y")
            {
                if(transaction_type.ToUpper() == "REV_INACTIVITY CHARGE")
                {
                 req_pos_card_auth_type = 9;
                 trans_type  = "07";
                 tlog_obj_cd = 20312;
                }
                else
                {
                 req_pos_card_auth_type = 9;
                 trans_type  = "02";
                 tlog_obj_cd = 20311;
                }
            }
            if(transaction_type == "ACCOUNT EXPIRATION")
            {
             req_pos_card_auth_type = 9;
             trans_type  = "02";

             tlog_obj_cd = tlog_obj_cd1;
            }
            if(transaction_type.ToUpper() == "REV_BLOCK ACTIVATION TRANSACTION")
            {
             req_pos_card_auth_type = 9;
             trans_type  = "02";
             tlog_obj_cd = tlog_obj_cd1;
            }

            if(consig_tlog == "T" && create_act_uk != "T")
            {
                 req_pos_card_auth_type = 9;
                 trans_type  = "02";
                 tlog_obj_cd = 73816;
            }
            if (consig_tlog == "T" && create_act_uk == "T")
            {
             req_pos_card_auth_type = 8;
             trans_type  = "00";
             tlog_obj_cd = 80815;
            }

            //move #tlog_obj_cd to $tlog_obj_cd 099999

            var balance_sign = '+';
            var remain_balance = 0;

            //move #remain_balance to $remain_balance 099999999

            File.WriteAllText(tlogdir, trans_code + trans_type + flags + tlog_acct_no + trans_amt_sign + transaction_amt + balance_sign + remain_balance + tlog_authcode + tlog_obj_cd);
            File.WriteAllText(backupdir, trans_code + trans_type + flags + tlog_acct_no + trans_amt_sign + transaction_amt + balance_sign + remain_balance + tlog_authcode + tlog_obj_cd);

            return;
        }

        /// <summary>
        /// write_99_record
        /// This procedure writes the Gift card Record
        /// </summary>
        public void write_99_record(string tlog_acct_no, string trans_amt_sign, string transaction_amt, string tlogwdir, string tlogbackup)
        {

            var trans_code        = "99";
            var trans_type        = "00";
            var flags             = "00000";
            var foreign_curr      = "000000000";
            var usa_cash          = "000000000";
            var taxes             = "000000000";
            var sales_person      = "000000000";
            var orig_sales_person = "000000000";

            File.WriteAllText(tlogwdir, trans_code + trans_type + flags + tlog_acct_no + trans_amt_sign + transaction_amt + foreign_curr + usa_cash + taxes + trans_amt_sign + transaction_amt + sales_person + orig_sales_person);
            File.WriteAllText(tlogbackup, trans_code + trans_type + flags + tlog_acct_no + trans_amt_sign + transaction_amt + foreign_curr + usa_cash + taxes + trans_amt_sign + transaction_amt + sales_person + orig_sales_person);


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


        public void update_trans_no(Int64 tlog_store_no, Int64 register_no, Int64 tlog_trans_no)
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

            //TLOG_HIST TLOGHIST = db.TLOG_HISTs.FirstOrDefault(i => i.DA_TIMESTMP_CRE == DateTime.Now).OrderByAscending(i => i.STORE_NO && i.REGISTER_NO);

            //var TLOGHIST_STORE_NO = TLOGHIST.STORE_NO;
            //var TLOGHIST_REGISTER_NO = TLOGHIST.REGISTER_NO;

            //update_register();
            return;
        }

        /// <summary>
        /// update_register 
        /// The Procedure which Updates the STORE_TRANS_NO Record.
        /// </summary>
        /// <param name="tloghist_store_no"></param>
        /// <param name="tloghist_register_no"></param>
        public void update_register(Int64 tloghist_store_no, Int64 tloghist_register_no)
        {
            //#ifdef debuga
            // #debugb  show 'FLOW: update_register'
            //#endif
            STORE_TRANS_NO storetrans = db.STORE_TRANS_NOs.FirstOrDefault(i => i.STORE_NO == tloghist_store_no && i.REGISTER_ID == tloghist_register_no);
            storetrans.DA_TIMESTMP_MOD = DateTime.Now.ToString();
            db.SubmitChanges();

            return;
        }

        /// <summary>
        /// update_register1
        /// The Procedure which Updates the STORE_TRANS_NO Record.
        /// </summary>
        /// <param name="tlog_store_no"></param>
        /// <param name="register_no"></param>
        public void update_register1(Int64 tlog_store_no, string register_no)
        {
            //#ifdef debuga
            // #debugb  show 'FLOW: update_register1'
            //#endif
            STORE_TRANS_NO storetrans = db.STORE_TRANS_NOs.FirstOrDefault(i => i.STORE_NO == tlog_store_no && i.REGISTER_ID == Convert.ToInt64(register_no));
            storetrans.DA_TIMESTMP_MOD = DateTime.Now.ToString();
            db.SubmitChanges();

            return;
        }

        /// <summary>
        /// Insert_GC_Inv_Temp 
        /// This is called from the Process_OLTP_Record procedure.
        /// </summary>
        /// <param name="data"></param>
        public void  Insert_GC_Inv_Temp(string VL_PROMOTION_CODE, string STORE_NO)
        {
            string Sql_Msg = "Insert_GC_Inv_Temp    PROBLEM";
            string RJP_WORK = "Insert_GC_Inv_Temp         ";
            //#debuga display ' '
            //#debuga display $RJP_WORK
            //#debuga display ' '

            var inv_trans_date = DateTime.Now;

            //#debuga show '$i1_promotion_code        is ' $i1_promotion_code
            //#debugb show '$store_no                 is ' $store_no
            //#debuga show '$transaction_datetime     is ' $transaction_datetime
            //#debuga show '$ws_timestmp_cre          is ' $ws_timestmp_cre
            GIFT_CARD_INV_PR10_TEMP giftcard = new GIFT_CARD_INV_PR10_TEMP();
            giftcard.VL_PROMOTION_CODE = VL_PROMOTION_CODE;
            giftcard.STORE_NO = Convert.ToInt64(STORE_NO);
            giftcard.TRANSACTION_DATE = DateTime.Now;
            giftcard.TRANS_POST_DATE = DateTime.Now;
            giftcard.CARDS_USED_QTY = 1;
            db.GIFT_CARD_INV_PR10_TEMPs.InsertOnSubmit(giftcard);
            db.SubmitChanges();

            RJP_WORK = "Insert_GC_Inv_Temp   OK     ";
            //#debuga display ' '
            //#debuga display $RJP_WORK
            //#debuga display ' '
            return;
        }

        /// <summary>
        ///  This is called from the Begin_Report procedure.
        /// </summary>
        public void Process_Gift_Card_Tbl(string VL_PROMOTION_CODE, string STORE_NO, string TRANSACTION_DATE, string TRANS_POST_DATE, string CARDS_USED_QTY)
        {
            //GIFT_CARD_INV_PR10_TEMP  PR10GCI = db.GIFT_CARD_INV_PR10_TEMPs.Groupby(i => i.VL_PROMOTION_CODE, i.STORE_NO, i.TRANSACTION_DATE, i.TRANS_POST_DATE);

            string pr10gci_vl_promotion_code = VL_PROMOTION_CODE;
            string pr10gci_store_no = STORE_NO;
            string pr10gci_transaction_date = TRANSACTION_DATE;
            string pr10gci_trans_post_date = TRANS_POST_DATE;
            string pr10gci_cards_used_qty = CARDS_USED_QTY;

            string Sql_Msg = "Insert_Gift_Card_Rec   PROBLEM";
            string RJP_WORK = "Insert_Gift_Card_Rec        ";
            //#debugb display ' '
            //#debugb display $RJP_WORK
            //#debugb display ' '

            GIFT_CARD_INV_PR10_TEMP gftcard = new GIFT_CARD_INV_PR10_TEMP();

            gftcard.VL_PROMOTION_CODE = VL_PROMOTION_CODE;
            gftcard.STORE_NO = Convert.ToInt64(STORE_NO);
            gftcard.TRANSACTION_DATE = Convert.ToDateTime(TRANSACTION_DATE);
            gftcard.TRANS_POST_DATE = Convert.ToDateTime(TRANS_POST_DATE);
            gftcard.CARDS_USED_QTY = Convert.ToInt64(CARDS_USED_QTY);
            db.GIFT_CARD_INV_PR10_TEMPs.InsertOnSubmit(gftcard);
            db.SubmitChanges();
            RJP_WORK = "Insert_Gift_Card_Rec    OK     ";
            //#debugb display ' '
            //#debugb display $RJP_WORK
            //#debugb display ' '

            return;
        }



        /// <summary>
        /// Open_tlog_Files  
        /// This is called from the create_tlog procedure. 
        /// </summary>
        public string Open_tlog_Files()
        {
            var ok = false;
            var tlogwdir = TLOGWDIR + "tlog.txt";
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
                else
                {
                    ok = true;
                }
            }

            var tlogbkup = TLOGBKUP + "tlog" + ws_currdttm + ".txt";
            //open $tlogbkup as 60 for_writing record=100:vary status=#filestat

            if (!File.Exists(tlogbkup))
            {
                File.Create(TLOGBKUP + "tlog" + ws_currdttm + ".txt").Close(); ;
            }
            Thread.Sleep(5000);
            var backupok = false;
            //open $tlogwdir as 20 for_writing record=100:vary status=#filestat
            count = 0;
            while (backupok != true)
            {
                int lineCount = File.ReadAllLines(tlogbkup).Length;
                if (lineCount >= 2000)
                {
                    count++;
                    if (File.Exists(TLOGWDIR + "tlog" + count + ".txt"))
                    {
                        tlogwdir = TLOGBKUP + "tlog" + ws_currdttm + count + ".txt";
                    }
                    else
                    {
                        File.Create(TLOGBKUP + "tlog" + ws_currdttm + count + ".txt").Close();
                    }
                }
                else
                {
                    backupok = true;
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
            string ws_currdttm = ws_timestmp_cre.Substring(1,4) + ws_timestmp_cre.Substring(6,2) + ws_timestmp_cre.Substring(9,2) + ws_timestmp_cre.Substring(11,6);

            return ws_currdttm;
        }

        /// <summary>
        /// reset_flags
        /// The procedure to Reset all the Flags.
        /// </summary>
        public void reset_flags()
        {
            var inactivity_charge_tlog = 'F';
            var promo_cd_cre_tlog      = 'F';
            var sub_promo_cd           = 'F';
            var sub_promo_cd_cre_tlog  = 'F';
            var source_cd_cre_tlog     = 'F';
            var req_cd_cre_tlog        = 'F';
            var tlog_created           = 'F';
            var spc_promo_code_sub     = " ";
            var bin_range_cre_tlog     = 'F';
            var consig_tlog            = 'F';
            var create_act_uk          = 'F';
            return;
        }

        /// <summary>
        /// create_tlog_go_file    
        /// This is called from the Begin_Report procedure.
        /// </summary>
        public void create_tlog_go_file()
        {
            bool ok = false;
            string TlogGoFileName = TLOGWDIR + "pt_tlog.GO";
            //open $TlogGoFileName as 200 for_writing record=100:vary status=#filestat
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
                    if (File.Exists(TLOGWDIR + "pt_tlog" + count + ".GO"))
                    {
                        TlogGoFileName = TLOGWDIR+ "pt_tlog" + count + ".GO";
                    }
                    else
                    {
                        File.Create(TLOGWDIR + "pt_tlog" + count + ".GO");
                    }
                }
                else
                {
                    ok = true;
                }
            }


            string Tlogdummy = "VL_TLOG.GO";

            File.WriteAllText(TlogGoFileName, Tlogdummy + Environment.NewLine);
            return;
        }

        /// <summary>
        /// backup_file
        /// This is called from the Begin_Report procedure. 
        /// </summary>
        public void backup_file(string ptdfile)
        {
            var dos_string1 = "cmd /c copy ";
            var dos_string2 = PTDWDIR + ptdfile + " ";
            char[] MyChar = {'t','x','t','.'};
            var dos_string3 = PTDBKUP + ptdfile.TrimEnd(MyChar);
            var dos_string4 = "." + ws_currdttm + ".txt";
            var dos_string  = dos_string1 + dos_string2 + dos_string3 + dos_string4;
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
            if(process.ExitCode < 32)
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
            string dos_string = "cmd /c del " + PTDWDIR + ptdfile;
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
            if(process.ExitCode >= 32)
            {
                Console.WriteLine(" ");
                Console.WriteLine("***** deleted ****");
                Console.WriteLine(" ");
                Console.WriteLine(ptdfile);
                Console.WriteLine(" ");
            }
            Console.WriteLine("_____________________");
            return;
        }

        /// <summary>
        /// send_email
        /// Mail file creating Procedure
        /// </summary>
        public void send_email(string ExpFileName)
        {

            string mailbody = "Find below the Paymentech Giftcard Activation Exceptions file. ";
            string mail_to = "kim.fedo@spencergifts.com,vickie.britton@spencergifts.com,Viktoryia.Durante@spencergifts.com";
            string cc_to   = "murali.kaithi@spencergifts.com";

            string mailfiletext1 = "_ _body ";
            string mailfiletext2 = mailfiletext1 + '\"';
            string mailfiletext3 = mailfiletext2 + mailbody;
            string mailfiletext4 = mailfiletext3 + '|';
            string mailfiletext5 = mailfiletext4 + ExpFileName;
            string mailfiletext6 = mailfiletext5 + '\"';
            string mailfiletext7 = mailfiletext6 + " _subject ";
            string mailfiletext8 = mailfiletext7 + '\"';
            string mailfiletext9 = mailfiletext8 + "Paymentech Giftcard Activation Exceptions.";
            string mailfiletext10= mailfiletext9 + '\"';
            string mailfiletext11= mailfiletext10 + " _to ";
            string mailfiletext12= mailfiletext11 + mail_to;
            string mailfiletext13= mailfiletext12 + " _cc ";
            string mailfiletext14= mailfiletext13 + cc_to;

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
            process.WaitForExit();
            if(process.ExitCode == 0)
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
        //public void SQL_Error()
        //{
        //    //#ifdef DB2
        //    //when = 6100    !DB2 error for empty_table result set
        //    //break
        //    //#end_if

        //    //#ifdef DB2UNIX
        //    //when = 6100    !DB2 error for empty_table result set
        //    //break
        //    //#end_if

        //    //when = _99999  !Token "when" clause for non_DB2 environments
        //    //when_other
        //    Console.Write(sqr_program);
        //    Console.WriteLine(": ");
        //    Console.Write(ReportID);
        //    Console.WriteLine(" _ SQL Statement = ");
        //    Console.WriteLine(SQL_STATEMENT);
        //    Console.Write("SQL Status =");
        //    Console.Write(sql_status + 99999);
        //    Console.WriteLine(" ");
        //    Console.WriteLine("SQL Error  = ");
        //    Console.WriteLine(sql_error);
        //    Console.WriteLine(Sql_Msg);
        //    Rollback_Transaction();
        //    var sgi_err_msg   = "Stop at SQL Processing";
        //    SGI_Stop_Job();
        //    return;
        //}

        ///// <summary>
        ///// SQL_Error1
        ///// Reports SQL Errors Called by various procedures.
        ///// </summary>
        //public void SQL_Error1()
        //{
        //    if(i1_request_code != "00500")
        //    {
        //        //#ifdef DB2
        //        //when = 6100    !DB2 error for empty_table result set
        //        //break
        //        //#end_if

        //        //#ifdef DB2UNIX
        //        //when = 6100    !DB2 error for empty_table result set
        //        //break
        //        //#end_if

        //        //when = _99999  !Token "when" clause for non_DB2 environments
        //        //when_other
        //        Console.WriteLine(sqr_program);
        //        Console.WriteLine(": ");
        //        Console.Write(ReportID);
        //        Console.WriteLine(" _ SQL Statement = ");
        //        Console.WriteLine(SQL_STATEMENT);
        //        Console.WriteLine("SQL Status =");
        //        Console.WriteLine(sql_status + 99999);
        //        Console.WriteLine(" ");
        //        Console.WriteLine("SQL Error  = ");
        //        Console.WriteLine(sql_error);
        //        Console.WriteLine(Sql_Msg);
        //        Rollback_Transaction();
        //        var sgi_err_msg   = "Stop at SQL Processing";
        //        SGI_Stop_Job();
        //    }
        //    return;
        //}

//!______________________________________________________________________!
//! Called SQC Procedures                                                !
//!______________________________________________________________________!
// #include 'reset.sqc'     ! Reset printer procedure
// #include 'tranctrl.sqc'  ! Tools transaction control module
// #include 'sgerror.sqc'   ! SGI Error Handling procedure
    }
}

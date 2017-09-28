using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Xml;
using System.Threading.Tasks;
using SpencerGifts.Translate.TLog.PSTAudit;
using System.Runtime.Serialization.Formatters.Binary;

namespace SpencerGifts.Translate.TLog.PSTAudit
{
    class Program
    {
        static void Main(string[] args)
        {
            open_main_file();
        }

        //public static string COPYFROM = "\\sgawapp\\Retail_apps\\auditworks\\OUTPUT\\Post\\";
        //public static string COPYTO = "E:\\PostAudit\\";
        //public static string MOVEFROM = "\\sgawapp\\Retail_apps\\auditworks\\OUTPUT\\Post\\";
        //public static string MOVETO = "E:\\PostAudit\\POSTWORK\\";
        //public static string BACKUP = "\\sgawapp\\Retail_apps\\auditworks\\OUTPUT\\Post\\backup\\";
        //public static string WORKDIR = "E:\\PostAudit\\POSTWORK\\";
        //public static string CCDIR = "\\SGAPP\\ReconNet\\uardata\\creditcard\\";
        //public static string CCBKDIR = "\\SGAPP\\ReconNet\\uardata\\creditcard\\backup\\";

        public static string COPYFROM = "F:\\Test\\PSTAudit\\Post\\";
        public static string COPYTO = "F:\\Test\\PSTAudit\\CopyTo\\";
        public static string MOVEFROM = "F:\\Test\\PSTAudit\\Post\\";
        public static string MOVETO = "F:\\Test\\PSTAudit\\PostWork\\";
        public static string BACKUP = "F:\\Test\\PSTAudit\\Backup\\";
        public static string WORKDIR = "F:\\Test\\PSTAudit\\PostWork\\";
        public static string CCDIR = "F:\\Test\\PSTAudit\\creditcard\\";
        public static string CCBKDIR = "F:\\Test\\PSTAudit\\creditcard\\backup\\";
        public static string postauditfile;
        public static Stream stream;
        public static string auth_error_record = "N";

        /// <summary>
        /// File Opening Procedure.
        /// </summary>
        public static void open_main_file()
        {
            Console.WriteLine("open_main_file   X");

            var postauditmainfile = WORKDIR + "POSTAUDIT.DAT";
            //string[] postauditdata;
            if (File.Exists(postauditmainfile))
            {

                var logFile = File.ReadAllLines(postauditmainfile);
                List<string> LogList = new List<string>(logFile);
                foreach (var item in LogList)
                {
                    postauditfile = item;
                    //var record_count = 0;
                    process_main(postauditfile);
                }
            }
            else
            {
                Console.WriteLine("Could not open Post audit Main file ");
                Console.WriteLine(postauditmainfile);
                return;
            }

            //var file_count = Convert.ToString(postauditdata.Count());
            //var total_files_processed = file_count + " file(s) processed.";
            Console.Write(" ");
            Console.WriteLine("End of input.");
            //Console.WriteLine(total_files_processed);
        }

        /// <summary>
        /// This is highest level driving procedure called from Begin-Report.                                   
        /// </summary>
        public static void process_main(string file)
        {
            Console.WriteLine("Begin of the Postaudit Process: " + DateTime.Now);

            Delay_1_Minute();
            Delay_1_Minute();
            copytonew();
            Delay_1_Minute();
            string[] dir = open_file(file).Split(',');

            process_upload(dir[0]);
            analyze_tables();
            load_postaudit();
            string date = DateTime.Now.ToString();
            move_files(dir[1], date);
            Delay_1_Minute();

            //Get_Current_DateTime();
            Console.WriteLine("End of the Postaudit Process: " + DateTime.Now);
            return;
        }

        /// <summary>
        /// Truncates all POST Tables.
        /// </summary>
        public static void truncate_tables()
        {
            datadbDataContext db = new datadbDataContext();
            db.ExecuteCommand("TRUNCATE TABLE POST_HEADER_H_TMP");
            db.ExecuteCommand("TRUNCATE TABLE POST_HEADER_H_TMP");
            db.ExecuteCommand("TRUNCATE TABLE POST_LINE_L_TMP");
            db.ExecuteCommand("TRUNCATE TABLE POST_AUTH_A_TMP");
            db.ExecuteCommand("TRUNCATE TABLE POST_CUST_C_TMP");
            db.ExecuteCommand("TRUNCATE TABLE POST_CUST_EXTRA_E_TMP");
            db.ExecuteCommand("TRUNCATE TABLE POST_DETAIL_D_TMP");
            db.ExecuteCommand("TRUNCATE TABLE POST_NOTES_N_TMP");
            db.ExecuteCommand("TRUNCATE TABLE POST_MERCH_M_TMP");
            db.ExecuteCommand("TRUNCATE TABLE POST_PAYROLL_P_TMP");
            db.ExecuteCommand("TRUNCATE TABLE POST_VOID_V_TMP");
            db.ExecuteCommand("TRUNCATE TABLE POST_RETURN_R_TMP");
            db.ExecuteCommand("TRUNCATE TABLE POST_SPEC_ORD_O_TMP");
            db.ExecuteCommand("TRUNCATE TABLE POST_STOCK_CONTROL_S_TMP");
            db.ExecuteCommand("TRUNCATE TABLE POST_TAX_OVERRIDE_T_TMP");
            db.SubmitChanges();

        }

        /// <summary>
        /// File Opening Procedure.
        /// </summary>
        public static string open_file(string postauditfile)
        {
            var postauditfile1 = COPYTO + postauditfile; //"post_tran_us.txt"
            var postauditbackupfile = MOVEFROM + postauditfile; //"post_tran_us.txt"
            FileStream stream = File.Open(postauditfile1, FileMode.Open);

            if (!File.Exists(postauditfile1) || stream.CanRead == false)
            {
                Console.WriteLine("Could not open Post audit file");
                Console.WriteLine(postauditfile1);
            }
            stream.Close();
            return postauditfile1 + "," + postauditbackupfile;
        }


        /// <summary>
        /// The Main Procedure which does validation of all Records.
        /// </summary>
        public static void process_upload(string file)
        {
            string[] postauditdata = File.ReadAllLines(file);
            int record_count = 0;
            foreach (var item in postauditdata)
            {
                postauditfile = item;
                string sep = "\t";
                //#debuga   show 'sep ' var sep
                record_count++;
                //   console.record_count
                string[] itemdata = item.Split('\t');
                var if_entry_no = itemdata[0];
                var intf_cntl_flag = itemdata[1];
                var record_type = itemdata[2];

                record_type = record_type.Trim();
                Console.WriteLine(record_type);
                if (record_type == "H")
                {
                    insert_trans_hdr(item, sep);
                }

                if (record_type == "L")
                {
                    //if(line_void_flag_ln == 1)      //Added by Murali Kaithi to not load
                    //{
                    insert_trans_ln(item, sep);           //Void lines.
                    //}                          //(Call ID:00006361)
                }
                if (record_type == "M")
                {
                    insert_merchandise_dtl(item, sep);
                }
                if (record_type == "D")
                {
                    insert_disc_dtl(item, sep);
                }
                if (record_type == "R")
                {
                    insert_return_dtl(item, sep);
                }
                if (record_type == "O")
                {
                    insert_sp_ord_dtl(item, sep);
                }
                if (record_type == "P")
                {
                    insert_payroll_dtl(item, sep);
                }
                if (record_type == "S")
                {
                    insert_stock_cntl_dtl(item, sep);
                }
                if (record_type == "T")
                {
                    insert_tax_override_dtl(item, sep);
                }
                if (record_type == "V")
                {
                    insert_post_void_dtl(item, sep);
                }
                if (record_type == "A")
                {
                    insert_auth_dtl(item, sep);
                }
                if (record_type == "C")
                {
                    insert_cust(item, sep);
                }
                if (record_type == "E")
                {
                    insert_cust_dtl(item, sep);
                }
                if (record_type == "N")
                {
                    insert_ln_note(item, sep);
                }
            }

            var total_processed = record_count + " record(s) processed.";
            Console.Write(" ");
            Console.WriteLine("End of input.");
            Console.WriteLine(total_processed);
            return;
        }

        public static void insert_trans_hdr(string loadrecord, string sep)
        {
            string[] loadrecordarray = loadrecord.Split(new string[] { sep }, StringSplitOptions.None);

            var if_entry_no_hdr_raw = loadrecordarray[0];
            var intf_cntl_flag_hdr_raw = loadrecordarray[1];
            var record_type_hdr_raw = loadrecordarray[2];
            var store_no_hdr_raw = loadrecordarray[3];
            var register_no_hdr_raw = loadrecordarray[4];
            var trans_dt_hdr_raw = loadrecordarray[5];
            var entry_dttm_hdr_raw = loadrecordarray[6];
            var trans_series_hdr_raw = loadrecordarray[7];
            var trans_no_hdr_raw = loadrecordarray[8];
            var cashier_no_hdr_raw = loadrecordarray[9];
            var trans_category_hdr_raw = loadrecordarray[10];
            var trans_void_flag_hdr_raw = loadrecordarray[11];
            var emp_no_hdr_raw = loadrecordarray[12];
            var trans_remark_hdr_raw = loadrecordarray[13];
            var updated_by_username_hdr_raw = loadrecordarray[14];
            var company_no_hdr_raw = loadrecordarray[15];

            int if_entry_no_hdr = Convert.ToInt32(if_entry_no_hdr_raw.Trim());
            int intf_cntl_flag_hdr = Convert.ToInt32(intf_cntl_flag_hdr_raw.Trim());
            string record_type_hdr = record_type_hdr_raw.Trim();
            string store_no_hdr = store_no_hdr_raw.Trim().Substring(5, 5);
            int register_no_hdr = Convert.ToInt32(register_no_hdr_raw.Trim());
            DateTime trans_dt_hdr = Convert.ToDateTime(trans_dt_hdr_raw.Trim());
            string entry_dttm_hdr = entry_dttm_hdr_raw.Trim();
            string trans_series_hdr = trans_series_hdr_raw.Trim();
            int trans_no_hdr = Convert.ToInt32(trans_no_hdr_raw.Trim());
            int cashier_no_hdr = Convert.ToInt32(cashier_no_hdr_raw.Trim());
            int trans_category_hdr = Convert.ToInt32(trans_category_hdr_raw.Trim());
            int trans_void_flag_hdr = Convert.ToInt32(trans_void_flag_hdr_raw.Trim());
            int emp_no_hdr = Convert.ToInt32(emp_no_hdr_raw.Trim());
            string trans_remark_hdr = trans_remark_hdr_raw.Trim();
            string updated_by_username_hdr = updated_by_username_hdr_raw.Trim();
            int company_no_hdr = Convert.ToInt32(company_no_hdr_raw.Trim());

            Console.WriteLine("date " + trans_dt_hdr.ToString() + ".");
            datadbDataContext db = new datadbDataContext();
            POST_HEADER_H_TMP data = new POST_HEADER_H_TMP();
            data.IF_ENTRY_NO = if_entry_no_hdr;
            data.INTERFACE_CONTROL_FLAG = intf_cntl_flag_hdr;
            data.RECORD_TYPE = record_type_hdr;
            data.STORE_NO = store_no_hdr;
            data.REGISTER_NO = register_no_hdr;
            data.TRANSACTION_DATE = trans_dt_hdr_raw;
            data.ENTRY_DATE_TIME = Convert.ToDateTime(entry_dttm_hdr);
            data.TRANSACTION_SERIES = trans_series_hdr;
            data.TRANSACTION_NO = trans_no_hdr;
            data.CASHIER_NO = cashier_no_hdr;
            data.TRANSACTION_CATEGORY = trans_category_hdr;
            data.TRANSACTION_VOID_FLAG = trans_void_flag_hdr;
            data.EMPLOYEE_NO = emp_no_hdr;
            data.TRANSACTION_REMARK = trans_remark_hdr;
            data.UPDATED_BY_USER_NAME = updated_by_username_hdr;
            data.COMPANY_NO = company_no_hdr;
            db.POST_HEADER_H_TMPs.InsertOnSubmit(data);
            db.SubmitChanges();

            return;
        }

        public static void insert_trans_ln(string loadrecord, string sep)
        {
            string[] loadrecordarray = loadrecord.Split(new string[] { sep }, StringSplitOptions.None);

            var if_entry_no_ln_raw = loadrecordarray[0];
            var intf_cntl_flag_ln_raw = loadrecordarray[1];
            var record_type_ln_raw = loadrecordarray[2];
            var line_id_ln_raw = loadrecordarray[3];
            var line_object_type_ln_raw = loadrecordarray[4];
            var line_object_ln_raw = loadrecordarray[5];
            var line_action_ln_raw = loadrecordarray[6];
            var ref_no_ln_raw = loadrecordarray[7];
            var gross_line_amt_ln_raw = loadrecordarray[8];
            var pos_disc_amt_ln_raw = loadrecordarray[9];
            var db_cr_none_ln_raw = loadrecordarray[10];
            var ref_type_ln_raw = loadrecordarray[11];
            var void_rev_flag_ln_raw = loadrecordarray[12];
            var line_void_flag_ln_raw = loadrecordarray[13];

            int if_entry_no_ln = Convert.ToInt32(if_entry_no_ln_raw.Trim());
            int intf_cntl_flag_ln = Convert.ToInt32(intf_cntl_flag_ln_raw.Trim());
            string record_type_ln = record_type_ln_raw.Trim();
            int line_id_ln = Convert.ToInt32(line_id_ln_raw.Trim());
            int line_object_type_ln = Convert.ToInt32(line_object_type_ln_raw.Trim());
            int line_object_ln = Convert.ToInt32(line_object_ln_raw.Trim());
            int line_action_ln = Convert.ToInt32(line_action_ln_raw.Trim());
            string ref_no_ln = ref_no_ln_raw.Trim();
            int gross_line_amt_ln = Convert.ToInt32(gross_line_amt_ln_raw) / 100;
            int pos_disc_amt_ln = Convert.ToInt32(pos_disc_amt_ln_raw) / 100;
            int db_cr_none_ln = Convert.ToInt32(db_cr_none_ln_raw.Trim());
            int ref_type_ln = Convert.ToInt32(ref_type_ln_raw.Trim());
            int void_rev_flag_ln = Convert.ToInt32(void_rev_flag_ln_raw.Trim());
            int line_void_flag_ln = Convert.ToInt32(line_void_flag_ln_raw.Trim());

            datadbDataContext db = new datadbDataContext();
            POST_LINE_L_TMP data = new POST_LINE_L_TMP();

            data.IF_ENTRY_NO = if_entry_no_ln;
            data.INTERFACE_CONTROL_FLAG = intf_cntl_flag_ln;
            data.RECORD_TYPE = record_type_ln;
            data.LINE_ID = line_id_ln;
            data.LINE_OBJECT_TYPE = line_object_type_ln;
            data.LINE_OBJECT = line_object_ln;
            data.LINE_ACTION = line_action_ln;
            data.REFERENCE_NO = ref_no_ln;
            data.GROSS_LINE_AMOUNT = gross_line_amt_ln;
            data.POS_DISCOUNT_AMOUNT = pos_disc_amt_ln;
            data.DB_CR_NONE = db_cr_none_ln;
            data.REFERENCE_TYPE = ref_type_ln;
            data.VOIDING_REVERSAL_FLAG = void_rev_flag_ln;
            data.LINE_VOID_FLAG = line_void_flag_ln;
            db.POST_LINE_L_TMPs.InsertOnSubmit(data);
            db.SubmitChanges();
            return;
        }

        public static void insert_merchandise_dtl(string loadrecord, string sep)
        {
            string[] loadrecordarray = loadrecord.Split(new string[] { sep }, StringSplitOptions.None);

            var if_entry_no_md_raw = loadrecordarray[0];
            var intf_cntl_flag_md_raw = loadrecordarray[1];
            var record_type_md_raw = loadrecordarray[2];
            var line_id_md_raw = loadrecordarray[3];
            var merch_catg_md_raw = loadrecordarray[4];
            var upc_lu_div_md_raw = loadrecordarray[5];
            var upc_no_md_raw = loadrecordarray[6];
            var sku_id_md_raw = loadrecordarray[7];
            var style_ref_id_md_raw = loadrecordarray[8];
            var class_cd_md_raw = loadrecordarray[9];
            var subclass_cd_md_raw = loadrecordarray[10];
            var pos_iden_type_md_raw = loadrecordarray[11];
            var pos_iden_md_raw = loadrecordarray[12];
            var pos_deptclass_md_raw = loadrecordarray[13];
            var units_md_raw = loadrecordarray[14];
            var sp_md_raw = loadrecordarray[15];
            var sp2_md_raw = loadrecordarray[16];
            var tick_pr_md_raw = loadrecordarray[17];
            var sold_at_pr_md_raw = loadrecordarray[18];
            var pr_or_md_raw = loadrecordarray[19];
            var upc_miss_md_raw = loadrecordarray[20];
            var scan_md_raw = loadrecordarray[21];


            int if_entry_no_md = Convert.ToInt32(if_entry_no_md_raw.Trim());
            int intf_cntl_flag_md = Convert.ToInt32(intf_cntl_flag_md_raw.Trim());
            string record_type_md = record_type_md_raw.Trim();
            int line_id_md = Convert.ToInt32(line_id_md_raw.Trim());
            int merch_catg_md = Convert.ToInt32(merch_catg_md_raw.Trim());
            int upc_lu_div_md = Convert.ToInt32(upc_lu_div_md_raw.Trim());
            int upc_no_md = Convert.ToInt32(upc_no_md_raw.Trim());
            int sku_id_md = Convert.ToInt32(sku_id_md_raw.Trim());
            int style_ref_id_md = Convert.ToInt32(style_ref_id_md_raw.Trim());
            int class_cd_md = Convert.ToInt32(class_cd_md_raw.Trim());
            int subclass_cd_md = Convert.ToInt32(subclass_cd_md_raw.Trim());
            int pos_iden_type_md = Convert.ToInt32(pos_iden_type_md_raw.Trim());
            int pos_iden_md = Convert.ToInt32(pos_iden_md_raw.Trim());
            int pos_deptclass_md = Convert.ToInt32(pos_deptclass_md_raw.Trim());
            int units_md = Convert.ToInt32(units_md_raw.Trim());
            int sp_md = Convert.ToInt32(sp_md_raw.Trim());
            int sp2_md = Convert.ToInt32(sp2_md_raw.Trim());
            int tick_pr_md = Convert.ToInt32(tick_pr_md_raw.Trim()) / 100;
            int sold_at_pr_md = Convert.ToInt32(sold_at_pr_md_raw.Trim()) / 100;
            int pr_or_md = Convert.ToInt32(pr_or_md_raw.Trim());
            int upc_miss_md = Convert.ToInt32(upc_miss_md_raw.Trim());
            int scan_md = Convert.ToInt32(scan_md_raw.Trim());

            datadbDataContext db = new datadbDataContext();
            POST_MERCH_M_TMP data = new POST_MERCH_M_TMP();
            data.IF_ENTRY_NO = if_entry_no_md;
            data.INTERFACE_CONTROL_FLAG = intf_cntl_flag_md;
            data.RECORD_TYPE = record_type_md;
            data.LINE_ID = line_id_md;
            data.MERCHANDISE_CATEGORY = merch_catg_md;
            data.UPC_LOOKUP_DIVISION = upc_lu_div_md;
            data.UPC_NO = upc_no_md;
            data.SKU_ID = sku_id_md;
            data.STYLE_REFERENCE_ID = style_ref_id_md;
            data.CLASS_CODE = class_cd_md;
            data.SUBCLASS_CODE = subclass_cd_md;
            data.POS_IDENTIFIER_TYPE = pos_iden_type_md;
            data.POS_IDENTIFIER = pos_iden_md;
            data.POS_DEPTCLASS = pos_deptclass_md;
            data.UNITS = units_md;
            data.SALESPERSON = sp_md;
            data.SALESPERSON2 = sp2_md;
            data.TICKET_PRICE = tick_pr_md;
            data.SOLD_AT_PRICE = sold_at_pr_md;
            data.PRICE_OVERRIDE = pr_or_md;
            data.POS_IPLU_MISSING = upc_miss_md;
            data.SCANNED = scan_md;
            db.POST_MERCH_M_TMPs.InsertOnSubmit(data);
            db.SubmitChanges();
            return;
        }

        public static void insert_disc_dtl(string loadrecord, string sep)
        {
            string[] loadrecordarray = loadrecord.Split(new string[] { sep }, StringSplitOptions.None);

            var if_entry_no_dd_raw = loadrecordarray[0];
            var intf_cntl_flag_dd_raw = loadrecordarray[1];
            var record_type_dd_raw = loadrecordarray[2];
            var line_id_dd_raw = loadrecordarray[3];
            var app_by_ln_id_dd_raw = loadrecordarray[4];
            var pos_disc_level_dd_raw = loadrecordarray[5];
            var pos_disc_type_dd_raw = loadrecordarray[6];
            var pos_disc_amt_dd_raw = loadrecordarray[7];
            var app_flag_dd_raw = loadrecordarray[8];
            var pos_disc_ser_no_dd_raw = loadrecordarray[9];

            int if_entry_no_dd = Convert.ToInt32(if_entry_no_dd_raw.Trim());
            int intf_cntl_flag_dd = Convert.ToInt32(intf_cntl_flag_dd_raw.Trim());
            string record_type_dd = record_type_dd_raw.Trim();
            int line_id_dd = Convert.ToInt32(line_id_dd_raw.Trim());
            int app_by_ln_id_dd = Convert.ToInt32(app_by_ln_id_dd_raw.Trim());
            int pos_disc_level_dd = Convert.ToInt32(pos_disc_level_dd_raw.Trim());
            int pos_disc_type_dd = Convert.ToInt32(pos_disc_type_dd_raw.Trim());
            int pos_disc_amt_dd = Convert.ToInt32(pos_disc_amt_dd_raw.Trim()) / 100;
            int app_flag_dd = Convert.ToInt32(app_flag_dd_raw.Trim());
            string pos_disc_ser_no_dd = pos_disc_ser_no_dd_raw.Trim();

            datadbDataContext db = new datadbDataContext();
            POST_DETAIL_D_TMP data = new POST_DETAIL_D_TMP();
            data.IF_ENTRY_NO = if_entry_no_dd;
            data.INTERFACE_CONTROL_FLAG = intf_cntl_flag_dd;
            data.RECORD_TYPE = record_type_dd;
            data.LINE_ID = line_id_dd;
            data.APPLIED_BY_LINE_ID = app_by_ln_id_dd;
            data.POS_DISCOUNT_LEVEL = pos_disc_level_dd;
            data.POS_DISCOUNT_TYPE = pos_disc_type_dd;
            data.POS_DISCOUNT_AMOUNT = pos_disc_amt_dd;
            data.APPLIED_FLAG = app_flag_dd;
            data.POS_DISCOUNT_SERIAL_NO = pos_disc_ser_no_dd;
            db.POST_DETAIL_D_TMPs.InsertOnSubmit(data);
            db.SubmitChanges();
            return;
        }

        public static void insert_return_dtl(string loadrecord, string sep)
        {
            string[] loadrecordarray = loadrecord.Split(new string[] { sep }, StringSplitOptions.None);
            var if_entry_no_rd_raw = loadrecordarray[0];
            var intf_cntl_flag_rd_raw = loadrecordarray[1];
            var record_type_rd_raw = loadrecordarray[2];
            var line_id_rd_raw = loadrecordarray[3];
            var return_reason_msg_rd_raw = loadrecordarray[4];
            var return_reason_cd_rd_raw = loadrecordarray[5];
            var mdse_disp_cd_rd_raw = loadrecordarray[6];
            var via_wh_rd_raw = loadrecordarray[7];
            var orig_sp_rd_raw = loadrecordarray[8];
            var orig_sp2_rd_raw = loadrecordarray[9];
            var return_from_store_rd_raw = loadrecordarray[10];
            var return_from_reg_rd_raw = loadrecordarray[11];
            var return_from_dt_rd_raw = loadrecordarray[12];
            var return_from_trans_no_rd_raw = loadrecordarray[13];

            int if_entry_no_rd = Convert.ToInt32(if_entry_no_rd_raw.Trim());
            int intf_cntl_flag_rd = Convert.ToInt32(intf_cntl_flag_rd_raw.Trim());
            string record_type_rd = record_type_rd_raw.Trim();
            int line_id_rd = Convert.ToInt32(line_id_rd_raw.Trim());
            string return_reason_msg_rd = return_reason_msg_rd_raw.Trim();
            int return_reason_cd_rd = Convert.ToInt32(return_reason_cd_rd_raw.Trim());
            int mdse_disp_cd_rd = Convert.ToInt32(mdse_disp_cd_rd_raw.Trim());
            int via_wh_rd = Convert.ToInt32(via_wh_rd_raw.Trim());
            int orig_sp_rd = Convert.ToInt32(orig_sp_rd_raw.Trim());
            int orig_sp2_rd = Convert.ToInt32(orig_sp2_rd_raw.Trim());
            string return_from_store_rd = return_from_store_rd_raw.Trim();
            int return_from_reg_rd = Convert.ToInt32(return_from_reg_rd_raw.Trim());
            string return_from_dt_rd = return_from_dt_rd_raw.Trim();
            int return_from_trans_no_rd = Convert.ToInt32(return_from_trans_no_rd_raw.Trim());

            string return_from_date_rd = return_from_dt_rd.ToString() + ".";

            datadbDataContext db = new datadbDataContext();
            POST_RETURN_R_TMP data = new POST_RETURN_R_TMP();
            data.IF_ENTRY_NO = if_entry_no_rd;
            data.INTERFACE_CONTORL_FLAG = intf_cntl_flag_rd;
            data.RECORD_TYPE = record_type_rd;
            data.LINE_ID = line_id_rd;
            data.RETURN_REASON_MESSAGE = return_reason_msg_rd;
            data.RETURN_REASON_CODE = return_reason_cd_rd;
            data.MDSE_DISPOSITION_CODE = mdse_disp_cd_rd;
            data.VIA_WAREHOUSE_FLAG = via_wh_rd;
            data.ORIGINAL_SALESPERSON = orig_sp_rd;
            data.ORIGINAL_SALESPERSON2 = orig_sp2_rd;
            data.RETURN_FROM_STORE = return_from_store_rd;
            data.RETURN_FROM_REG = return_from_reg_rd;
            data.RETURN_FROM_DATE = return_from_date_rd;
            data.RETURN_FROM_TRANSNO = return_from_trans_no_rd;
            db.POST_RETURN_R_TMPs.InsertOnSubmit(data);
            db.SubmitChanges();
            return;
        }

        public static void insert_sp_ord_dtl(string loadrecord, string sep)
        {
            string[] loadrecordarray = loadrecord.Split(new string[] { sep }, StringSplitOptions.None);
            var if_entry_no_so_raw = loadrecordarray[0];
            var intf_cntl_flag_so_raw = loadrecordarray[1];
            var record_type_so_raw = loadrecordarray[2];
            var line_id_so_raw = loadrecordarray[3];
            var units_so_raw = loadrecordarray[4];
            var sp_so_raw = loadrecordarray[5];
            var merch_desc_so_raw = loadrecordarray[6];
            var exp_del_on_so_raw = loadrecordarray[7];
            var col_desc_so_raw = loadrecordarray[8];
            var size_desc_so_raw = loadrecordarray[9];
            var width_desc_so_raw = loadrecordarray[10];
            var vndr_name_so_raw = loadrecordarray[11];
            var vndr_style_desc_so_raw = loadrecordarray[12];
            var spo_class_desc_so_raw = loadrecordarray[13];
            var vndr_no_so_raw = loadrecordarray[14];

            int if_entry_no_so = Convert.ToInt32(if_entry_no_so_raw.Trim());
            int intf_cntl_flag_so = Convert.ToInt32(intf_cntl_flag_so_raw.Trim());
            string record_type_so = record_type_so_raw.Trim();
            int line_id_so = Convert.ToInt32(line_id_so_raw.Trim());
            int units_so = Convert.ToInt32(units_so_raw.Trim());
            int sp_so = Convert.ToInt32(sp_so_raw.Trim());
            string merch_desc_so = merch_desc_so_raw.Trim();
            string exp_del_on_so = exp_del_on_so_raw.Trim();
            string col_desc_so = col_desc_so_raw.Trim();
            string size_desc_so = size_desc_so_raw.Trim();
            string width_desc_so = width_desc_so_raw.Trim();
            string vndr_name_so = vndr_name_so_raw.Trim();
            string vndr_style_desc_so = vndr_style_desc_so_raw.Trim();
            string spo_class_desc_so = spo_class_desc_so_raw.Trim();
            string vndr_no_so = vndr_no_so_raw.Trim();

            datadbDataContext db = new datadbDataContext();
            POST_SPEC_ORD_O_TMP data = new POST_SPEC_ORD_O_TMP();
            data.IF_ENTRY_NO = if_entry_no_so;
            data.INTERFACE_CONTROL_FLAG = intf_cntl_flag_so;
            data.RECORD_TYPE = record_type_so;
            data.LINE_ID = line_id_so;
            data.UNITS = units_so;
            data.SALESPERSON = sp_so;
            data.MERCHANDISE_DESCRIPTION = merch_desc_so;
            data.EXPECTING_DELIVERY_ON = exp_del_on_so;
            data.COLOR_DESCRIPTION = col_desc_so;
            data.SIZE_DESCRIPTION = size_desc_so;
            data.WIDTH_DESCRIPTION = width_desc_so;
            data.VENDOR_NAME = vndr_name_so;
            data.VENDOR_STYLE_DESCRIPTION = vndr_style_desc_so;
            data.SPO_CLASS_DESCRIPTION = spo_class_desc_so;
            data.VENDOR_NO = vndr_no_so;
            db.POST_SPEC_ORD_O_TMPs.InsertOnSubmit(data);
            db.SubmitChanges();
            return;
        }

        public static void insert_payroll_dtl(string loadrecord, string sep)
        {
            string[] loadrecordarray = loadrecord.Split(new string[] { sep }, StringSplitOptions.None);
            var if_entry_no_pd_raw = loadrecordarray[0];
            var intf_cntl_flag_pd_raw = loadrecordarray[1];
            var record_type_pd_raw = loadrecordarray[2];
            var line_id_pd_raw = loadrecordarray[3];
            var emp_no_pd_raw = loadrecordarray[4];
            var pr_dt_pd_raw = loadrecordarray[5];
            var emp_pr_id_pd_raw = loadrecordarray[6];
            var emp_type_pd_raw = loadrecordarray[7];
            var pr_entry_type_pd_raw = loadrecordarray[8];

            int if_entry_no_pd = Convert.ToInt32(if_entry_no_pd_raw.Trim());
            int intf_cntl_flag_pd = Convert.ToInt32(intf_cntl_flag_pd_raw.Trim());
            string record_type_pd = record_type_pd_raw.Trim();
            int line_id_pd = Convert.ToInt32(line_id_pd_raw.Trim());
            int emp_no_pd = Convert.ToInt32(emp_no_pd_raw.Trim());
            string pr_date_pd = pr_dt_pd_raw.Trim();
            string emp_pr_id_pd = emp_pr_id_pd_raw.Trim();
            string emp_type_pd = emp_type_pd_raw.Trim();
            int pr_entry_type_pd = Convert.ToInt32(pr_entry_type_pd_raw.Trim());

            string pr_dt_pd = pr_date_pd.ToString() + ".";

            datadbDataContext db = new datadbDataContext();
            POST_PAYROLL_P_TMP data = new POST_PAYROLL_P_TMP();
            data.IF_ENTRY_NO = if_entry_no_pd;
            data.INTERFACE_CONTROL_FLAG = intf_cntl_flag_pd;
            data.RECORD_TYPE = record_type_pd;
            data.LINE_ID = line_id_pd;
            data.EMPLOYEE_NO = emp_no_pd;
            data.PAYROLL_DATE = pr_dt_pd;
            data.EMPLOYEE_PAYROLL_ID = emp_pr_id_pd;
            data.EMPLOYEE_TYPE = emp_type_pd;
            data.PAYROLL_ENTRY_TYPE = pr_entry_type_pd;
            db.POST_PAYROLL_P_TMPs.InsertOnSubmit(data);
            db.SubmitChanges();
            return;
        }

        public static void insert_stock_cntl_dtl(string loadrecord, string sep)
        {
            string[] loadrecordarray = loadrecord.Split(new string[] { sep }, StringSplitOptions.None);
            var if_entry_no_sc_raw = loadrecordarray[0];
            var intf_cntl_flag_sc_raw = loadrecordarray[1];
            var record_type_sc_raw = loadrecordarray[2];
            var line_id_sc_raw = loadrecordarray[3];
            var upc_no_sc_raw = loadrecordarray[4];
            var merch_key_sc_raw = loadrecordarray[5];
            var ini_by_hostsc_raw = loadrecordarray[6];
            var units_sc_raw = loadrecordarray[7];
            var oth_str_no_sc_raw = loadrecordarray[8];
            var loc_no_sc_raw = loadrecordarray[9];
            var vndr_no_sc_raw = loadrecordarray[10];
            var cnt_dt_sc_raw = loadrecordarray[11];

            int if_entry_no_sc = Convert.ToInt32(if_entry_no_sc_raw);
            int intf_cntl_flag_sc = Convert.ToInt32(intf_cntl_flag_sc_raw);
            string record_type_sc = record_type_sc_raw.Trim();
            int line_id_sc = Convert.ToInt32(line_id_sc_raw);
            int upc_no_sc = Convert.ToInt32(upc_no_sc_raw);
            int merch_key_sc = Convert.ToInt32(merch_key_sc_raw);
            int ini_by_host_sc = Convert.ToInt32(ini_by_hostsc_raw.Trim());
            int units_sc = Convert.ToInt32(units_sc_raw);
            string oth_str_no_sc = oth_str_no_sc_raw.Trim().Substring(6, 5);
            int loc_no_sc = Convert.ToInt32(loc_no_sc_raw);
            string vndr_no_sc = vndr_no_sc_raw.Trim();
            string cnt_dt_sc = cnt_dt_sc_raw.Trim();

            string cnt_date_sc = cnt_dt_sc.ToString() + ".";

            datadbDataContext db = new datadbDataContext();
            POST_STOCK_CONTROL_S_TMP data = new POST_STOCK_CONTROL_S_TMP();
            data.IF_ENTRY_NO = if_entry_no_sc;
            data.INTERFACE_CONTROL_FLAG = intf_cntl_flag_sc;
            data.RECORD_TYPE = record_type_sc;
            data.LINE_ID = line_id_sc;
            data.UPC_NO = upc_no_sc;
            data.MERCHANDISE_KEY = merch_key_sc;
            data.INITIATED_BY_HOST = ini_by_host_sc;
            data.UNITS = units_sc;
            data.OTHER_STORE_NO = Convert.ToInt32(oth_str_no_sc);
            data.LOCATION_NO = loc_no_sc;
            data.VENDOR_NO = vndr_no_sc;
            data.COUNT_DATE = cnt_date_sc;
            db.POST_STOCK_CONTROL_S_TMPs.InsertOnSubmit(data);
            db.SubmitChanges();
            return;
        }

        public static void insert_tax_override_dtl(string loadrecord, string sep)
        {
            string[] loadrecordarray = loadrecord.Split(new string[] { sep }, StringSplitOptions.None);
            var if_entry_no_td_raw = loadrecordarray[0];
            var intf_cntl_flag_td_raw = loadrecordarray[1];
            var record_type_td_raw = loadrecordarray[2];
            var line_id_td_raw = loadrecordarray[3];
            var tax_level_td_raw = loadrecordarray[4];
            var tax_catg_td_raw = loadrecordarray[5];
            var taxable_td_raw = loadrecordarray[6];
            var excp_tax_jur_td_raw = loadrecordarray[7];
            var tax_exep_no_td_raw = loadrecordarray[8];

            int if_entry_no_td = Convert.ToInt32(if_entry_no_td_raw.Trim());
            int intf_cntl_flag_td = Convert.ToInt32(intf_cntl_flag_td_raw.Trim());
            string record_type_td = record_type_td_raw.Trim();
            int line_id_td = Convert.ToInt32(line_id_td_raw.Trim());
            int tax_level_td = Convert.ToInt32(tax_level_td_raw.Trim());
            int tax_catg_td = Convert.ToInt32(tax_catg_td_raw.Trim());
            int taxable_td = Convert.ToInt32(taxable_td_raw.Trim());
            string excp_tax_jur_td = excp_tax_jur_td_raw.Trim();
            string tax_exep_no_td = tax_exep_no_td_raw.Trim();
            datadbDataContext db = new datadbDataContext();
            POST_TAX_OVERRIDE_T_TMP data = new POST_TAX_OVERRIDE_T_TMP();
            data.IF_ENTRY_NO = if_entry_no_td;
            data.INTERFACE_CONTROL_FLAG = intf_cntl_flag_td;
            data.RECORD_TYPE = record_type_td;
            data.LINE_ID = line_id_td;
            data.TAX_LEVEL = tax_level_td;
            data.TAX_CATEGORY = tax_catg_td;
            data.TAXABLE = taxable_td;
            data.EXCEPTION_TAX_JURISDICTION = excp_tax_jur_td;
            data.TAX_EXEMPT_NO = tax_exep_no_td;
            db.POST_TAX_OVERRIDE_T_TMPs.InsertOnSubmit(data);
            db.SubmitChanges();

            return;
        }

        public static void insert_post_void_dtl(string loadrecord, string sep)
        {
            string[] loadrecordarray = loadrecord.Split(new string[] { sep }, StringSplitOptions.None);
            var if_entry_no_pv_raw = loadrecordarray[0];
            var intf_cntl_flag_pv_raw = loadrecordarray[1];
            var record_type_pv_raw = loadrecordarray[2];
            var line_id_pv_raw = loadrecordarray[3];
            var post_void_reg_pv_raw = loadrecordarray[4];
            var post_void_trans_no_pv_raw = loadrecordarray[5];
            var post_void_succ_pv_raw = loadrecordarray[6];
            var post_void_reason_cd_pv_raw = loadrecordarray[7];

            int if_entry_no_pv = Convert.ToInt32(if_entry_no_pv_raw.Trim());
            int intf_cntl_flag_pv = Convert.ToInt32(intf_cntl_flag_pv_raw.Trim());
            string record_type_pv = record_type_pv_raw.Trim();
            int line_id_pv = Convert.ToInt32(line_id_pv_raw.Trim());
            int post_void_reg_pv = Convert.ToInt32(post_void_reg_pv_raw.Trim());
            int post_void_trans_no_pv = Convert.ToInt32(post_void_trans_no_pv_raw.Trim());
            int post_void_succ_pv = Convert.ToInt32(post_void_succ_pv_raw.Trim());
            int post_void_reason_cd_pv = Convert.ToInt32(post_void_reason_cd_pv_raw.Trim());

            datadbDataContext db = new datadbDataContext();
            POST_VOID_V_TMP data = new POST_VOID_V_TMP();
            data.IF_ENTRY_NO = if_entry_no_pv;
            data.INTERFACE_CONTROL_FLAG = intf_cntl_flag_pv;
            data.RECORD_TYPE = record_type_pv;
            data.LINE_ID = line_id_pv;
            data.POST_VOIDED_REGISTER = post_void_reg_pv;
            data.POST_VOIDED_TRANS_NO = post_void_trans_no_pv;
            data.POST_VOID_SUCCESSFUL = post_void_succ_pv;
            data.POST_VOID_REASON_CODE = post_void_reason_cd_pv;
            db.POST_VOID_V_TMPs.InsertOnSubmit(data);
            db.SubmitChanges();
            return;
        }

        public static void insert_auth_dtl(string loadrecord, string sep)
        {
            string[] loadrecordarray = loadrecord.Split(new string[] { sep }, StringSplitOptions.None);
            var if_entry_no_ad_raw = loadrecordarray[0];
            var intf_cntl_flag_ad_raw = loadrecordarray[1];
            var record_type_ad_raw = loadrecordarray[2];
            var line_id_ad_raw = loadrecordarray[3];
            var card_type_ad_raw = loadrecordarray[4];
            var auth_no_ad_raw = loadrecordarray[5];
            var exp_dt_ad_raw = loadrecordarray[6];
            var swipe_ind_ad_raw = loadrecordarray[7];
            var app_msg_ad_raw = loadrecordarray[8];
            var lic_no_ad_raw = loadrecordarray[9];
            var pos_st_cd_ad_raw = loadrecordarray[10];
            var other_id_type_ad_raw = loadrecordarray[11];
            var other_id_ad_raw = loadrecordarray[12];
            var def_bill_dt_ad_raw = loadrecordarray[13];
            var def_bill_plan_ad_raw = loadrecordarray[14];
            var cust_sign_obt_ad_raw = loadrecordarray[15];

            int if_entry_no_ad = Convert.ToInt32(if_entry_no_ad_raw.Trim());
            int intf_cntl_flag_ad = Convert.ToInt32(intf_cntl_flag_ad_raw.Trim());
            string record_type_ad = record_type_ad_raw.Trim();
            int line_id_ad = Convert.ToInt32(line_id_ad_raw.Trim());
            string card_type_ad = card_type_ad_raw.Trim();
            string auth_no_ad = auth_no_ad_raw.Trim();
            int exp_dt_ad = Convert.ToInt32(exp_dt_ad_raw.Trim());
            int swipe_ind_ad = Convert.ToInt32(swipe_ind_ad_raw.Trim());
            string app_msg_ad = app_msg_ad_raw.Trim();
            string lic_no_ad = lic_no_ad_raw.Trim();
            string pos_st_cd_ad = pos_st_cd_ad_raw.Trim();
            int other_id_type_ad = Convert.ToInt32(other_id_type_ad_raw.Trim());
            string other_id_ad = other_id_ad_raw.Trim();
            string def_bill_dt_ad = def_bill_dt_ad_raw.Trim();
            int def_bill_plan_ad = Convert.ToInt32(def_bill_plan_ad_raw.Trim());
            int cust_sign_obt_ad = Convert.ToInt32(cust_sign_obt_ad_raw.Trim());

            string def_bill_date_ad = def_bill_dt_ad.ToString() + ".";


            datadbDataContext db = new datadbDataContext();
            POST_AUTH_A_TMP data = new POST_AUTH_A_TMP();
            data.IF_ENTRY_NO = if_entry_no_ad;
            data.INTERFACE_CONTROL_FLAG = intf_cntl_flag_ad;
            data.RECORD_TYPE = record_type_ad;
            data.LINE_ID = line_id_ad;
            data.CARD_TYPE = card_type_ad;
            data.AUTHORIZATION_NO = auth_no_ad;
            data.EXPIRY_DATE = exp_dt_ad;
            data.SWIPE_INDICATOR = swipe_ind_ad;
            data.APPROVAL_MESSAGE = app_msg_ad;
            data.LICENSE_NO = lic_no_ad;
            data.POS_STATE_CODE = pos_st_cd_ad;
            data.OTHER_ID_TYPE = other_id_type_ad;
            if (other_id_ad == "")
            {
                data.OTHER_ID = 0;
            }
            else
            {
                data.OTHER_ID = Convert.ToInt32(other_id_ad);
            }
            data.DEFERRED_BILLING_DATE = def_bill_date_ad;
            data.DEFERRED_BILLING_PLAN = def_bill_plan_ad;
            data.CUSTOMER_SIGNATURE_OBTAINED = cust_sign_obt_ad;
            db.POST_AUTH_A_TMPs.InsertOnSubmit(data);
            db.SubmitChanges();
            return;
        }

        public static void insert_cust(string loadrecord, string sep)
        {
            string[] loadrecordarray = loadrecord.Split(new string[] { sep }, StringSplitOptions.None);
            var if_entry_no_cd_raw = loadrecordarray[0];
            var intf_cntl_flag_cd_raw = loadrecordarray[1];
            var record_type_cd_raw = loadrecordarray[2];
            var from_line_id_cd_raw = loadrecordarray[3];
            var cust_role_cd_raw = loadrecordarray[4];
            var title_cd_raw = loadrecordarray[5];
            var first_name_cd_raw = loadrecordarray[6];
            var last_name_cd_raw = loadrecordarray[7];
            var addr1_cd_raw = loadrecordarray[8];
            var addr2_cd_raw = loadrecordarray[9];
            var city_cd_raw = loadrecordarray[10];
            var county_cd_raw = loadrecordarray[11];
            var state_cd_raw = loadrecordarray[12];
            var country_cd_raw = loadrecordarray[13];
            var post_cd_raw = loadrecordarray[14];
            var tel_no1_cd_raw = loadrecordarray[15];
            var tel_no2_cd_raw = loadrecordarray[16];
            var cust_no_cd_raw = loadrecordarray[17];

            int if_entry_no_cd = Convert.ToInt32(if_entry_no_cd_raw.Trim());
            int intf_cntl_flag_cd = Convert.ToInt32(intf_cntl_flag_cd_raw.Trim());
            string record_type_cd = record_type_cd_raw.Trim();
            int from_line_id_cd = Convert.ToInt32(from_line_id_cd_raw.Trim());
            int cust_role_cd = Convert.ToInt32(cust_role_cd_raw.Trim());
            string title_cd = title_cd_raw.Trim();
            string first_name_cd = first_name_cd_raw.Trim();
            string last_name_cd = last_name_cd_raw.Trim();
            string addr1_cd = addr1_cd_raw.Trim();
            string addr2_cd = addr2_cd_raw.Trim();
            string city_cd = city_cd_raw.Trim();
            string county_cd = county_cd_raw.Trim();
            string state_cd = state_cd_raw.Trim();
            string country_cd = country_cd_raw.Trim();
            string post_cd_cd = post_cd_raw.Trim();
            int tel_no1_cd = Convert.ToInt32(tel_no1_cd_raw.Trim());
            int tel_no2_cd = Convert.ToInt32(tel_no2_cd_raw.Trim());
            int cust_no_cd = Convert.ToInt32(cust_no_cd_raw.Trim());

            datadbDataContext db = new datadbDataContext();
            POST_CUST_C_TMP data = new POST_CUST_C_TMP();
            data.IF_ENTRY_NO = if_entry_no_cd;
            data.INTRFACE_CONTROL_FLAG = intf_cntl_flag_cd;
            data.RECORD_TYPE = record_type_cd;
            data.FROM_LINE_ID = from_line_id_cd;
            data.CUSTOMER_ROLE = cust_role_cd;
            data.TITLE = title_cd;
            data.FIRST_NAME = first_name_cd;
            data.LAST_NAME = last_name_cd;
            data.ADDRESS_1 = addr1_cd;
            data.ADDRESS_2 = addr2_cd;
            data.CITY = city_cd;
            data.COUNTRY = county_cd;
            data.STATE = state_cd;
            data.COUNTRY = country_cd;
            data.POST_CODE = post_cd_cd;
            data.TELEPHONE_NO1 = tel_no1_cd;
            data.TELEPHONE_NO2 = tel_no2_cd;
            data.CUSTOMER_NO = cust_no_cd;
            db.POST_CUST_C_TMPs.InsertOnSubmit(data);
            db.SubmitChanges();
            return;
        }

        public static void insert_cust_dtl(string loadrecord, string sep)
        {
            string[] loadrecordarray = loadrecord.Split(new string[] { sep }, StringSplitOptions.None);
            var if_entry_no_ec_raw = loadrecordarray[0];
            var intf_cntl_flag_ec_raw = loadrecordarray[1];
            var record_type_ec_raw = loadrecordarray[2];
            var from_line_id_ec_raw = loadrecordarray[3];
            var cust_role_ec_raw = loadrecordarray[4];
            var cust_info_type_ec_raw = loadrecordarray[5];
            var cust_info_ec_raw = loadrecordarray[6];

            int if_entry_no_ec = Convert.ToInt32(if_entry_no_ec_raw.Trim());
            int intf_cntl_flag_ec = Convert.ToInt32(intf_cntl_flag_ec_raw.Trim());
            string record_type_ec = record_type_ec_raw.Trim();
            int from_line_id_ec = Convert.ToInt32(from_line_id_ec_raw.Trim());
            int cust_role_ec = Convert.ToInt32(cust_role_ec_raw.Trim());
            int cust_info_type_ec = Convert.ToInt32(cust_info_type_ec_raw.Trim());
            string cust_info_ec = cust_info_ec_raw.Trim();

            datadbDataContext db = new datadbDataContext();
            POST_CUST_EXTRA_E_TMP data = new POST_CUST_EXTRA_E_TMP();
            data.IF_ENTRY_NO = if_entry_no_ec;
            data.INTERFACE_CONTROL_FLAG = intf_cntl_flag_ec;
            data.RECORD_TYPE = record_type_ec;
            data.FROM_LINE_ID = from_line_id_ec;
            data.CUSTOMER_ROLE = cust_role_ec;
            data.CUSTOMER_INFO_TYPE = cust_info_type_ec;
            data.CUSTOMER_INFO = cust_info_ec;
            db.POST_CUST_EXTRA_E_TMPs.InsertOnSubmit(data);
            db.SubmitChanges();
            return;
        }

        public static void insert_ln_note(string loadrecord, string sep)
        {
            //#ifdef debuga
            //Console.WriteLine("FLOW: define-lnnotes-variables");
            //#endif

            Console.WriteLine("FLOW: define-lnnotes-variables ");
            string[] loadrecordarray = loadrecord.Split(new string[] { sep }, StringSplitOptions.None);
            var if_entry_no_lnt_raw = loadrecordarray[0];
            var intf_cntl_flag_lnt_raw = loadrecordarray[1];
            var record_type_lnt_raw = loadrecordarray[2];
            var line_id_lnt_raw = loadrecordarray[3];
            var line_note_type_lnt_raw = loadrecordarray[4];
            var line_note_lnt_raw = loadrecordarray[5];

            var if_entry_no_lnt = Convert.ToInt32(if_entry_no_lnt_raw.Trim());
            var intf_cntl_flag_lnt = Convert.ToInt32(intf_cntl_flag_lnt_raw.Trim());
            var record_type_lnt = record_type_lnt_raw.Trim();
            var line_id_lnt = Convert.ToInt32(line_id_lnt_raw.Trim());
            var line_note_type_lnt = Convert.ToInt32(line_note_type_lnt_raw.Trim());
            var line_note_lnt = line_note_lnt_raw.Trim();

            if (line_note_lnt == "")
            {
                line_note_lnt = " ";
            }

            //#ifdef debuga
            //show 'FLOW: define-lnnotes-variables  ok '
            //#endif

            Console.WriteLine("FLOW: define-lnnotes-variables  ok ");

            Console.WriteLine("FLOW: insert-ln-note");
            datadbDataContext db = new datadbDataContext();
            POST_NOTES_N_TMP data = new POST_NOTES_N_TMP();
            data.IF_ENTRY_NO = if_entry_no_lnt;
            data.INTERFACE_CONTROL_FLAG = intf_cntl_flag_lnt;
            data.RECORD_TYPE = record_type_lnt;
            data.LINE_ID = line_id_lnt;
            data.NOTE_TYPE = line_note_type_lnt;
            data.LINE_NOTE = line_note_lnt;
            data.DATE_CREATED = DateTime.Now;
            db.POST_NOTES_N_TMPs.InsertOnSubmit(data);
            db.SubmitChanges();

            Console.WriteLine("FLOW: insert-ln-note ok ");
            return;
        }

        /// <summary>
        /// The Main Procedure which Analyzes the TEMP Tables
        /// </summary>
        public static void analyze_tables()
        {
            datadbDataContext db = new datadbDataContext();

            List<POST_AUTH_A_TMP> post_auth_a_tmp = db.POST_AUTH_A_TMPs.ToList();
            List<POST_CUST_C_TMP> post_cust_c_tmp = db.POST_CUST_C_TMPs.ToList();
            List<POST_CUST_EXTRA_E_TMP> post_cust_extra_e_tmp = db.POST_CUST_EXTRA_E_TMPs.ToList();
            List<POST_DETAIL_D_TMP> post_detail_d_tmp = db.POST_DETAIL_D_TMPs.ToList();
            List<POST_HEADER_H_TMP> post_header_h_tmp = db.POST_HEADER_H_TMPs.ToList();
            List<POST_MERCH_M_TMP> post_merch_m_tmp = db.POST_MERCH_M_TMPs.ToList();
            List<POST_NOTES_N_TMP> post_notes_n_tmp = db.POST_NOTES_N_TMPs.ToList();
            List<POST_PAYROLL_P_TMP> post_payroll_p_tmp = db.POST_PAYROLL_P_TMPs.ToList();
            List<POST_RETURN_R_TMP> post_return_r_tmp = db.POST_RETURN_R_TMPs.ToList();
            List<POST_SPEC_ORD_O_TMP> post_spec_ord_o_tmp = db.POST_SPEC_ORD_O_TMPs.ToList();
            List<POST_STOCK_CONTROL_S_TMP> post_stock_control_s_tmp = db.POST_STOCK_CONTROL_S_TMPs.ToList();
            List<POST_TAX_OVERRIDE_T_TMP> post_tax_overide_t_tmp = db.POST_TAX_OVERRIDE_T_TMPs.ToList();
            List<POST_VOID_V_TMP> post_void_v_tmp = db.POST_VOID_V_TMPs.ToList();

            return;
        }

        public static void load_postaudit()
        {
            //#ifdef debuga
            //   show 'FLOW: load-postaudit'
            //#endif

            datadbDataContext db = new datadbDataContext();
            List<POST_HEADER_H_TMP> TRANS_HDR_TMP = db.POST_HEADER_H_TMPs.OrderBy(i => i.IF_ENTRY_NO).ToList();
            foreach (var item in TRANS_HDR_TMP)
            {
                select_line(item);
            }

            return;
        }

        /// <summary>
        /// The Main Procedure which loads all the Post audit Records
        /// </summary>
        /// <param name="post_line_l_tmp"></param>
        public static void select_line(POST_HEADER_H_TMP phht)
        {
            int if_entry_no_hdr = Convert.ToInt32(phht.IF_ENTRY_NO);
            int interface_control_flag_hdr = Convert.ToInt32(phht.INTERFACE_CONTROL_FLAG);
            datadbDataContext db = new datadbDataContext();
            string giftcard_lnexists = "N";
            int entry_no = Convert.ToInt32(phht.IF_ENTRY_NO);
            int icf = Convert.ToInt32(phht.INTERFACE_CONTROL_FLAG);
            POST_LINE_L_TMP post_line_l_tmp = db.POST_LINE_L_TMPs.FirstOrDefault(pllt => pllt.IF_ENTRY_NO == entry_no && pllt.INTERFACE_CONTROL_FLAG == icf);
            //#ifdef debuga
            //show 'FLOW: select-line'
            //#endif

            int if_entry_no_ln = Convert.ToInt32(post_line_l_tmp.IF_ENTRY_NO);
            int interface_control_flag_ln = Convert.ToInt32(post_line_l_tmp.INTERFACE_CONTROL_FLAG);
            string record_type_ln = post_line_l_tmp.RECORD_TYPE;
            int line_id_ln = Convert.ToInt32(post_line_l_tmp.LINE_ID);
            int line_object_type_ln = Convert.ToInt32(post_line_l_tmp.LINE_OBJECT_TYPE);
            int line_object_ln = Convert.ToInt32(post_line_l_tmp.LINE_OBJECT);
            int line_action_ln = Convert.ToInt32(post_line_l_tmp.LINE_ACTION);
            POST_LINE_L_TMP transln_records = new POST_LINE_L_TMP();
            string exists = validate_record(line_object_ln, line_action_ln);
            if (exists == "Y")
            {
                giftcard_lnexists = "Y";
                transln_records = process_transln_records(phht, if_entry_no_hdr, line_id_ln, interface_control_flag_hdr, line_object_ln, line_action_ln);
            }
            POST_LINE_L_TMP cc_transln_records = new POST_LINE_L_TMP();
            string cc_exists = validate_cc_record(line_object_ln, line_action_ln);           //Added Murali Kaithi 07/19/07 to process credit card transactions.
            if (cc_exists == "Y")
            {
                cc_transln_records = process_cc_transln_records(if_entry_no_hdr, line_id_ln, interface_control_flag_hdr, line_object_ln, line_action_ln, phht);
            }

            if (giftcard_lnexists == "Y")
            {
                if (auth_error_record == "N")
                {
                    insert_giftcard_header(phht);
                    process_store_date_control(phht.STORE_NO, phht.TRANSACTION_DATE);
                }
            }
            return;
        }

        public static string validate_record(int line_object_ln, int line_action_ln)
        {
            datadbDataContext db = new datadbDataContext();

            string exists = "N";

            int data = db.OBJECT_ACTION_HDRs.Where(i => i.LINE_OBJECT == line_object_ln && i.LINE_OBJECT == line_action_ln).Count();

            //string in_auto_recon = data.IN_AUTO_RECON;
            if (data != 0)
            {
                exists = "Y";
            }
            return exists;
        }

        public static string validate_cc_record(int line_object_ln, int line_action_ln)
        {
            datadbDataContext db = new datadbDataContext();
            string cc_exists = "N";
            int data = db.OBJECT_ACTION_CC_HDRs.Where(i => i.LINE_OBJECT == line_object_ln && i.LINE_ACTION == line_action_ln).Count();
            if (data != 0)
            {
                cc_exists = "Y";
            }
            return cc_exists;
        }

        public static POST_LINE_L_TMP process_transln_records(POST_HEADER_H_TMP phht, int if_entry_no_hdr_raw, int line_id_ln_raw, int interface_control_flag_hdr_raw, int line_object_ln_raw, int line_action_ln_raw)
        {
            datadbDataContext db = new datadbDataContext();
            POST_LINE_L_TMP TRANS_LINE_TMP = db.POST_LINE_L_TMPs.FirstOrDefault(i => i.IF_ENTRY_NO == if_entry_no_hdr_raw && i.LINE_ID == line_id_ln_raw && i.INTERFACE_CONTROL_FLAG == interface_control_flag_hdr_raw && i.LINE_OBJECT == line_object_ln_raw && i.LINE_ACTION == line_action_ln_raw);

            int if_entry_no_ln = Convert.ToInt32(TRANS_LINE_TMP.IF_ENTRY_NO);
            int interface_control_flag_ln = Convert.ToInt32(TRANS_LINE_TMP.INTERFACE_CONTROL_FLAG);
            var record_type_ln = TRANS_LINE_TMP.RECORD_TYPE;
            int line_id_ln = Convert.ToInt32(TRANS_LINE_TMP.LINE_ID);
            var line_object_type_ln = TRANS_LINE_TMP.LINE_OBJECT_TYPE;
            var line_object_ln = TRANS_LINE_TMP.LINE_OBJECT;
            var line_action_ln = TRANS_LINE_TMP.LINE_ACTION;
            var reference_no_ln = TRANS_LINE_TMP.REFERENCE_NO;
            var gross_line_amount_ln = TRANS_LINE_TMP.GROSS_LINE_AMOUNT;
            var pos_discount_amount_ln = TRANS_LINE_TMP.POS_DISCOUNT_AMOUNT;
            var db_cr_none_ln = TRANS_LINE_TMP.DB_CR_NONE;
            var gross_line_amount_recon = gross_line_amount_ln * db_cr_none_ln;
            var reference_type_ln = TRANS_LINE_TMP.REFERENCE_TYPE;
            var voiding_reversal_flag_ln = TRANS_LINE_TMP.VOIDING_REVERSAL_FLAG;
            var line_void_flag_ln = TRANS_LINE_TMP.LINE_VOID_FLAG;

            process_lnnotedetail_records(phht, if_entry_no_ln, interface_control_flag_ln, line_id_ln, TRANS_LINE_TMP);

            if (auth_error_record == "N")
            {
                insert_giftcardln_record(TRANS_LINE_TMP);
                process_merch_records(if_entry_no_ln, interface_control_flag_ln, line_id_ln);
                process_discdetail_records(if_entry_no_ln, interface_control_flag_ln, line_id_ln);
                process_retdetail_records(if_entry_no_ln, interface_control_flag_ln, line_id_ln);
                process_specialorder_records(if_entry_no_ln, interface_control_flag_ln, line_id_ln);
                process_payrolldetail_records(if_entry_no_ln, interface_control_flag_ln, line_id_ln);
                process_stkcntl_records(if_entry_no_ln, interface_control_flag_ln, line_id_ln);
                process_taxdetail_records(if_entry_no_ln, interface_control_flag_ln, line_id_ln);
                process_postvoiddetail_records(if_entry_no_ln, interface_control_flag_ln, line_id_ln);
                process_authdetail_records(if_entry_no_ln, interface_control_flag_ln, line_id_ln);
                process_custdetail_records(if_entry_no_ln, interface_control_flag_ln, line_id_ln);
                process_expcustdetail_records(if_entry_no_ln, interface_control_flag_ln, line_id_ln);
            }
            return TRANS_LINE_TMP;
        }

        public static POST_LINE_L_TMP process_cc_transln_records(int if_entry_no_hdr, int line_id_ln, int interface_control_flag_hdr, int line_object_ln, int line_action_ln, POST_HEADER_H_TMP hdr)
        {
            //#ifdef debuga
            //show 'FLOW: process-cc-transln-records'
            //#endif
            datadbDataContext db = new datadbDataContext();
            POST_LINE_L_TMP TRANS_LINE_CC_TMP = db.POST_LINE_L_TMPs.FirstOrDefault(i => i.IF_ENTRY_NO == if_entry_no_hdr && i.LINE_ID == line_id_ln && i.INTERFACE_CONTROL_FLAG == interface_control_flag_hdr && i.LINE_OBJECT == line_object_ln && i.LINE_ACTION == line_action_ln);

            var if_entry_no_ln_cc = TRANS_LINE_CC_TMP.IF_ENTRY_NO;
            var interface_control_flag_ln_cc = TRANS_LINE_CC_TMP.INTERFACE_CONTROL_FLAG;
            var record_type_ln_cc = TRANS_LINE_CC_TMP.RECORD_TYPE;
            var line_id_ln_cc = TRANS_LINE_CC_TMP.LINE_ID;
            var line_object_type_ln_cc = TRANS_LINE_CC_TMP.LINE_OBJECT_TYPE;
            var line_object_ln_cc = TRANS_LINE_CC_TMP.LINE_OBJECT;
            var line_action_ln_cc = TRANS_LINE_CC_TMP.LINE_ACTION;
            var reference_no_ln_cc = TRANS_LINE_CC_TMP.REFERENCE_NO;
            var gross_line_amount_ln_cc = TRANS_LINE_CC_TMP.GROSS_LINE_AMOUNT;
            var pos_discount_amount_ln_cc = TRANS_LINE_CC_TMP.POS_DISCOUNT_AMOUNT;
            var db_cr_none_ln_cc = TRANS_LINE_CC_TMP.DB_CR_NONE;
            var gross_line_amount_recon_cc = gross_line_amount_ln_cc * db_cr_none_ln_cc;
            var reference_type_ln_cc = TRANS_LINE_CC_TMP.REFERENCE_TYPE;
            var voiding_reversal_flag_ln_cc = TRANS_LINE_CC_TMP.VOIDING_REVERSAL_FLAG;
            var line_void_flag_ln_cc = TRANS_LINE_CC_TMP.LINE_VOID_FLAG;

            if (Convert.ToInt32(hdr.STORE_NO) < 3000 || Convert.ToInt32(hdr.STORE_NO) > 399) //dont know where this came from
            {
                process_recon_post_cc_audit(hdr, TRANS_LINE_CC_TMP);
            }

            return TRANS_LINE_CC_TMP;
        }

        public static void insert_giftcardln_record(POST_LINE_L_TMP data)
        {
            datadbDataContext db = new datadbDataContext();
            //#debuga show '#if_entry_no_ln             is ' #if_entry_no_ln
            //#debuga show '#interface_control_flag_ln  is ' #interface_control_flag_ln
            //#debuga show '$record_type_ln             is ' $record_type_ln
            //#debuga show '#line_id_ln                 is ' #line_id_ln
            //#debuga show '#line_object_type_ln        is ' #line_object_type_ln
            //#debuga show '#line_object_ln             is ' #line_object_ln
            //#debuga show '#line_action_ln             is ' #line_action_ln
            //#debuga show '$reference_no_ln            is ' $reference_no_ln
            //#debuga show '#gross_line_amount_ln       is ' #gross_line_amount_ln
            //#debuga show '#pos_discount_amount_ln     is ' #pos_discount_amount_ln
            //#debuga show '#db_cr_none_ln              is ' #db_cr_none_ln
            //#debuga show '#reference_type_ln          is ' #reference_type_ln
            //#debuga show '#voiding_reversal_flag_ln   is ' #voiding_reversal_flag_ln
            //#debuga show '#line_void_flag_ln          is ' #line_void_flag_ln

            POST_LINE_L_TMP POST_LINE_L = new POST_LINE_L_TMP();
            POST_LINE_L.IF_ENTRY_NO = data.IF_ENTRY_NO;
            POST_LINE_L.INTERFACE_CONTROL_FLAG = data.INTERFACE_CONTROL_FLAG;
            POST_LINE_L.RECORD_TYPE = data.RECORD_TYPE;
            POST_LINE_L.LINE_ID = data.LINE_ID;
            POST_LINE_L.LINE_OBJECT_TYPE = data.LINE_OBJECT_TYPE;
            POST_LINE_L.LINE_OBJECT = data.LINE_OBJECT;
            POST_LINE_L.LINE_ACTION = data.LINE_ACTION;
            POST_LINE_L.REFERENCE_NO = data.REFERENCE_NO;
            POST_LINE_L.GROSS_LINE_AMOUNT = data.GROSS_LINE_AMOUNT;
            POST_LINE_L.POS_DISCOUNT_AMOUNT = data.POS_DISCOUNT_AMOUNT;
            POST_LINE_L.DB_CR_NONE = data.DB_CR_NONE;
            POST_LINE_L.REFERENCE_TYPE = data.REFERENCE_TYPE;
            POST_LINE_L.VOIDING_REVERSAL_FLAG = data.VOIDING_REVERSAL_FLAG;
            POST_LINE_L.LINE_VOID_FLAG = data.LINE_VOID_FLAG;
            db.POST_LINE_L_TMPs.InsertOnSubmit(POST_LINE_L);
            db.SubmitChanges();
            return;
        }

        public static void process_merch_records(int if_entry_no_ln, int interface_control_flag_ln, int line_id_ln)
        {
            //#ifdef debuga
            //show 'FLOW: process-merch-records'
            //#endif
            datadbDataContext db = new datadbDataContext();
            POST_MERCH_M_TMP TRANS_MERCH_TMP = db.POST_MERCH_M_TMPs.FirstOrDefault(i => i.IF_ENTRY_NO == if_entry_no_ln && i.INTERFACE_CONTROL_FLAG == interface_control_flag_ln && i.LINE_ID == line_id_ln);
            var if_entry_no_md = TRANS_MERCH_TMP.IF_ENTRY_NO;
            var intf_cntl_flag_md = TRANS_MERCH_TMP.INTERFACE_CONTROL_FLAG;
            var record_type_md = TRANS_MERCH_TMP.RECORD_TYPE;
            var line_id_md = TRANS_MERCH_TMP.LINE_ID;
            var merch_catg_md = TRANS_MERCH_TMP.MERCHANDISE_CATEGORY;
            var upc_lu_div_md = TRANS_MERCH_TMP.UPC_LOOKUP_DIVISION;
            var upc_no_md = TRANS_MERCH_TMP.UPC_NO;
            var sku_id_md = TRANS_MERCH_TMP.SKU_ID;
            var style_ref_id_md = TRANS_MERCH_TMP.STYLE_REFERENCE_ID;
            var class_cd_md = TRANS_MERCH_TMP.CLASS_CODE;
            var subclass_cd_md = TRANS_MERCH_TMP.SUBCLASS_CODE;
            var pos_iden_type_md = TRANS_MERCH_TMP.POS_IDENTIFIER_TYPE;
            var pos_iden_md = TRANS_MERCH_TMP.POS_IDENTIFIER;
            var pos_deptclass_md = TRANS_MERCH_TMP.POS_DEPTCLASS;
            var units_md = TRANS_MERCH_TMP.UNITS;
            var sp_md = TRANS_MERCH_TMP.SALESPERSON;
            var sp2_md = TRANS_MERCH_TMP.SALESPERSON2;
            var tick_pr_md = TRANS_MERCH_TMP.TICKET_PRICE;
            var sold_at_pr_md = TRANS_MERCH_TMP.SOLD_AT_PRICE;
            var pr_or_md = TRANS_MERCH_TMP.PRICE_OVERRIDE;
            var upc_miss_md = TRANS_MERCH_TMP.POS_IPLU_MISSING;
            var scan_md = TRANS_MERCH_TMP.SCANNED;

            insert_giftcardmerc_record(TRANS_MERCH_TMP);


            return;
        }

        public static void insert_giftcardmerc_record(POST_MERCH_M_TMP data)
        {
            datadbDataContext db = new datadbDataContext();
            POST_MERCH_M_TMP POST_MERCH_M = new POST_MERCH_M_TMP();
            POST_MERCH_M.IF_ENTRY_NO = data.IF_ENTRY_NO;
            POST_MERCH_M.INTERFACE_CONTROL_FLAG = data.INTERFACE_CONTROL_FLAG;
            POST_MERCH_M.RECORD_TYPE = data.RECORD_TYPE;
            POST_MERCH_M.LINE_ID = data.LINE_ID;
            POST_MERCH_M.MERCHANDISE_CATEGORY = data.MERCHANDISE_CATEGORY;
            POST_MERCH_M.UPC_LOOKUP_DIVISION = data.UPC_LOOKUP_DIVISION;
            POST_MERCH_M.UPC_NO = data.UPC_NO;
            POST_MERCH_M.SKU_ID = data.SKU_ID;
            POST_MERCH_M.STYLE_REFERENCE_ID = data.STYLE_REFERENCE_ID;
            POST_MERCH_M.CLASS_CODE = data.CLASS_CODE;
            POST_MERCH_M.SUBCLASS_CODE = data.SUBCLASS_CODE;
            POST_MERCH_M.POS_IDENTIFIER_TYPE = data.POS_IDENTIFIER_TYPE;
            POST_MERCH_M.POS_IDENTIFIER = data.POS_IDENTIFIER;
            POST_MERCH_M.POS_DEPTCLASS = data.POS_DEPTCLASS;
            POST_MERCH_M.UNITS = data.UNITS;
            POST_MERCH_M.SALESPERSON = data.SALESPERSON;
            POST_MERCH_M.SALESPERSON2 = data.SALESPERSON2;
            POST_MERCH_M.TICKET_PRICE = data.TICKET_PRICE;
            POST_MERCH_M.SOLD_AT_PRICE = data.SOLD_AT_PRICE;
            POST_MERCH_M.PRICE_OVERRIDE = data.PRICE_OVERRIDE;
            POST_MERCH_M.POS_IPLU_MISSING = data.POS_IPLU_MISSING;
            POST_MERCH_M.SCANNED = data.SCANNED;
            db.POST_MERCH_M_TMPs.InsertOnSubmit(POST_MERCH_M);
            db.SubmitChanges();
        }

        public static void process_discdetail_records(int if_entry_no_ln, int interface_control_flag_ln, int line_id_ln)
        {
            //#ifdef debuga
            //show 'FLOW: process-discdetail-records'
            //#endif
            datadbDataContext db = new datadbDataContext();
            POST_DETAIL_D_TMP TRANS_DD_TMP = db.POST_DETAIL_D_TMPs.FirstOrDefault(i => i.IF_ENTRY_NO == if_entry_no_ln && i.INTERFACE_CONTROL_FLAG == interface_control_flag_ln && i.LINE_ID == line_id_ln);

            var if_entry_no_dd = TRANS_DD_TMP.IF_ENTRY_NO;
            var intf_cntl_flag_dd = TRANS_DD_TMP.INTERFACE_CONTROL_FLAG;
            var record_type_dd = TRANS_DD_TMP.RECORD_TYPE;
            var line_id_dd = TRANS_DD_TMP.LINE_ID;
            var app_by_ln_id_dd = TRANS_DD_TMP.APPLIED_BY_LINE_ID;
            var pos_disc_level_dd = TRANS_DD_TMP.POS_DISCOUNT_LEVEL;
            var pos_disc_type_dd = TRANS_DD_TMP.POS_DISCOUNT_TYPE;
            var pos_disc_amt_dd = TRANS_DD_TMP.POS_DISCOUNT_AMOUNT;
            var app_flag_dd = TRANS_DD_TMP.APPLIED_FLAG;
            var pos_disc_ser_no_dd = TRANS_DD_TMP.POS_DISCOUNT_SERIAL_NO;

            insert_giftcarddd_record(TRANS_DD_TMP);

            return;
        }

        public static void insert_giftcarddd_record(POST_DETAIL_D_TMP data)
        {
            datadbDataContext db = new datadbDataContext();
            POST_DETAIL_D_TMP POST_DETAIL_D = new POST_DETAIL_D_TMP();
            POST_DETAIL_D.IF_ENTRY_NO = data.IF_ENTRY_NO;
            POST_DETAIL_D.INTERFACE_CONTROL_FLAG = data.INTERFACE_CONTROL_FLAG;
            POST_DETAIL_D.RECORD_TYPE = data.RECORD_TYPE;
            POST_DETAIL_D.LINE_ID = data.LINE_ID;
            POST_DETAIL_D.APPLIED_BY_LINE_ID = data.APPLIED_BY_LINE_ID;
            POST_DETAIL_D.POS_DISCOUNT_LEVEL = data.POS_DISCOUNT_LEVEL;
            POST_DETAIL_D.POS_DISCOUNT_TYPE = data.POS_DISCOUNT_TYPE;
            POST_DETAIL_D.POS_DISCOUNT_AMOUNT = data.POS_DISCOUNT_AMOUNT;
            POST_DETAIL_D.APPLIED_FLAG = data.APPLIED_FLAG;
            POST_DETAIL_D.POS_DISCOUNT_SERIAL_NO = data.POS_DISCOUNT_SERIAL_NO;
            db.POST_DETAIL_D_TMPs.InsertOnSubmit(POST_DETAIL_D);
            db.SubmitChanges();
            return;
        }

        public static void process_retdetail_records(int if_entry_no_ln, int interface_control_flag_ln, int line_id_ln)
        {
            //#ifdef debuga
            //show 'FLOW: process-retdetail-records'
            //#endif
            datadbDataContext db = new datadbDataContext();
            POST_RETURN_R_TMP TRANS_RD_TMP = db.POST_RETURN_R_TMPs.FirstOrDefault(i => i.IF_ENTRY_NO == if_entry_no_ln && i.INTERFACE_CONTORL_FLAG == interface_control_flag_ln && i.LINE_ID == line_id_ln);

            var if_entry_no_rd = TRANS_RD_TMP.IF_ENTRY_NO;
            var intf_cntl_flag_rd = TRANS_RD_TMP.INTERFACE_CONTORL_FLAG;
            var record_type_rd = TRANS_RD_TMP.RECORD_TYPE;
            var line_id_rd = TRANS_RD_TMP.LINE_ID;
            var return_reason_msg_rd = TRANS_RD_TMP.RETURN_REASON_MESSAGE;
            var return_reason_cd_rd = TRANS_RD_TMP.RETURN_REASON_CODE;
            var mdse_disp_cd_rd = TRANS_RD_TMP.MDSE_DISPOSITION_CODE;
            var via_wh_rd = TRANS_RD_TMP.VIA_WAREHOUSE_FLAG;
            var orig_sp_rd = TRANS_RD_TMP.ORIGINAL_SALESPERSON;
            var orig_sp2_rd = TRANS_RD_TMP.ORIGINAL_SALESPERSON2;
            var return_from_store_rd = TRANS_RD_TMP.RETURN_FROM_STORE;
            var return_from_reg_rd = TRANS_RD_TMP.RETURN_FROM_REG;
            var return_from_dt_rd = TRANS_RD_TMP.RETURN_FROM_DATE;
            var return_from_trans_no_rd = TRANS_RD_TMP.RETURN_FROM_TRANSNO;

            insert_giftcardrd_record(TRANS_RD_TMP);

            return;
        }

        public static void insert_giftcardrd_record(POST_RETURN_R_TMP TRANS_RD_TMP)
        {
            datadbDataContext db = new datadbDataContext();
            POST_RETURN_R_TMP POST_RETURN_R = new POST_RETURN_R_TMP();
            POST_RETURN_R.IF_ENTRY_NO = TRANS_RD_TMP.IF_ENTRY_NO;
            POST_RETURN_R.INTERFACE_CONTORL_FLAG = TRANS_RD_TMP.INTERFACE_CONTORL_FLAG;
            POST_RETURN_R.RECORD_TYPE = TRANS_RD_TMP.RECORD_TYPE;
            POST_RETURN_R.LINE_ID = TRANS_RD_TMP.LINE_ID;
            POST_RETURN_R.RETURN_REASON_MESSAGE = TRANS_RD_TMP.RETURN_REASON_MESSAGE;
            POST_RETURN_R.RETURN_REASON_CODE = TRANS_RD_TMP.RETURN_REASON_CODE;
            POST_RETURN_R.MDSE_DISPOSITION_CODE = TRANS_RD_TMP.MDSE_DISPOSITION_CODE;
            POST_RETURN_R.VIA_WAREHOUSE_FLAG = TRANS_RD_TMP.VIA_WAREHOUSE_FLAG;
            POST_RETURN_R.ORIGINAL_SALESPERSON = TRANS_RD_TMP.ORIGINAL_SALESPERSON;
            POST_RETURN_R.ORIGINAL_SALESPERSON2 = TRANS_RD_TMP.ORIGINAL_SALESPERSON2;
            POST_RETURN_R.RETURN_FROM_STORE = TRANS_RD_TMP.RETURN_FROM_STORE;
            POST_RETURN_R.RETURN_FROM_REG = TRANS_RD_TMP.RETURN_FROM_REG;
            POST_RETURN_R.RETURN_FROM_DATE = TRANS_RD_TMP.RETURN_FROM_DATE;
            POST_RETURN_R.RETURN_FROM_TRANSNO = TRANS_RD_TMP.RETURN_FROM_TRANSNO;
            return;
        }


        public static void process_specialorder_records(int if_entry_no_ln, int interface_control_flag_ln, int line_id_ln)
        {

            //#ifdef debuga
            //show 'FLOW: process-specialorder-records'
            //#endif
            datadbDataContext db = new datadbDataContext();

            POST_SPEC_ORD_O_TMP TRANS_SO_TMP = db.POST_SPEC_ORD_O_TMPs.FirstOrDefault(i => i.IF_ENTRY_NO == if_entry_no_ln && i.INTERFACE_CONTROL_FLAG == interface_control_flag_ln && i.LINE_ID == line_id_ln);

            var if_entry_no_so = TRANS_SO_TMP.IF_ENTRY_NO;
            var intf_cntl_flag_so = TRANS_SO_TMP.INTERFACE_CONTROL_FLAG;
            var record_type_so = TRANS_SO_TMP.RECORD_TYPE;
            var line_id_so = TRANS_SO_TMP.LINE_ID;
            var units_so = TRANS_SO_TMP.UNITS;
            var sp_so = TRANS_SO_TMP.SALESPERSON;
            var merch_desc_so = TRANS_SO_TMP.MERCHANDISE_DESCRIPTION;
            var exp_del_on_so = TRANS_SO_TMP.EXPECTING_DELIVERY_ON;
            var col_desc_so = TRANS_SO_TMP.COLOR_DESCRIPTION;
            var size_desc_so = TRANS_SO_TMP.SIZE_DESCRIPTION;
            var width_desc_so = TRANS_SO_TMP.WIDTH_DESCRIPTION;
            var vndr_name_so = TRANS_SO_TMP.VENDOR_NAME;
            var vndr_style_desc_so = TRANS_SO_TMP.VENDOR_STYLE_DESCRIPTION;
            var spo_class_desc_so = TRANS_SO_TMP.SPO_CLASS_DESCRIPTION;
            var vndr_no_so = TRANS_SO_TMP.VENDOR_NO;


            insert_giftcardso_record(TRANS_SO_TMP);

            return;
        }

        public static void insert_giftcardso_record(POST_SPEC_ORD_O_TMP TRANS_SO_TMP)
        {
            datadbDataContext db = new datadbDataContext();
            POST_SPEC_ORD_O_TMP POST_SPEC_ORD_O = new POST_SPEC_ORD_O_TMP();
            POST_SPEC_ORD_O.IF_ENTRY_NO = TRANS_SO_TMP.IF_ENTRY_NO;
            POST_SPEC_ORD_O.INTERFACE_CONTROL_FLAG = TRANS_SO_TMP.INTERFACE_CONTROL_FLAG;
            POST_SPEC_ORD_O.RECORD_TYPE = TRANS_SO_TMP.RECORD_TYPE;
            POST_SPEC_ORD_O.LINE_ID = TRANS_SO_TMP.LINE_ID;
            POST_SPEC_ORD_O.UNITS = TRANS_SO_TMP.UNITS;
            POST_SPEC_ORD_O.SALESPERSON = TRANS_SO_TMP.SALESPERSON;
            POST_SPEC_ORD_O.MERCHANDISE_DESCRIPTION = TRANS_SO_TMP.MERCHANDISE_DESCRIPTION;
            POST_SPEC_ORD_O.EXPECTING_DELIVERY_ON = TRANS_SO_TMP.EXPECTING_DELIVERY_ON;
            POST_SPEC_ORD_O.COLOR_DESCRIPTION = TRANS_SO_TMP.COLOR_DESCRIPTION;
            POST_SPEC_ORD_O.SIZE_DESCRIPTION = TRANS_SO_TMP.SIZE_DESCRIPTION;
            POST_SPEC_ORD_O.WIDTH_DESCRIPTION = TRANS_SO_TMP.WIDTH_DESCRIPTION;
            POST_SPEC_ORD_O.VENDOR_NAME = TRANS_SO_TMP.VENDOR_NAME;
            POST_SPEC_ORD_O.VENDOR_STYLE_DESCRIPTION = TRANS_SO_TMP.VENDOR_STYLE_DESCRIPTION;
            POST_SPEC_ORD_O.SPO_CLASS_DESCRIPTION = TRANS_SO_TMP.SPO_CLASS_DESCRIPTION;
            POST_SPEC_ORD_O.VENDOR_NO = TRANS_SO_TMP.VENDOR_NO;
            db.POST_SPEC_ORD_O_TMPs.InsertOnSubmit(POST_SPEC_ORD_O);
            db.SubmitChanges();
        }

        public static void process_payrolldetail_records(int if_entry_no_ln_raw, int interface_control_flag_ln_raw, int line_id_ln_raw)
        {
            //#ifdef debuga
            //   show 'FLOW: process-payrolldetail-records'
            //#endif
            datadbDataContext db = new datadbDataContext();
            POST_PAYROLL_P_TMP TRANS_PR_TMP = db.POST_PAYROLL_P_TMPs.FirstOrDefault(i => i.IF_ENTRY_NO == if_entry_no_ln_raw && i.INTERFACE_CONTROL_FLAG == interface_control_flag_ln_raw && i.LINE_ID == line_id_ln_raw);
            var if_entry_no_pd = TRANS_PR_TMP.IF_ENTRY_NO;
            var intf_cntl_flag_pd = TRANS_PR_TMP.INTERFACE_CONTROL_FLAG;
            var record_type_pd = TRANS_PR_TMP.RECORD_TYPE;
            var line_id_pd = TRANS_PR_TMP.LINE_ID;
            var emp_no_pd = TRANS_PR_TMP.EMPLOYEE_NO;
            var pr_date_pd = TRANS_PR_TMP.PAYROLL_DATE;
            var emp_pr_id_pd = TRANS_PR_TMP.EMPLOYEE_PAYROLL_ID;
            var emp_type_pd = TRANS_PR_TMP.EMPLOYEE_TYPE;
            var pr_entry_type_pd = TRANS_PR_TMP.PAYROLL_ENTRY_TYPE;


            insert_giftcardpr_record(TRANS_PR_TMP);

            return;
        }

        public static void insert_giftcardpr_record(POST_PAYROLL_P_TMP data)
        {
            datadbDataContext db = new datadbDataContext();
            POST_PAYROLL_P_TMP POST_PAYROLL_P = new POST_PAYROLL_P_TMP();
            POST_PAYROLL_P.IF_ENTRY_NO = data.IF_ENTRY_NO;
            POST_PAYROLL_P.INTERFACE_CONTROL_FLAG = data.INTERFACE_CONTROL_FLAG;
            POST_PAYROLL_P.RECORD_TYPE = data.RECORD_TYPE;
            POST_PAYROLL_P.LINE_ID = data.LINE_ID;
            POST_PAYROLL_P.EMPLOYEE_NO = data.EMPLOYEE_NO;
            POST_PAYROLL_P.PAYROLL_DATE = data.PAYROLL_DATE;
            POST_PAYROLL_P.EMPLOYEE_PAYROLL_ID = data.EMPLOYEE_PAYROLL_ID;
            POST_PAYROLL_P.EMPLOYEE_TYPE = data.EMPLOYEE_TYPE;
            POST_PAYROLL_P.PAYROLL_ENTRY_TYPE = Convert.ToInt32(data.EMPLOYEE_TYPE);
            db.POST_PAYROLL_P_TMPs.InsertOnSubmit(POST_PAYROLL_P);
            db.SubmitChanges();

            return;
        }

        public static void process_stkcntl_records(int if_entry_no_ln, int interface_control_flag_ln, int line_id_ln)
        {
            datadbDataContext db = new datadbDataContext();
            POST_STOCK_CONTROL_S_TMP TRANS_SC_TMP = db.POST_STOCK_CONTROL_S_TMPs.FirstOrDefault(i => i.IF_ENTRY_NO == if_entry_no_ln && i.INTERFACE_CONTROL_FLAG == interface_control_flag_ln && i.LINE_ID == line_id_ln);

            //#ifdef debuga
            //   show 'FLOW: process-stkcntl-records'
            //#endif

            insert_giftcardsc_record(TRANS_SC_TMP);
            return;
        }

        public static void insert_giftcardsc_record(POST_STOCK_CONTROL_S_TMP data)
        {
            datadbDataContext db = new datadbDataContext();
            POST_STOCK_CONTROL_S_TMP POST_STOCK_CONTROL_S = new POST_STOCK_CONTROL_S_TMP();
            POST_STOCK_CONTROL_S.IF_ENTRY_NO = data.IF_ENTRY_NO;
            POST_STOCK_CONTROL_S.INTERFACE_CONTROL_FLAG = data.INTERFACE_CONTROL_FLAG;
            POST_STOCK_CONTROL_S.RECORD_TYPE = data.RECORD_TYPE;
            POST_STOCK_CONTROL_S.LINE_ID = Convert.ToInt32(data.RECORD_TYPE);
            POST_STOCK_CONTROL_S.UPC_NO = data.UPC_NO;
            POST_STOCK_CONTROL_S.MERCHANDISE_KEY = data.MERCHANDISE_KEY;
            POST_STOCK_CONTROL_S.INITIATED_BY_HOST = data.INITIATED_BY_HOST;
            POST_STOCK_CONTROL_S.UNITS = data.UNITS;
            POST_STOCK_CONTROL_S.OTHER_STORE_NO = data.OTHER_STORE_NO;
            POST_STOCK_CONTROL_S.LOCATION_NO = data.LOCATION_NO;
            POST_STOCK_CONTROL_S.VENDOR_NO = data.VENDOR_NO;
            POST_STOCK_CONTROL_S.COUNT_DATE = data.COUNT_DATE;
            db.POST_STOCK_CONTROL_S_TMPs.InsertOnSubmit(POST_STOCK_CONTROL_S);
            db.SubmitChanges();

            return;
        }

        public static void process_taxdetail_records(int if_entry_no_ln, int interface_control_flag_ln, int line_id_ln)
        {

            //#ifdef debuga
            //   show 'FLOW: process-taxdetail-records'
            //#endif

            datadbDataContext db = new datadbDataContext();

            POST_TAX_OVERRIDE_T_TMP TRANS_TX_TMP = db.POST_TAX_OVERRIDE_T_TMPs.FirstOrDefault(i => i.IF_ENTRY_NO == if_entry_no_ln && i.INTERFACE_CONTROL_FLAG == interface_control_flag_ln && i.LINE_ID == line_id_ln);
            insert_giftcardtx_record(TRANS_TX_TMP);
            return;
        }

        public static void insert_giftcardtx_record(POST_TAX_OVERRIDE_T_TMP data)
        {
            datadbDataContext db = new datadbDataContext();
            POST_TAX_OVERRIDE_T_TMP POST_TAX_OVERRIDE_T = new POST_TAX_OVERRIDE_T_TMP();
            POST_TAX_OVERRIDE_T.IF_ENTRY_NO = data.IF_ENTRY_NO;
            POST_TAX_OVERRIDE_T.INTERFACE_CONTROL_FLAG = data.INTERFACE_CONTROL_FLAG;
            POST_TAX_OVERRIDE_T.RECORD_TYPE = data.RECORD_TYPE;
            POST_TAX_OVERRIDE_T.LINE_ID = data.LINE_ID;
            POST_TAX_OVERRIDE_T.TAX_LEVEL = data.TAX_LEVEL;
            POST_TAX_OVERRIDE_T.TAX_CATEGORY = data.TAX_CATEGORY;
            POST_TAX_OVERRIDE_T.TAXABLE = data.TAXABLE;
            POST_TAX_OVERRIDE_T.EXCEPTION_TAX_JURISDICTION = data.EXCEPTION_TAX_JURISDICTION;
            POST_TAX_OVERRIDE_T.TAX_EXEMPT_NO = data.TAX_EXEMPT_NO;
            db.POST_TAX_OVERRIDE_T_TMPs.InsertOnSubmit(POST_TAX_OVERRIDE_T);
            db.SubmitChanges();

            return;
        }

        public static void process_postvoiddetail_records(int if_entry_no_ln, int interface_control_flag_ln, int line_id_ln)
        {
            //#ifdef debuga
            //   show 'FLOW: process_postvoiddetail_records'
            //#endif
            datadbDataContext db = new datadbDataContext();
            POST_VOID_V_TMP TRANS_PV_TMP = db.POST_VOID_V_TMPs.FirstOrDefault(i => i.IF_ENTRY_NO == if_entry_no_ln && i.INTERFACE_CONTROL_FLAG == interface_control_flag_ln && i.LINE_ID == line_id_ln);
            insert_giftcardpv_record(TRANS_PV_TMP);
            return;
        }

        public static void insert_giftcardpv_record(POST_VOID_V_TMP data)
        {
            datadbDataContext db = new datadbDataContext();
            POST_VOID_V_TMP POST_VOID_V = new POST_VOID_V_TMP();
            POST_VOID_V.IF_ENTRY_NO = data.IF_ENTRY_NO;
            POST_VOID_V.INTERFACE_CONTROL_FLAG = data.INTERFACE_CONTROL_FLAG;
            POST_VOID_V.RECORD_TYPE = data.RECORD_TYPE;
            POST_VOID_V.LINE_ID = data.LINE_ID;
            POST_VOID_V.POST_VOIDED_REGISTER = data.POST_VOIDED_REGISTER;
            POST_VOID_V.POST_VOIDED_TRANS_NO = data.POST_VOIDED_TRANS_NO;
            POST_VOID_V.POST_VOID_SUCCESSFUL = data.POST_VOID_SUCCESSFUL;
            POST_VOID_V.POST_VOID_REASON_CODE = data.POST_VOID_REASON_CODE;
            db.POST_VOID_V_TMPs.InsertOnSubmit(POST_VOID_V);
            db.SubmitChanges();
            return;
        }

        public static void process_authdetail_records(int if_entry_no_ln, int interface_control_flag_ln, int line_id_ln)
        {
            //#ifdef debuga
            //   show 'FLOW: process_authdetail_records'
            //#endif
            datadbDataContext db = new datadbDataContext();
            POST_AUTH_A_TMP POST_VOID_V = db.POST_AUTH_A_TMPs.FirstOrDefault(i => i.IF_ENTRY_NO == if_entry_no_ln && i.INTERFACE_CONTROL_FLAG == interface_control_flag_ln && i.LINE_ID == line_id_ln);

            insert_giftcardad_record(POST_VOID_V);

            return;
        }

        public static void insert_giftcardad_record(POST_AUTH_A_TMP data)
        {
            datadbDataContext db = new datadbDataContext();
            POST_VOID_V_TMP POST_VOID_V = new POST_VOID_V_TMP();
            POST_VOID_V.IF_ENTRY_NO = data.IF_ENTRY_NO;
            POST_VOID_V.INTERFACE_CONTROL_FLAG = data.INTERFACE_CONTROL_FLAG;
            POST_VOID_V.RECORD_TYPE = data.RECORD_TYPE;
            POST_VOID_V.LINE_ID = data.LINE_ID;
            POST_VOID_V.CARD_TYPE = data.CARD_TYPE;
            POST_VOID_V.AUTHORIZATION_NO = data.AUTHORIZATION_NO;
            POST_VOID_V.EXPIRY_DATE = Convert.ToDateTime(data.EXPIRY_DATE);
            POST_VOID_V.SWIPE_INDICATOR = data.SWIPE_INDICATOR;
            POST_VOID_V.APPROVAL_MESSAGE = data.APPROVAL_MESSAGE;
            POST_VOID_V.LICENSE_NO = data.LICENSE_NO;
            POST_VOID_V.POS_STATE_CODE = data.POS_STATE_CODE;
            POST_VOID_V.OTHER_ID_TYPE = data.OTHER_ID_TYPE;
            POST_VOID_V.OTHER_ID = Convert.ToString(data.OTHER_ID);
            POST_VOID_V.DEFERRED_BILLING_DATE = data.DEFERRED_BILLING_DATE;
            POST_VOID_V.DEFERRED_BILLING_PLAN = data.DEFERRED_BILLING_PLAN;
            POST_VOID_V.CUSTOMER_SIGNATURE_OBTAINED = data.CUSTOMER_SIGNATURE_OBTAINED;
            db.POST_VOID_V_TMPs.InsertOnSubmit(POST_VOID_V);
            db.SubmitChanges();

            return;
        }

        public static void process_custdetail_records(int if_entry_no_ln, int interface_control_flag_ln, int line_id_ln)
        {
            //#ifdef debuga
            //   show 'FLOW: process_custdetail_records'
            //#endif
            datadbDataContext db = new datadbDataContext();
            POST_CUST_C_TMP TRANS_CD_TMP = db.POST_CUST_C_TMPs.FirstOrDefault(i => i.IF_ENTRY_NO == if_entry_no_ln && i.INTRFACE_CONTROL_FLAG == interface_control_flag_ln && i.FROM_LINE_ID == line_id_ln);
            insert_giftcardcd_record(TRANS_CD_TMP);
            return;
        }

        public static void insert_giftcardcd_record(POST_CUST_C_TMP data)
        {
            datadbDataContext db = new datadbDataContext();
            POST_CUST_C_TMP POST_CUST_C = new POST_CUST_C_TMP();
            POST_CUST_C.IF_ENTRY_NO = data.IF_ENTRY_NO;
            POST_CUST_C.INTRFACE_CONTROL_FLAG = data.INTRFACE_CONTROL_FLAG;
            POST_CUST_C.RECORD_TYPE = data.RECORD_TYPE;
            POST_CUST_C.FROM_LINE_ID = data.FROM_LINE_ID;
            POST_CUST_C.CUSTOMER_ROLE = data.CUSTOMER_ROLE;
            POST_CUST_C.TITLE = data.TITLE;
            POST_CUST_C.FIRST_NAME = data.FIRST_NAME;
            POST_CUST_C.LAST_NAME = data.LAST_NAME;
            POST_CUST_C.ADDRESS_1 = data.ADDRESS_1;
            POST_CUST_C.ADDRESS_2 = data.ADDRESS_2;
            POST_CUST_C.CITY = data.CITY;
            POST_CUST_C.COUNTY = data.COUNTY;
            POST_CUST_C.STATE = data.STATE;
            POST_CUST_C.COUNTRY = data.COUNTRY;
            POST_CUST_C.POST_CODE = data.POST_CODE;
            POST_CUST_C.TELEPHONE_NO1 = data.TELEPHONE_NO1;
            POST_CUST_C.TELEPHONE_NO2 = data.TELEPHONE_NO2;
            POST_CUST_C.CUSTOMER_NO = data.CUSTOMER_NO;
            db.POST_CUST_C_TMPs.InsertOnSubmit(POST_CUST_C);
            db.SubmitChanges();
            return;
        }

        public static void process_expcustdetail_records(int if_entry_no_ln, int interface_control_flag_ln, int line_id_ln)
        {

            //#ifdef debuga
            //   show 'FLOW: process-expcustdetail-records'
            //#endif
            datadbDataContext db = new datadbDataContext();
            POST_CUST_EXTRA_E_TMP TRANS_EC_TMP = db.POST_CUST_EXTRA_E_TMPs.FirstOrDefault(i => i.IF_ENTRY_NO == if_entry_no_ln && i.INTERFACE_CONTROL_FLAG == interface_control_flag_ln && i.FROM_LINE_ID == line_id_ln);

            insert_giftcardec_record(TRANS_EC_TMP);


            return;
        }

        public static void insert_giftcardec_record(POST_CUST_EXTRA_E_TMP data)
        {
            datadbDataContext db = new datadbDataContext();
            POST_CUST_EXTRA_E_TMP POST_CUST_EXTRA_E = new POST_CUST_EXTRA_E_TMP();
            POST_CUST_EXTRA_E.IF_ENTRY_NO = data.IF_ENTRY_NO;
            POST_CUST_EXTRA_E.INTERFACE_CONTROL_FLAG = data.INTERFACE_CONTROL_FLAG;
            POST_CUST_EXTRA_E.RECORD_TYPE = data.RECORD_TYPE;
            POST_CUST_EXTRA_E.FROM_LINE_ID = data.FROM_LINE_ID;
            POST_CUST_EXTRA_E.CUSTOMER_ROLE = data.CUSTOMER_ROLE;
            POST_CUST_EXTRA_E.CUSTOMER_INFO_TYPE = data.CUSTOMER_INFO_TYPE;
            POST_CUST_EXTRA_E.CUSTOMER_INFO = data.CUSTOMER_INFO;
            db.POST_CUST_EXTRA_E_TMPs.InsertOnSubmit(POST_CUST_EXTRA_E);
            db.SubmitChanges();

            return;
        }

        public static void process_lnnotedetail_records(POST_HEADER_H_TMP phht, int if_entry_no_ln, int interface_control_flag_ln, int line_id_ln, POST_LINE_L_TMP TRANS_LINE_TMP)
        {

            //#ifdef debuga
            //   show 'FLOW: process-lnnotedetail-records'
            //#endif

            datadbDataContext db = new datadbDataContext();
            POST_NOTES_N_TMP TRANS_LT_TMP = db.POST_NOTES_N_TMPs.FirstOrDefault(i => i.IF_ENTRY_NO == if_entry_no_ln && i.INTERFACE_CONTROL_FLAG == interface_control_flag_ln && i.LINE_ID == line_id_ln);

            string authorization_found = "N";

            string account_balance = "N";

            var authorization_no_post = "999999";
            var account_balance_post = 0;
            var pos_trace_no_post = "000000";


            if (TRANS_LT_TMP.NOTE_TYPE == 31)
            {
                account_balance = "Y";
                account_balance_post = Convert.ToInt32(TRANS_LT_TMP.LINE_NOTE);
            }
            if (TRANS_LT_TMP.NOTE_TYPE == 32)
            {
                authorization_found = "Y";
                authorization_no_post = TRANS_LT_TMP.LINE_NOTE;
                if (TRANS_LT_TMP.LINE_ID == 630)
                {
                    authorization_no_post = "000000";
                }
            }

            if (TRANS_LT_TMP.NOTE_TYPE == 33)
            {
                pos_trace_no_post = TRANS_LT_TMP.LINE_NOTE;
            }

            insert_giftcardlt_record(TRANS_LT_TMP);


            // code added on 02/08/2001 to default Authorization No for *!
            // Balance Transfers                                        *!

            if (TRANS_LT_TMP.LINE_ID == 630)
            {
                authorization_found = "Y";
                authorization_no_post = "000000";
                account_balance_post = 0;
                pos_trace_no_post = "000000";
            }

            //code ended

            if (account_balance == "Y")
            {
                account_balance_post = 0;
            }

            if (authorization_found == "N")
            {
                auth_error_record = "N";
                authorization_no_post = "999999";
                account_balance_post = 0;
                pos_trace_no_post = "000000";
                insert_recon_post_audit(phht, TRANS_LT_TMP, TRANS_LINE_TMP, authorization_no_post, account_balance_post, pos_trace_no_post, auth_error_record);
            }
            else
            {
                auth_error_record = "N";
                insert_recon_post_audit(phht, TRANS_LT_TMP, TRANS_LINE_TMP, authorization_no_post, account_balance_post, pos_trace_no_post, auth_error_record);
            }
            return;
        }

        public static void insert_giftcardlt_record(POST_NOTES_N_TMP data)
        {

            //#debuga show 'I am in POST_NOTES_N insert'
            //#debuga show '#if_entry_no_lnt     is '#if_entry_no_lnt
            //#debuga show '#intf_cntl_flag_lnt  is '#intf_cntl_flag_lnt
            //#debuga show '$record_type_lnt     is '$record_type_lnt
            //#debuga show '#line_id_lnt         is '#line_id_lnt
            //#debuga show '#line_note_type_lnt  is '#line_note_type_lnt
            //#debuga show '$line_note_lnt       is '$line_note_lnt
            datadbDataContext db = new datadbDataContext();
            POST_NOTES_N_TMP POST_NOTES_N = new POST_NOTES_N_TMP();
            POST_NOTES_N.IF_ENTRY_NO = data.IF_ENTRY_NO;
            POST_NOTES_N.INTERFACE_CONTROL_FLAG = data.INTERFACE_CONTROL_FLAG;
            POST_NOTES_N.RECORD_TYPE = data.RECORD_TYPE;
            POST_NOTES_N.LINE_ID = data.LINE_ID;
            POST_NOTES_N.NOTE_TYPE = data.NOTE_TYPE;
            POST_NOTES_N.LINE_NOTE = data.LINE_NOTE;
            POST_NOTES_N.DATE_CREATED = data.DATE_CREATED;
            db.POST_NOTES_N_TMPs.InsertOnSubmit(POST_NOTES_N);
            db.SubmitChanges();

            return;
        }

        public static void delete_giftcardlt_record(int if_entry_no_ln, int line_id_ln)
        {
            //#debuga show 'I am in POST_NOTES_N delete'
            datadbDataContext db = new datadbDataContext();
            POST_NOTES_N_TMP delete = db.POST_NOTES_N_TMPs.FirstOrDefault(i => i.IF_ENTRY_NO == if_entry_no_ln && i.LINE_ID == line_id_ln);
            db.POST_NOTES_N_TMPs.DeleteOnSubmit(delete);
            db.SubmitChanges();
        }

        public static void insert_giftcard_header(POST_HEADER_H_TMP data)
        {
            datadbDataContext db = new datadbDataContext();
            POST_HEADER_H_TMP POST_HEADER_H = new POST_HEADER_H_TMP();
            POST_HEADER_H.IF_ENTRY_NO = data.IF_ENTRY_NO;
            POST_HEADER_H.INTERFACE_CONTROL_FLAG = data.INTERFACE_CONTROL_FLAG;
            POST_HEADER_H.RECORD_TYPE = data.RECORD_TYPE;
            POST_HEADER_H.STORE_NO = data.STORE_NO;
            POST_HEADER_H.RECORD_TYPE = data.RECORD_TYPE;
            POST_HEADER_H.TRANSACTION_DATE = data.TRANSACTION_DATE;
            POST_HEADER_H.ENTRY_DATE_TIME = data.ENTRY_DATE_TIME;
            POST_HEADER_H.TRANSACTION_SERIES = data.TRANSACTION_SERIES;
            POST_HEADER_H.TRANSACTION_NO = data.TRANSACTION_NO;
            POST_HEADER_H.CASHIER_NO = data.CASHIER_NO;
            POST_HEADER_H.TRANSACTION_CATEGORY = data.TRANSACTION_CATEGORY;
            POST_HEADER_H.TRANSACTION_VOID_FLAG = data.TRANSACTION_VOID_FLAG;
            POST_HEADER_H.EMPLOYEE_NO = data.EMPLOYEE_NO;
            POST_HEADER_H.TRANSACTION_REMARK = data.TRANSACTION_REMARK;
            POST_HEADER_H.UPDATED_BY_USER_NAME = data.UPDATED_BY_USER_NAME;
            POST_HEADER_H.COMPANY_NO = data.COMPANY_NO;
            db.POST_HEADER_H_TMPs.InsertOnSubmit(POST_HEADER_H);
            db.SubmitChanges();

            return;
        }

        public static void process_store_date_control(string store_no_hdr, string transaction_date_hdr)
        {

            //#ifdef debuga
            //   show 'FLOW: process-store_date_control'
            //#endif
            datadbDataContext db = new datadbDataContext();
            var update_record_found = 'N';
            STORE_DATE_CONTROL STR_DT_CNTL = db.STORE_DATE_CONTROLs.FirstOrDefault(i => i.STORE_NO == store_no_hdr && i.TRANSACTION_DATE == transaction_date_hdr);

            var store_number = STR_DT_CNTL.STORE_NO;
            var transaction_date = STR_DT_CNTL.TRANSACTION_DATE;
            var postauditdtfirst = STR_DT_CNTL.POST_AUDIT_DATE_FIRST;

            update_record_found = 'Y';

            if (postauditdtfirst == null)
            {
                update_store_date_control1(store_number, transaction_date);
            }
            else
            {
                update_store_date_control2(store_number, transaction_date);
            }

            if (update_record_found == 'N')
            {
                insert_store_date_control(STR_DT_CNTL);
            }
            return;
        }

        public static void update_store_date_control1(string store_no_hdr, string transaction_date_hdr)
        {
            datadbDataContext db = new datadbDataContext();
            //#ifdef debuga
            //   show 'FLOW: update-store_date_control1'
            //#endif
            STORE_DATE_CONTROL update = db.STORE_DATE_CONTROLs.FirstOrDefault(i => i.STORE_NO == store_no_hdr && i.TRANSACTION_DATE == transaction_date_hdr);
            update.POST_AUDIT_DATE_LAST = DateTime.Now;
            update.RECON_DATE = "";
            db.SubmitChanges();
            return;
        }

        public static void update_store_date_control2(string store_no_hdr, string transaction_date_hdr)
        {

            //#ifdef debuga
            //   show 'FLOW: update-store_date_control2'
            //#endif
            datadbDataContext db = new datadbDataContext();
            STORE_DATE_CONTROL update = db.STORE_DATE_CONTROLs.FirstOrDefault(i => i.STORE_NO == store_no_hdr && i.TRANSACTION_DATE == transaction_date_hdr);
            update.POST_AUDIT_DATE_FIRST = DateTime.Now;
            update.POST_AUDIT_DATE_LAST = DateTime.Now;
            db.SubmitChanges();

            return;
        }

        public static void insert_store_date_control(STORE_DATE_CONTROL data)
        {

            //SHOW ' I AM IN INSERT'
            //SHOW ' $store_no_hdr IS ' $store_no_hdr
            //SHOW ' $AsofToday IS ' $AsofToday
            //let $present_date = $AsofToday ||' '||$AsofNow
            //SHOW '$present_date IS ' $present_date
            datadbDataContext db = new datadbDataContext();
            STORE_DATE_CONTROL storedate = new STORE_DATE_CONTROL();
            storedate.STORE_NO = data.STORE_NO;
            storedate.TRANSACTION_DATE = data.TRANSACTION_DATE;
            storedate.PTD_DATE_FIRST = data.PTD_DATE_FIRST;
            storedate.PTD_DATE_LAST = data.PTD_DATE_LAST;
            storedate.POST_AUDIT_DATE_FIRST = data.POST_AUDIT_DATE_FIRST;
            storedate.PRE_AUDIT_DATE_FIRST = data.PRE_AUDIT_DATE_FIRST;
            storedate.POST_AUDIT_DATE_LAST = data.POST_AUDIT_DATE_LAST;
            storedate.PRE_AUDIT_DATE_LAST = data.PRE_AUDIT_DATE_LAST;
            storedate.RECON_FLAG = data.RECON_FLAG;
            storedate.RECON_DATE = data.RECON_DATE;
            storedate.DA_TIMESTMP_CRE = data.DA_TIMESTMP_CRE;
            db.STORE_DATE_CONTROLs.InsertOnSubmit(storedate);
            db.SubmitChanges();

            return;

        }


        public static void insert_recon_post_audit(POST_HEADER_H_TMP phht, POST_NOTES_N_TMP data, POST_LINE_L_TMP data2, string authorization_no_post, int account_balance_post, string pos_trace_no_post, string auth_error_record)
        {
            //#debuga   show 'I am in insert-recon_post_audit'
            // #debuga show '$entry_date_time_hdr is ' $entry_date_time_hdr
            //find_recon_date();
            //#debuga   show '$recon_date is ' $recon_date
            //move #gross_line_amount_recon to $recon_amt 0999999.99
            datadbDataContext db = new datadbDataContext();
            var recon_cat_key = data2.REFERENCE_NO + authorization_no_post + phht.STORE_NO + phht.TRANSACTION_NO.ToString() + data.DATE_CREATED + data2.GROSS_LINE_AMOUNT;
            string recon_indicator = "";
            OBJECT_ACTION_HDR hdr = db.OBJECT_ACTION_HDRs.FirstOrDefault(i => i.LINE_OBJECT == data2.LINE_OBJECT && i.LINE_OBJECT == data2.LINE_ACTION);
            //var present_date = $AsofToday ||' '||$AsofNow
            string post_recon_date = DateTime.Now.ToString();
            string post_recon_flag = hdr.IN_AUTO_RECON.ToString().ToUpper();

            if (hdr.IN_AUTO_RECON.ToString().ToUpper() == "N")
            {
                post_recon_date = DateTime.Now.ToString();
                recon_indicator = "S";
            }
            else
            {
                post_recon_date = "";
                recon_indicator = "";
            }
            RECON_POST_AUDIT recon = new RECON_POST_AUDIT();
            recon.IF_ENTRY_NO = data.IF_ENTRY_NO;
            recon.REFERENCE_NUMBER = data2.REFERENCE_NO;
            recon.AUTHORIZATION_NO = authorization_no_post;
            recon.INTERFACE_CONTROL_FLAG = data2.INTERFACE_CONTROL_FLAG;
            recon.STORE_NO = phht.STORE_NO;
            recon.REGISTER_NO = phht.REGISTER_NO;
            recon.TRANSACTION_DATE = Convert.ToDateTime(phht.TRANSACTION_DATE);
            recon.ENTRY_DATE_TIME = phht.ENTRY_DATE_TIME;
            recon.TRANSACTION_SERIES = phht.TRANSACTION_SERIES;
            recon.TRANSACTION_NO = phht.TRANSACTION_NO;
            recon.CASHIER_NO = phht.CASHIER_NO;
            recon.TRANSACTION_CATEGORY = phht.TRANSACTION_CATEGORY;
            recon.TRANSACTION_VOID_FLAG = phht.TRANSACTION_VOID_FLAG;
            recon.EMPLOYEE_NO = phht.EMPLOYEE_NO;
            recon.TRANSACTION_REMARK = phht.TRANSACTION_REMARK;
            recon.UPDATED_BY_USER_NAME = Convert.ToInt32(phht.UPDATED_BY_USER_NAME);
            recon.COMPANY_NO = phht.COMPANY_NO;
            recon.LINE_ID = data2.LINE_ID;
            recon.LINE_OBJECT_TYPE = data2.LINE_OBJECT;
            recon.LINE_OBJECT = data2.LINE_OBJECT;
            recon.LINE_ACTION = data2.LINE_ACTION;
            recon.GROSS_LINE_AMOUNT = data2.GROSS_LINE_AMOUNT;
            recon.POS_DISCOUNT_AMOUNT = data2.POS_DISCOUNT_AMOUNT;
            recon.DB_CR_NONE = data2.DB_CR_NONE;
            recon.REFERENCE_TYPE = data2.REFERENCE_TYPE;
            recon.VOIDING_REVERSAL_FLAG = data2.VOIDING_REVERSAL_FLAG;
            recon.LINE_VOID_FLAG = data2.LINE_VOID_FLAG;
            recon.RECON_MATCH_AUTHCODE = authorization_no_post;
            recon.RECON_MATCH_STORE = phht.STORE_NO;
            recon.RECON_ACCOUNT_NUMBER = data2.REFERENCE_NO;
            recon.RECON_TRANSACTION_AMOUNT = data2.GROSS_LINE_AMOUNT;
            recon.RECON_TRANSACTION_NO = phht.TRANSACTION_NO;
            recon.RECON_TERMINAL_TRANS_DATE = Convert.ToDateTime(phht.TRANSACTION_DATE);
            recon.RECON_CAT_KEY = recon_cat_key;
            recon.CREATE_DATE = DateTime.Now;
            recon.RECON_INDICATOR = recon_indicator;
            recon.RECON_EMP_MOD = "";
            recon.POST_RECON_FLAG = post_recon_flag;
            recon.ACCOUNT_BALANCE = account_balance_post;
            recon.POS_TRACE_NUMBER = pos_trace_no_post;
            recon.POST_RECON_DATE = Convert.ToDateTime(post_recon_date);
            db.RECON_POST_AUDITs.InsertOnSubmit(recon);
            db.SubmitChanges();

            RECON_POST_AUDIT_PAYTECH paytech = new RECON_POST_AUDIT_PAYTECH();
            paytech.IF_ENTRY_NO = phht.IF_ENTRY_NO;
            paytech.REFERENCE_NUMBER = data2.REFERENCE_NO;
            paytech.AUTHORIZATION_NO = authorization_no_post;
            paytech.INTERFACE_CONTROL_FLAG = data.INTERFACE_CONTROL_FLAG;
            paytech.STORE_NO = phht.STORE_NO;
            paytech.REGISTER_NO = phht.REGISTER_NO;
            paytech.TRANSACTION_DATE = phht.TRANSACTION_DATE;
            paytech.ENTRY_DATE_TIME = phht.ENTRY_DATE_TIME;
            paytech.TRANSACTION_SERIES = phht.TRANSACTION_SERIES;
            paytech.TRANSACTION_NO = phht.TRANSACTION_NO;
            paytech.CASHIER_NO = Convert.ToInt32(phht.CASHIER_NO);
            paytech.TRANSACTION_CATEGORY = phht.TRANSACTION_CATEGORY;
            paytech.TRANSACTION_VOID_FLAG = phht.TRANSACTION_VOID_FLAG;
            paytech.EMPLOYEE_NO = phht.EMPLOYEE_NO;
            paytech.TRANSACTION_REMARK = phht.TRANSACTION_REMARK;
            paytech.UPDATED_BY_USER_NAME = phht.UPDATED_BY_USER_NAME;
            paytech.COMPANY_NO = phht.COMPANY_NO;
            paytech.LINE_ID = data2.LINE_ID;
            paytech.LINE_OBJECT_TYPE = data2.LINE_OBJECT_TYPE;
            paytech.LINE_OBJECT = data2.LINE_OBJECT;
            paytech.LINE_ACTION = data2.LINE_ACTION;
            paytech.GROSS_LINE_AMOUNT = data2.GROSS_LINE_AMOUNT;
            paytech.POS_DISCOUNT_AMOUNT = data2.POS_DISCOUNT_AMOUNT;
            paytech.DB_CR_NONE = Convert.ToInt32(data2.DB_CR_NONE);
            paytech.REFERENCE_TYPE = data2.REFERENCE_TYPE;
            paytech.VOIDING_REVERSAL_FLAG = data2.VOIDING_REVERSAL_FLAG;
            paytech.LINE_VOID_FLAG = data2.LINE_VOID_FLAG;
            paytech.RECON_MATCH_AUTHCODE = authorization_no_post;
            paytech.RECON_MATCH_STORE = phht.STORE_NO;
            paytech.RECON_ACCOUNT_NUMBER = data2.REFERENCE_NO;
            paytech.RECON_TRANSACTION_AMOUNT = data2.GROSS_LINE_AMOUNT;
            paytech.RECON_TRANSACTION_NO = phht.TRANSACTION_NO;
            paytech.RECON_TERMINAL_TRANS_DATE = Convert.ToDateTime(phht.TRANSACTION_DATE);
            paytech.RECON_CAT_KEY = recon_cat_key;
            paytech.CREATE_DATE = DateTime.Now;
            paytech.RECON_INDICATOR = recon_indicator;
            paytech.RECON_EMP_MOD = "";
            paytech.POST_RECON_FLAG = post_recon_flag;
            paytech.ACCOUNT_BALANCE = account_balance_post;
            paytech.POS_TRACE_NUMBER = pos_trace_no_post;
            paytech.POST_RECON_DATE = Convert.ToDateTime(post_recon_date);
            db.RECON_POST_AUDIT_PAYTECHes.InsertOnSubmit(paytech);
            db.SubmitChanges();
            return;
        }

        public static void process_recon_post_cc_audit(POST_HEADER_H_TMP hdr, POST_LINE_L_TMP TRANS_LINE_CC_TMP)
        {
            //#debuga   show 'I am in process-recon_post_cc_audit'
            // #debuga show '$entry_date_time_hdr is ' $entry_date_time_hdr

            //int store_no_hdr = Convert.ToInt32(storehdr);

            find_transid(hdr, TRANS_LINE_CC_TMP);

            return;
        }



        public static void find_transid(POST_HEADER_H_TMP hdr, POST_LINE_L_TMP TRANS_LINE_CC_TMP)
        {
            var av_trans_id = 0;


            datadbDataContext db = new datadbDataContext();
            av_transaction_header av = db.av_transaction_headers.FirstOrDefault(i => i.store_no == Convert.ToInt32(hdr.STORE_NO) && i.register_no == hdr.REGISTER_NO && i.transaction_date == hdr.TRANSACTION_DATE && i.transaction_no == hdr.TRANSACTION_NO);
            string weborderno = find_weborderno(Convert.ToInt32(av.transaction_no));
            string authorization_no = find_authno(Convert.ToInt32(av.transaction_no), Convert.ToInt32(TRANS_LINE_CC_TMP.LINE_ID));
            insert_recon_post_cc_audit(TRANS_LINE_CC_TMP, hdr, weborderno, authorization_no);
            return;
        }

        public static string find_weborderno(int av_trans_id)
        {

            string weborderno = " ";
            datadbDataContext db = new datadbDataContext();
            av_customer customer = db.av_customers.FirstOrDefault(i => i.av_transaction_id == av_trans_id);

            weborderno = customer.telephone_no2;

            return weborderno;
        }

        public static string find_authno(int av_trans_id, int line_id_ln_cc)
        {

            string authno = " ";

            var authorization_no = av_trans_id;


            datadbDataContext db = new datadbDataContext();
            RECON_POST_CC_AUDIT avauth = db.RECON_POST_CC_AUDITs.FirstOrDefault(i => i.TRANSACTION_ID == av_trans_id && i.LINE_ACTION == line_id_ln_cc);

            return avauth.AUTHORIZATION_NO;
        }
        public static void insert_recon_post_cc_audit(POST_LINE_L_TMP data, POST_HEADER_H_TMP hdr, string weborderno, string authorization_no)
        {
            //#debuga   show 'I am in insert-recon_post_cc_audit'
            // #debuga show '$entry_date_time_hdr is ' $entry_date_time_hdr

            //let #store_no_hdr = to_number($store_no_hdr)

            //!  #debuga   show '$recon_date is ' $recon_date
            //move #gross_line_amount_recon to $recon_amt 0999999.99

            //var present_date = $AsofToday ||' '||$AsofNow;

            //#debugc show ' #if_entry_no_hdr              is  ' #if_entry_no_hdr
            //#debugc show ' $reference_no_ln_cc           is  ' $reference_no_ln_cc
            //#debugc show ' $store_no_hdr                 is  ' $store_no_hdr
            //#debugc show ' $transaction_date_hdr         is  ' $transaction_date_hdr
            //#debugc show ' #transaction_no_hdr           is  ' #transaction_no_hdr
            //#debugc show ' #line_object_ln_cc            is  ' #line_object_ln_cc
            //#debugc show ' #line_action_ln_cc            is  ' #line_action_ln_cc
            //#debugc show ' #gross_line_amount_ln_cc      is  ' #gross_line_amount_ln_cc
            //#debugc show ' $AsofToday                    is  ' $AsofToday
            //#debugc show ' #register_no_hdr              is  ' #register_no_hdr
            //#debugc show ' '


            if (data.LINE_ACTION == 27)
            {
                var gross_line_amount_ln_cc = -1 * Convert.ToInt32(data.GROSS_LINE_AMOUNT);
            }

            if (data.REFERENCE_NO.Trim() == null)
            {
                var reference_no_ln_cc = weborderno;
            }

            datadbDataContext db = new datadbDataContext();
            RECON_POST_CC_AUDIT reconpostaudit = new RECON_POST_CC_AUDIT();
            reconpostaudit.IF_ENTRY_NO = data.IF_ENTRY_NO;
            reconpostaudit.REFERENCE_NUMBER = data.REFERENCE_NO;
            reconpostaudit.STORE_NO = Convert.ToInt32(hdr.STORE_NO);
            reconpostaudit.TRANSACTION_DATE = Convert.ToDateTime(hdr.TRANSACTION_DATE);
            reconpostaudit.TRANSACTION_NO = hdr.TRANSACTION_NO;
            reconpostaudit.LINE_OBJECT = data.LINE_OBJECT;
            reconpostaudit.LINE_ACTION = data.LINE_ACTION;
            reconpostaudit.GROSS_LINE_AMOUNT = data.GROSS_LINE_AMOUNT.ToString();
            reconpostaudit.CREATE_DATE = DateTime.Now;
            reconpostaudit.REGISTER_NO = hdr.REGISTER_NO;
            reconpostaudit.TRANSACTION_ID = hdr.TRANSACTION_NO;
            reconpostaudit.WEB_ORDER_NO = weborderno;
            reconpostaudit.AUTHORIZATION_NO = authorization_no;
            reconpostaudit.RECON_FLAG = data.VOIDING_REVERSAL_FLAG.ToString();
            db.RECON_POST_CC_AUDITs.InsertOnSubmit(reconpostaudit);
            return;
        }


        //public static void find_recon_date(string transaction_date_hdr)
        //{


        //char recon_date = Convert.ToChar(transaction_date_hdr);

        ////  move &recon_date to $recon_date
        ////from dual

        //    return;
        //}


        public static void commit_record()
        {
            datadbDataContext db = new datadbDataContext();
            db.SubmitChanges();
            return;
        }

        public static void delete_go_file()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "cmd /c del " + MOVEFROM + "US_POST_TRAN.GO";
            process.StartInfo = startInfo;
            process.Start();

            Console.WriteLine("***** deleted ****");

            return;
        }

        public static void delete_processed_files()
        {

            var dos_string = "cmd /c del" + COPYTO + "US_POST_TRAN.*";
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
                //Console.WriteLine(file);
                Console.WriteLine(" ");
            }
            Console.WriteLine("---------------------");
            return;
        }

        public static void move_files(string pstauditbackupfile, string processed_date)
        {

            Console.WriteLine("I am in Move files");

            var dos_string1 = "cmd /c move ";
            var dos_string2 = pstauditbackupfile;
            var dos_string3 = BACKUP + postauditfile + "." + processed_date;
            var dos_string = dos_string1 + dos_string2 + " " + dos_string3;
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
                Console.WriteLine("* Moving Files Now  *");
                Console.WriteLine(" ");
                var copy_flag = 'Y';
                Console.WriteLine(dos_string2);
                Console.WriteLine(" ");
            }
            else
            {
                Console.WriteLine(" ");
                Console.WriteLine("***** Moving files failed");
                Console.WriteLine(" ");
            }
            Console.WriteLine("---------------------");
            return;
        }
        public static void copytonew()
        {
            //#debuga display 'copytonew  '
            var dos1 = "cmd /c copy ";
            var dos2 = COPYFROM;
            var dos3 = "US_POST_TRAN.* ";
            var dos4 = COPYTO;
            var dos_string = dos1 + dos2 + dos3 + dos4;
            //#debuga display '$dos4  '
            //#debuga display  $dos4

            Console.WriteLine("***** FILE XX ****");
            Console.WriteLine(" ");
            Console.WriteLine("dos_string");
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
            Console.WriteLine("---------------------");
            return;
        }

        public static void files_copy()
        {
            //#debuga display 'Files-copy  '
            var dos1 = "cmd /c dir ";
            var dos2 = MOVEFROM;
            var dos3 = "US_POST_TRAN.* /ON/B > ";
            var dos4 = WORKDIR + "POSTAUDIT.DAT";
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
            if (process.ExitCode == 0)
            {
                Console.WriteLine(" ");
                Console.WriteLine("* Processing Files Now  *");
                Console.WriteLine(" ");
                var copy_flag = 'Y';
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

        public static void process_CC_file()
        {
            var CCFileName = "pos_sales";
            var CCFileExt = ".txt";
            var CCFullFileName = CCDIR + CCFileName + DateTime.Now + CCFileExt;
            var CCBKUPFileName = CCBKDIR + CCFileName + DateTime.Now + CCFileExt;

            //var postauditmainfile = WORKDIR + "POSTAUDIT.DAT";
            if (File.Exists(CCFullFileName))
            {
                var logFile = File.ReadAllLines(CCFullFileName);
                List<string> LogList = new List<string>(logFile);
            }
            else
            {
                Console.WriteLine(CCFullFileName);
                Console.WriteLine("Open for file 100 failed");
            }

            if (File.Exists(CCBKUPFileName))
            {
                var logFile2 = File.ReadAllLines(CCBKUPFileName);
                List<string> LogList = new List<string>(logFile2);
            }
            else
            {
                Console.WriteLine(CCBKUPFileName);
                Console.WriteLine("Open for file 200 failed");
            }

            select_recon_file(CCFullFileName, CCBKUPFileName);

            return;
        }

        public static void select_recon_file(string CCFullFileName, string CCBKUPFileName)
        {
            datadbDataContext db = new datadbDataContext();
            RECON_POST_CC_AUDIT CC = db.RECON_POST_CC_AUDITs.FirstOrDefault(i => i.RECON_FLAG == "N");

            var store_no_cc = CC.STORE_NO;
            var transdt_cc = CC.TRANSACTION_DATE;
            var amt_cc = CC.GROSS_LINE_AMOUNT;
            var web_orderno_cc = CC.WEB_ORDER_NO.Trim();
            var no_cc = CC.REFERENCE_NUMBER.Trim();
            var transid_cc = CC.TRANSACTION_ID;
            var authno_cc = CC.AUTHORIZATION_NO.Trim();
            var lineobj_cc = CC.LINE_OBJECT;
            string[] amazondir = amazondir = open_Amazon_files().Split(',');
            string file_open_Amazon = amazondir[2];
            if (Convert.ToInt32(lineobj_cc) == 638)
            {
                if (file_open_Amazon == "Y")
                {
                    write_CC_file_Amazon(CC, amazondir[0], amazondir[1]);
                }
                else
                {
                    write_CC_file_Amazon(CC, amazondir[0], amazondir[1]);
                }
            }
            else
            {
                write_CC_file(CC, CCFullFileName, CCBKUPFileName);
            }
            return;
        }

        public static string open_Amazon_files()
        {

            var CCFileName_amazon = "pos_sales_amazon";
            var CCFileExt = ".txt";
            var CCFullFileName_amazon = CCDIR + CCFileName_amazon + DateTime.Now + CCFileExt;
            var CCBKUPFileName_amazon = CCBKDIR + CCFileName_amazon + DateTime.Now + CCFileExt;
            if (File.Exists(CCFullFileName_amazon))
            {
                var logFile = File.ReadAllLines(CCFullFileName_amazon);
                List<string> LogList = new List<string>(logFile);
            }
            else
            {
                Console.WriteLine(CCFullFileName_amazon);
                Console.WriteLine("Open for file 300 failed");
            }

            if (File.Exists(CCBKUPFileName_amazon))
            {
                var logFile = File.ReadAllLines(CCBKUPFileName_amazon);
                List<string> LogList = new List<string>(logFile);
            }
            else
            {
                Console.WriteLine(CCBKUPFileName_amazon);
                Console.WriteLine("Open for file 400 failed");
            }

            var file_open_Amazon = "Y";
            return CCFullFileName_amazon + "," + CCBKUPFileName_amazon;
        }

        public static void write_CC_file(RECON_POST_CC_AUDIT CC, string CCFullFileName, string CCBKUPFileName)
        {

            var sep = ',';

            if (CC.WEB_ORDER_NO == "")
            {
                var web_orderno_cc = " ";
            }
            if (CC.AUTHORIZATION_NO == "")
            {
                var authno_cc = " ";
            }
            File.WriteAllText(CCFullFileName, CC.STORE_NO.ToString() + sep + CC.TRANSACTION_DATE.ToString() + sep + CC.GROSS_LINE_AMOUNT + sep + CC.WEB_ORDER_NO + sep + CC.TRANSACTION_NO + sep + CC.TRANSACTION_ID + sep + CC.AUTHORIZATION_NO + sep + CC.LINE_OBJECT);
            File.WriteAllText(CCBKUPFileName, CC.STORE_NO.ToString() + sep + CC.TRANSACTION_DATE.ToString() + sep + CC.GROSS_LINE_AMOUNT + sep + CC.WEB_ORDER_NO + sep + CC.TRANSACTION_NO + sep + CC.TRANSACTION_ID + sep + CC.AUTHORIZATION_NO + sep + CC.LINE_OBJECT);
            return;
        }

        public static void write_CC_file_Amazon(RECON_POST_CC_AUDIT CC, string CCFullFileName, string CCBKUPFileName)
        {
            var sep = ',';

            if (CC.WEB_ORDER_NO == "")
            {
                var web_orderno_cc = " ";
            }
            if (CC.AUTHORIZATION_NO == "")
            {
                var authno_cc = " ";
            }
            File.AppendAllText(CCFullFileName, CC.STORE_NO.ToString() + sep + CC.TRANSACTION_DATE + sep + CC.GROSS_LINE_AMOUNT + sep + CC.WEB_ORDER_NO + sep + CC.TRANSACTION_NO + sep + CC.TRANSACTION_ID + sep + CC.AUTHORIZATION_NO + sep + CC.LINE_OBJECT);
            File.AppendAllText(CCBKUPFileName, CC.STORE_NO.ToString() + sep + CC.TRANSACTION_DATE + sep + CC.GROSS_LINE_AMOUNT + sep + CC.WEB_ORDER_NO + sep + CC.TRANSACTION_NO + sep + CC.TRANSACTION_ID + sep + CC.AUTHORIZATION_NO + sep + CC.LINE_OBJECT);
            return;
        }

        public static void update_recon_flag()
        {
            datadbDataContext db = new datadbDataContext();
            RECON_POST_CC_AUDIT update = db.RECON_POST_CC_AUDITs.FirstOrDefault(i => i.RECON_FLAG == "N");
            update.RECON_FLAG = "Y";
            db.SubmitChanges();
            return;
        }

        public static void Delay_1_Minute()
        {
            //#debuga display 'Delay-1-Minute  '
            var Sql_Msg = "Delay-1-Minute  PROBLEM";

            //System.Threading.Thread.Sleep(60000);

            Console.WriteLine(" ");
            Console.WriteLine("Delay 1 Minute  COMPLETED");
            Console.WriteLine(" ");

        }

        //public static void SQL_Error()
        //{

        //    //#debuga  SHOW $loadrecord

        //    //evaluate #sql-status
        //    //#ifdef DB2
        //    //when = 6100    !DB2 error for empty-table result set
        //    //break
        //    //#end-if

        //    //#ifdef DB2UNIX
        //    //when = 6100    !DB2 error for empty-table result set
        //    //break
        //    //#end-if

        //    //when = -99999  !Token "when" clause for non-DB2 environments
        //    //when-other
        //    Console.Write(sqr_program);
        //    Console.Write(": ");
        //    Console.WriteLine(ReportID);
        //    Console.WriteLine(" - SQL Statement = ");
        //    Console.WriteLine(SQL_STATEMENT);
        //    Console.WriteLine("SQL Status =");
        //    Console.WriteLine(sql-status +  "99999");
        //    Console.Write(" ");
        //    Console.WriteLine("SQL Error  = ");
        //    Console.WriteLine(sql_error);
        //    Console.WriteLine(Sql_Msg);
        //    //#debuga  SHOW $loadrecord
        //    Rollback_Transaction();

        //return;
        //}
    }
}

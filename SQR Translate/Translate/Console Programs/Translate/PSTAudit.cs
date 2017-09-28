using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SpencerGifts.Translate
{
    class PSTAudit
    {
        public string COPYFROM = "\\sgawapp\\Retail_apps\\auditworks\\OUTPUT\\Post\\";
        public string COPYTO = "E:\\PostAudit\\";
        public string MOVEFROM = "\\sgawapp\\Retail_apps\\auditworks\\OUTPUT\\Post\\";
        public string MOVETO = "E:\\PostAudit\\POSTWORK\\";
        public string BACKUP = "\\sgawapp\\Retail_apps\\auditworks\\OUTPUT\\Post\\backup\\";
        public string WORKDIR = "E:\\PostAudit\\POSTWORK\\";
        public string CCDIR = "\\SGAPP\\ReconNet\\uardata\\creditcard\\";
        public string CCBKDIR = "\\SGAPP\\ReconNet\\uardata\\creditcard\\backup\\";
        public string postauditfile;

        private void open_main_file()
        {
            Console.WriteLine("open-main-file   X");
            var postauditmainfile = WORKDIR + "POSTAUDIT.DAT";
            string[] postauditdata;
            if (File.Exists(postauditmainfile))
            {
                Stream stream = File.Open(postauditmainfile, FileMode.Open);

                BinaryFormatter bFormatter = new BinaryFormatter();
                postauditdata = (string[])bFormatter.Deserialize(stream);
                stream.Close();
                foreach (var item in postauditdata)
                {
                    postauditfile = item;
                    //var record_count = 0;
                    process_main();
                }
            }
            else
            {
                    Console.WriteLine("Could not open Post audit Main file ");
                    Console.WriteLine(postauditmainfile);
                    return;
            }

            var file_count = Convert.ToString(postauditdata.Count());
            var total_files_processed = file_count + " file(s) processed.";
            Console.Write(" ");
            Console.WriteLine("End of input.");
            Console.WriteLine(total_files_processed);
        }

        public void process_main()
        {
            return;
        }

        public void open_file()
        {
            var postauditfile1 = COPYTO + postauditfile; //"post_tran_us.txt"
            var postauditbackupfile = MOVEFROM + postauditfile; //"post_tran_us.txt"
            Stream stream = File.Open(postauditfile1, FileMode.Open);

            BinaryFormatter bFormatter = new BinaryFormatter();
            string[] postauditdata = (string[])bFormatter.Deserialize(stream);
            stream.Close();

            if (!File.Exists(postauditfile1))
            {
                Console.WriteLine("Could not open Post audit file");
                Console.WriteLine(postauditfile1); 
            }
            return;
        }

        public void process_upload()
        {
            return;
        }

        public void define_hdr_variables(string sep)
        {
            return;
        }

        public void define_ln_variables()
        {
            return;
        }

        public void define_merchandise_variables()
        {
            return;
        }

        public void define_discdetail_variables()
        {
            return;
        }

        public void define_returndetail_variables()
        {
            return;
        }

        public void define_sporderdetail_variables()
        {
            return;
        }

        public void define_stockcntldetail_variables()
        {
            return;
        }

        public void define_taxdetail_variables()
        {
            return;
        }

        public void define_postvoiddetail_variables()
        {
            return;
        }

        public void define_authdetail_variables()
        {
            return;
        }

        public void define_custdetail_variables()
        {
            return;
        }

        public void define_expcustdetail_variables()
        {
            return;
        }

        public void define_lnnotes_variables()
        {
            return;
        }

        public void insert_trans_hdr()
        {
            return;
        }

        public void insert_trans_ln()
        {
            return;
        }

        public void insert_merchandise_dtl()
        {
            return;
        }

        public void insert_disc_dtl()
        {
            return;
        }

    }
}

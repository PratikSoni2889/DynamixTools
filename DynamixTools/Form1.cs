using CrmHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamixTools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void BtnConnectToCrm_Click(object sender, EventArgs e)
        {
            var organizationDetails = Crm.GetOrganizationDetailCollection(Resource.PHCRM3);



            var crm = new Crm();

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

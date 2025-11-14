using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace Act2_Mizal
{
    public partial class FoodOrderForm : Form
    {
        
        private static string strConnString =
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=DB_Ordering_Mizal;Integrated Security=True;Trust Server Certificate=True";

        

        private Label lblCust, lblItem, lblQty, lblPrice, lblHeader;
        private TextBox txtCust, txtItem, txtQty, txtPrice;
        private Button btnOrder, btnClear;
        private DataGridView dgvFoodOrders;
        private Panel pnlInputs;

        public FoodOrderForm()
        {
            InitializeCustomUI();
            LoadFoodOrders();
            this.Focus();
        }

        private void InitializeCustomUI()
        {
            this.Text = "Mizal Food Ordering Transaction";
            this.BackColor = Color.LightYellow;
            this.Size = new Size(700, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            Font headerFont = new Font("Segoe UI Semibold", 20, FontStyle.Bold);
            Font labelFont = new Font("Segoe UI", 10, FontStyle.Regular);
            Font textFont = new Font("Segoe UI", 10);


            lblHeader = new Label()
            {
                Text = "New Food Order",
                Font = headerFont,
                ForeColor = Color.DarkGreen,
                AutoSize = true,
                Location = new Point(250, 15)
            };


            pnlInputs = new Panel()
            {
                Size = new Size(650, 180),
                Location = new Point(20, 60),
                BackColor = Color.Honeydew,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblCust = new Label() { Text = "Customer:", Location = new Point(20, 30), AutoSize = true, ForeColor = Color.DarkGreen, Font = labelFont };
            lblItem = new Label() { Text = "Food Item:", Location = new Point(20, 70), AutoSize = true, ForeColor = Color.DarkGreen, Font = labelFont };
            lblQty = new Label() { Text = "Quantity:", Location = new Point(350, 30), AutoSize = true, ForeColor = Color.DarkGreen, Font = labelFont };
            lblPrice = new Label() { Text = "Price:", Location = new Point(350, 70), AutoSize = true, ForeColor = Color.DarkGreen, Font = labelFont };

            txtCust = new TextBox() { Location = new Point(115, 27), Width = 180, BackColor = Color.White, Font = textFont, BorderStyle = BorderStyle.FixedSingle };
            txtItem = new TextBox() { Location = new Point(115, 67), Width = 180, BackColor = Color.White, Font = textFont, BorderStyle = BorderStyle.FixedSingle };
            txtQty = new TextBox() { Location = new Point(430, 27), Width = 80, BackColor = Color.White, Font = textFont, BorderStyle = BorderStyle.FixedSingle };
            txtPrice = new TextBox() { Location = new Point(430, 67), Width = 80, BackColor = Color.White, Font = textFont, BorderStyle = BorderStyle.FixedSingle };


            btnOrder = new Button()
            {
                Text = "Place & Pay",
                Location = new Point(550, 25),
                Size = new Size(90, 35),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOrder.FlatAppearance.BorderSize = 0;
            btnOrder.Click += BtnOrder_Click;

            btnClear = new Button()
            {
                Text = "Clear",
                Location = new Point(550, 75),
                Size = new Size(90, 35),
                BackColor = Color.Peru,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (sender, e) => ClearInputs();

            
            dgvFoodOrders = new DataGridView()
            {
                Location = new Point(20, 250),
                Size = new Size(650, 350),
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.LightYellow,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AlternatingRowsDefaultCellStyle = { BackColor = Color.LightGreen },
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

           
            pnlInputs.Controls.AddRange(new Control[] { lblCust, txtCust, lblItem, txtItem, lblQty, txtQty, lblPrice, txtPrice, btnOrder, btnClear });
            this.Controls.Add(lblHeader);
            this.Controls.Add(pnlInputs);
            this.Controls.Add(dgvFoodOrders);
        }


        private void BtnOrder_Click(object sender, EventArgs e)
        {
            string customer = txtCust.Text.Trim();
            string item = txtItem.Text.Trim();

            if (string.IsNullOrEmpty(customer) || string.IsNullOrEmpty(item) ||
                !int.TryParse(txtQty.Text.Trim(), out int quantity) || quantity <= 0 ||
                !decimal.TryParse(txtPrice.Text.Trim(), out decimal price) || price <= 0)
            {
                MessageBox.Show("Please ensure all valid order details (Customer, Item, Quantity, Price) are entered.", "Input Error");
                return;
            }

            
            try
            {
                
                int row = 0;
                using (SqlConnection conn = new SqlConnection(strConnString))
                {
                    conn.Open();
                    SqlCommand cmdInsert = new SqlCommand(
                        "INSERT INTO TblFoodOrders (CustomerName, FoodItemName, Quantity, Price) VALUES (@cust, @item, @qty, @price)",
                        conn
                    );
                    cmdInsert.Parameters.AddWithValue("@cust", customer);
                    cmdInsert.Parameters.AddWithValue("@item", item);
                    cmdInsert.Parameters.AddWithValue("@qty", quantity);
                    cmdInsert.Parameters.AddWithValue("@price", price);

                    row = cmdInsert.ExecuteNonQuery(); 
                }

                if (row == 1)
                {
                    
                    if (ProcessPaymentTransaction())
                    {
                        MessageBox.Show($"Food Order for {customer} placed and paid successfully!", "Success");
                        ClearInputs();
                        LoadFoodOrders();
                    }
                }
                else
                {
                    MessageBox.Show("Failed to place the food order!", "Database Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing food order: " + ex.Message, "System Error");
            }
        }


        private bool ProcessPaymentTransaction()
        {
            int orderIdToPay = 0;

            try
            {
                
                using (SqlConnection conn = new SqlConnection(strConnString))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(
                        "SELECT TOP 1 FoodOrderID FROM TblFoodOrders WHERE OrderStatus = 'Pending Payment' ORDER BY OrderDate DESC",
                        conn
                    );
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count == 0) return false;
                    orderIdToPay = Convert.ToInt32(dt.Rows[0]["FoodOrderID"]);
                }

          
                int row = 0;
                using (SqlConnection conn = new SqlConnection(strConnString))
                {
                    conn.Open();
                    SqlCommand cmdUpdate = new SqlCommand(
                        "UPDATE TblFoodOrders SET OrderStatus = 'Paid', PaymentDate = GETDATE(), PaymentMethod = 'Cash' WHERE FoodOrderID = @id AND OrderStatus = 'Pending Payment'",
                        conn
                    );
                    cmdUpdate.Parameters.AddWithValue("@id", orderIdToPay);
                    row = cmdUpdate.ExecuteNonQuery(); 
                }

                if (row == 1) return true;

                MessageBox.Show($"Payment status update failed for Order ID: {orderIdToPay}.", "Payment Failed");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Payment Transaction Error: " + ex.Message, "Transaction Error");
                return false;
            }
        }

        private void LoadFoodOrders()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnString))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(
                        "SELECT FoodOrderID, CustomerName, FoodItemName, Quantity, Price, OrderStatus FROM TblFoodOrders ORDER BY OrderDate DESC",
                        conn
                    );
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvFoodOrders.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading food orders. Have you created the 'TblFoodOrders' table? Error: " + ex.Message);
            }
        }

        private void ClearInputs()
        {
            txtCust.Text = "";
            txtItem.Text = "";
            txtQty.Text = "";
            txtPrice.Text = "";
            txtCust.Focus();
        }
    }
}
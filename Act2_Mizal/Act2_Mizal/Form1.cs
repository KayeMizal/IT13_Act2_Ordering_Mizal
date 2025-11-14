using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace Act2_Mizal
{
    public partial class Form1 : Form
    {
        
        private static string strConnString =
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=DB_Ordering_Mizal;Integrated Security=True;Trust Server Certificate=True";


        private Label lblCust, lblItem, lblQty, lblPrice, lblHeader;
        
        private TextBox txtCust, txtItem, txtQty, txtPrice;
        private Button btnAdd, btnDelete, btnOpenFoodOrders; 
        private DataGridView dgvOrders;
        private Panel pnlInputs;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomUI();
            LoadOrders();
        }

        private void InitializeCustomUI()
        {
            this.Text = "Mizal Ordering System";
            this.BackColor = Color.MistyRose;
            this.Size = new Size(650, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            Font headerFont = new Font("Segoe UI Semibold", 18, FontStyle.Bold);
            Font labelFont = new Font("Segoe UI", 10, FontStyle.Regular);
            Font textFont = new Font("Segoe UI", 10);


            lblHeader = new Label()
            {
                Text = "Place Your Order",
                Font = headerFont,
                ForeColor = Color.DarkMagenta,
                AutoSize = true,
                Location = new Point(180, 10)
            };

            pnlInputs = new Panel()
            {
                Size = new Size(600, 180),
                Location = new Point(20, 50),
                BackColor = Color.LavenderBlush,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblCust = new Label() { Text = "Customer:", Location = new Point(20, 25), AutoSize = true, ForeColor = Color.MediumVioletRed, Font = labelFont };
            lblItem = new Label() { Text = "Item:", Location = new Point(20, 65), AutoSize = true, ForeColor = Color.MediumVioletRed, Font = labelFont };
            lblQty = new Label() { Text = "Quantity:", Location = new Point(20, 105), AutoSize = true, ForeColor = Color.MediumVioletRed, Font = labelFont };
            lblPrice = new Label() { Text = "Price:", Location = new Point(300, 65), AutoSize = true, ForeColor = Color.MediumVioletRed, Font = labelFont };


            txtCust = new TextBox() { Location = new Point(110, 22), Width = 150, BackColor = Color.White, Font = textFont, BorderStyle = BorderStyle.FixedSingle }; // FIX 1
            txtItem = new TextBox() { Location = new Point(110, 62), Width = 150, BackColor = Color.White, Font = textFont, BorderStyle = BorderStyle.FixedSingle }; // FIX 1
            txtQty = new TextBox() { Location = new Point(110, 102), Width = 80, BackColor = Color.White, Font = textFont, BorderStyle = BorderStyle.FixedSingle }; // FIX 1
            txtPrice = new TextBox() { Location = new Point(355, 62), Width = 100, BackColor = Color.White, Font = textFont, BorderStyle = BorderStyle.FixedSingle }; // FIX 1


            btnAdd = new Button()
            {
                Text = "Add Order",
                Location = new Point(480, 20),
                Size = new Size(100, 35),
                BackColor = Color.MediumPurple,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            btnDelete = new Button()
            {
                Text = "Delete",
                Location = new Point(480, 70),
                Size = new Size(100, 35),
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;

            
            btnOpenFoodOrders = new Button()
            {
                Text = "Order Food", 
                Location = new Point(480, 120), 
                Size = new Size(100, 35),
                BackColor = Color.DarkGreen, 
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOpenFoodOrders.FlatAppearance.BorderSize = 0;
            btnOpenFoodOrders.Click += BtnOpenFoodOrders_Click; 

            pnlInputs.Controls.Add(lblCust);
            pnlInputs.Controls.Add(txtCust);
            pnlInputs.Controls.Add(lblItem);
            pnlInputs.Controls.Add(txtItem);
            pnlInputs.Controls.Add(lblQty);
            pnlInputs.Controls.Add(txtQty);
            pnlInputs.Controls.Add(lblPrice);
            pnlInputs.Controls.Add(txtPrice);
            pnlInputs.Controls.Add(btnAdd);
            pnlInputs.Controls.Add(btnDelete);
            pnlInputs.Controls.Add(btnOpenFoodOrders); 


            dgvOrders = new DataGridView()
            {
                Location = new Point(20, 250),
                Size = new Size(600, 300),
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.MistyRose,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AlternatingRowsDefaultCellStyle = { BackColor = Color.Thistle },
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };


            this.Controls.Add(lblHeader);
            this.Controls.Add(pnlInputs);
            this.Controls.Add(dgvOrders);


            pnlInputs.Focus();
        }

        private void BtnOpenFoodOrders_Click(object sender, EventArgs e)
        {
          
            try
            {
                FoodOrderForm foodForm = new FoodOrderForm();
                foodForm.ShowDialog();
         
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Food Order Form. Have you created the 'FoodOrderForm.cs' file? " + ex.Message, "Error");
            }
        }
        
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string customer = txtCust.Text.Trim();
            string item = txtItem.Text.Trim();
            int quantity;
            decimal price;

            if (string.IsNullOrEmpty(customer))
            {
                MessageBox.Show("Customer Name is required!");
                return;
            }
            if (string.IsNullOrEmpty(item))
            {
                MessageBox.Show("Item Name is required!");
                return;
            }
            if (!int.TryParse(txtQty.Text.Trim(), out quantity) || quantity <= 0)
            {
                MessageBox.Show("Enter a valid quantity!");
                return;
            }
            if (!decimal.TryParse(txtPrice.Text.Trim(), out price) || price <= 0)
            {
                MessageBox.Show("Enter a valid price!");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(strConnString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO TblOrders (CustomerName, ItemName, Quantity, Price) VALUES (@cust, @item, @qty, @price)",
                        conn
                    );
                    cmd.Parameters.AddWithValue("@cust", customer);
                    cmd.Parameters.AddWithValue("@item", item);
                    cmd.Parameters.AddWithValue("@qty", quantity);
                    cmd.Parameters.AddWithValue("@price", price);

                    int row = cmd.ExecuteNonQuery();
                    if (row == 1)
                    {
                        MessageBox.Show("Order added successfully!");
                        ClearInputs();
                        LoadOrders();
                    }
                    else
                    {
                        MessageBox.Show("Failed to add order!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select an order to delete!");
                return;
            }

            int orderId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["OrderID"].Value);

            try
            {
                using (SqlConnection conn = new SqlConnection(strConnString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM TblOrders WHERE OrderID=@id", conn);
                    cmd.Parameters.AddWithValue("@id", orderId);

                    int row = cmd.ExecuteNonQuery();
                    if (row == 1)
                    {
                        MessageBox.Show("Order deleted successfully!");
                        LoadOrders();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete order!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void LoadOrders()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnString))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM TblOrders ORDER BY OrderDate DESC", conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvOrders.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading orders: " + ex.Message);
            }
        }

        private void ClearInputs()
        {
            txtCust.Text = "";
            txtItem.Text = "";
            txtQty.Text = "";
            txtPrice.Text = "";
        }
    }
}